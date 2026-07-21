#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Rules;
using Xunit;

/// <summary>
/// Contains edge case tests for the CustomRuleBuilder class.
/// Tests untested paths, error conditions, and default behaviors.
/// </summary>
public sealed class CustomRuleBuilderEdgeTests
{
    /// <summary>
    /// Tests that building without a predicate throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void Build_WithoutPredicate_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS003", "Test Rule");

        // Act & Assert
        var act = () => builder.Build();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("A violation predicate must be configured before building the rule.");
    }

    /// <summary>
    /// Tests that building without a message factory uses default message.
    /// </summary>
    [Fact]
    public void Build_WithoutMessageFactory_UsesDefaultMessage()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS004", "Default Message Rule")
            .For(RuleCategory.CodeStructure)
            .WithSeverity(SeverityLevel.Info)
            .WithDescription("Test rule without custom message")
            .When(element => element.Name.StartsWith("Bad"));

        // Act
        var rule = builder.Build();

        // Assert
        rule.Id.Should().Be("CUS004");
        rule.Name.Should().Be("Default Message Rule");
        rule.Category.Should().Be(RuleCategory.CodeStructure);
        rule.DefaultSeverity.Should().Be(SeverityLevel.Info);
        rule.Description.Should().Be("Test rule without custom message");

        // Test default message factory behavior
        var testElement = new CodeElement("BadClass", CodeElementType.Class, "/src/Test.cs");
        var message = rule.MessageFactory(testElement);
        message.Should().Be("Rule 'Default Message Rule' was violated by 'BadClass'.");
    }

    /// <summary>
    /// Tests that building with empty description defaults to rule name.
    /// </summary>
    [Fact]
    public void Build_WithEmptyDescription_DefaultsToRuleName()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS005", "Name Based Rule")
            .For(RuleCategory.NamingConvention)
            .WithSeverity(SeverityLevel.Warning)
            .When(element => element.Name.Contains("Invalid"));

        // Act
        var rule = builder.Build();

        // Assert
        rule.Description.Should().Be("Name Based Rule");
    }

    /// <summary>
    /// Tests that building with null description defaults to rule name.
    /// </summary>
    [Fact]
    public void Build_WithNullDescription_DefaultsToRuleName()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS006", "Null Description Rule")
            .For(RuleCategory.AsyncPattern)
            .WithSeverity(SeverityLevel.Error)
            .WithDescription(null)
            .When(element => element.IsAsync);

        // Act
        var rule = builder.Build();

        // Assert
        rule.Description.Should().Be("Null Description Rule"); // Build() converts null to name
    }

    /// <summary>
    /// Tests that Create throws ArgumentException for null or empty id.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidId_ThrowsArgumentException(string invalidId)
    {
        // Act & Assert
        var act = () => CustomRuleBuilder.Create(invalidId, "Valid Name");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that Create throws ArgumentException for null or empty name.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ThrowsArgumentException(string invalidName)
    {
        // Act & Assert
        var act = () => CustomRuleBuilder.Create("CUS007", invalidName);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that WithMessage throws ArgumentException for null or empty message.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithMessage_WithInvalidMessage_ThrowsArgumentException(string invalidMessage)
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS008", "Message Validation Rule");

        // Act & Assert
        var act = () => builder.WithMessage(invalidMessage);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that When throws ArgumentNullException for null predicate.
    /// </summary>
    [Fact]
    public void When_WithNullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS009", "Null Predicate Rule");

        // Act & Assert
        var act = () => builder.When(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that WithMessage with Func throws ArgumentNullException for null factory.
    /// </summary>
    [Fact]
    public void WithMessage_WithNullFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS010", "Null Factory Rule");

        // Act & Assert
        var act = () => builder.WithMessage((Func<CodeElement, string>)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that WithDescription accepts null and Build converts it to rule name.
    /// </summary>
    [Fact]
    public void WithDescription_WithNull_BuildDefaultsToRuleName()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS011", "Null Description Test")
            .WithDescription(null)
            .When(element => true);

        // Act
        var rule = builder.Build();

        // Assert
        rule.Description.Should().Be("Null Description Test"); // Build() converts null to name
    }

    /// <summary>
    /// Tests that duplicate configuration calls use last-wins approach.
    /// </summary>
    [Fact]
    public void For_WithDuplicateCalls_LastWins()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS012", "Duplicate Category Rule")
            .For(RuleCategory.NamingConvention)
            .For(RuleCategory.AsyncPattern)
            .For(RuleCategory.NullSafety)
            .When(element => true);

        // Act
        var rule = builder.Build();

        // Assert
        rule.Category.Should().Be(RuleCategory.NullSafety);
    }

    /// <summary>
    /// Tests that duplicate severity calls use last-wins approach.
    /// </summary>
    [Fact]
    public void WithSeverity_WithDuplicateCalls_LastWins()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS013", "Duplicate Severity Rule")
            .WithSeverity(SeverityLevel.Info)
            .WithSeverity(SeverityLevel.Warning)
            .WithSeverity(SeverityLevel.Error)
            .When(element => true);

        // Act
        var rule = builder.Build();

        // Assert
        rule.DefaultSeverity.Should().Be(SeverityLevel.Error);
    }

    /// <summary>
    /// Tests that duplicate description calls use last-wins approach.
    /// </summary>
    [Fact]
    public void WithDescription_WithDuplicateCalls_LastWins()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS014", "Duplicate Description Rule")
            .WithDescription("First description")
            .WithDescription("Second description")
            .WithDescription("Third description")
            .When(element => true);

        // Act
        var rule = builder.Build();

        // Assert
        rule.Description.Should().Be("Third description");
    }

    /// <summary>
    /// Tests that duplicate predicate calls use last-wins approach.
    /// </summary>
    [Fact]
    public void When_WithDuplicateCalls_LastWins()
    {
        // Arrange
        var firstPredicateCalled = false;
        var secondPredicateCalled = false;

        var builder = CustomRuleBuilder.Create("CUS015", "Duplicate Predicate Rule")
            .When(element => { firstPredicateCalled = true; return element.Name.StartsWith("A"); })
            .When(element => { secondPredicateCalled = true; return element.Name.StartsWith("B"); });

        // Act
        var rule = builder.Build();

        // Assert - only the last predicate should be used
        firstPredicateCalled.Should().BeFalse();
        secondPredicateCalled.Should().BeFalse(); // Not called yet

        // Test the actual predicate
        var testElementA = new CodeElement("Apple", CodeElementType.Class, "/src/Test.cs");
        rule.ViolationPredicate(testElementA).Should().BeFalse(); // "Apple" doesn't start with "B"

        var testElementB = new CodeElement("Banana", CodeElementType.Class, "/src/Test.cs");
        rule.ViolationPredicate(testElementB).Should().BeTrue(); // "Banana" starts with "B"
    }

    /// <summary>
    /// Tests that duplicate message calls use last-wins approach.
    /// </summary>
    [Fact]
    public void WithMessage_WithDuplicateCalls_LastWins()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS016", "Duplicate Message Rule")
            .When(element => element.Name.Contains("Bad"))
            .WithMessage("First message")
            .WithMessage("Second message")
            .WithMessage("Third message");

        // Act
        var rule = builder.Build();

        // Assert
        var testElement = new CodeElement("BadClass", CodeElementType.Class, "/src/Test.cs");
        rule.MessageFactory(testElement).Should().Be("Third message");
    }

    /// <summary>
    /// Tests default category is CodeStructure.
    /// </summary>
    [Fact]
    public void Build_WithoutCategory_DefaultsToCodeStructure()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS017", "Default Category Rule")
            .When(element => true);

        // Act
        var rule = builder.Build();

        // Assert
        rule.Category.Should().Be(RuleCategory.CodeStructure);
    }

    /// <summary>
    /// Tests default severity is Warning.
    /// </summary>
    [Fact]
    public void Build_WithoutSeverity_DefaultsToWarning()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS018", "Default Severity Rule")
            .When(element => true);

        // Act
        var rule = builder.Build();

        // Assert
        rule.DefaultSeverity.Should().Be(SeverityLevel.Warning);
    }

    /// <summary>
    /// Tests that the built rule has correct metadata matching expected values.
    /// </summary>
    [Fact]
    public void Build_WithAllProperties_CorrectMetadata()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS019", "Metadata Test Rule")
            .For(RuleCategory.LayerDependency)
            .WithSeverity(SeverityLevel.Critical)
            .WithDescription("This is a comprehensive metadata test")
            .When(element => element.Complexity > 10)
            .WithMessage(element => $"High complexity detected: {element.Complexity}");

        // Act
        var rule = builder.Build();

        // Assert
        rule.Id.Should().Be("CUS019");
        rule.Name.Should().Be("Metadata Test Rule");
        rule.Category.Should().Be(RuleCategory.LayerDependency);
        rule.DefaultSeverity.Should().Be(SeverityLevel.Critical);
        rule.Description.Should().Be("This is a comprehensive metadata test");

        // Test predicate
        var simpleElement = new CodeElement("Simple", CodeElementType.Method, "/src/Simple.cs") { Complexity = 5 };
        rule.ViolationPredicate(simpleElement).Should().BeFalse();

        var complexElement = new CodeElement("Complex", CodeElementType.Method, "/src/Complex.cs") { Complexity = 15 };
        rule.ViolationPredicate(complexElement).Should().BeTrue();

        // Test message factory
        var message = rule.MessageFactory(complexElement);
        message.Should().Be("High complexity detected: 15");
    }

    /// <summary>
    /// Tests that WithMessage with string parameter creates correct message factory.
    /// </summary>
    [Fact]
    public void WithMessage_StringParameter_CreatesCorrectMessageFactory()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS020", "String Message Rule")
            .When(element => element.Name.Contains("Error"))
            .WithMessage("This element contains 'Error' in its name");

        // Act
        var rule = builder.Build();

        // Assert
        var testElement = new CodeElement("ErrorHandler", CodeElementType.Class, "/src/ErrorHandler.cs");
        var message = rule.MessageFactory(testElement);
        message.Should().Be("This element contains 'Error' in its name");
    }

    /// <summary>
    /// Tests chained configuration calls maintain state correctly.
    /// </summary>
    [Fact]
    public void ChainedConfiguration_MaintainsState()
    {
        // Arrange
        var builder = CustomRuleBuilder.Create("CUS021", "Chained Rule")
            .For(RuleCategory.NamingConvention)
            .WithSeverity(SeverityLevel.Error)
            .WithDescription("Chained configuration test")
            .When(element => element.IsPublic)
            .WithMessage("Public elements not allowed");

        // Act
        var rule = builder.Build();

        // Assert
        rule.Id.Should().Be("CUS021");
        rule.Name.Should().Be("Chained Rule");
        rule.Category.Should().Be(RuleCategory.NamingConvention);
        rule.DefaultSeverity.Should().Be(SeverityLevel.Error);
        rule.Description.Should().Be("Chained configuration test");

        var publicElement = new CodeElement("PublicClass", CodeElementType.Class, "/src/PublicClass.cs") { IsPublic = true };
        rule.ViolationPredicate(publicElement).Should().BeTrue();
        rule.MessageFactory(publicElement).Should().Be("Public elements not allowed");

        var privateElement = new CodeElement("PrivateClass", CodeElementType.Class, "/src/PrivateClass.cs") { IsPublic = false };
        rule.ViolationPredicate(privateElement).Should().BeFalse();
    }
}