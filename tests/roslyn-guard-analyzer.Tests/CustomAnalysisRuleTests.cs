#nullable enable

using System;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Rules;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="CustomAnalysisRule"/> and its fluent builder
/// <see cref="CustomRuleBuilder"/>.
/// </summary>
public class CustomAnalysisRuleTests
{
    private static CodeElement CreateDummyElement(string name = "DummyElement")
    {
        // Most CodeElement implementations expose a settable Name property.
        // If the real type differs, this helper can be adjusted without
        // affecting the test logic.
        return new CodeElement { Name = name };
    }

    [Fact]
    public void Build_WithAllOptions_ReturnsConfiguredRule()
    {
        // Arrange
        var builder = CustomRuleBuilder
            .Create("R001", "Test Rule")
            .For(RuleCategory.LayerDependency)
            .WithSeverity(SeverityLevel.Error)
            .WithDescription("Custom description")
            .When(_ => true)
            .WithMessage("Custom violation message");

        // Act
        var rule = builder.Build();

        // Assert
        rule.Id.Should().Be("R001");
        rule.Name.Should().Be("Test Rule");
        rule.Description.Should().Be("Custom description");
        rule.Category.Should().Be(RuleCategory.LayerDependency);
        rule.DefaultSeverity.Should().Be(SeverityLevel.Error);
        rule.IsEnabled.Should().BeTrue(); // default from base class
        rule.ViolationPredicate.Should().NotBeNull();
        rule.MessageFactory.Should().NotBeNull();

        // The predicate should return true for any element.
        rule.ViolationPredicate(CreateDummyElement()).Should().BeTrue();

        // The message factory should return the constant we supplied.
        rule.MessageFactory(CreateDummyElement()).Should().Be("Custom violation message");
    }

    [Fact]
    public void Build_WithoutMessageFactory_UsesDefaultMessage()
    {
        // Arrange
        var builder = CustomRuleBuilder
            .Create("R002", "DefaultMsgRule")
            .When(_ => false); // predicate irrelevant for this test

        // Act
        var rule = builder.Build();

        // Assert
        var element = CreateDummyElement("MyClass");
        var message = rule.MessageFactory(element);

        // Default message format: "Rule '{RuleName}' was violated by '{Element.Name}'."
        message.Should().Be($"Rule 'DefaultMsgRule' was violated by '{element.Name}'.");
    }

    [Fact]
    public void When_NullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("R003", "NullPredicateRule");

        // Act
        Action act = () => builder.When(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("predicate");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithMessage_NullOrEmptyString_ThrowsArgumentException(string? message)
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("R004", "BadMessageRule");

        // Act
        Action act = () => builder.WithMessage(message!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("message");
    }

    [Fact]
    public void WithMessageFactory_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("R005", "NullFactoryRule");

        // Act
        Action act = () => builder.WithMessage((Func<CodeElement, string>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("messageFactory");
    }

    [Fact]
    public void Build_WithoutPredicate_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = CustomRuleBuilder
            .Create("R006", "MissingPredicateRule")
            .WithSeverity(SeverityLevel.Info);

        // Act
        Action act = () => builder.Build();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("A violation predicate must be configured before building the rule.");
    }

    [Fact]
    public void WithDescription_Null_UsesEmptyString()
    {
        // Arrange
        var builder = CustomRuleBuilder
            .Create("R007", "NullDescRule")
            .WithDescription(null!)
            .When(_ => true);

        // Act
        var rule = builder.Build();

        // Assert
        // When description is null, the builder falls back to the rule name.
        rule.Description.Should().Be("NullDescRule");
    }

    [Fact]
    public void For_SetsCategoryCorrectly()
    {
        // Arrange
        var builder = CustomRuleBuilder
            .Create("R008", "CategoryRule")
            .For(RuleCategory.NamingConvention)
            .When(_ => true);

        // Act
        var rule = builder.Build();

        // Assert
        rule.Category.Should().Be(RuleCategory.NamingConvention);
    }
}
