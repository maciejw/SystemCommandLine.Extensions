using System.Text.RegularExpressions;

namespace SystemCommandLine.Extensions;

/// <summary>
/// Provides extension methods for converting names to kebab-case format with optional prefixing.
/// </summary>
/// <remarks>Kebab-case formatting replaces word boundaries with hyphens and converts all characters to lowercase.
/// The methods in this class are intended for scenarios where consistent, URL-friendly or configuration-friendly naming
/// is required. This class is static and cannot be instantiated.</remarks>
public static partial class NameFormatExtensions
{
    /// <summary>
    /// Converts the specified name to kebab-case and prepends the given prefix.
    /// </summary>
    /// <remarks>Kebab-case formatting replaces word boundaries with hyphens and converts all characters to
    /// lowercase. If the prefix is not empty, it is added directly before the kebab-case name without a
    /// separator.</remarks>
    /// <param name="prefix">The string to prepend to the resulting kebab-case name. Can be empty if no prefix is desired.</param>
    /// <param name="name">The name to convert to kebab-case. Cannot be null.</param>
    /// <returns>A string containing the prefix followed by the kebab-case representation of the name.</returns>
    public static string ToKebabCase(string prefix, string name)
    {
        name = MyRegex().Replace(name, "-$1").Trim('-').ToLower();
        return $"{prefix}{name}";
    }

    [GeneratedRegex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
