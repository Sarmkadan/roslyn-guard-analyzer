#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RoslynGuardAnalyzer.Utilities;

/// <summary>
/// Extension methods for string manipulation, naming convention conversion, and validation.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a string to PascalCase (e.g., "hello_world" -> "HelloWorld").
    /// </summary>
    /// <param name="text">The input string to convert.</param>
    /// <returns>The PascalCase string, or <see langword="null"/> if the input is <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/></exception>
    public static string ToPascalCase(this string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (string.IsNullOrWhiteSpace(text))
            return text;

        var parts = text.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();

        foreach (var part in parts)
        {
            if (part.Length > 0)
                sb.Append(char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant());
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a string to camelCase (e.g., "hello_world" -> "helloWorld").
    /// </summary>
    /// <param name="text">The input string to convert.</param>
    /// <returns>The camelCase string, or <see langword="null"/> if the input is <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/></exception>
    public static string ToCamelCase(this string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var pascal = text.ToPascalCase();
        if (pascal.Length == 0)
            return pascal;

        return char.ToLowerInvariant(pascal[0]) + pascal[1..];
    }

    /// <summary>
    /// Converts a string to snake_case (e.g., "HelloWorld" -> "hello_world").
    /// </summary>
    /// <param name="text">The input string to convert.</param>
    /// <returns>The snake_case string, or <see langword="null"/> if the input is <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/></exception>
    public static string ToSnakeCase(this string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (string.IsNullOrEmpty(text))
            return text;

        var sb = new StringBuilder();
        var previousWasUpper = false;

        for (int i = 0; i < text.Length; i++)
        {
            var currentChar = text[i];
            var currentIsUpper = char.IsUpper(currentChar);

            if (currentIsUpper && i > 0 && !previousWasUpper)
                sb.Append('_');

            sb.Append(char.ToLowerInvariant(currentChar));
            previousWasUpper = currentIsUpper;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a string to kebab-case (e.g., "HelloWorld" -> "hello-world").
    /// </summary>
    /// <param name="text">The input string to convert.</param>
    /// <returns>The kebab-case string, or <see langword="null"/> if the input is <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/></exception>
    public static string ToKebabCase(this string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return text.ToSnakeCase().Replace('_', '-');
    }

    /// <summary>
    /// Truncates a string to a maximum length, optionally adding an ellipsis.
    /// </summary>
    /// <param name="text">The input string to truncate.</param>
    /// <param name="maxLength">The maximum length of the resulting string.</param>
    /// <param name="addEllipsis">Whether to append an ellipsis if truncation occurs.</param>
    /// <returns>The truncated string, or <see langword="null"/> if the input is <see langword="null"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is negative.</exception>
    public static string Truncate(this string text, int maxLength, bool addEllipsis = true)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        if (text.Length <= maxLength)
            return text;

        var result = text[..maxLength];
        return addEllipsis ? result + "..." : result;
    }

    /// <summary>
    /// Checks if a string starts with any of the given prefixes (case-insensitive).
    /// </summary>
    /// <param name="text">The string to check.</param>
    /// <param name="prefixes">The prefixes to match against.</param>
    /// <returns><see langword="true"/> if the string starts with any prefix; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> or <paramref name="prefixes"/> is <see langword="null"/></exception>
    public static bool StartsWithAny(this string text, params string[] prefixes)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(prefixes);

        return prefixes.Any(p => text.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if a string ends with any of the given suffixes (case-insensitive).
    /// </summary>
    /// <param name="text">The string to check.</param>
    /// <param name="suffixes">The suffixes to match against.</param>
    /// <returns><see langword="true"/> if the string ends with any suffix; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> or <paramref name="suffixes"/> is <see langword="null"/></exception>
    public static bool EndsWithAny(this string text, params string[] suffixes)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(suffixes);

        return suffixes.Any(s => text.EndsWith(s, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Counts the occurrences of a substring in a string.
    /// </summary>
    /// <param name="text">The input string to search.</param>
    /// <param name="substring">The substring to count.</param>
    /// <returns>The number of occurrences, or 0 if either input is <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> or <paramref name="substring"/> is <see langword="null"/></exception>
    public static int CountOccurrences(this string text, string substring)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(substring);

        int count = 0;
        int index = 0;

        while ((index = text.IndexOf(substring, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += substring.Length;
        }

        return count;
    }

    /// <summary>
    /// Removes all whitespace from a string.
    /// </summary>
    /// <param name="text">The input string.</param>
    /// <returns>A new string with all whitespace removed, or <see langword="null"/> if the input is <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/></exception>
    public static string RemoveWhitespace(this string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return new string(text.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    /// <summary>
    /// Repeats a string a specified number of times.
    /// </summary>
    /// <param name="text">The string to repeat.</param>
    /// <param name="count">The number of times to repeat the string.</param>
    /// <returns>A new string containing the repeated content, or <see cref="string.Empty"/> if count is 0 or negative.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/></exception>
    public static string Repeat(this string text, int count)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if (count == 0)
            return string.Empty;

        var sb = new StringBuilder(count * text.Length);
        for (int i = 0; i < count; i++)
        {
            sb.Append(text);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Validates that a string matches a specific pattern (regex).
    /// </summary>
    /// <param name="text">The string to validate.</param>
    /// <param name="pattern">The regex pattern to match against.</param>
    /// <returns><see langword="true"/> if the string matches the pattern; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> or <paramref name="pattern"/> is <see langword="null"/></exception>
    public static bool MatchesPattern(this string text, string pattern)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(pattern);

        try
        {
            return Regex.IsMatch(text, pattern);
        }
        catch (RegexParseException)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a string is a valid C# identifier.
    /// </summary>
    /// <param name="text">The string to validate.</param>
    /// <returns><see langword="true"/> if the string is a valid C# identifier; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/></exception>
    public static bool IsValidIdentifier(this string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length == 0)
            return false;

        if (!char.IsLetter(text[0]) && text[0] != '_')
            return false;

        return text.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    /// <summary>
    /// Gets the Levenshtein distance between two strings (for fuzzy matching).
    /// </summary>
    /// <param name="text1">The first string.</param>
    /// <param name="text2">The second string.</param>
    /// <returns>The Levenshtein distance between the two strings.</returns>
    public static int LevenshteinDistance(this string text1, string text2)
    {
        ArgumentNullException.ThrowIfNull(text1);
        ArgumentNullException.ThrowIfNull(text2);

        var matrix = new int[text1.Length + 1, text2.Length + 1];

        for (int i = 0; i <= text1.Length; i++)
            matrix[i, 0] = i;

        for (int j = 0; j <= text2.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= text1.Length; i++)
        {
            for (int j = 1; j <= text2.Length; j++)
            {
                var cost = text1[i - 1] == text2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[text1.Length, text2.Length];
    }
}