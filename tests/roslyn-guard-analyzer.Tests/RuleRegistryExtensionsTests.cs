#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Services;
using Xunit;

/// <summary>
/// Tests for the RuleRegistryExtensions class.
/// </summary>
public sealed class RuleRegistryExtensionsTests
{
    /// <summary>
    /// Verifies that GetRulesBySeverity returns only the rules whose default severity is Error.
    /// </summary>
    [Fact]
    public void GetRulesBySeverity_Error_ReturnsOnlyErrorRules()
    {
        // Arrange
        var registry = new RuleRegistry();

        // Act
        var rules = registry.GetRulesBySeverity(SeverityLevel.Error);

        // Assert
        rules.Should().ContainSingle()
            .Which.Id.Should().Be("LYR001");
    }

    /// <summary>
    /// Verifies that GetRulesBySeverity returns all default rules with Warning severity.
    /// </summary>
    [Fact]
    public void GetRulesBySeverity_Warning_ReturnsAllWarningRules()
    {
        // Arrange
        var registry = new RuleRegistry();

        // Act
        var rules = registry.GetRulesBySeverity(SeverityLevel.Warning);

        // Assert
        rules.Should().HaveCount(3);
        rules.Should().OnlyContain(r => r.DefaultSeverity == SeverityLevel.Warning);
    }

    /// <summary>
    /// Verifies that GetRulesBySeverity returns an empty list when no rule has the requested severity.
    /// </summary>
    [Fact]
    public void GetRulesBySeverity_NoMatchingRules_ReturnsEmptyList()
    {
        // Arrange
        var registry = new RuleRegistry();

        // Act
        var rules = registry.GetRulesBySeverity(SeverityLevel.Critical);

        // Assert
        rules.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that GetRulesBySeverity throws ArgumentNullException when the registry is null.
    /// </summary>
    [Fact]
    public void GetRulesBySeverity_NullRegistry_ThrowsArgumentNullException()
    {
        // Arrange
        RuleRegistry? registry = null;

        // Act
        var act = () => registry!.GetRulesBySeverity(SeverityLevel.Error);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
