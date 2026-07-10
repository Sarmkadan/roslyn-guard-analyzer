#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using RoslynGuardAnalyzer.Utilities;
using Xunit;

/// <summary>
/// Contains tests for string extension methods.
/// </summary>
public sealed class StringExtensionsTests
{
    /// <summary>
    /// Tests the ToPascalCase method.
    /// </summary>
    [Fact]
    public void ToPascalCase_UnderscoreSeparatedInput_ReturnsPascalCase()
    {
        // Arrange
        const string input = "hello_world_foo";

        // Act
        var result = input.ToPascalCase();

        // Assert
        result.Should().Be("HelloWorldFoo");
    }

    /// <summary>
    /// Tests the ToCamelCase method.
    /// </summary>
    [Fact]
    public void ToCamelCase_HyphenSeparatedInput_ReturnsCamelCase()
    {
        // Arrange
        const string input = "hello-world";

        // Act
        var result = input.ToCamelCase();

        // Assert
        result.Should().Be("helloWorld");
    }

    /// <summary>
    /// Tests the ToSnakeCase method.
    /// </summary>
    [Fact]
    public void ToSnakeCase_PascalCaseInput_InsertsUnderscoreBeforeUpperCaseTransitions()
    {
        // Arrange
        const string input = "AnalysisService";

        // Act
        var result = input.ToSnakeCase();

        // Assert
        result.Should().Be("analysis_service");
    }

    /// <summary>
    /// Tests the LevenshteinDistance method.
    /// </summary>
    /// <param name="s">The input string.</param>
    /// <returns>The Levenshtein distance between the input string and itself.</returns>
    [Fact]
    public void LevenshteinDistance_IdenticalStrings_ReturnsZero()
    {
        // Arrange
        const string s = "RuleEngine";

        // Act
        var distance = s.LevenshteinDistance(s);

        // Assert
        distance.Should().Be(0);
    }

    /// <summary>
    /// Tests the CountOccurrences method.
    /// </summary>
    /// <param name="text">The text to search in.</param>
    /// <param name="sub">The substring to search for.</param>
    /// <returns>The number of occurrences of the substring in the text.</returns>
    [Fact]
    public void CountOccurrences_NonOverlappingSubstring_ReturnsCorrectCount()
    {
        // Arrange
        const string text = "abababab";
        const string sub  = "ab";

        // Act
        var count = text.CountOccurrences(sub);

        // Assert
        count.Should().Be(4);
    }
}
