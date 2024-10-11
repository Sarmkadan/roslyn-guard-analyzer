// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

using RoslynGuardAnalyzer.Core;

namespace RoslynGuardAnalyzer.CodeFixes;

/// <summary>
/// Represents an auto-fix action that can be applied to a source file to resolve
/// a detected architectural rule violation.
/// </summary>
public sealed class CodeFix
{
    /// <summary>Gets or sets the unique identifier of this code fix.</summary>
    public string Id { get; set; }

    /// <summary>Gets or sets the identifier of the rule violation this fix addresses.</summary>
    public string ViolationId { get; set; }

    /// <summary>Gets or sets the rule identifier that produced this fix.</summary>
    public string RuleId { get; set; }

    /// <summary>Gets or sets the human-readable title of the fix action.</summary>
    public string Title { get; set; }

    /// <summary>Gets or sets the detailed description of what this fix will change.</summary>
    public string Description { get; set; }

    /// <summary>Gets or sets the absolute path to the file where the fix should be applied.</summary>
    public string FilePath { get; set; }

    /// <summary>Gets or sets the 1-based line number where the fix begins.</summary>
    public int StartLine { get; set; }

    /// <summary>Gets or sets the 1-based line number where the fix ends.</summary>
    public int EndLine { get; set; }

    /// <summary>Gets or sets the exact code token that will be replaced.</summary>
    public string OriginalCode { get; set; }

    /// <summary>Gets or sets the replacement code that resolves the violation.</summary>
    public string ReplacementCode { get; set; }

    /// <summary>Gets or sets the severity level inherited from the source violation.</summary>
    public SeverityLevel Severity { get; set; }

    /// <summary>Gets or sets the UTC timestamp when this fix was generated.</summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether applying this fix may alter observable behaviour
    /// for callers (for example, a rename that changes a public API surface).
    /// </summary>
    public bool IsBreakingChange { get; set; }

    /// <summary>Initialises a new <see cref="CodeFix"/> with default values.</summary>
    public CodeFix()
    {
        Id = Guid.NewGuid().ToString();
        ViolationId = string.Empty;
        RuleId = string.Empty;
        Title = string.Empty;
        Description = string.Empty;
        FilePath = string.Empty;
        OriginalCode = string.Empty;
        ReplacementCode = string.Empty;
        Severity = SeverityLevel.Warning;
        GeneratedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns a formatted one-line summary of this fix for display or logging purposes.
    /// </summary>
    /// <returns>A string in the form <c>[RuleId] Title — FilePath:StartLine</c>.</returns>
    public string GetSummary()
    {
        return $"[{RuleId}] {Title} — {FilePath}:{StartLine}";
    }

    /// <summary>
    /// Determines whether this fix has the minimum data required to be applied.
    /// </summary>
    /// <returns><see langword="true"/> if all required fields are populated; otherwise <see langword="false"/>.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(RuleId)
            && !string.IsNullOrWhiteSpace(FilePath)
            && !string.IsNullOrWhiteSpace(OriginalCode)
            && StartLine > 0;
    }
}

/// <summary>
/// Encapsulates the outcome of applying one or more <see cref="CodeFix"/> actions
/// to their target source files.
/// </summary>
public sealed class CodeFixResult
{
    /// <summary>Gets or sets a value indicating whether all requested fixes were applied successfully.</summary>
    public bool IsSuccess { get; set; }

    /// <summary>Gets or sets the collection of fixes that were successfully applied.</summary>
    public List<CodeFix> AppliedFixes { get; set; }

    /// <summary>Gets or sets the collection of fixes that could not be applied.</summary>
    public List<CodeFix> FailedFixes { get; set; }

    /// <summary>Gets or sets human-readable messages produced during the fix operation.</summary>
    public List<string> Messages { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the fix operation completed.</summary>
    public DateTime AppliedAt { get; set; }

    /// <summary>Gets the total number of fix operations that were attempted.</summary>
    public int TotalAttempted => AppliedFixes.Count + FailedFixes.Count;

    /// <summary>Initialises a new <see cref="CodeFixResult"/> with empty collections.</summary>
    public CodeFixResult()
    {
        AppliedFixes = new List<CodeFix>();
        FailedFixes = new List<CodeFix>();
        Messages = new List<string>();
        AppliedAt = DateTime.UtcNow;
    }
}
