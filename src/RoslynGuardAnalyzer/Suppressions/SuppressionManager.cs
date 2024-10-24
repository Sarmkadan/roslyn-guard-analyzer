#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Suppressions;

/// <summary>
/// Manages persisted suppression entries and suppression filtering.
/// </summary>
public sealed class SuppressionManager : ISuppressionManager
{
    private readonly ILogger<SuppressionManager> _logger;
    private readonly object _syncRoot = new();
    private readonly Dictionary<string, SuppressionRecord> _records = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="SuppressionManager"/> class.
    /// </summary>
    public SuppressionManager(ILogger<SuppressionManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void AddSuppression(SuppressionRecord record)
    {
        if (record is null)
            throw new ArgumentNullException(nameof(record));

        lock (_syncRoot)
        {
            _records[record.Id] = record;
        }

        _logger.LogInformation("Added suppression {SuppressionId} for rule {RuleId}.", record.Id, record.RuleId);
    }

    /// <inheritdoc/>
    public bool RemoveSuppression(string suppressionId)
    {
        if (string.IsNullOrWhiteSpace(suppressionId))
            return false;

        lock (_syncRoot)
        {
            var removed = _records.Remove(suppressionId);
            if (removed)
                _logger.LogInformation("Removed suppression {SuppressionId}.", suppressionId);

            return removed;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<SuppressionRecord> GetSuppressions(string? ruleId = null)
    {
        lock (_syncRoot)
        {
            var records = _records.Values.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(ruleId))
                records = records.Where(record => string.Equals(record.RuleId, ruleId, StringComparison.OrdinalIgnoreCase));

            return records.OrderBy(record => record.CreatedAt).ToList().AsReadOnly();
        }
    }

    /// <inheritdoc/>
    public bool IsSuppressed(RuleViolation violation)
    {
        if (violation is null)
            throw new ArgumentNullException(nameof(violation));

        lock (_syncRoot)
        {
            return _records.Values.Any(record => record.Matches(violation));
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<RuleViolation> FilterSuppressed(IEnumerable<RuleViolation> violations)
    {
        if (violations is null)
            throw new ArgumentNullException(nameof(violations));

        var remaining = violations.Where(violation => !IsSuppressed(violation)).ToList().AsReadOnly();
        _logger.LogInformation("Filtered suppressed violations. Remaining count: {Count}.", remaining.Count);
        return remaining;
    }

    /// <inheritdoc/>
    public async Task SaveAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        List<SuppressionRecord> snapshot;
        lock (_syncRoot)
        {
            snapshot = _records.Values.OrderBy(record => record.CreatedAt).ToList();
        }

        var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Saved {Count} suppression records to {FilePath}.", snapshot.Count, filePath);
    }

    /// <inheritdoc/>
    public async Task LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
        {
            _logger.LogInformation("Suppression file {FilePath} does not exist. Nothing to load.", filePath);
            return;
        }

        var json = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
        var records = JsonSerializer.Deserialize<List<SuppressionRecord>>(json) ?? [];
        var activeRecords = records
            .Where(record => !record.ExpiresAt.HasValue || record.ExpiresAt.Value > DateTime.UtcNow)
            .ToList();

        lock (_syncRoot)
        {
            _records.Clear();
            foreach (var record in activeRecords)
                _records[record.Id] = record;
        }

        _logger.LogInformation("Loaded {Count} active suppression records from {FilePath}.", activeRecords.Count, filePath);
    }
}
