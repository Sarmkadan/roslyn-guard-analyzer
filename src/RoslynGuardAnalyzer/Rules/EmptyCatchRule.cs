#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.IO;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Rules;

/// <summary>
/// Rule that flags catch blocks with empty bodies or containing only a comment.
/// Empty catch blocks swallow exceptions silently, making debugging difficult.
/// </summary>
public static class EmptyCatchRule
{
    /// <summary>
    /// Creates and returns the EmptyCatchRule instance.
    /// </summary>
    public static CustomAnalysisRule Create()
    {
        return CustomRuleBuilder.Create("EC001", "Empty Catch Blocks Must Be Removed Or Handle Exception")
            .For(RuleCategory.CodeStructure)
            .WithSeverity(SeverityLevel.Error)
            .WithDescription("Detects catch blocks with empty bodies or containing only comments. Empty catch blocks swallow exceptions silently.")
            .When(IsEmptyCatchBlock)
            .WithMessage(CreateViolationMessage)
            .Build();
    }

    private static bool IsEmptyCatchBlock(CodeElement element)
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

            // Check lines starting from the catch block line
            for (int i = element.StartLineNumber; i < Math.Min(element.StartLineNumber + 20, lines.Length); i++)
            {
                var line = lines[i - 1].Trim(); // Convert to 0-based index

                // Look for opening brace of catch block
                if (line.StartsWith("{", StringComparison.Ordinal) || line.EndsWith("{", StringComparison.Ordinal))
                {
                    // Found the opening brace, now check the next few lines for content
                    int braceLine = i;
                    int openBraceCount = 0;
                    int closeBraceCount = 0;
                    bool hasContent = false;
                    bool hasOnlyComments = true;

                    // Scan forward to find matching braces
                    for (int j = braceLine; j < Math.Min(braceLine + 20, lines.Length); j++)
                    {
                        var scanLine = lines[j - 1];
                        openBraceCount += scanLine.Count(c => c == '{');
                        closeBraceCount += scanLine.Count(c => c == '}');

                        // Check if line has actual code (not just whitespace or comments)
                        var trimmed = scanLine.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmed) &&
                            !trimmed.StartsWith("//", StringComparison.Ordinal) &&
                            !trimmed.StartsWith("/*", StringComparison.Ordinal) &&
                            !trimmed.StartsWith("*", StringComparison.Ordinal))
                        {
                            hasContent = true;
                            hasOnlyComments = false;
                            break;
                        }
                    }

                    // If we found the catch block body and it has no content
                    return !hasContent && hasOnlyComments;
                }
            }
        }
        catch
        {
            // If we can't read the file, don't flag it
            return false;
        }

        return false;
    }

    private static string CreateViolationMessage(CodeElement element)
    {
        var blockName = element.Name;
        var fileLocation = element.GetLocation();

        return $"Empty catch block '{blockName}' at {fileLocation} found. " +
               "Either remove the catch block or add proper exception handling (rethrow or logging).";
    }
}
