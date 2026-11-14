using System.Text.RegularExpressions;

namespace SystemCommandLine.Extensions;

/// <summary>Formats conventional command-line option names.</summary>
public static partial class NameFormatExtensions
{
    /// <summary>Converts a name to lower-case kebab case and prepends the supplied prefix.</summary>
    /// <param name="prefix">The text to prepend, such as <c>--</c> for a long option.</param>
    /// <param name="name">The name to format, such as <c>LogEventLevel</c>.</param>
    /// <returns>The prefixed name, for example <c>--log-event-level</c>.</returns>
    /// <remarks>Leading and trailing hyphens are removed from the formatted name before the prefix is added. Lowercasing uses the current culture.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
    public static string ToKebabCase(string prefix, string name)
    {
        name = MyRegex().Replace(name, "-$1").Trim('-').ToLower();
        return $"{prefix}{name}";
    }

    [GeneratedRegex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
