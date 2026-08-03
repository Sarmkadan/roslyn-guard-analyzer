// Copyright (c) 2024.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public sealed class RuleRegistryExtensionsTests
{
    private static RuleRegistry CreateRegistry(IEnumerable<AnalysisRule> rules)
    {
        // The concrete RuleRegistry type in the repository has a public constructor that
        // accepts an IEnumerable<AnalysisRule>. If the signature changes, adjust this
        // helper accordingly.
        return new RuleRegistry(rules);
    }

    private static AnalysisRule NewRule(string id, string category = "default")
        => new AnalysisRule
        {
            Id = id,
            Category = category,
            // other required properties can stay at their defaults
        };

    #region GetRequiredRule

    [Fact]
    public void GetRequiredRule_ReturnsRule_WhenPresent()
    {
        var rule = NewRule("R001");
        var registry = CreateRegistry(new[] { rule });

        var result = registry.GetRequiredRule("R001");

        Assert.Same(rule, result);
    }

    [Fact]
    public void GetRequiredRule_ThrowsArgumentNullException_WhenRegistryIsNull()
    {
        RuleRegistry? registry = null;

        Assert.Throws<ArgumentNullException>(() => registry!.GetRequiredRule("any"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetRequiredRule_ThrowsArgumentException_WhenIdInvalid(string? id)
    {
        var registry = CreateRegistry(Array.Empty<AnalysisRule>());

        Assert.Throws<ArgumentException>(() => registry.GetRequiredRule(id!));
    }

    [Fact]
    public void GetRequiredRule_ThrowsKeyNotFoundException_WhenRuleMissing()
    {
        var registry = CreateRegistry(Array.Empty<AnalysisRule>());

        Assert.Throws<KeyNotFoundException>(() => registry.GetRequiredRule("Missing"));
    }

    #endregion

    #region ContainsRule

    [Fact]
    public void ContainsRule_ReturnsTrue_WhenRuleExists()
    {
        var rule = NewRule("R002");
        var registry = CreateRegistry(new[] { rule });

        var exists = registry.ContainsRule("R002");

        Assert.True(exists);
    }

    [Fact]
    public void ContainsRule_ReturnsFalse_WhenRuleDoesNotExist()
    {
        var registry = CreateRegistry(Array.Empty<AnalysisRule>());

        var exists = registry.ContainsRule("R999");

        Assert.False(exists);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ContainsRule_ReturnsFalse_WhenIdInvalid(string? id)
    {
        var registry = CreateRegistry(Array.Empty<AnalysisRule>());

        var exists = registry.ContainsRule(id!);

        Assert.False(exists);
    }

    [Fact]
    public void ContainsRule_ThrowsArgumentNullException_WhenRegistryIsNull()
    {
        RuleRegistry? registry = null;

        Assert.Throws<ArgumentNullException>(() => registry!.ContainsRule("any"));
    }

    #endregion

    #region GetRuleCountByCategory

    [Fact]
    public void GetRuleCountByCategory_ReturnsCorrectCount()
    {
        var rules = new[]
        {
            NewRule("A1", "cat1"),
            NewRule("A2", "cat1"),
            NewRule("B1", "cat2")
        };
        var registry = CreateRegistry(rules);

        var countCat1 = registry.GetRuleCountByCategory("cat1");
        var countCat2 = registry.GetRuleCountByCategory("cat2");
        var countMissing = registry.GetRuleCountByCategory("nonexistent");

        Assert.Equal(2, countCat1);
        Assert.Equal(1, countCat2);
        Assert.Equal(0, countMissing);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetRuleCountByCategory_ReturnsZero_WhenCategoryInvalid(string? category)
    {
        var registry = CreateRegistry(Array.Empty<AnalysisRule>());

        var count = registry.GetRuleCountByCategory(category!);

        Assert.Equal(0, count);
    }

    [Fact]
    public void GetRuleCountByCategory_ThrowsArgumentNullException_WhenRegistryIsNull()
    {
        RuleRegistry? registry = null;

        Assert.Throws<ArgumentNullException>(() => registry!.GetRuleCountByCategory("any"));
    }

    #endregion

    #region GetAllRuleIds

    [Fact]
    public void GetAllRuleIds_ReturnsAllIds_InOrder()
    {
        var rules = new[]
        {
            NewRule("X1"),
            NewRule("X2"),
            NewRule("X3")
        };
        var registry = CreateRegistry(rules);

        var ids = registry.GetAllRuleIds();

        Assert.Equal(new[] { "X1", "X2", "X3" }, ids);
    }

    [Fact]
    public void GetAllRuleIds_ReturnsEmpty_WhenNoRules()
    {
        var registry = CreateRegistry(Array.Empty<AnalysisRule>());

        var ids = registry.GetAllRuleIds();

        Assert.Empty(ids);
    }

    [Fact]
    public void GetAllRuleIds_ThrowsArgumentNullException_WhenRegistryIsNull()
    {
        RuleRegistry? registry = null;

        Assert.Throws<ArgumentNullException>(() => registry!.GetAllRuleIds());
    }

    #endregion
}
