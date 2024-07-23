namespace Broker.SourceGenerator.Extensions;

internal static class SyntaxTreeExtensions
{
    public static IEnumerable<ClassDeclarationSyntax> GetClassDeclarationSyntax(this SyntaxTree syntaxTree)
    {
        return syntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>();
    }
}