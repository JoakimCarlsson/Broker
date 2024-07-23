namespace Broker.SourceGenerator.Extensions;

internal static class IndentedTextWriterExtensions
{
    internal static void OpenCodeBlock(this IndentedTextWriter writer)
    {
        writer.WriteLine("{");
        writer.Indent++;
    }

    internal static void CloseCodeBlock(this IndentedTextWriter writer)
    {
        writer.Indent--;
        writer.WriteLine("}");
    }
}