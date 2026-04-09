#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RoslynGuardAnalyzer.Utilities;

/// <summary>
/// Matches type names against patterns, supporting wildcards and namespace matching.
/// Used for rule configuration to specify which types to analyze.
/// </summary>
public sealed class TypeNameMatcher
{
    private readonly string _pattern;
    private readonly Regex? _regex;
    private readonly bool _isExact;

    public TypeNameMatcher(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

        _pattern = pattern;

        // Check if pattern contains wildcards
        if (pattern.Contains('*') || pattern.Contains('?'))
        {
            _regex = WildcardToRegex(pattern);
        }
        else
        {
            _isExact = true;
        }
    }

    /// <summary>
    /// Checks if a type name matches the pattern.
    /// </summary>
    public bool Matches(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return false;

        if (_isExact)
            return typeName.Equals(_pattern, StringComparison.OrdinalIgnoreCase);

        return _regex?.IsMatch(typeName) ?? false;
    }

    /// <summary>
    /// Checks if a fully qualified type name matches the pattern.
    /// </summary>
    public bool MatchesFullyQualified(string @namespace, string typeName)
    {
        var fullName = string.IsNullOrEmpty(@namespace)
            ? typeName
            : @namespace + "." + typeName;

        return Matches(fullName);
    }

    /// <summary>
    /// Filters a collection of type names by this pattern.
    /// </summary>
    public IEnumerable<string> Filter(IEnumerable<string> typeNames)
    {
        return typeNames.Where(Matches);
    }

    /// <summary>
    /// Creates a matcher that matches any of the given patterns.
    /// </summary>
    public static bool MatchesAny(string typeName, params string[] patterns)
    {
        return patterns.Any(p => new TypeNameMatcher(p).Matches(typeName));
    }

    /// <summary>
    /// Gets the description of the pattern.
    /// </summary>
    public override string ToString() => $"TypeNameMatcher({_pattern})";

    /// <summary>
    /// Converts a wildcard pattern to a regex.
    /// * matches any sequence of characters
    /// ? matches any single character
    /// </summary>
    private static Regex WildcardToRegex(string pattern)
    {
        var regexPattern = "^" + Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".")
            + "$";

        return new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}

/// <summary>
/// Matches namespace names against patterns.
/// </summary>
public sealed class NamespaceMatcher
{
    private readonly string[] _segments;
    private readonly List<int> _wildcardSegments;

    public NamespaceMatcher(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

        _segments = pattern.Split('.');
        _wildcardSegments = _segments
            .Select((s, i) => s == "*" ? i : -1)
            .Where(i => i >= 0)
            .ToList();
    }

    /// <summary>
    /// Checks if a namespace matches the pattern.
    /// Pattern "A.*.C" matches "A.B.C", "A.X.Y.C", etc.
    /// </summary>
    public bool Matches(string namespaceName)
    {
        if (string.IsNullOrWhiteSpace(namespaceName))
            return false;

        var parts = namespaceName.Split('.');

        if (_wildcardSegments.Count == 0)
        {
            // No wildcards, must be exact
            if (parts.Length != _segments.Length)
                return false;

            return parts.SequenceEqual(_segments, StringComparer.OrdinalIgnoreCase);
        }

        // With wildcards, matching is more complex
        var segmentIndex = 0;

        for (int i = 0; i < _segments.Length; i++)
        {
            if (_segments[i] == "*")
            {
                // Skip at least one namespace part
                if (segmentIndex >= parts.Length)
                    return false;

                // Look for the next literal pattern
                if (i + 1 < _segments.Length)
                {
                    var nextLiteral = _segments[i + 1];
                    while (segmentIndex < parts.Length && !parts[segmentIndex].Equals(nextLiteral, StringComparison.OrdinalIgnoreCase))
                        segmentIndex++;

                    if (segmentIndex >= parts.Length)
                        return false;
                }
                else
                {
                    segmentIndex = parts.Length;
                }
            }
            else
            {
                if (segmentIndex >= parts.Length)
                    return false;

                if (!parts[segmentIndex].Equals(_segments[i], StringComparison.OrdinalIgnoreCase))
                    return false;

                segmentIndex++;
            }
        }

        return segmentIndex == parts.Length;
    }

    /// <summary>
    /// Gets the description of the pattern.
    /// </summary>
    public override string ToString() => $"NamespaceMatcher({string.Join(".", _segments)})";
}
