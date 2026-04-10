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
    public static string ToPascalCase(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var parts = text.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();

        foreach (var part in parts)
        {
            if (part.Length > 0)
                sb.Append(char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant());
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a string to camelCase (e.g., "hello_world" -> "helloWorld").
    /// </summary>
    public static string ToCamelCase(this string text)
    {
        var pascal = text.ToPascalCase();
        if (pascal.Length == 0)
            return pascal;

        return char.ToLowerInvariant(pascal[0]) + pascal.Substring(1);
    }

    /// <summary>
    /// Converts a string to snake_case (e.g., "HelloWorld" -> "hello_world").
    /// </summary>
    public static string ToSnakeCase(this string text)
    {
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
    public static string ToKebabCase(this string text)
    {
        return text.ToSnakeCase().Replace('_', '-');
    }

    /// <summary>
    /// Truncates a string to a maximum length, optionally adding an ellipsis.
    /// </summary>
    public static string Truncate(this string text, int maxLength, bool addEllipsis = true)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        var result = text.Substring(0, maxLength);
        return addEllipsis ? result + "..." : result;
    }

    /// <summary>
    /// Checks if a string starts with any of the given prefixes (case-insensitive).
    /// </summary>
    public static bool StartsWithAny(this string text, params string[] prefixes)
    {
        return prefixes.Any(p => text.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if a string ends with any of the given suffixes (case-insensitive).
    /// </summary>
    public static bool EndsWithAny(this string text, params string[] suffixes)
    {
        return suffixes.Any(s => text.EndsWith(s, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Counts the occurrences of a substring in a string.
    /// </summary>
    public static int CountOccurrences(this string text, string substring)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(substring))
            return 0;

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
    public static string RemoveWhitespace(this string text)
    {
        return new string(text.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    /// <summary>
    /// Repeats a string a specified number of times.
    /// </summary>
    public static string Repeat(this string text, int count)
    {
        if (count <= 0)
            return string.Empty;

        var sb = new StringBuilder();
        for (int i = 0; i < count; i++)
            sb.Append(text);

        return sb.ToString();
    }

    /// <summary>
    /// Validates that a string matches a specific pattern (regex).
    /// </summary>
    public static bool MatchesPattern(this string text, string pattern)
    {
        try
        {
            return Regex.IsMatch(text, pattern);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a string is a valid C# identifier.
    /// </summary>
    public static bool IsValidIdentifier(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        if (!char.IsLetter(text[0]) && text[0] != '_')
            return false;

        return text.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    /// <summary>
    /// Gets the Levenshtein distance between two strings (for fuzzy matching).
    /// </summary>
    public static int LevenshteinDistance(this string text1, string text2)
    {
        if (string.IsNullOrEmpty(text1))
            return text2?.Length ?? 0;

        if (string.IsNullOrEmpty(text2))
            return text1.Length;

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
