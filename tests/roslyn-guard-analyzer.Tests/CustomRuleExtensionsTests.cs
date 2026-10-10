#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Rules;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="CustomRuleExtensions"/>.
/// </summary>
public sealed class CustomRuleExtensionsTests
{
    private static CustomAnalysisRule CreateRule(string id, bool isEnabled)
    {
        var rule = CustomRuleBuilder
            .Create(id, $"Rule {id}")
            .When(_ => true)
            .WithMessage("message")
            .Build();

        rule.IsEnabled = isEnabled;
        return rule;
    }

    [Fact]
    public void WhereEnabled_MixedRules_ReturnsOnlyEnabledInOriginalOrder()
    {
        // Arrange
        var rules = new List<AnalysisRule>
        {
            CreateRule("R001", isEnabled: true),
            CreateRule("R002", isEnabled: false),
            CreateRule("R003", isEnabled: true),
            CreateRule("R004", isEnabled: false)
        };

        // Act
        var enabled = rules.WhereEnabled().ToList();

        // Assert
        enabled.Select(r => r.Id).Should().Equal("R001", "R003");
    }

    [Fact]
    public void WhereEnabled_AllDisabled_ReturnsEmpty()
    {
        // Arrange
        var rules = new List<AnalysisRule>
        {
            CreateRule("R001", isEnabled: false),
            CreateRule("R002", isEnabled: false)
        };

        // Act
        var enabled = rules.WhereEnabled();

        // Assert
        enabled.Should().BeEmpty();
    }

    [Fact]
    public void WhereEnabled_EmptyCollection_ReturnsEmpty()
    {
        // Arrange
        var rules = Array.Empty<AnalysisRule>();

        // Act
        var enabled = rules.WhereEnabled();

        // Assert
        enabled.Should().BeEmpty();
    }

    [Fact]
    public void WhereEnabled_NullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<AnalysisRule>? rules = null;

        // Act
        Action act = () => rules!.WhereEnabled();

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("rules");
    }
}
