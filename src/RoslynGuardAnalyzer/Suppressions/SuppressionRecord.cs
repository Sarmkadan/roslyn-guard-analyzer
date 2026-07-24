#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Suppressions;

/// <summary>
/// Represents a persisted rule suppression entry.
/// </summary>
public sealed class SuppressionRecord : IEquatable<SuppressionRecord>
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

        if (!string.IsNullOrWhiteSpace(TargetFile))
        {
            var normalizedTargetFile = PathNormalizer.Normalize(TargetFile);
            var normalizedViolationFile = PathNormalizer.Normalize(violation.FilePath);
            if (!PathNormalizer.AreEquivalent(normalizedTargetFile, normalizedViolationFile))
                return false;
        }

        if (string.IsNullOrWhiteSpace(TargetElement))
            return true;

        var elementName = violation.GetMetadata("ElementName") ?? violation.GetMetadata("TargetElement");
        return string.Equals(TargetElement, elementName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the specified <see cref="SuppressionRecord"/> is equal to this instance.
    /// Two suppression records are considered equal if they have the same RuleId and TargetFile (after normalization).
    /// </summary>
    /// <param name="other">The suppression record to compare with this instance.</param>
    /// <returns>true if the specified object is equal to this instance; otherwise, false.</returns>
    public bool Equals(SuppressionRecord? other)
    {
        if (other is null)
            return false;

        // Two suppression records are considered equal if they suppress the same rule in the same file
        // This matches the logic used in Matches() method for consistency
        return string.Equals(RuleId, other.RuleId, StringComparison.OrdinalIgnoreCase) &&
               PathNormalizer.AreEquivalent(TargetFile ?? string.Empty, other.TargetFile ?? string.Empty);
    }

    /// <summary>
    /// Determines whether the specified object is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>true if the specified object is equal to this instance; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as SuppressionRecord);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance.</returns>
    public override int GetHashCode()
    {
        // Use the same fields that are compared in Equals for consistency
        // with hash-based collections
        unchecked
        {
            var hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(RuleId);
            hashCode = (hashCode * 397) ^ PathNormalizer.GetHashCode(TargetFile ?? string.Empty);
            return hashCode;
        }
    }

    /// <summary>
    /// Determines whether two suppression records are equal.
    /// </summary>
    /// <param name="left">The first suppression record to compare.</param>
    /// <param name="right">The second suppression record to compare.</param>
    /// <returns>true if the two suppression records are equal; otherwise, false.</returns>
    public static bool operator ==(SuppressionRecord? left, SuppressionRecord? right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two suppression records are not equal.
    /// </summary>
    /// <param name="left">The first suppression record to compare.</param>
    /// <param name="right">The second suppression record to compare.</param>
    /// <returns>true if the two suppression records are not equal; otherwise, false.</returns>
    public static bool operator !=(SuppressionRecord? left, SuppressionRecord? right) => !(left == right);
}