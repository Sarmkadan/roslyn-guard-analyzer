#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Rules;
using RoslynGuardAnalyzer.Services;
using Xunit;
using System.IO;

/// <summary>
/// Contains unit tests for the <see cref="EmptyCatchBlockRule"/> class.
/// </summary>
public sealed class EmptyCatchBlockRuleTests
{
    /// <summary>
    /// Tests that the rule flags a catch block with no statements and no throw statement.
    /// </summary>
    [Fact]
    public async Task EmptyCatchBlockRule_FlagsCatchBlockWithNoStatementsAndNoThrow()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var content = @"
try
{
}
catch (Exception)
{
}";
            File.WriteAllText(tempFile, content);

            var element = new CodeElement("CatchBlock", CodeElementType.CatchBlock, tempFile)
            {
                StartLineNumber = 4 // Line where catch starts
            };

            var rule = EmptyCatchBlockRule.Create();

            // Act
            var isViolation = rule.ViolationPredicate(element);
            var message = rule.MessageFactory(element);

            // Assert
            isViolation.Should().BeTrue();
            message.Should().Contain("Empty catch block 'CatchBlock'");
            message.Should().Contain("Either remove the catch block or add proper exception handling");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Tests that the rule does not flag a catch block containing a throw statement.
    /// </summary>
    [Fact]
    public async Task EmptyCatchBlockRule_DoesNotFlagCatchBlockWithThrow()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var content = @"
try
{
}
catch (Exception)
{
    throw;
}
";
            File.WriteAllText(tempFile, content);

            var element = new CodeElement("CatchBlock", CodeElementType.CatchBlock, tempFile)
            {
                StartLineNumber = 4
            };

            var rule = EmptyCatchBlockRule.Create();

            // Act
            var isViolation = rule.ViolationPredicate(element);

            // Assert
            isViolation.Should().BeFalse();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Tests that the rule does not flag a catch block containing a throw new statement.
    /// </summary>
    [Fact]
    public async Task EmptyCatchBlockRule_DoesNotFlagCatchBlockWithThrowNew()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var content = @"
try
{
}
catch (Exception ex)
{
    throw new Exception(""Error"", ex);
}
";
            File.WriteAllText(tempFile, content);

            var element = new CodeElement("CatchBlock", CodeElementType.CatchBlock, tempFile)
            {
                StartLineNumber = 4
            };

            var rule = EmptyCatchBlockRule.Create();

            // Act
            var isViolation = rule.ViolationPredicate(element);

            // Assert
            isViolation.Should().BeFalse();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Tests that the rule does not flag a catch block containing a logging statement.
    /// </summary>
    [Fact]
    public async Task EmptyCatchBlockRule_DoesNotFlagCatchBlockWithLogging()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var content = @"
try
{
}
catch (Exception ex)
{
    System.Diagnostics.Debug.WriteLine(ex.Message);
}
";
            File.WriteAllText(tempFile, content);

            var element = new CodeElement("CatchBlock", CodeElementType.CatchBlock, tempFile)
            {
                StartLineNumber = 4
            };

            var rule = EmptyCatchBlockRule.Create();

            // Act
            var isViolation = rule.ViolationPredicate(element);

            // Assert
            isViolation.Should().BeFalse();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Tests that the rule does not flag a catch block containing only comments.
    /// </summary>
    [Fact]
    public async Task EmptyCatchBlockRule_DoesNotFlagCatchBlockWithCommentOnly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var content = @"
try
{
}
catch (Exception ex)
{
    // This exception is expected
    // Another comment
}
";
            File.WriteAllText(tempFile, content);

            var element = new CodeElement("CatchBlock", CodeElementType.CatchBlock, tempFile)
            {
                StartLineNumber = 4
            };

            var rule = EmptyCatchBlockRule.Create();

            // Act
            var isViolation = rule.ViolationPredicate(element);

            // Assert
            isViolation.Should().BeFalse();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Tests that the rule's message contains fix suggestions for an empty catch block.
    /// </summary>
    [Fact]
    public async Task EmptyCatchBlockRule_MessageContainsFixSuggestions()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var content = @"
try
{
}
catch (Exception)
{
}
";
            File.WriteAllText(tempFile, content);

            var element = new CodeElement("CatchBlock", CodeElementType.CatchBlock, tempFile)
            {
                StartLineNumber = 4
            };

            var rule = EmptyCatchBlockRule.Create();
            var message = rule.MessageFactory(element);

            // Act & Assert
            message.Should().Contain("Suggestions:");
            message.Should().Contain("Remove the catch block");
            message.Should().Contain("Add 'throw;'");
            message.Should().Contain("Add logging");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
