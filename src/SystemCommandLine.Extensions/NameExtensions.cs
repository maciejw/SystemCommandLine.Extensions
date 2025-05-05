using System.Text.RegularExpressions;

namespace ConsoleApp.CommandBuilders;

internal static partial class NameExtensions
{
    public static string ToKebabCase(string prefix, string name)
    {
        name = MyRegex().Replace(name, "-$1").Trim('-').ToLower();
        return $"{prefix}{name}";
    }

    [GeneratedRegex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
