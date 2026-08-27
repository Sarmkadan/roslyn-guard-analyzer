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
/// Contains unit tests for the EmptyCatchRule, which detects empty catch blocks.
/// </summary>
public sealed class EmptyCatchRuleTests
{
    /// <summary>
    /// Verifies that the EmptyCatchRule flags a catch block with no statements inside.
    /// </summary>
    [Fact]
    public async Task EmptyCatchRule_FlagsEmptyCatchBlock()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var content = @"
try
{
}
catch (Exception) {
}";
            File.WriteAllText(tempFile, content);

            var element = new CodeElement("CatchBlock", CodeElementType.CatchBlock, tempFile)
            {
                StartLineNumber = 5 // Line where catch starts
            };

            var rule = EmptyCatchRule.Create();

            // Act
            var isViolation = rule.ViolationPredicate(element);
            var message = rule.MessageFactory(element);

            // Assert
            isViolation.Should().BeTrue();
            message.Should().Contain("Empty catch block 'CatchBlock'");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Verifies that the EmptyCatchRule flags a catch block containing only comments.
    /// </summary>
    [Fact]
    public async Task EmptyCatchRule_FlagsCatchBlockWithCommentsOnly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var content = @"
try
{
}
catch (Exception) {
    // Ignored
}";
            File.WriteAllText(tempFile, content);

            var element = new CodeElement("CatchBlock", CodeElementType.CatchBlock, tempFile)
            {
                StartLineNumber = 5
            };

            var rule = EmptyCatchRule.Create();

            // Act
            var isViolation = rule.ViolationPredicate(element);

            // Assert
            isViolation.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}