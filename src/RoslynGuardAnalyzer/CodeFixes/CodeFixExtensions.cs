#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using RoslynGuardAnalyzer.Core;

namespace RoslynGuardAnalyzer.CodeFixes;

/// <summary>
/// Provides extension methods for working with <see cref="CodeFix"/> instances.
/// </summary>
public static class CodeFixExtensions
{
    /// <summary>
    /// Determines whether this fix is more severe than another fix based on severity level.
    /// </summary>
    /// <param name="fix">The code fix to compare.</param>
    /// <param name="other">The other code fix to compare against.</param>
    /// <returns>
    /// <see langword="true"/> if this fix has higher severity than the other; otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="fix"/> or <paramref name="other"/> is <see langword="null"/>.</exception>
    public static bool IsMoreSevereThan(this CodeFix fix, CodeFix other)
    {
        ArgumentNullException.ThrowIfNull(fix);
        ArgumentNullException.ThrowIfNull(other);

        return fix.Severity > other.Severity;
    }

    /// <summary>
    /// Determines whether this fix is less severe than another fix based on severity level.
    /// </summary>
    /// <param name="fix">The code fix to compare.</param>
    /// <param name="other">The other code fix to compare against.</param>
    /// <returns>
    /// <see langword="true"/> if this fix has lower severity than the other; otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="fix"/> or <paramref name="other"/> is <see langword="null"/>.</exception>
    public static bool IsLessSevereThan(this CodeFix fix, CodeFix other)
    {
        ArgumentNullException.ThrowIfNull(fix);
        ArgumentNullException.ThrowIfNull(other);

        return fix.Severity < other.Severity;
    }

    /// <summary>
    /// Gets the severity level as a human-readable string.
    /// </summary>
    /// <param name="fix">The code fix.</param>
    /// <returns>A string representation of the severity level.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fix"/> is <see langword="null"/>.</exception>
    public static string GetSeverityString(this CodeFix fix)
    {
        ArgumentNullException.ThrowIfNull(fix);

        return fix.Severity switch
        {
            SeverityLevel.Info => "Info",
            SeverityLevel.Warning => "Warning",
            SeverityLevel.Error => "Error",
            SeverityLevel.Critical => "Critical",
            _ => throw new InvalidOperationException($"Unknown severity level: {fix.Severity}")
        };
    }

    /// <summary>
    /// Determines whether this fix is a breaking change.
    /// </summary>
    /// <param name="fix">The code fix to check.</param>
    /// <returns>
    /// <see langword="true"/> if this fix is marked as a breaking change; otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="fix"/> is <see langword="null"/>.</exception>
    public static bool IsBreaking(this CodeFix fix)
    {
        ArgumentNullException.ThrowIfNull(fix);

        return fix.IsBreakingChange;
    }

    /// <summary>
    /// Gets a display-friendly summary of the fix including severity and breaking change status.
    /// </summary>
    /// <param name="fix">The code fix.</param>
    /// <returns>A formatted string with severity and breaking change indicator.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fix"/> is <see langword="null"/>.</exception>
    public static string GetDisplaySummary(this CodeFix fix)
    {
        ArgumentNullException.ThrowIfNull(fix);

        var breakingIndicator = fix.IsBreakingChange ? "🔴 BREAKING" : string.Empty;
        var severity = fix.GetSeverityString();

        var builder = new System.Text.StringBuilder();
        builder.Append($"[{fix.RuleId}] {fix.Title} — {severity} {breakingIndicator}");

        if (!string.IsNullOrWhiteSpace(fix.FilePath) || fix.StartLine > 0)
        {
            builder.Append($"{Environment.NewLine}{fix.FilePath}:{fix.StartLine}");
        }

        if (!string.IsNullOrWhiteSpace(fix.Description))
        {
            builder.Append($"{Environment.NewLine}{fix.Description}");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Gets the age of this fix in a human-readable format (e.g., "2h ago", "3d ago").
    /// </summary>
    /// <param name="fix">The code fix.</param>
    /// <returns>A string representing how long ago the fix was generated.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fix"/> is <see langword="null"/>.</exception>
    public static string GetAge(this CodeFix fix)
    {
        ArgumentNullException.ThrowIfNull(fix);

        var now = DateTime.UtcNow;
        var age = now - fix.GeneratedAt;

        return age.TotalSeconds switch
        {
            < 60 => $"{age.TotalSeconds:F0}s ago",
            < 3600 => $"{age.TotalMinutes:F0}m ago",
            < 86400 => $"{age.TotalHours:F0}h ago",
            < 2592000 => $"{age.TotalDays:F0}d ago",
            _ => $"{age.TotalDays / 30:F1}mo ago"
        };
    }

    /// <summary>
    /// Determines whether this fix targets the specified file path.
    /// </summary>
    /// <param name="fix">The code fix.</param>
    /// <param name="filePath">The file path to check against.</param>
    /// <returns>
    /// <see langword="true"/> if this fix targets the specified file; otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="fix"/> or <paramref name="filePath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="filePath"/> is empty or whitespace.</exception>
    public static bool TargetsFile(this CodeFix fix, string filePath)
    {
        ArgumentNullException.ThrowIfNull(fix);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        return string.Equals(fix.FilePath, filePath, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the file extension of the target file for this fix.
    /// </summary>
    /// <param name="fix">The code fix.</param>
    /// <returns>The file extension including the dot (e.g., ".cs", ".cshtml"), or an empty string if not available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fix"/> is <see langword="null"/>.</exception>
    public static string GetFileExtension(this CodeFix fix)
    {
        ArgumentNullException.ThrowIfNull(fix);

        if (string.IsNullOrWhiteSpace(fix.FilePath))
        {
            return string.Empty;
        }

        try
        {
            return Path.GetExtension(fix.FilePath);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Determines whether this fix is within the specified line range.
    /// </summary>
    /// <param name="fix">The code fix.</param>
    /// <param name="startLine">The starting line number (1-based).</param>
    /// <param name="endLine">The ending line number (1-based).</param>
    /// <returns>
    /// <see langword="true"/> if this fix overlaps with the specified line range; otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="fix"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="startLine"/> or <paramref name="endLine"/> is less than 1.</exception>
    public static bool IsInLineRange(this CodeFix fix, int startLine, int endLine)
    {
        ArgumentNullException.ThrowIfNull(fix);
        ArgumentOutOfRangeException.ThrowIfLessThan(startLine, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(endLine, 1);

        // Normalize the range (startLine should be <= endLine)
        var rangeStart = Math.Min(startLine, endLine);
        var rangeEnd = Math.Max(startLine, endLine);

        // Check if there's any overlap
        return fix.StartLine <= rangeEnd && fix.EndLine >= rangeStart;
    }

    /// <summary>
    /// Gets the replacement code with context around the original code for display purposes.
    /// </summary>
    /// <param name="fix">The code fix.</param>
    /// <param name="contextLines">The number of lines of context to include before and after.</param>
    /// <returns>A formatted string showing the original code with context and replacement.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fix"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="contextLines"/> is negative.</exception>
    public static string GetCodeContext(this CodeFix fix, int contextLines = 2)
    {
        ArgumentNullException.ThrowIfNull(fix);
        ArgumentOutOfRangeException.ThrowIfNegative(contextLines);

        if (string.IsNullOrWhiteSpace(fix.OriginalCode) && string.IsNullOrWhiteSpace(fix.ReplacementCode))
        {
            return "No code context available.";
        }

        var lines = new List<string>();

        if (!string.IsNullOrWhiteSpace(fix.OriginalCode))
        {
            lines.Add($"Original ({fix.StartLine}):");
            lines.Add(fix.OriginalCode.Trim());
        }

        if (!string.IsNullOrWhiteSpace(fix.ReplacementCode))
        {
            if (lines.Count > 0)
            {
                lines.Add(string.Empty);
            }
            lines.Add($"Replacement ({fix.StartLine}):");
            lines.Add(fix.ReplacementCode.Trim());
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Determines whether this fix should be prioritized based on severity and breaking change status.
    /// </summary>
    /// <param name="fix">The code fix.</param>
    /// <returns>
    /// <see langword="true"/> if this fix should be prioritized (Critical/Error severity or breaking change); otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="fix"/> is <see langword="null"/>.</exception>
    public static bool ShouldPrioritize(this CodeFix fix)
    {
        ArgumentNullException.ThrowIfNull(fix);

        return fix.Severity >= SeverityLevel.Error || fix.IsBreakingChange;
    }

    /// <summary>
    /// Gets a priority score for this fix (higher values indicate higher priority).
    /// </summary>
    /// <param name="fix">The code fix.</param>
    /// <returns>A numeric priority score where higher values indicate higher priority.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fix"/> is <see langword="null"/>.</exception>
    public static int GetPriorityScore(this CodeFix fix)
    {
        ArgumentNullException.ThrowIfNull(fix);

        var score = (int)fix.Severity * 100;
        if (fix.IsBreakingChange)
        {
            score += 500;
        }

        return score;
    }

    /// <summary>
    /// Determines whether this fix is valid and can be applied.
    /// </summary>
    /// <param name="fix">The code fix.</param>
    /// <returns>
    /// <see langword="true"/> if this fix is valid and can be applied; otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="fix"/> is <see langword="null"/>.</exception>
    public static bool CanBeApplied(this CodeFix fix)
    {
        ArgumentNullException.ThrowIfNull(fix);

        return fix.IsValid();
    }

    /// <summary>
    /// Gets the file name without path from the file path.
    /// </summary>
    /// <param name="fix">The code fix.</param>
    /// <returns>The file name with extension, or an empty string if not available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fix"/> is <see langword="null"/>.</exception>
    public static string GetFileName(this CodeFix fix)
    {
        ArgumentNullException.ThrowIfNull(fix);

        if (string.IsNullOrWhiteSpace(fix.FilePath))
        {
            return string.Empty;
        }

        try
        {
            return Path.GetFileName(fix.FilePath);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets the directory name from the file path.
    /// </summary>
    /// <param name="fix">The code fix.</param>
    /// <returns>The directory name, or an empty string if not available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fix"/> is <see langword="null"/>.</exception>
    public static string GetDirectoryName(this CodeFix fix)
    {
        ArgumentNullException.ThrowIfNull(fix);

        if (string.IsNullOrWhiteSpace(fix.FilePath))
        {
            return string.Empty;
        }

        try
        {
            return Path.GetDirectoryName(fix.FilePath) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}