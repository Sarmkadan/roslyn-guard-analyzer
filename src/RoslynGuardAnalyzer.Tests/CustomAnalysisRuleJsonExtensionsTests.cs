// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text.Json;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Rules;
using RoslynGuardAnalyzer.Utilities;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class CustomAnalysisRuleJsonExtensionsTests
{
    private static CustomAnalysisRule CreateSampleRule()
    {
        // Minimal valid rule – all required ctor parameters are supplied.
        // The predicate and message generator are simple no‑op lambdas.
        return new CustomAnalysisRule(
            id: "R001",
            name: "Sample Rule",
            description: "A simple rule used for unit‑testing.",
            category: RuleCategory.Security,
            defaultSeverity: SeverityLevel.Warning,
            predicate: _ => true,
            messageGenerator: _ => "Violation detected."
        )
        {
            // Optional properties can be left at defaults; we set a harmless pattern.
            RulePattern = @"\btest\b",
            Configuration = new Dictionary<string, object>
            {
                { "threshold", 10 },
                { "enabled", true }
            }
        };
    }

    [Fact]
    public void ToJson_HappyPath_ReturnsValidJson()
    {
        // Arrange
        var rule = CreateSampleRule();

        // Act
        string json = rule.ToJson(indented: true);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
        // The JSON should contain the rule id and name.
        Assert.Contains("\"Id\":\"R001\"", json, StringComparison.Ordinal);
        Assert.Contains("\"Name\":\"Sample Rule\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsEquivalentRule()
    {
        // Arrange
        var original = CreateSampleRule();
        string json = original.ToJson();

        // Act
        var deserialized = CustomAnalysisRuleJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.Description, deserialized.Description);
        Assert.Equal(original.Category, deserialized.Category);
        Assert.Equal(original.DefaultSeverity, deserialized.DefaultSeverity);
        Assert.Equal(original.RulePattern, deserialized.RulePattern);
        Assert.Equal(original.Configuration.Count, deserialized.Configuration.Count);
        foreach (var kvp in original.Configuration)
        {
            Assert.True(deserialized.Configuration.ContainsKey(kvp.Key));
            Assert.Equal(kvp.Value, deserialized.Configuration[kvp.Key]);
        }
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndRule()
    {
        // Arrange
        var rule = CreateSampleRule();
        string json = rule.ToJson();

        // Act
        bool result = CustomAnalysisRuleJsonExtensions.TryFromJson(json, out var parsedRule);

        // Assert
        Assert.True(result);
        Assert.NotNull(parsedRule);
        Assert.Equal(rule.Id, parsedRule!.Id);
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CustomAnalysisRuleJsonExtensions.FromJson(json!));
    }

    [Fact]
    public void FromJson_EmptyJson_ThrowsArgumentException()
    {
        // Arrange
        string json = "   ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => CustomAnalysisRuleJsonExtensions.FromJson(json));
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Arrange
        // Missing required fields – this will cause deserialization to fail validation.
        string invalidJson = "{\"Id\":\"\",\"Name\":\"\",\"Description\":\"\",\"Category\":0,\"DefaultSeverity\":0}";

        // Act
        bool result = CustomAnalysisRuleJsonExtensions.TryFromJson(invalidJson, out var rule);

        // Assert
        Assert.False(result);
        Assert.Null(rule);
    }

    [Fact]
    public void FromJson_InvalidRule_ThrowsJsonException()
    {
        // Arrange
        // Id is empty which violates the validation rules.
        string json = "{\"Id\":\"\",\"Name\":\"Bad\",\"Description\":\"Bad\",\"Category\":1,\"DefaultSeverity\":1}";

        // Act & Assert
        var ex = Assert.Throws<JsonException>(() => CustomAnalysisRuleJsonExtensions.FromJson(json));
        Assert.Contains("Rule ID cannot be null or empty", ex.Message);
    }
}
