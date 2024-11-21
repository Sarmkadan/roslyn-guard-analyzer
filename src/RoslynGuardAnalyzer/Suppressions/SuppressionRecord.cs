#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Suppressions;

/// <summary>
/// Represents a persisted rule suppression entry.
/// </summary>
public sealed class SuppressionRecord
{
    /// <summary>
    /// Gets or sets the unique suppression identifier.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the rule identifier covered by this suppression.
    /// </summary>
    public string RuleId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional target file path.
    /// </summary>
    public string? TargetFile { get; set; }

    /// <summary>
    /// Gets or sets the optional target element name.
    /// </summary>
    public string? TargetElement { get; set; }

    /// <summary>
    /// Gets or sets the justification for the suppression.
    /// </summary>
    public string Justification { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the optional expiration timestamp.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the author who created the suppression.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the suppression is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Determines whether this suppression matches the supplied violation.
    /// </summary>
    public bool Matches(RuleViolation violation)
    {
        if (violation is null)
            throw new ArgumentNullException(nameof(violation));

        if (!IsActive || !string.Equals(RuleId, violation.RuleId, StringComparison.OrdinalIgnoreCase))
            return false;

        if (ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow)
            return false;

        if (!string.IsNullOrWhiteSpace(TargetFile) &&
            !string.Equals(TargetFile, violation.FilePath, StringComparison.OrdinalIgnoreCase))
            return false;

        if (string.IsNullOrWhiteSpace(TargetElement))
            return true;

        var elementName = violation.GetMetadata("ElementName") ?? violation.GetMetadata("TargetElement");
        return string.Equals(TargetElement, elementName, StringComparison.OrdinalIgnoreCase);
    }
}
