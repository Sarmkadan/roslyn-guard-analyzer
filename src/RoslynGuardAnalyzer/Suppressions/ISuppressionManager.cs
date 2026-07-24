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
    void AddSuppression(SuppressionRecord record);

    /// <summary>
    /// Removes a suppression by identifier.
    /// </summary>
    bool RemoveSuppression(string suppressionId);

    /// <summary>
    /// Returns suppressions, optionally filtered by rule identifier.
    /// </summary>
    IReadOnlyList<SuppressionRecord> GetSuppressions(string? ruleId = null);

    /// <summary>
    /// Determines whether a violation is currently suppressed.
    /// </summary>
    bool IsSuppressed(RuleViolation violation);

    /// <summary>
    /// Returns violations that are not currently suppressed.
    /// </summary>
    IReadOnlyList<RuleViolation> FilterSuppressed(IEnumerable<RuleViolation> violations);

    /// <summary>
    /// Saves suppressions to a JSON file.
    /// </summary>
    /// <param name="filePath">The file path to save suppressions to.</param>
    /// <param name="cancellationToken">A cancellation token to observe while saving.</param>
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
    /// <remarks>
    /// If the file does not exist, the operation completes silently without throwing.
    /// If the file exists but cannot be loaded (e.g., due to corruption, permissions, or format errors),
    /// the exception is swallowed and logged by the implementation. Callers should not need to handle
    /// exceptions from this method.
    /// </remarks>
    Task LoadAsync(string filePath, CancellationToken cancellationToken = default);
}
