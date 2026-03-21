// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using FluentAssertions;
using Moq;
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
    public async System.Threading.Tasks.Task ExecuteRuleAsync_MockedEngine_ReturnsConfiguredViolationsAndVerifiesInteraction()
    {
        // Arrange
        var mockEngine = new Mock<IRuleEngine>();
        var rule = new AnalysisRule("LYR001", "Layer Rule", "Enforces layer boundaries", RuleCategory.LayerDependency);
        var elements = new List<CodeElement>();

        var expectedViolation = new RuleViolation("LYR001", "Layer Rule", "Illegal cross-layer dependency", "/src/Api/OrderController.cs")
        {
            LineNumber = 15,
            ColumnNumber = 4,
            Severity = SeverityLevel.Error
        };

        mockEngine
            .Setup(e => e.ExecuteRuleAsync(It.IsAny<AnalysisRule>(), It.IsAny<List<CodeElement>>()))
            .ReturnsAsync(new List<RuleViolation> { expectedViolation });

        // Act
        var violations = await mockEngine.Object.ExecuteRuleAsync(rule, elements);

        // Assert
        violations.Should().HaveCount(1);
        violations[0].RuleId.Should().Be("LYR001");
        violations[0].Severity.Should().Be(SeverityLevel.Error);
        violations[0].IsCritical().Should().BeTrue();
        mockEngine.Verify(
            e => e.ExecuteRuleAsync(It.IsAny<AnalysisRule>(), It.IsAny<List<CodeElement>>()),
            Times.Once);
    }
}
