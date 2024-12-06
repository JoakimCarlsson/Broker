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
                var varPrefix = requestType.GetHashCode().ToString("X4");
                var handlerVar = $"h{varPrefix}";

                nonGenericIndentedWriter.WriteLine($"case {requestType} cmd:");
                nonGenericIndentedWriter.Indent(4);
                nonGenericIndentedWriter.WriteLine($"var {handlerVar} = GetNonGenericHandler<{requestType}>();");
                nonGenericIndentedWriter.WriteLine("await ProcessPreProcessors(cmd, cancellationToken);");
                nonGenericIndentedWriter.WriteLine($"await ExecutePipelineBehaviors(cmd, () => {handlerVar}.HandleAsync(cmd, cancellationToken), cancellationToken);");
                nonGenericIndentedWriter.WriteLine("await ProcessPostProcessors(cmd, cancellationToken);");
                nonGenericIndentedWriter.WriteLine("return;");
                nonGenericIndentedWriter.Indent(-4);
            }

            if (@interface.TypeArguments.Length == 2)
            {
                var responseType = @interface.TypeArguments[1];
                var responseTypeString = responseType.ToString();
                var isNullable = responseTypeString.EndsWith("?");

                if (isNullable)
                    responseTypeString = responseTypeString.TrimEnd('?');
                    
                genericIndentedWriter.WriteLine($"case {requestType} cmd:");
                genericIndentedWriter.Indent(4);

                var varPrefix = requestType.GetHashCode().ToString("X4");
                var handlerVar = $"h{varPrefix}";
                var resultVar = $"r{varPrefix}";
                
                genericIndentedWriter.WriteLine($"var {handlerVar} = GetGenericHandler<{requestType}, {responseType}>();");
                genericIndentedWriter.WriteLine("await ProcessPreProcessors(cmd, cancellationToken);");
                genericIndentedWriter.WriteLine($"var {resultVar} = await ExecutePipelineBehaviors(cmd, () => {handlerVar}.HandleAsync(cmd, cancellationToken), cancellationToken);");
                genericIndentedWriter.WriteLine($"await ProcessPostProcessors(cmd, {resultVar}, cancellationToken);");
                genericIndentedWriter.WriteLine($"return System.Runtime.CompilerServices.Unsafe.As<{responseTypeString}, TResponse>(ref {resultVar});");
                
                genericIndentedWriter.Indent(-4);
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
                                 private readonly ConcurrentDictionary<Type, object> _preProcessorCache = new();
                                 private readonly ConcurrentDictionary<Type, object> _postProcessorCache = new();
                                 private readonly ConcurrentDictionary<Type, object> _pipelineBehaviorCache = new();
                             
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

                                 private IEnumerable<IRequestPreProcessor<TRequest>> GetPreProcessors<TRequest>()
                                 {
                                     return (IEnumerable<IRequestPreProcessor<TRequest>>)_preProcessorCache.GetOrAdd(typeof(IRequestPreProcessor<TRequest>), _ => _serviceProvider.GetServices<IRequestPreProcessor<TRequest>>());
                                 }

                                 private IEnumerable<IRequestPostProcessor<TRequest>> GetNonGenericPostProcessors<TRequest>()
                                 {
                                     return (IEnumerable<IRequestPostProcessor<TRequest>>)_postProcessorCache.GetOrAdd(typeof(IRequestPostProcessor<TRequest>), _ => _serviceProvider.GetServices<IRequestPostProcessor<TRequest>>());
                                 }

                                 private IEnumerable<IRequestPostProcessor<TRequest, TResponse>> GetGenericPostProcessors<TRequest, TResponse>()
                                 {
                                     return (IEnumerable<IRequestPostProcessor<TRequest, TResponse>>)_postProcessorCache.GetOrAdd(typeof(IRequestPostProcessor<TRequest, TResponse>), _ => _serviceProvider.GetServices<IRequestPostProcessor<TRequest, TResponse>>());
                                 }

                                 private IEnumerable<IRequestPipelineBehavior<TRequest>> GetPipelineBehaviorsWithoutResponse<TRequest>()
                                 {
                                     return (IEnumerable<IRequestPipelineBehavior<TRequest>>)_pipelineBehaviorCache.GetOrAdd(typeof(IRequestPipelineBehavior<TRequest>), _ => _serviceProvider.GetServices<IRequestPipelineBehavior<TRequest>>());
                                 }

                                 private IEnumerable<IRequestPipelineBehavior<TRequest, TResponse>> GetPipelineBehaviors<TRequest, TResponse>()
                                 {
                                     return (IEnumerable<IRequestPipelineBehavior<TRequest, TResponse>>)_pipelineBehaviorCache.GetOrAdd(typeof(IRequestPipelineBehavior<TRequest, TResponse>), _ => _serviceProvider.GetServices<IRequestPipelineBehavior<TRequest, TResponse>>());
                                 }

                                 private async Task ProcessPreProcessors<TRequest>(TRequest request, CancellationToken cancellationToken)
                                 {
                                     var preProcessors = GetPreProcessors<TRequest>();
                                     foreach (var preProcessor in preProcessors)
                                     {
                                         await preProcessor.ProcessAsync(request, cancellationToken);
                                     }
                                 }

                                 private async Task ProcessPostProcessors<TRequest>(TRequest request, CancellationToken cancellationToken)
                                 {
                                     var postProcessors = GetNonGenericPostProcessors<TRequest>();
                                     foreach (var postProcessor in postProcessors)
                                     {
                                         await postProcessor.ProcessAsync(request, cancellationToken);
                                     }
                                 }

                                 private async Task ProcessPostProcessors<TRequest, TResponse>(TRequest request, TResponse response, CancellationToken cancellationToken)
                                 {
                                     var postProcessors = GetGenericPostProcessors<TRequest, TResponse>();
                                     foreach (var postProcessor in postProcessors)
                                     {
                                         await postProcessor.ProcessAsync(request, response, cancellationToken);
                                     }
                                 }

                                 private async Task ExecutePipelineBehaviors<TRequest>(TRequest request, Func<Task> handlerDelegate, CancellationToken cancellationToken)
                                 {
                                     var behaviors = GetPipelineBehaviorsWithoutResponse<TRequest>();
                                     var next = new RequestHandlerDelegate(async () => await handlerDelegate());
                                     foreach (var behavior in behaviors.Reverse())
                                     {
                                         var nextCopy = next;
                                         next = new RequestHandlerDelegate(async () => { await behavior.HandleAsync(request, nextCopy, cancellationToken); });
                                     }

                                     await next();
                                 }

                                 private async Task<TResponse> ExecutePipelineBehaviors<TRequest, TResponse>(TRequest request, Func<Task<TResponse>> handlerDelegate, CancellationToken cancellationToken)
                                 {
                                     var behaviors = GetPipelineBehaviors<TRequest, TResponse>();
                                     var next = new RequestHandlerDelegate<TResponse>(handlerDelegate);
                                     foreach (var behavior in behaviors.Reverse())
                                     {
                                         var nextCopy = next;
                                         next = new RequestHandlerDelegate<TResponse>(() => behavior.HandleAsync(request, nextCopy, cancellationToken));
                                     }

                                     return await next();
                                 }

                                 public async Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
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
                                 }
                             }
                             """;

        context.AddSource("Sender.g.cs", senderSource);
    }
}
