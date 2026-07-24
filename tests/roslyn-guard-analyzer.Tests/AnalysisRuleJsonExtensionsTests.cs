#nullable enable

using System;
using FluentAssertions;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Core;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="AnalysisRuleJsonExtensions"/>.
/// </summary>
public class AnalysisRuleJsonExtensionsTests
{
    #region ToJson

    [Fact]
    public void ToJson_WithValidAnalysisRule_ReturnsJsonString()
    {
        // Arrange
        var rule = new AnalysisRule("rule-1", "Test Rule", "This is a test rule", RuleCategory.CodeStructure)
        {
            DefaultSeverity = SeverityLevel.Error,
            IsEnabled = true,
            DocumentationUrl = "https://example.com/docs",
            Author = "Test Author",
            Version = new Version("1.0.0"),
            Configuration = new() { { "threshold", 10 } }
        };

        // Act
        var json = rule.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("rule-1");
        json.Should().Contain("Test Rule");
        json.Should().Contain("codeStructure");
        json.Should().Contain("error");
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var rule = new AnalysisRule("rule-1", "Test Rule", "Description", RuleCategory.CodeStructure)
        {
            IsEnabled = false
        };

        // Act
        var json = rule.ToJson(indented: true);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("{");
        json.Should().Contain("}");
        json.Should().Contain("\n"); // Should have newlines for formatting
    }

    [Fact]
    public void ToJson_WithIndentedFalse_ReturnsCompactJson()
    {
        // Arrange
        var rule = new AnalysisRule("rule-1", "Test Rule", "Description", RuleCategory.CodeStructure);

        // Act
        var json = rule.ToJson(indented: false);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().NotContain("\n"); // Should be compact without newlines
    }

    [Fact]
    public void ToJson_WithNullAnalysisRule_ThrowsArgumentNullException()
    {
        // Arrange
        AnalysisRule? rule = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => rule!.ToJson());
    }

    [Fact]
    public void ToJson_WithRuleContainingNullValues_SerializesWithoutNullProperties()
    {
        // Arrange
        var rule = new AnalysisRule("rule-1", "Test Rule", "Description", RuleCategory.CodeStructure)
        {
            RulePattern = null,
            DocumentationUrl = null,
            Author = null,
            Version = null
        };

        // Act
        var json = rule.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().NotContain("null"); // Should not contain null values due to JsonIgnoreCondition.WhenWritingNull
    }

    #endregion

    #region FromJson

    [Fact]
    public void FromJson_WithValidJson_ReturnsAnalysisRuleInstance()
    {
        // Arrange - Create a rule and serialize it to get valid JSON
        var originalRule = new AnalysisRule("rule-1", "Test Rule", "This is a test rule", RuleCategory.CodeStructure)
        {
            DefaultSeverity = SeverityLevel.Warning,
            IsEnabled = true,
            RulePattern = "pattern-here",
            Configuration = new() { { "threshold", 10 } },
            DocumentationUrl = "https://example.com/docs",
            Author = "Test Author",
            Version = new Version("1.0.0")
        };
        var json = originalRule.ToJson();

        // Act
        var rule = AnalysisRuleJsonExtensions.FromJson(json);

        // Assert
        rule.Should().NotBeNull();
        rule!.Id.Should().Be("rule-1");
        rule.Name.Should().Be("Test Rule");
        rule.Description.Should().Be("This is a test rule");
        rule.Category.Should().Be(RuleCategory.CodeStructure);
        rule.DefaultSeverity.Should().Be(SeverityLevel.Warning);
        rule.IsEnabled.Should().BeTrue();
        rule.RulePattern.Should().Be("pattern-here");
        rule.Configuration.Should().ContainKey("threshold");
        rule.DocumentationUrl.Should().Be("https://example.com/docs");
        rule.Author.Should().Be("Test Author");
        rule.Version.Should().Be(new Version("1.0.0"));
    }

    [Fact]
    public void FromJson_WithCamelCaseProperties_ReturnsAnalysisRuleInstance()
    {
        // Arrange - Create a rule and serialize it to get valid JSON with camelCase properties
        var originalRule = new AnalysisRule("rule-2", "Another Rule", "Another description", RuleCategory.CodeStructure)
        {
            DefaultSeverity = SeverityLevel.Error
        };
        var json = originalRule.ToJson();

        // Act
        var rule = AnalysisRuleJsonExtensions.FromJson(json);

        // Assert
        rule.Should().NotBeNull();
        rule!.Id.Should().Be("rule-2");
        rule.Name.Should().Be("Another Rule");
        rule.DefaultSeverity.Should().Be(SeverityLevel.Error);
    }

    [Fact]
    public void FromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AnalysisRuleJsonExtensions.FromJson(json!));
    }

    [Fact]
    public void FromJson_WithEmptyJson_ThrowsArgumentException()
    {
        // Arrange
        var json = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => AnalysisRuleJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_WithWhitespaceJson_ThrowsArgumentException()
    {
        // Arrange
        var json = "   \n\t  ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => AnalysisRuleJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_WithInvalidJson_ReturnsNull()
    {
        // Arrange
        var json = "invalid json {{{";

        // Act
        var rule = AnalysisRuleJsonExtensions.FromJson(json);

        // Assert
        rule.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithMissingRequiredProperties_ReturnsPartialRule()
    {
        // Arrange
        var json = "{\"id\":\"rule-3\",\"description\":\"Only ID and description\"}";

        // Act
        var rule = AnalysisRuleJsonExtensions.FromJson(json);

        // Assert
        rule.Should().NotBeNull();
        rule!.Id.Should().Be("rule-3");
        rule.Description.Should().Be("Only ID and description");
        rule.Name.Should().Be(""); // Empty string since not provided in JSON
    }

    #endregion

    #region TryFromJson

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndSetsValue()
    {
        // Arrange - Create a rule and serialize it to get valid JSON
        var originalRule = new AnalysisRule("rule-4", "Try Rule", "Testing TryFromJson", RuleCategory.CodeStructure)
        {
            DefaultSeverity = SeverityLevel.Info
        };
        var json = originalRule.ToJson();

        AnalysisRule? rule = null;

        // Act
        var result = AnalysisRuleJsonExtensions.TryFromJson(json, out rule);

        // Assert
        result.Should().BeTrue();
        rule.Should().NotBeNull();
        rule!.Id.Should().Be("rule-4");
        rule.Name.Should().Be("Try Rule");
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndSetsValueToNull()
    {
        // Arrange
        var json = "invalid json {{{";
        AnalysisRule? rule = new AnalysisRule("old-id", "Old Name", "Old Desc", RuleCategory.CodeStructure);

        // Act
        var result = AnalysisRuleJsonExtensions.TryFromJson(json, out rule);

        // Assert
        result.Should().BeFalse();
        rule.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;
        AnalysisRule? rule = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AnalysisRuleJsonExtensions.TryFromJson(json!, out rule));
    }

    [Fact]
    public void TryFromJson_WithEmptyJson_ThrowsArgumentException()
    {
        // Arrange
        var json = "";
        AnalysisRule? rule = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => AnalysisRuleJsonExtensions.TryFromJson(json, out rule));
    }

    [Fact]
    public void TryFromJson_WithWhitespaceJson_ReturnsFalseAndSetsValueToNull()
    {
        // Arrange
        var json = "   \n\t  ";
        AnalysisRule? rule = new AnalysisRule("old-id", "Old Name", "Old Desc", RuleCategory.CodeStructure);

        // Act
        var result = AnalysisRuleJsonExtensions.TryFromJson(json, out rule);

        // Assert
        result.Should().BeFalse();
        rule.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithComplexRule_RoundTripsCorrectly()
    {
        // Arrange
        var originalRule = new AnalysisRule("complex-rule", "Complex Rule", "A complex test rule", RuleCategory.CodeStructure)
        {
            DefaultSeverity = SeverityLevel.Critical,
            IsEnabled = false,
            RulePattern = "pattern-regex",
            Configuration = new() { { "maxDepth", 5 }, { "enabledFeatures", new[] { "feature1", "feature2" } } },
            DocumentationUrl = "https://example.com/security-rules",
            Author = "Security Team",
            Version = new Version("2.1.0")
        };

        // Serialize to JSON
        var json = originalRule.ToJson();

        // Act - deserialize back
        var result = AnalysisRuleJsonExtensions.TryFromJson(json, out var deserializedRule);

        // Assert
        result.Should().BeTrue();
        deserializedRule.Should().NotBeNull();
        deserializedRule!.Id.Should().Be(originalRule.Id);
        deserializedRule.Name.Should().Be(originalRule.Name);
        deserializedRule.Description.Should().Be(originalRule.Description);
        deserializedRule.Category.Should().Be(originalRule.Category);
        deserializedRule.DefaultSeverity.Should().Be(originalRule.DefaultSeverity);
        deserializedRule.IsEnabled.Should().Be(originalRule.IsEnabled);
        deserializedRule.RulePattern.Should().Be(originalRule.RulePattern);
        deserializedRule.DocumentationUrl.Should().Be(originalRule.DocumentationUrl);
        deserializedRule.Author.Should().Be(originalRule.Author);
        deserializedRule.Version.Should().Be(originalRule.Version);
    }

    #endregion
}