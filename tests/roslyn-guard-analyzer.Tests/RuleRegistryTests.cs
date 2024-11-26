#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Exceptions;
using RoslynGuardAnalyzer.Services;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public sealed class RuleRegistryTests
{
    [Fact]
    public void RuleRegistry_DefaultInitialization_RegistersFourBuiltInRules()
    {
        // Arrange & Act
        var registry = new RuleRegistry();

        // Assert
        registry.GetAllRules().Should().HaveCount(4);
        registry.GetRule("LYR001").Should().NotBeNull();
        registry.GetRule("NAM001").Should().NotBeNull();
        registry.GetRule("ASY001").Should().NotBeNull();
        registry.GetRule("NUL001").Should().NotBeNull();
    }

    [Fact]
    public void RegisterRule_DuplicateRuleId_ThrowsConfigurationException()
    {
        // Arrange
        var registry = new RuleRegistry();
        var duplicate = new AnalysisRule("LYR001", "Layer Rule", "Duplicate rule", RuleCategory.LayerDependency);

        // Act
        var act = () => registry.RegisterRule(duplicate);

        // Assert
        act.Should().Throw<ConfigurationException>()
            .WithMessage("*LYR001*");
    }

    [Fact]
    public void RuleViolation_IsCritical_ReturnsTrueOnlyForErrorAndCriticalSeverity()
    {
        // Arrange
        var warningViolation = new RuleViolation("NAM001", "Naming Rule", "Name mismatch", "/src/Foo.cs")
        {
            Severity = SeverityLevel.Warning
        };

        var errorViolation = warningViolation.WithSeverity(SeverityLevel.Error);
        var criticalViolation = warningViolation.WithSeverity(SeverityLevel.Critical);

        // Act & Assert
        warningViolation.IsCritical().Should().BeFalse();
        errorViolation.IsCritical().Should().BeTrue();
        criticalViolation.IsCritical().Should().BeTrue();
    }

    [Fact]
    public void RegisterRule_ValidRule_RegistersSuccessfully()
    {
        // Arrange
        var registry = new RuleRegistry();
        registry.Clear();
        var rule = new AnalysisRule("NEW001", "New Rule", "Description", RuleCategory.NamingConvention);

        // Act
        registry.RegisterRule(rule);

        // Assert
        registry.GetRule("NEW001").Should().BeEquivalentTo(rule);
        registry.GetRuleCount().Should().Be(1);
    }

    [Fact]
    public void RegisterRule_NullRule_ThrowsArgumentNullException()
    {
        // Arrange
        var registry = new RuleRegistry();

        // Act
        var act = () => registry.RegisterRule(null!);

        // Assert
        act.Should().Throw<System.ArgumentNullException>();
    }

    [Fact]
    public void GetRule_NonExistentRuleId_ReturnsNull()
    {
        // Arrange
        var registry = new RuleRegistry();

        // Act
        var rule = registry.GetRule("NONEXISTENT");

        // Assert
        rule.Should().BeNull();
    }

    [Fact]
    public void GetRulesByCategory_MatchingCategory_ReturnsCorrectRules()
    {
        // Arrange
        var registry = new RuleRegistry();
        registry.Clear();
        var rule1 = new AnalysisRule("NAM001", "Naming Rule 1", "Desc", RuleCategory.NamingConvention);
        var rule2 = new AnalysisRule("NAM002", "Naming Rule 2", "Desc", RuleCategory.NamingConvention);
        var rule3 = new AnalysisRule("LYR001", "Layer Rule", "Desc", RuleCategory.LayerDependency);
        registry.RegisterRule(rule1);
        registry.RegisterRule(rule2);
        registry.RegisterRule(rule3);

        // Act
        var namingRules = registry.GetRulesByCategory(RuleCategory.NamingConvention.ToString());

        // Assert
        namingRules.Should().HaveCount(2);
        namingRules.Should().Contain(rule1);
        namingRules.Should().Contain(rule2);
        namingRules.Should().NotContain(rule3);
    }

    [Fact]
    public void RemoveRule_ExistingRule_ReturnsTrueAndRemovesRule()
    {
        // Arrange
        var registry = new RuleRegistry();
        var ruleId = "NAM001";

        // Act
        var result = registry.RemoveRule(ruleId);
        var rule = registry.GetRule(ruleId);

        // Assert
        result.Should().BeTrue();
        rule.Should().BeNull();
    }

    [Fact]
    public void GetEnabledRules_ReturnsOnlyEnabledRules()
    {
        // Arrange
        var registry = new RuleRegistry();
        registry.Clear();
        var enabledRule = new AnalysisRule("EN001", "Enabled", "Desc", RuleCategory.NamingConvention) { IsEnabled = true };
        var disabledRule = new AnalysisRule("DI001", "Disabled", "Desc", RuleCategory.NamingConvention) { IsEnabled = false };
        registry.RegisterRule(enabledRule);
        registry.RegisterRule(disabledRule);

        // Act
        var enabledRules = registry.GetEnabledRules();

        // Assert
        enabledRules.Should().HaveCount(1);
        enabledRules.Should().Contain(enabledRule);
        enabledRules.Should().NotContain(disabledRule);
    }

    [Fact]
    public void Clear_RemovesAllRules()
    {
        // Arrange
        var registry = new RuleRegistry();

        // Act
        registry.Clear();

        // Assert
        registry.GetRuleCount().Should().Be(0);
        registry.GetAllRules().Should().BeEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteRuleAsync_MockedEngine_ReturnsConfiguredViolationsAndVerifiesInteraction()
    {
        // Arrange
        var mockEngine = Substitute.For<IRuleEngine>();
        var rule = new AnalysisRule("LYR001", "Layer Rule", "Enforces layer boundaries", RuleCategory.LayerDependency);
        var elements = new List<CodeElement>();

        var expectedViolation = new RuleViolation("LYR001", "Layer Rule", "Illegal cross-layer dependency", "/src/Api/OrderController.cs")
        {
            LineNumber = 15,
            ColumnNumber = 4,
            Severity = SeverityLevel.Error
        };

        mockEngine
            .ExecuteRuleAsync(Arg.Any<AnalysisRule>(), Arg.Any<List<CodeElement>>())
            .Returns(System.Threading.Tasks.Task.FromResult(new List<RuleViolation> { expectedViolation }));

        // Act
        var violations = await mockEngine.ExecuteRuleAsync(rule, elements);

        // Assert
        violations.Should().HaveCount(1);
        violations[0].RuleId.Should().Be("LYR001");
        violations[0].Severity.Should().Be(SeverityLevel.Error);
        violations[0].IsCritical().Should().BeTrue();
        await mockEngine.Received(1).ExecuteRuleAsync(Arg.Any<AnalysisRule>(), Arg.Any<List<CodeElement>>());
    }
}
