#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Suppressions;

/// <summary>
/// Provides extension methods for <see cref="SuppressionManager"/> to simplify common suppression operations.
/// </summary>
public static class SuppressionManagerExtensions
{
    /// <summary>
    /// Adds a suppression for a specific rule violation.
    /// </summary>
    /// <param name="manager">The suppression manager.</param>
    /// <param name="violation">The violation to suppress.</param>
    /// <param name="justification">Optional justification for the suppression.</param>
    /// <param name="author">Optional author of the suppression (defaults to current user).</param>
    /// <param name="expiresAt">Optional expiration date for the suppression.</param>
    /// <returns>The created suppression record.</returns>
    public static SuppressionRecord AddSuppression(
        this SuppressionManager manager,
        RuleViolation violation,
        string? justification = null,
        string? author = null,
        DateTime? expiresAt = null)
    {
        if (manager is null)
            throw new ArgumentNullException(nameof(manager));

        if (violation is null)
            throw new ArgumentNullException(nameof(violation));

        var record = new SuppressionRecord
        {
            RuleId = violation.RuleId,
            TargetFile = violation.FilePath,
            Justification = justification ?? string.Empty,
            Author = author ?? Environment.UserName,
            ExpiresAt = expiresAt
        };

        // Copy element name from violation metadata if available
        var elementName = violation.GetMetadata("ElementName") ?? violation.GetMetadata("TargetElement");
        if (!string.IsNullOrWhiteSpace(elementName))
        {
            record.TargetElement = elementName;
        }

        manager.AddSuppression(record);
        return record;
    }

    /// <summary>
    /// Removes all suppressions that match the specified rule ID.
    /// </summary>
    /// <param name="manager">The suppression manager.</param>
    /// <param name="ruleId">The rule ID to match.</param>
    /// <returns>The number of suppressions removed.</returns>
    public static int RemoveSuppressionsByRuleId(
        this SuppressionManager manager,
        string ruleId)
    {
        if (manager is null)
            throw new ArgumentNullException(nameof(manager));

        if (string.IsNullOrWhiteSpace(ruleId))
            throw new ArgumentException("Rule ID cannot be null or empty.", nameof(ruleId));

        var suppressions = manager.GetSuppressions(ruleId);
        int removedCount = 0;

        foreach (var suppression in suppressions)
        {
            if (manager.RemoveSuppression(suppression.Id))
            {
                removedCount++;
            }
        }

        return removedCount;
    }

    /// <summary>
    /// Checks if any violations in the collection are suppressed.
    /// </summary>
    /// <param name="manager">The suppression manager.</param>
    /// <param name="violations">The violations to check.</param>
    /// <returns>True if any violation is suppressed; otherwise false.</returns>
    public static bool HasAnySuppressed(
        this SuppressionManager manager,
        IEnumerable<RuleViolation> violations)
    {
        if (manager is null)
            throw new ArgumentNullException(nameof(manager));

        if (violations is null)
            throw new ArgumentNullException(nameof(violations));

        return violations.Any(violation => manager.IsSuppressed(violation));
    }

    /// <summary>
    /// Gets the count of active suppressions for a specific rule.
    /// </summary>
    /// <param name="manager">The suppression manager.</param>
    /// <param name="ruleId">Optional rule ID to filter by. If null, returns total count.</param>
    /// <returns>The count of active suppressions.</returns>
    public static int GetSuppressionCount(
        this SuppressionManager manager,
        string? ruleId = null)
    {
        if (manager is null)
            throw new ArgumentNullException(nameof(manager));

        var suppressions = manager.GetSuppressions(ruleId);

        // Filter out expired suppressions
        var activeCount = suppressions.Count(record =>
            !record.ExpiresAt.HasValue || record.ExpiresAt.Value > DateTime.UtcNow);

        return activeCount;
    }

    /// <summary>
    /// Exports all active suppressions to a new list.
    /// </summary>
    /// <param name="manager">The suppression manager.</param>
    /// <param name="ruleId">Optional rule ID to filter by.</param>
    /// <returns>A new list containing all active suppression records.</returns>
    public static IReadOnlyList<SuppressionRecord> ExportActiveSuppressions(
        this SuppressionManager manager,
        string? ruleId = null)
    {
        if (manager is null)
            throw new ArgumentNullException(nameof(manager));

        var allSuppressions = manager.GetSuppressions(ruleId);

        var activeSuppressions = allSuppressions
            .Where(record => !record.ExpiresAt.HasValue || record.ExpiresAt.Value > DateTime.UtcNow)
            .ToList()
            .AsReadOnly();

        return activeSuppressions;
    }

    /// <summary>
    /// Removes all expired suppressions from the manager.
    /// </summary>
    /// <param name="manager">The suppression manager.</param>
    /// <returns>The number of expired suppressions removed.</returns>
    public static int CleanupExpiredSuppressions(
        this SuppressionManager manager)
    {
        if (manager is null)
            throw new ArgumentNullException(nameof(manager));

        var allSuppressions = manager.GetSuppressions();
        var expiredCount = 0;

        foreach (var suppression in allSuppressions)
        {
            if (suppression.ExpiresAt.HasValue && suppression.ExpiresAt.Value <= DateTime.UtcNow)
            {
                if (manager.RemoveSuppression(suppression.Id))
                {
                    expiredCount++;
                }
            }
        }

        return expiredCount;
    }

    /// <summary>
    /// Checks if a specific rule has any active suppressions.
    /// </summary>
    /// <param name="manager">The suppression manager.</param>
    /// <param name="ruleId">The rule ID to check.</param>
    /// <returns>True if the rule has active suppressions; otherwise false.</returns>
    public static bool HasActiveSuppressionsForRule(
        this SuppressionManager manager,
        string ruleId)
    {
        if (manager is null)
            throw new ArgumentNullException(nameof(manager));

        if (string.IsNullOrWhiteSpace(ruleId))
            throw new ArgumentException("Rule ID cannot be null or empty.", nameof(ruleId));

        var suppressions = manager.GetSuppressions(ruleId);
        return suppressions.Any(record =>
            !record.ExpiresAt.HasValue || record.ExpiresAt.Value > DateTime.UtcNow);
    }
}
