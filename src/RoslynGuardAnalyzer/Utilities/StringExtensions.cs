#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Buffers;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

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

        // Early-out: if already in PascalCase format, return as-is
        if (IsPascalCase(text))
            return text;

        // Fast path for common cases without separators
        if (text.Length <= 128 && !ContainsAny(text, '_', '-', ' '))
        {
            return CapitalizeFirstLetter(text);
        }

        return ConvertToPascalCase(text);

        static string CapitalizeFirstLetter(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            if (s.Length == 1)
                return char.ToUpperInvariant(s[0]).ToString();

            return char.ToUpperInvariant(s[0]) + s[1..];
        }

        static bool IsPascalCase(string s)
        {
            if (string.IsNullOrEmpty(s))
                return true;

            // Check if already in PascalCase (no separators, first char uppercase)
            if (s.Length > 0 && !char.IsUpper(s[0]))
                return false;

            // Check for separators that shouldn't be in PascalCase
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '_' || c == '-' || c == ' ')
                    return false;
            }

            return true;
        }

        static string ConvertToPascalCase(string text)
        {
            var separators = new[] { '_', '-', ' ' };
            bool needsConversion = false;

            // First pass: check if conversion is needed
            for (int i = 0; i < text.Length; i++)
            {
                if (separators.Contains(text[i]))
                {
                    needsConversion = true;
                    break;
                }
            }

            if (!needsConversion)
                return CapitalizeFirstLetter(text);

            // Count parts to determine if we need allocation
            int partCount = 1;
            for (int i = 0; i < text.Length; i++)
            {
                if (separators.Contains(text[i]))
                    partCount++;
            }

            // Use ArrayPool for part tracking if needed
            int[] partStarts = partCount <= 16
                ? stackalloc int[16].ToArray()
                : ArrayPool<int>.Shared.Rent(partCount);

            try
            {
                int partIndex = 0;
                partStarts[partIndex++] = 0;

                for (int i = 0; i < text.Length; i++)
                {
                    if (separators.Contains(text[i]))
                    {
                        partStarts[partIndex++] = i + 1;
                    }
                }

                // Calculate total length needed
                int resultLength = 0;
                for (int i = 0; i < partIndex; i++)
                {
                    int start = partStarts[i];
                    int end = i < partIndex - 1 ? partStarts[i + 1] - 1 : text.Length;
                    int partLength = end - start;

                    if (partLength > 0)
                    {
                        resultLength += 1; // First char uppercase
                        resultLength += partLength - 1; // Rest lowercase
                    }
                }

                if (resultLength == 0)
                    return string.Empty;

                // Use string.Create for zero-allocation result
                return string.Create(resultLength, text, (span, state) =>
                {
                    int charIndex = 0;
                    for (int i = 0; i < partIndex; i++)
                    {
                        int start = partStarts[i];
                        int end = i < partIndex - 1 ? partStarts[i + 1] - 1 : state.Length;
                        int partLength = end - start;

                        if (partLength > 0)
                        {
                            // Capitalize first letter
                            if (charIndex < span.Length)
                                span[charIndex++] = char.ToUpperInvariant(state[start]);

                            // Lowercase remaining letters
                            for (int j = start + 1; j < end && charIndex < span.Length; j++)
                            {
                                span[charIndex++] = char.ToLowerInvariant(state[j]);
                            }
                        }
                    }
                });
            }
            finally
            {
                if (partCount > 16)
                    ArrayPool<int>.Shared.Return(partStarts);
            }
        }
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

        if (string.IsNullOrWhiteSpace(text))
            return text;

        // Early-out: if already in camelCase format, return as-is
        if (IsCamelCase(text))
            return text;

        var pascal = text.ToPascalCase();
        if (pascal.Length == 0)
            return pascal;

        return char.ToLowerInvariant(pascal[0]) + pascal[1..];

        static bool IsCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s))
                return true;

            // Check if already in camelCase (no separators, first char lowercase)
            if (s.Length > 0 && !char.IsLower(s[0]))
                return false;

            // Check for separators that shouldn't be in camelCase
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '_' || c == '-' || c == ' ')
                    return false;
            }

            return true;
        }
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

        // Early-out: if already in snake_case format, return as-is
        if (IsSnakeCase(text))
            return text;

        // Fast path for strings that don't need conversion
        if (text.Length <= 128)
        {
            bool needsConversion = false;
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                {
                    needsConversion = true;
                    break;
                }
            }

            if (!needsConversion)
                return text.ToLowerInvariant();
        }

        return ConvertToSnakeCase(text);

        static bool IsSnakeCase(string s)
        {
            if (string.IsNullOrEmpty(s))
                return true;

            // Check if already in snake_case (only lowercase and underscores)
            bool hasUnderscore = false;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '_')
                    hasUnderscore = true;
                else if (char.IsUpper(c))
                    return false;
            }

            return hasUnderscore || !s.Contains('_');
        }

        static string ConvertToSnakeCase(string text)
        {
            // Calculate required length first
            int resultLength = text.Length;
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != '_')
                    resultLength++;
            }

            if (resultLength == text.Length)
                return text.ToLowerInvariant();

            // Use string.Create for zero-allocation result
            return string.Create(resultLength, text, (span, state) =>
            {
                int charIndex = 0;
                for (int i = 0; i < state.Length; i++)
                {
                    char c = state[i];
                    if (char.IsUpper(c))
                    {
                        // Insert underscore before uppercase letter (except first character)
                        if (i > 0 && charIndex < span.Length)
                            span[charIndex++] = '_';

                        if (charIndex < span.Length)
                            span[charIndex++] = char.ToLowerInvariant(c);
                    }
                    else
                    {
                        if (charIndex < span.Length)
                            span[charIndex++] = c;
                    }
                }
            });
        }
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

        if (string.IsNullOrWhiteSpace(text))
            return text;

        // Early-out: if already in kebab-case format, return as-is
        if (IsKebabCase(text))
            return text;

        return ConvertToKebabCase(text);

        static bool IsKebabCase(string s)
        {
            if (string.IsNullOrEmpty(s))
                return true;

            // Check if already in kebab-case (only lowercase and hyphens)
            bool hasHyphen = false;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '-')
                    hasHyphen = true;
                else if (char.IsUpper(c))
                    return false;
            }

            return hasHyphen || !s.Contains('-');
        }

        static string ConvertToKebabCase(string text)
        {
            var separators = new[] { '_', '-', ' ' };
            bool needsConversion = false;

            // First pass: check if conversion is needed
            for (int i = 0; i < text.Length; i++)
            {
                if (separators.Contains(text[i]))
                {
                    needsConversion = true;
                    break;
                }
            }

            if (!needsConversion)
                return text.ToLowerInvariant();

            // Count parts
            int partCount = 1;
            for (int i = 0; i < text.Length; i++)
            {
                if (separators.Contains(text[i]))
                    partCount++;
            }

            int[] partStarts = partCount <= 16
                ? stackalloc int[16].ToArray()
                : ArrayPool<int>.Shared.Rent(partCount);

            try
            {
                int partIndex = 0;
                partStarts[partIndex++] = 0;

                for (int i = 0; i < text.Length; i++)
                {
                    if (separators.Contains(text[i]))
                    {
                        partStarts[partIndex++] = i + 1;
                    }
                }

                // Calculate total length
                int resultLength = 0;
                for (int i = 0; i < partIndex; i++)
                {
                    int start = partStarts[i];
                    int end = i < partIndex - 1 ? partStarts[i + 1] - 1 : text.Length;
                    int partLength = end - start;

                    if (partLength > 0)
                    {
                        resultLength += partLength;
                        if (i < partIndex - 1)
                            resultLength++; // hyphen
                    }
                }

                if (resultLength == 0)
                    return string.Empty;

                return string.Create(resultLength, text, (span, state) =>
                {
                    int charIndex = 0;
                    for (int i = 0; i < partIndex; i++)
                    {
                        int start = partStarts[i];
                        int end = i < partIndex - 1 ? partStarts[i + 1] - 1 : state.Length;
                        int partLength = end - start;

                        if (partLength > 0)
                        {
                            for (int j = start; j < end; j++)
                            {
                                if (charIndex < span.Length)
                                    span[charIndex++] = char.ToLowerInvariant(state[j]);
                            }

                            if (i < partIndex - 1 && charIndex < span.Length)
                                span[charIndex++] = '-';
                        }
                    }
                });
            }
            finally
            {
                if (partCount > 16)
                    ArrayPool<int>.Shared.Return(partStarts);
            }
        }
    }

    /// <summary>
    /// Checks if a string contains any of the specified characters.
    /// </summary>
    /// <param name="s">The string to check.</param>
    /// <param name="chars">Characters to search for.</param>
    /// <returns>True if any character is found; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ContainsAny(string s, params char[] chars)
    {
        foreach (char c in s)
        {
            for (int j = 0; j < chars.Length; j++)
            {
                if (c == chars[j])
                    return true;
            }
        }
        return false;
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
            return System.Text.RegularExpressions.Regex.IsMatch(text, pattern);
        }
        catch (System.Text.RegularExpressions.RegexParseException)
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