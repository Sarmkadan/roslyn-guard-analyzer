#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Suppressions;

/// <summary>
/// Defines suppression management and persistence operations.
/// </summary>
public interface ISuppressionManager
{
    /// <summary>
    /// Adds a suppression record.
    /// </summary>
    /// <param name="record">The suppression record to add.</param>
    void AddSuppression(SuppressionRecord record);

    /// <summary>
    /// Removes a suppression by identifier.
    /// </summary>
    /// <param name="suppressionId">The identifier of the suppression to remove.</param>
    /// <returns><c>true</c> if a suppression with the given identifier was removed; otherwise, <c>false</c>.</returns>
    bool RemoveSuppression(string suppressionId);

    /// <summary>
    /// Returns suppressions, optionally filtered by rule identifier.
    /// </summary>
    /// <param name="ruleId">The rule identifier to filter by, or <c>null</c> to return all suppressions.</param>
    /// <returns>A read-only list of matching suppression records.</returns>
    IReadOnlyList<SuppressionRecord> GetSuppressions(string? ruleId = null);

    /// <summary>
    /// Determines whether a violation is currently suppressed.
    /// </summary>
    /// <param name="violation">The rule violation to check.</param>
    /// <returns><c>true</c> if the violation is currently suppressed; otherwise, <c>false</c>.</returns>
    bool IsSuppressed(RuleViolation violation);

    /// <summary>
    /// Returns violations that are not currently suppressed.
    /// </summary>
    /// <param name="violations">The rule violations to filter.</param>
    /// <returns>A read-only list containing only the violations that are not currently suppressed.</returns>
    IReadOnlyList<RuleViolation> FilterSuppressed(IEnumerable<RuleViolation> violations);

    /// <summary>
    /// Saves suppressions to a JSON file.
    /// </summary>
    /// <param name="filePath">The file path to save suppressions to.</param>
    /// <param name="cancellationToken">A cancellation token to observe while saving.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    /// <remarks>
    /// If the file cannot be written (e.g., due to permissions, disk errors, or invalid paths),
    /// the exception is swallowed and logged by the implementation. Callers should not need to handle
    /// exceptions from this method.
    /// </remarks>
    Task SaveAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads suppressions from a JSON file.
    /// </summary>
    /// <param name="filePath">The file path to load suppressions from.</param>
    /// <param name="cancellationToken">A cancellation token to observe while loading.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    /// <remarks>
    /// If the file does not exist, the operation completes silently without throwing.
    /// If the file exists but cannot be loaded (e.g., due to corruption, permissions, or format errors),
    /// the exception is swallowed and logged by the implementation. Callers should not need to handle
    /// exceptions from this method.
    /// </remarks>
    Task LoadAsync(string filePath, CancellationToken cancellationToken = default);
}
