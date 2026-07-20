#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Rules;
using RoslynGuardAnalyzer.Services;
using Xunit;

public sealed class AsyncVoidWarningRuleTests
{
    [Fact]
    public async Task AsyncVoidWarningRule_FlagsAsyncVoidMethod()
    {
        // Arrange
        var element = new CodeElement("TestMethod", CodeElementType.Method, "/test/file.cs")
        {
            IsAsync = true,
            ReturnType = "void",
            StartLineNumber = 10,
            EndLineNumber = 12
        };

        var rule = AsyncVoidWarningRule.Create();

        // Act
        var isViolation = rule.ViolationPredicate(element);
        var message = rule.MessageFactory(element);

        // Assert
        isViolation.Should().BeTrue();
        message.Should().Contain("Async void method 'TestMethod'");
        message.Should().Contain("should be avoided");
        rule.DefaultSeverity.Should().Be(SeverityLevel.Warning);
    }

    [Fact]
    public async Task AsyncVoidWarningRule_DoesNotFlagNonVoidReturnType()
    {
        // Arrange
        var element = new CodeElement("TestMethod", CodeElementType.Method, "/test/file.cs")
        {
            IsAsync = true,
            ReturnType = "Task",
            StartLineNumber = 10,
            EndLineNumber = 12
        };

        var rule = AsyncVoidWarningRule.Create();

        // Act
        var isViolation = rule.ViolationPredicate(element);

        // Assert
        isViolation.Should().BeFalse();
    }

    [Fact]
    public async Task AsyncVoidWarningRule_DoesNotFlagNonAsyncMethod()
    {
        // Arrange
        var element = new CodeElement("TestMethod", CodeElementType.Method, "/test/file.cs")
        {
            IsAsync = false,
            ReturnType = "void",
            StartLineNumber = 10,
            EndLineNumber = 12
        };

        var rule = AsyncVoidWarningRule.Create();

        // Act
        var isViolation = rule.ViolationPredicate(element);

        // Assert
        isViolation.Should().BeFalse();
    }

    [Fact]
    public async Task AsyncVoidWarningRule_DoesNotFlagEventHandlerMethod()
    {
        // Arrange
        var element = new CodeElement("OnClick", CodeElementType.Method, "/test/file.cs")
        {
            IsAsync = true,
            ReturnType = "void",
            StartLineNumber = 10,
            EndLineNumber = 12,
            Attributes = new List<string> { "EventHandler" }
        };

        var rule = AsyncVoidWarningRule.Create();

        // Act
        var isViolation = rule.ViolationPredicate(element);

        // Assert
        isViolation.Should().BeFalse();
    }

    [Fact]
    public async Task AsyncVoidWarningRule_DoesNotFlagNonMethodElement()
    {
        // Arrange
        var element = new CodeElement("TestClass", CodeElementType.Class, "/test/file.cs")
        {
            IsAsync = true,
            ReturnType = "void",
            StartLineNumber = 10,
            EndLineNumber = 12
        };

        var rule = AsyncVoidWarningRule.Create();

        // Act
        var isViolation = rule.ViolationPredicate(element);

        // Assert
        isViolation.Should().BeFalse();
    }

    [Fact]
    public async Task AsyncVoidWarningRule_MessageContainsLocation()
    {
        // Arrange
        var element = new CodeElement("ProblemMethod", CodeElementType.Method, "/src/MyProject/Service.cs")
        {
            IsAsync = true,
            ReturnType = "void",
            StartLineNumber = 42,
            EndLineNumber = 45
        };

        var rule = AsyncVoidWarningRule.Create();

        // Act
        var message = rule.MessageFactory(element);

        // Assert
        message.Should().Contain("ProblemMethod");
        message.Should().Contain("Service.cs(42-45)");
    }
}