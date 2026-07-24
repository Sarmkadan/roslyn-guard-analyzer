using System;
using System.Collections.Generic;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class AnalysisRuleTests
{
    [Fact]
    public void Constructor_InitializesDefaults()
    {
        // Act
        var rule = new AnalysisRule();

        // Assert
        Assert.Equal(string.Empty, rule.Id);
        Assert.Equal(string.Empty, rule.Name);
        Assert.Equal(string.Empty, rule.Description);
        Assert.Equal(RuleCategory.CodeStructure, rule.Category);
        Assert.Equal(SeverityLevel.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabled);
        Assert.NotNull(rule.Configuration);
        Assert.Empty(rule.Configuration);
    }

    [Theory]
    [InlineData("R001", "ValidRuleName")]
    [InlineData("123", "12345")]
    [InlineData("1234567890", "12345678901234567890")]
    public void IsValid_ValidRule_ReturnsTrue(string id, string name)
    {
        // Arrange
        var rule = new AnalysisRule(id, name, "Description", RuleCategory.CodeStructure);

        // Act & Assert
        Assert.True(rule.IsValid());
    }

    [Theory]
    [InlineData("R1", "ValidName")] // Id too short
    [InlineData("R0000000001", "ValidName")] // Id too long
    [InlineData("R001", "Name")] // Name too short
    [InlineData("R001", "A")] // Name too short
    [InlineData("R001", "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901")] // Name too long (101 chars)
    public void IsValid_InvalidRule_ReturnsFalse(string id, string name)
    {
        // Arrange
        var rule = new AnalysisRule(id, name, "Description", RuleCategory.CodeStructure);

        // Act & Assert
        Assert.False(rule.IsValid());
    }

    [Fact]
    public void Configuration_Methods_WorkCorrectly()
    {
        // Arrange
        var rule = new AnalysisRule();
        rule.SetConfigurationValue("key1", "value1");
        rule.SetConfigurationValue("key2", 123);

        // Act & Assert
        Assert.Equal("value1", rule.GetConfigurationValue<string>("key1"));
        Assert.Equal(123, rule.GetConfigurationValue<int>("key2"));
        Assert.Equal("default", rule.GetConfigurationValue<string>("nonexistent", "default"));
    }

    [Fact]
    public void SetConfigurationValue_NullKey_ThrowsArgumentException()
    {
        // Arrange
        var rule = new AnalysisRule();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => rule.SetConfigurationValue<string>(null!, "value"));
    }

    [Fact]
    public void WithSeverity_CreatesCopyWithNewSeverity()
    {
        // Arrange
        var rule = new AnalysisRule("R001", "ValidRuleName", "Desc", RuleCategory.CodeStructure);
        rule.SetConfigurationValue("key", "value");

        // Act
        var newRule = rule.WithSeverity(SeverityLevel.Error);

        // Assert
        Assert.NotSame(rule, newRule);
        Assert.Equal(SeverityLevel.Error, newRule.DefaultSeverity);
        Assert.Equal(rule.Id, newRule.Id);
        Assert.Equal("value", newRule.GetConfigurationValue<string>("key"));
    }

    [Fact]
    public void MarkAsModified_UpdatesModifiedAt()
    {
        // Arrange
        var rule = new AnalysisRule();
        Assert.Null(rule.ModifiedAt);

        // Act
        rule.MarkAsModified();

        // Assert
        Assert.NotNull(rule.ModifiedAt);
        Assert.True(rule.ModifiedAt <= DateTime.UtcNow);
    }
}
