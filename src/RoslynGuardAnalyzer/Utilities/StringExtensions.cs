#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using RoslynGuardAnalyzer.Rules;

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

        var words = SplitIntoWords(text);
        if (words.Count == 0)
            return string.Empty;

        var sb = new StringBuilder(text.Length);
        foreach (var word in words)
        {
            if (word.Length == 0) continue;
            sb.Append(char.ToUpperInvariant(word[0]));
            if (word.Length > 1)
                sb.Append(word.AsSpan(1).ToString().ToLowerInvariant());
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

        var words = SplitIntoWords(text);
        if (words.Count == 0)
            return string.Empty;

        return string.Join("_", words).ToLowerInvariant();
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

        var words = SplitIntoWords(text);
        if (words.Count == 0)
            return string.Empty;

        return string.Join("-", words).ToLowerInvariant();
    }

    /// <summary>
    /// Splits a string into words by detecting separators ('_', '-', ' ') and camelCase/PascalCase boundaries,
    /// including proper handling of acronyms (e.g., "XMLHttpRequest" -> ["XML", "Http", "Request"]).
    /// </summary>
    private static List<string> SplitIntoWords(string text)
    {
        var words = new List<string>();
        var current = new StringBuilder();

        // First, split by explicit separators
        var segments = new List<string>();
        var seg = new StringBuilder();
        bool prevSep = true;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '_' || c == '-' || c == ' ')
            {
                if (!prevSep && seg.Length > 0)
                {
                    segments.Add(seg.ToString());
                    seg.Clear();
                }
                prevSep = true;
            }
            else
            {
                seg.Append(c);
                prevSep = false;
            }
        }
        if (seg.Length > 0)
            segments.Add(seg.ToString());

        // Then split each segment by camelCase/PascalCase boundaries
        foreach (var segment in segments)
        {
            current.Clear();
            for (int i = 0; i < segment.Length; i++)
            {
                char c = segment[i];
                if (i == 0)
                {
                    current.Append(c);
                    continue;
                }

                bool isUpper = char.IsUpper(c);
                bool prevIsUpper = char.IsUpper(segment[i - 1]);
                bool prevIsLower = char.IsLower(segment[i - 1]);
                bool prevIsDigit = char.IsDigit(segment[i - 1]);
                bool nextIsLower = i + 1 < segment.Length && char.IsLower(segment[i + 1]);

                // Split before: uppercase after lowercase/digit, or uppercase before lowercase in acronym run
                if (isUpper && (prevIsLower || prevIsDigit))
                {
                    // "helloWorld" -> "hello" | "World"
                    if (current.Length > 0)
                    {
                        words.Add(current.ToString());
                        current.Clear();
                    }
                }
                else if (isUpper && prevIsUpper && nextIsLower)
                {
                    // "XMLHttp" -> "XML" | "Http" (split before the last uppercase before lowercase)
                    if (current.Length > 0)
                    {
                        words.Add(current.ToString());
                        current.Clear();
                    }
                }
                else if (char.IsDigit(c) && !char.IsDigit(segment[i - 1]) && !char.IsUpper(segment[i - 1]))
                {
                    // Only split digit from lowercase, not from uppercase
                    // "utf8String" keeps "utf8" together
                }

                current.Append(c);
            }
            if (current.Length > 0)
                words.Add(current.ToString());
        }

        return words;
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
    /// Checks if a string contains any of the given values (case-insensitive).
    /// </summary>
    /// <param name="text">The string to check.</param>
    /// <param name="values">The values to match against.</param>
    /// <returns><see langword="true"/> if the string contains any value; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> or <paramref name="values"/> is <see langword="null"/></exception>
    public static bool ContainsAny(this string text, params string[] values)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(values);

        return values.Any(value => text.Contains(value, StringComparison.OrdinalIgnoreCase));
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
            return System.Text.RegularExpressions.Regex.IsMatch(text, pattern, System.Text.RegularExpressions.RegexOptions.None, System.Text.RegularExpressions.Regex.InfiniteMatchTimeout);
        }
        catch (System.Text.RegularExpressions.RegexParseException)
        {
            return false;
        }
        catch (System.Text.RegularExpressions.RegexMatchTimeoutException)
        {
            return false; // Pattern caused timeout during matching
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
    /// <remarks>
    /// The Levenshtein distance is the minimum number of single-character edits (insertions, deletions, or substitutions)
    /// required to change one string into the other. This implementation uses the two-row variant with early-exit optimization,
    /// providing O(min(m,n)) space complexity instead of the naive O(m*n) 2D matrix approach.
    /// </remarks>
    /// <param name="text1">The first string.</param>
    /// <param name="text2">The second string.</param>
    /// <returns>The Levenshtein distance between the two strings.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text1"/> or <paramref name="text2"/> is <see langword="null"/></exception>
    public static int LevenshteinDistance(this string text1, string text2)
    {
        ArgumentNullException.ThrowIfNull(text1);
        ArgumentNullException.ThrowIfNull(text2);

        return LevenshteinDistance(text1, text2, maxDistance: int.MaxValue);
    }

    /// <summary>
    /// Gets the Levenshtein distance between two strings with an optional maximum distance threshold.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Levenshtein distance is the minimum number of single-character edits (insertions, deletions, or substitutions)
    /// required to change one string into the other. This implementation uses the two-row variant with early-exit optimization,
    /// providing O(min(m,n)) space complexity instead of the naive O(m*n) 2D matrix approach.
    /// </para>
    /// <para>
    /// The <paramref name="maxDistance"/> parameter enables early-exit optimization: if the distance exceeds the threshold,
    /// the algorithm terminates early without computing the full distance. This is particularly useful for fuzzy matching
    /// scenarios where you only care whether two strings are "close enough" (e.g., for 'did you mean' suggestions).
    /// </para>
    /// <para>
    /// Time complexity: O(m*n) in the worst case, but often much better with early-exit.
    /// Space complexity: O(min(m,n)) - only two rows are stored in memory.
    /// </para>
    /// </remarks>
    /// <param name="text1">The first string.</param>
    /// <param name="text2">The second string.</param>
    /// <param name="maxDistance">The maximum distance to compute. If the distance exceeds this value, the method returns <paramref name="maxDistance"/> + 1.</param>
    /// <returns>
    /// The Levenshtein distance between the two strings, or <paramref name="maxDistance"/> + 1 if the distance exceeds <paramref name="maxDistance"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="text1"/> or <paramref name="text2"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxDistance"/> is negative.</exception>
    public static int LevenshteinDistance(this string text1, string text2, int maxDistance)
    {
        ArgumentNullException.ThrowIfNull(text1);
        ArgumentNullException.ThrowIfNull(text2);
        ArgumentOutOfRangeException.ThrowIfNegative(maxDistance);

        // Ensure text1 is the shorter string to minimize space usage
        if (text2.Length < text1.Length)
            (text1, text2) = (text2, text1);

        int m = text1.Length;
        int n = text2.Length;

        // Early exit: if the difference in length already exceeds maxDistance, return immediately
        if (n - m > maxDistance)
            return maxDistance + 1;

        // Use two-row array instead of full matrix: O(min(m,n)) space
        // We only need to keep track of the current and previous rows
        int[] prevRow = new int[n + 1];
        int[] currRow = new int[n + 1];

        // Initialize the first row (empty text1 vs text2)
        for (int j = 0; j <= n; j++)
            prevRow[j] = j;

        // Process each character of text1
        for (int i = 1; i <= m; i++)
        {
            // Initialize first column (text1[0..i-1] vs empty text2)
            currRow[0] = i;

            int minInRow = currRow[0]; // Track minimum value in current row for early-exit optimization

            for (int j = 1; j <= n; j++)
            {
                int cost = text1[i - 1] == text2[j - 1] ? 0 : 1;

                // Compute the three possible operations:
                // 1. Deletion (from previous row, same column)
                // 2. Insertion (from current row, previous column)
                // 3. Substitution (from previous row, previous column)
                int deletion = prevRow[j] + 1;
                int insertion = currRow[j - 1] + 1;
                int substitution = prevRow[j - 1] + cost;

                currRow[j] = Math.Min(Math.Min(deletion, insertion), substitution);

                // Update minimum in current row
                if (currRow[j] < minInRow)
                    minInRow = currRow[j];
            }

            // Early-exit optimization: if the minimum value in the current row already exceeds maxDistance,
            // the final distance will also exceed maxDistance, so we can return early
            if (minInRow > maxDistance)
                return maxDistance + 1;

            // Swap rows for next iteration
            (prevRow, currRow) = (currRow, prevRow);
        }

        int distance = prevRow[n];

        // Clamp to maxDistance + 1 if exceeded
        return distance > maxDistance ? maxDistance + 1 : distance;
    }

    /// <summary>
    /// Converts all backslashes in the string to forward slashes.
    /// </summary>
    /// <param name="text">The input string, potentially containing backslashes.</param>
    /// <returns>A string with all backslashes replaced by forward slashes, or <see langword="null"/> if the input is <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/></exception>
    public static string ToForwardSlashes(this string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return text.Replace('\\', '/');
    }
}
