#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.IO;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Rules;

/// <summary>
/// Rule that flags catch blocks with no statements and no throw statements.
/// Empty catch blocks swallow exceptions silently, making debugging difficult.
/// This rule specifically targets catch blocks that have no statements at all
/// and no throw statement to rethrow the exception.
/// </summary>
public static class EmptyCatchBlockRule
{
    /// <summary>
    /// Creates and returns the EmptyCatchBlockRule instance.
    /// </summary>
    public static CustomAnalysisRule Create()
    {
        return CustomRuleBuilder.Create("ECB001", "Empty Catch Blocks Must Be Removed Or Handle Exception")
            .For(RuleCategory.CodeStructure)
            .WithSeverity(SeverityLevel.Error)
            .WithDescription("Detects catch blocks with no statements and no throw statement. Empty catch blocks swallow exceptions silently and should be removed or properly handle the exception.")
            .When(IsEmptyCatchBlockWithNoThrow)
            .WithMessage(CreateViolationMessage)
            .Build();
    }

    private static bool IsEmptyCatchBlockWithNoThrow(CodeElement element)
    {
        // Only check catch blocks
        if (element.ElementType != CodeElementType.CatchBlock)
            return false;

        // Check if we can read the source file
        if (string.IsNullOrWhiteSpace(element.FilePath) || !File.Exists(element.FilePath))
            return false;

        try
        {
            var fileContent = File.ReadAllText(element.FilePath);
            var lines = fileContent.Split('\n');

            // Look for opening brace of catch block starting from StartLineNumber
            // The StartLineNumber might point to the catch keyword line or the opening brace line
            int openingBraceLine = -1;
            for (int i = element.StartLineNumber; i < Math.Min(element.StartLineNumber + 20, lines.Length); i++)
            {
                var line = lines[i - 1].Trim();

                // Look for opening brace of catch block
                if (line.StartsWith("{", StringComparison.Ordinal) || line.EndsWith("{", StringComparison.Ordinal))
                {
                    openingBraceLine = i;
                    break;
                }
            }

            // If we couldn't find the opening brace, can't analyze
            if (openingBraceLine == -1)
                return false;

            // Scan forward to find matching closing brace and check for content/throw
            // Start scanning from the line AFTER the opening brace to avoid counting the brace itself as content
            bool hasContent = false;
            bool hasThrow = false;

            // Scan forward to find matching braces and check for content
            for (int j = openingBraceLine + 1; j < Math.Min(openingBraceLine + 50, lines.Length); j++)
            {
                var scanLine = lines[j - 1];

                // Check if line has actual code (not just whitespace or comments)
                var trimmed = scanLine.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed) &&
                    !trimmed.StartsWith("//", StringComparison.Ordinal) &&
                    !trimmed.StartsWith("/*", StringComparison.Ordinal) &&
                    !trimmed.All(c => c == '*'))
                {
                    // Check for throw statement
                    if (trimmed.StartsWith("throw", StringComparison.Ordinal))
                    {
                        hasThrow = true;
                        break;
                    }

                    hasContent = true;
                    break;
                }
            }

            // If we found the catch block body and it has no content and no throw
            return !hasContent && !hasThrow;
        }
        catch
        {
            // If we can't read the file, don't flag it
            return false;
        }
    }

    private static string CreateViolationMessage(CodeElement element)
    {
        var blockName = element.Name;
        var fileLocation = element.GetLocation();

        return $"Empty catch block '{blockName}' at {fileLocation} found. " +
               "Either remove the catch block or add proper exception handling. " +
               "Suggestions:\n" +
               " - Remove the catch block if the exception should not be caught\n" +
               " - Add 'throw;' to rethrow the exception\n" +
               " - Add logging with 'System.Diagnostics.Debug.WriteLine(ex.Message);'\n" +
               " - Add 'throw new Exception(\"...\", ex);' to wrap and rethrow";
    }
}