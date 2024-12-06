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
    
    internal static void Indent(this IndentedTextWriter writer, int count = 1)
    {
        writer.Indent += count;
    }
}