#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Rules;
using RoslynGuardAnalyzer.Services;
using Xunit;

/// <summary>
/// Contains unit tests for the <see cref="AsyncVoidWarningRule"/>. 
/// Each test verifies a specific aspect of the rule's behavior, such as 
/// correctly identifying violations, ensuring non‑violations are not flagged,
/// and checking that the generated diagnostic message includes the expected
/// method name and location information.
/// </summary>
public sealed class AsyncVoidWarningRuleTests
{
    /// <summary>
    /// Verifies that the rule flags an async method that returns <c>void</c>.
    /// The test creates a <see cref="CodeElement"/> representing an async void method,
    /// invokes the rule's <c>ViolationPredicate</c> and <c>MessageFactory</c>, and asserts
    /// that a violation is reported, the message contains the method name and a warning
    /// about async void usage, and that the rule's default severity is <c>Warning</c>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Ensures that the rule does not flag an async method whose return type is not <c>void</c>.
    /// The test creates a <see cref="CodeElement"/> with <c>IsAsync = true</c> and <c>ReturnType = "Task"</c>,
    /// then verifies that <c>ViolationPredicate</c> returns <c>false</c>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Confirms that a non‑async method returning <c>void</c> is not flagged as a violation.
    /// The test creates a synchronous <see cref="CodeElement"/> and asserts that the rule
    /// does not report a violation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Verifies that an async void method marked as an event handler is exempt from the rule.
    /// The test adds an <c>EventHandler</c> attribute to the <see cref="CodeElement"/> and checks
    /// that no violation is reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Ensures that the rule does not flag elements that are not methods, such as classes.
    /// The test creates a <see cref="CodeElement"/> of type <c>Class</c> with async void characteristics
    /// and verifies that the rule does not treat it as a violation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Checks that the diagnostic message produced by the rule includes the method name
    /// and the source file location with line numbers.
    /// The test creates a <see cref="CodeElement"/> with a specific file path and line range,
    /// then asserts that the generated message contains both the method name and the formatted
    /// location string (e.g., <c>Service.cs(42-45)</c>).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
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
