namespace Broker.SourceGenerator.Generators;

[Generator]
public sealed class SenderSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.CompilationProvider, (productionContext, compilation) =>
            {
                using var nonGenericRequestSwitchCases = new StringWriter();
                using var genericRequestSwitchCases = new StringWriter();

                foreach (var syntaxTree in compilation.SyntaxTrees)
                {
                    var semanticModel = compilation.GetSemanticModel(syntaxTree);
                    AppendSwitchCasesForSyntaxTree(
                        semanticModel,
                        syntaxTree.GetClassDeclarationSyntax(),
                        nonGenericRequestSwitchCases,
                        genericRequestSwitchCases
                    );
                }

                GenerateSenderClassFile(
                    productionContext,
                    nonGenericRequestSwitchCases.ToString(),
                    genericRequestSwitchCases.ToString()
                );
            });
    }

    private void AppendSwitchCasesForSyntaxTree(
        SemanticModel semanticModel,
        IEnumerable<ClassDeclarationSyntax> classDeclarations,
        StringWriter nonGenericRequestSwitchCases,
        StringWriter genericRequestSwitchCases
    )
    {
        foreach (var classDeclaration in classDeclarations)
        {
            if (semanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol symbol)
                continue;

            AppendSwitchCases(
                symbol,
                nonGenericRequestSwitchCases,
                genericRequestSwitchCases
            );
        }
    }

    private void AppendSwitchCases(
        INamedTypeSymbol symbol,
        StringWriter nonGenericRequestSwitchCases,
        StringWriter genericRequestSwitchCases
    )
    {
        using var nonGenericIndentedWriter = new IndentedTextWriter(nonGenericRequestSwitchCases);
        using var genericIndentedWriter = new IndentedTextWriter(genericRequestSwitchCases);
        foreach (var @interface in symbol.Interfaces)
        {
            if (@interface.Name != "IHandler") continue;
            var requestType = @interface.TypeArguments[0].ToString();
            if (@interface.TypeArguments.Length == 1)
            {
                nonGenericIndentedWriter.WriteLine(
                    $"case {requestType} command: return GetNonGenericHandler<{requestType}>().HandleAsync(command, cancellationToken);");
            }

            if (@interface.TypeArguments.Length == 2)
            {
                var responseType = @interface.TypeArguments[1];
                var responseTypeString = responseType.ToString();

                var isNullable = responseTypeString.EndsWith("?");

                if (isNullable)
                    responseTypeString = responseTypeString.TrimEnd('?');

                genericIndentedWriter.Indent += 3;
                genericIndentedWriter.WriteLine($"case {requestType} command:");
                genericIndentedWriter.Indent++;
                genericIndentedWriter.WriteLine($"if (typeof(TResponse) == typeof({responseTypeString}))");
                genericIndentedWriter.OpenCodeBlock();
                genericIndentedWriter.WriteLine($"var handler = GetGenericHandler<{requestType}, {responseType}>();");
                genericIndentedWriter.WriteLine($"var task = handler.HandleAsync(command, cancellationToken);");
                genericIndentedWriter.WriteLine($"await task;");
                genericIndentedWriter.WriteLine($"var response = task.Result;");

                if (isNullable)
                {
                    genericIndentedWriter.WriteLine("if (response != null)");
                    genericIndentedWriter.OpenCodeBlock();
                    genericIndentedWriter.WriteLine($"return System.Runtime.CompilerServices.Unsafe.As<{responseTypeString}, TResponse>(ref response);");
                    genericIndentedWriter.CloseCodeBlock();
                    genericIndentedWriter.WriteLine("else");
                    genericIndentedWriter.OpenCodeBlock();
                    genericIndentedWriter.WriteLine("return default(TResponse);");
                    genericIndentedWriter.CloseCodeBlock();
                }
                else
                {
                    genericIndentedWriter.WriteLine($"return System.Runtime.CompilerServices.Unsafe.As<{responseTypeString}, TResponse>(ref response);");
                }

                genericIndentedWriter.CloseCodeBlock();
                genericIndentedWriter.WriteLine("break;");
                genericIndentedWriter.Indent--;
            }
        }
    }

    private void GenerateSenderClassFile(
        SourceProductionContext context,
        string nonGenericRequestSwitchCases,
        string genericRequestSwitchCases
    )
    {
        var senderSource = $$"""
                             // <auto-generated />
                             #nullable enable

                             using System;
                             using System.Collections.Concurrent;
                             using System.Threading.Tasks;
                             using System.Threading;
                             using Broker.Abstractions;
                             using Microsoft.Extensions.DependencyInjection;

                             namespace Broker.SourceGenerator;

                             public class Sender : ISender
                             {
                                 private readonly IServiceProvider _serviceProvider;
                                 private readonly ConcurrentDictionary<Type, object> _handlerCache = new();
                             
                                 public Sender(IServiceProvider serviceProvider)
                                 {
                                     _serviceProvider = serviceProvider;
                                 }

                                 private IHandler<TRequest> GetNonGenericHandler<TRequest>() where TRequest : IRequest
                                 {
                                     return (IHandler<TRequest>)_handlerCache.GetOrAdd(typeof(IHandler<TRequest>), _ => _serviceProvider.GetRequiredService<IHandler<TRequest>>());
                                 }

                                 private IHandler<TRequest, TResponse> GetGenericHandler<TRequest, TResponse>() where TRequest : IRequest<TResponse>
                                 {
                                     return (IHandler<TRequest, TResponse>)_handlerCache.GetOrAdd(typeof(IHandler<TRequest, TResponse>), _ => _serviceProvider.GetRequiredService<IHandler<TRequest, TResponse>>());
                                 }

                                 public Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
                                     where TRequest : IRequest
                                 {
                                     switch (request)
                                     {
                                         {{nonGenericRequestSwitchCases}}
                                         default:
                                             throw new InvalidOperationException($"No handler registered for type {request.GetType()}");
                                     }
                                 }
                             
                                 public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
                                 {
                                     switch (request)
                                     {
                                         {{genericRequestSwitchCases}}
                                         default:
                                             throw new InvalidOperationException($"No handler registered for type {request.GetType()}");
                                     }
                                     throw new InvalidOperationException("No handler registered for type");
                                 }
                             }
                             """;

        context.AddSource("Sender.g.cs", senderSource);
    }
}