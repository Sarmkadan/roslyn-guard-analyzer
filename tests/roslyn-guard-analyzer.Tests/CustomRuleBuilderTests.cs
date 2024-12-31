#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Rules;
using RoslynGuardAnalyzer.Services;
using Xunit;

/// <summary>
/// Contains tests for the CustomRuleBuilder class.
/// </summary>
public sealed class CustomRuleBuilderTests
{
    /// <summary>
    /// Tests that a valid rule is created with the correct properties.
    /// </summary>
    [Fact]
    public void Build_ValidRule_CreatesRuleWithCorrectProperties()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS001", "Public type suffix")
            .For(RuleCategory.NamingConvention)
            .WithSeverity(SeverityLevel.Warning)
            .WithDescription("Ensures public classes end with Service")
            .When(element => element.IsPublic && !element.Name.EndsWith("Service"))
            .WithMessage("Public types must end with Service");

        // Act
        var rule = builder.Build();

        // Assert
        rule.Id.Should().Be("CUS001");
        rule.Name.Should().Be("Public type suffix");
        rule.Category.Should().Be(RuleCategory.NamingConvention);
        rule.DefaultSeverity.Should().Be(SeverityLevel.Warning);
        rule.Description.Should().Be("Ensures public classes end with Service");
        rule.ViolationPredicate(new CodeElement("OrderManager", CodeElementType.Class, "/src/OrderManager.cs") { IsPublic = true }).Should().BeTrue();
        rule.ViolationPredicate(new CodeElement("OrderService", CodeElementType.Class, "/src/OrderService.cs") { IsPublic = true }).Should().BeFalse();
    }

    /// <summary>
    /// Tests that the violation predicate filters correctly.
    /// </summary>
    /// <returns>A task that completes when the test is finished.</returns>
    [Fact]
    public async Task When_ViolationPredicate_FiltersCorrectly()
    {
        // Arrange
        var registry = new RuleRegistry();
        var rule = CustomRuleBuilder.Create("CUS002", "Async suffix")
            .For(RuleCategory.AsyncPattern)
            .WithSeverity(SeverityLevel.Error)
            .WithDescription("Requires async methods to use the Async suffix")
            .When(element => element.ElementType == CodeElementType.Method && element.IsAsync && !element.Name.EndsWith("Async"))
            .WithMessage(element => $"Method '{element.Name}' must end with Async")
            .Build();

        registry.RegisterRule(rule);
        var engine = new RuleEngine(registry);
        var elements = new List<CodeElement>
        {
            new("LoadData", CodeElementType.Method, "/src/Orders.cs") { IsAsync = true, StartLineNumber = 10 },
            new("LoadDataAsync", CodeElementType.Method, "/src/Orders.cs") { IsAsync = true, StartLineNumber = 20 },
            new("Calculate", CodeElementType.Method, "/src/Orders.cs") { IsAsync = false, StartLineNumber = 30 }
        };

        // Act
        var violations = await engine.ExecuteRuleAsync(rule, elements);

        // Assert
        violations.Should().HaveCount(1);
        violations[0].RuleId.Should().Be("CUS002");
        violations[0].Message.Should().Be("Method 'LoadData' must end with Async");
        violations[0].Severity.Should().Be(SeverityLevel.Error);
        violations[0].LineNumber.Should().Be(10);
    }
}
