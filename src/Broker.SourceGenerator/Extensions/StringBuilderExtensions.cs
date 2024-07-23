namespace Broker.SourceGenerator.Extensions;

internal static class StringBuilderExtensions
{
    internal static StringBuilder AddIndentedLine(this StringBuilder sb, string line, int indentLevel = 3)
    {
        sb.AppendLine(new string(' ', indentLevel * 4) + line);
        return sb;
    }
}