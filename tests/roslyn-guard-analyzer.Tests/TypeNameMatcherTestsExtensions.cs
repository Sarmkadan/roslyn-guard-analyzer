#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using FluentAssertions;
using RoslynGuardAnalyzer.Utilities;
using Xunit;

/// <summary>
/// Extension methods for <see cref="TypeNameMatcherTests"/> to provide additional testing utilities.
/// </summary>
/// <exception cref="ArgumentNullException"><paramref name="typeNamePattern"/> is <see langword="null"/></exception>
/// <exception cref="ArgumentNullException"><paramref name="expectedMatches"/> is <see langword="null"/></exception>
/// <exception cref="ArgumentNullException"><paramref name="expectedNonMatches"/> is <see langword="null"/></exception>
/// <exception cref="ArgumentNullException"><paramref name="namespacePattern"/> is <see langword="null"/></exception>
/// <exception cref="ArgumentNullException"><paramref name="testNamespace"/> is <see langword="null"/></exception>
/// <exception cref="ArgumentNullException"><paramref name="namespaceVariations"/> is <see langword="null"/></exception>
/// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null"/></exception>
namespace RoslynGuardAnalyzer.Tests;

public static class TypeNameMatcherTestsExtensions
{
    /// <summary>
    /// Creates a matcher and verifies it matches the expected type names.
    /// </summary>
    /// <param name="typeNamePattern">The type name pattern to match against.</param>
    /// <param name="expectedMatches">Types that should match the pattern.</param>
    /// <param name="expectedNonMatches">Types that should not match the pattern.</param>
    /// <exception cref="ArgumentNullException"><paramref name="typeNamePattern"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="expectedMatches"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="expectedNonMatches"/> is <see langword="null"/></exception>
    public static void ShouldMatchTypes(this TypeNameMatcherTests _, string typeNamePattern, string[] expectedMatches, string[] expectedNonMatches)
    {
        ArgumentNullException.ThrowIfNull(typeNamePattern);
        ArgumentNullException.ThrowIfNull(expectedMatches);
        ArgumentNullException.ThrowIfNull(expectedNonMatches);

        // Arrange
        var matcher = new TypeNameMatcher(typeNamePattern);

        // Act & Assert - verify matches
        foreach (var typeName in expectedMatches)
        {
            matcher.Matches(typeName).Should().BeTrue($"Expected '{typeName}' to match pattern '{typeNamePattern}'");
        }

        // Act & Assert - verify non-matches
        foreach (var typeName in expectedNonMatches)
        {
            matcher.Matches(typeName).Should().BeFalse($"Expected '{typeName}' to NOT match pattern '{typeNamePattern}'");
        }
    }

    /// <summary>
    /// Tests namespace matching with various wildcard patterns.
    /// </summary>
    /// <param name="namespacePattern">The namespace pattern to test.</param>
    /// <param name="testNamespace">The namespace to test against.</param>
    /// <param name="shouldMatch">Whether the namespace should match the pattern.</param>
    /// <exception cref="ArgumentNullException"><paramref name="namespacePattern"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="testNamespace"/> is <see langword="null"/></exception>
    public static void ShouldMatchNamespace(this TypeNameMatcherTests _, string namespacePattern, string testNamespace, bool shouldMatch)
    {
        ArgumentNullException.ThrowIfNull(namespacePattern);
        ArgumentNullException.ThrowIfNull(testNamespace);

        // Arrange
        var matcher = new NamespaceMatcher(namespacePattern);

        // Act & Assert
        matcher.Matches(testNamespace).Should().Be(shouldMatch,
            $"Expected namespace '{testNamespace}' to {(shouldMatch ? "match" : "NOT match")} pattern '{namespacePattern}'");
    }

    /// <summary>
    /// Tests fully qualified type matching with multiple namespace variations.
    /// </summary>
    /// <param name="typeNamePattern">The type name pattern.</param>
    /// <param name="namespaceVariations">Namespace variations to test.</param>
    /// <param name="typeName">The type name to match.</param>
    /// <param name="shouldMatch">Whether all variations should match.</param>
    /// <exception cref="ArgumentNullException"><paramref name="typeNamePattern"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="namespaceVariations"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null"/></exception>
    public static void ShouldMatchFullyQualifiedWithVariations(this TypeNameMatcherTests _, string typeNamePattern, string[] namespaceVariations, string typeName, bool shouldMatch)
    {
        ArgumentNullException.ThrowIfNull(typeNamePattern);
        ArgumentNullException.ThrowIfNull(namespaceVariations);
        ArgumentNullException.ThrowIfNull(typeName);

        // Arrange
        var matcher = new TypeNameMatcher(typeNamePattern);

        // Act & Assert
        foreach (var namespaceName in namespaceVariations)
        {
            matcher.MatchesFullyQualified(namespaceName, typeName)
                .Should().Be(shouldMatch,
                    $"Expected type '{typeName}' in namespace '{namespaceName}' to {(shouldMatch ? "match" : "NOT match")} pattern '{typeNamePattern}'");
        }
    }

    /// <summary>
    /// Tests that a pattern matches nothing (negative test case).
    /// </summary>
    /// <param name="typeNamePattern">The type name pattern that should match nothing.</param>
    /// <exception cref="ArgumentNullException"><paramref name="typeNamePattern"/> is <see langword="null"/></exception>
    public static void ShouldMatchNothing(this TypeNameMatcherTests _, string typeNamePattern)
    {
        ArgumentNullException.ThrowIfNull(typeNamePattern);

        // Arrange
        var matcher = new TypeNameMatcher(typeNamePattern);

        // Act & Assert - verify it matches nothing
        matcher.Matches("AnyTypeName").Should().BeFalse($"Expected pattern '{typeNamePattern}' to match nothing");
        matcher.Matches("AnotherType").Should().BeFalse($"Expected pattern '{typeNamePattern}' to match nothing");
    }
}