#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using RoslynGuardAnalyzer.Utilities;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public sealed class StringExtensionsTests
{
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
