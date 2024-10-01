// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using RoslynGuardAnalyzer.Utilities;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public sealed class TypeNameMatcherTests
{
    [Fact]
    public void Matches_ExactTypeName_ReturnsTrueCaseInsensitively()
    {
        // Arrange
        var matcher = new TypeNameMatcher("MyRepository");

        // Act & Assert
        matcher.Matches("myrepository").Should().BeTrue();
        matcher.Matches("MyRepository").Should().BeTrue();
        matcher.Matches("OtherClass").Should().BeFalse();
    }

    [Fact]
    public void Matches_StarWildcardSuffix_MatchesAllTypesWithGivenPrefix()
    {
        // Arrange
        var matcher = new TypeNameMatcher("*Service");

        // Act & Assert
        matcher.Matches("AnalysisService").Should().BeTrue();
        matcher.Matches("RuleService").Should().BeTrue();
        matcher.Matches("AnalysisRepository").Should().BeFalse();
    }

    [Fact]
    public void MatchesFullyQualified_WithNamespaceAndTypeName_CombinesBeforeMatching()
    {
        // Arrange
        var matcher = new TypeNameMatcher("MyApp.Services.*Service");

        // Act & Assert
        matcher.MatchesFullyQualified("MyApp.Services", "RuleService").Should().BeTrue();
        matcher.MatchesFullyQualified("MyApp.Services", "RuleRepository").Should().BeFalse();
    }

    [Fact]
    public void NamespaceMatcher_WildcardSegment_MatchesOneOrMoreIntermediateParts()
    {
        // Arrange
        var matcher = new NamespaceMatcher("MyApp.*.Services");

        // Act & Assert
        // single intermediate segment
        matcher.Matches("MyApp.Domain.Services").Should().BeTrue();
        // non-matching prefix
        matcher.Matches("OtherApp.Domain.Services").Should().BeFalse();
        // no intermediate segment at all (2 parts vs required 3-part structure)
        matcher.Matches("MyApp.Services").Should().BeFalse();
    }
}
