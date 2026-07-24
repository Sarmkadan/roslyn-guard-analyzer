#nullable enable

using System;
using System.Collections.Generic;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="AnalysisRuleExtensions"/> extension methods.
/// Tests all public methods: IsEquivalentTo, GetCategoryAndSeverityString, IsSubsetOf.
/// </summary>
public class AnalysisRuleExtensionsTests
{
    #region Test data setup

    private static AnalysisRule CreateTestRule(
        string id = "TEST001",
        string name = "Test Rule",
        string description = "Test description",
        RuleCategory category = RuleCategory.CodeStructure,
        SeverityLevel severity = SeverityLevel.Warning,
        bool isEnabled = true,
        string? rulePattern = "pattern1")
    {
        return new AnalysisRule
        {
            Id = id,
            Name = name,
            Description = description,
            Category = category,
            DefaultSeverity = severity,
            IsEnabled = isEnabled,
            RulePattern = rulePattern,
            Configuration = new Dictionary<string, object> { { "key1", "value1" } }
        };
    }

    private static AnalysisRule CreateEquivalentRule(AnalysisRule baseRule, string? rulePattern = null)
    {
        return new AnalysisRule
        {
            Id = baseRule.Id,
            Name = baseRule.Name,
            Description = baseRule.Description,
            Category = baseRule.Category,
            DefaultSeverity = baseRule.DefaultSeverity,
            IsEnabled = baseRule.IsEnabled,
            RulePattern = rulePattern ?? baseRule.RulePattern,
            Configuration = new Dictionary<string, object>(baseRule.Configuration)
        };
    }

    #endregion

    #region IsEquivalentTo tests

    [Fact]
    public void IsEquivalentTo_WithTwoIdenticalRules_ReturnsTrue()
    {
        // Arrange
        var rule1 = CreateTestRule();
        var rule2 = CreateEquivalentRule(rule1);

        // Act
        var result = rule1.IsEquivalentTo(rule2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsEquivalentTo_WithDifferentIds_ReturnsFalse()
    {
        // Arrange
        var rule1 = CreateTestRule(id: "TEST001");
        var rule2 = CreateTestRule(id: "TEST002");

        // Act
        var result = rule1.IsEquivalentTo(rule2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEquivalentTo_WithDifferentNames_ReturnsFalse()
    {
        // Arrange
        var rule1 = CreateTestRule(name: "Test Rule 1");
        var rule2 = CreateTestRule(name: "Test Rule 2");

        // Act
        var result = rule1.IsEquivalentTo(rule2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEquivalentTo_WithDifferentDescriptions_ReturnsFalse()
    {
        // Arrange
        var rule1 = CreateTestRule(description: "Description 1");
        var rule2 = CreateTestRule(description: "Description 2");

        // Act
        var result = rule1.IsEquivalentTo(rule2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEquivalentTo_WithDifferentCategories_ReturnsFalse()
    {
        // Arrange
        var rule1 = CreateTestRule(category: RuleCategory.CodeStructure);
        var rule2 = CreateTestRule(category: RuleCategory.LayerDependency);

        // Act
        var result = rule1.IsEquivalentTo(rule2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEquivalentTo_WithDifferentSeverities_ReturnsFalse()
    {
        // Arrange
        var rule1 = CreateTestRule(severity: SeverityLevel.Warning);
        var rule2 = CreateTestRule(severity: SeverityLevel.Error);

        // Act
        var result = rule1.IsEquivalentTo(rule2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEquivalentTo_WithDifferentEnabledStatus_ReturnsFalse()
    {
        // Arrange
        var rule1 = CreateTestRule(isEnabled: true);
        var rule2 = CreateTestRule(isEnabled: false);

        // Act
        var result = rule1.IsEquivalentTo(rule2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEquivalentTo_WithDifferentRulePatterns_ReturnsFalse()
    {
        // Arrange
        var rule1 = CreateTestRule(rulePattern: "pattern1");
        var rule2 = CreateTestRule(rulePattern: "pattern2");

        // Act
        var result = rule1.IsEquivalentTo(rule2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEquivalentTo_WithDifferentConfigurations_ReturnsFalse()
    {
        // Arrange
        var rule1 = CreateTestRule();
        var rule2 = CreateEquivalentRule(rule1);
        rule2.Configuration["key1"] = "differentValue";

        // Act
        var result = rule1.IsEquivalentTo(rule2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEquivalentTo_WithMissingConfigurationKey_ReturnsFalse()
    {
        // Arrange
        var rule1 = CreateTestRule();
        var rule2 = CreateEquivalentRule(rule1);
        rule2.Configuration.Clear();

        // Act
        var result = rule1.IsEquivalentTo(rule2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEquivalentTo_WithNullRule1_ThrowsArgumentNullException()
    {
        // Arrange
        AnalysisRule? rule1 = null;
        var rule2 = CreateTestRule();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => rule1!.IsEquivalentTo(rule2));
    }

    [Fact]
    public void IsEquivalentTo_WithNullRule2_ThrowsArgumentNullException()
    {
        // Arrange
        var rule1 = CreateTestRule();
        AnalysisRule? rule2 = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => rule1.IsEquivalentTo(rule2!));
    }

    #endregion

    #region GetCategoryAndSeverityString tests

    [Fact]
    public void GetCategoryAndSeverityString_WithCodeStructureAndWarning_ReturnsCorrectFormat()
    {
        // Arrange
        var rule = CreateTestRule(
            category: RuleCategory.CodeStructure,
            severity: SeverityLevel.Warning
        );

        // Act
        var result = rule.GetCategoryAndSeverityString();

        // Assert
        result.Should().Be("CodeStructure - Warning");
    }

    [Fact]
    public void GetCategoryAndSeverityString_WithLayerDependencyAndError_ReturnsCorrectFormat()
    {
        // Arrange
        var rule = CreateTestRule(
            category: RuleCategory.LayerDependency,
            severity: SeverityLevel.Error
        );

        // Act
        var result = rule.GetCategoryAndSeverityString();

        // Assert
        result.Should().Be("LayerDependency - Error");
    }

    [Fact]
    public void GetCategoryAndSeverityString_WithNullRule_ThrowsArgumentNullException()
    {
        // Arrange
        AnalysisRule? rule = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => rule!.GetCategoryAndSeverityString());
    }

    [Theory]
    [InlineData(RuleCategory.NamingConvention, SeverityLevel.Info, "NamingConvention - Info")]
    [InlineData(RuleCategory.AsyncPattern, SeverityLevel.Warning, "AsyncPattern - Warning")]
    [InlineData(RuleCategory.NullSafety, SeverityLevel.Error, "NullSafety - Error")]
    [InlineData(RuleCategory.CodeStructure, SeverityLevel.Critical, "CodeStructure - Critical")]
    public void GetCategoryAndSeverityString_WithAllSeverityLevels_ReturnsCorrectFormat(
        RuleCategory category, SeverityLevel severity, string expected)
    {
        // Arrange
        var rule = CreateTestRule(category: category, severity: severity);

        // Act
        var result = rule.GetCategoryAndSeverityString();

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region IsSubsetOf tests

    [Fact]
    public void IsSubsetOf_WithIdenticalPatterns_ReturnsTrue()
    {
        // Arrange
        var rule1 = CreateTestRule(rulePattern: "test.*");
        var rule2 = CreateTestRule(rulePattern: "test.*");

        // Act
        var result = rule1.IsSubsetOf(rule2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSubsetOf_WithSubsetPattern_ReturnsTrue()
    {
        // Arrange
        var rule1 = CreateTestRule(rulePattern: "test");
        var rule2 = CreateTestRule(rulePattern: "test.*");

        // Act
        var result = rule1.IsSubsetOf(rule2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSubsetOf_WithSupersetPattern_ReturnsFalse()
    {
        // Arrange
        var rule1 = CreateTestRule(rulePattern: "test.*");
        var rule2 = CreateTestRule(rulePattern: "test");

        // Act
        var result = rule1.IsSubsetOf(rule2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSubsetOf_WithCaseInsensitiveMatch_ReturnsTrue()
    {
        // Arrange
        var rule1 = CreateTestRule(rulePattern: "TEST");
        var rule2 = CreateTestRule(rulePattern: "test.*");

        // Act
        var result = rule1.IsSubsetOf(rule2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSubsetOf_WithEmptyPattern_ReturnsFalse()
    {
        // Arrange
        var rule1 = CreateTestRule(rulePattern: "");
        var rule2 = CreateTestRule(rulePattern: "test.*");

        // Act
        var result = rule1.IsSubsetOf(rule2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSubsetOf_WithNullPattern_ReturnsFalse()
    {
        // Arrange
        var rule1 = CreateTestRule(rulePattern: null);
        var rule2 = CreateTestRule(rulePattern: "test.*");

        // Act
        var result = rule1.IsSubsetOf(rule2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSubsetOf_WithNullOtherPattern_ReturnsFalse()
    {
        // Arrange
        var rule1 = CreateTestRule(rulePattern: "test");
        var rule2 = CreateTestRule(rulePattern: null);

        // Act
        var result = rule1.IsSubsetOf(rule2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSubsetOf_WithBothNullPatterns_ReturnsFalse()
    {
        // Arrange
        var rule1 = CreateTestRule(rulePattern: null);
        var rule2 = CreateTestRule(rulePattern: null);

        // Act
        var result = rule1.IsSubsetOf(rule2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSubsetOf_WithNullRule1_ThrowsArgumentNullException()
    {
        // Arrange
        AnalysisRule? rule1 = null;
        var rule2 = CreateTestRule(rulePattern: "test.*");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => rule1!.IsSubsetOf(rule2));
    }

    [Fact]
    public void IsSubsetOf_WithNullRule2_ThrowsArgumentNullException()
    {
        // Arrange
        var rule1 = CreateTestRule(rulePattern: "test");
        AnalysisRule? rule2 = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => rule1.IsSubsetOf(rule2!));
    }


    [Fact]
    public void IsSubsetOf_WithPatternContainedInOtherPattern_ReturnsTrue()
    {
        // Arrange
        var rule1 = CreateTestRule(rulePattern: "test.*");
        var rule2 = CreateTestRule(rulePattern: "test.*.cs");

        // Act
        var result = rule1.IsSubsetOf(rule2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSubsetOf_WithCompletelyDifferentPatterns_ReturnsFalse()
    {
        // Arrange
        var rule1 = CreateTestRule(rulePattern: "test");
        var rule2 = CreateTestRule(rulePattern: "other");

        // Act
        var result = rule1.IsSubsetOf(rule2);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}