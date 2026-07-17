namespace RoslynGuardAnalyzer.Suppressions;

/// <summary>
/// Extension methods for <see cref="SuppressionRecord"/>.
/// </summary>
public static class SuppressionRecordExtensions
{
    /// <summary>
    /// Determines whether a suppression record is expired.
    /// </summary>
    /// <param name="record">The suppression record to check.</param>
    /// <returns>true if the suppression record is expired; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="record"/> is null.</exception>
    public static bool IsExpired(this SuppressionRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        return record.ExpiresAt.HasValue && record.ExpiresAt.Value < DateTime.UtcNow;
    }

    /// <summary>
    /// Gets a human-readable description of the suppression record.
    /// </summary>
    /// <param name="record">The suppression record to describe.</param>
    /// <returns>A human-readable description of the suppression record.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="record"/> is null.</exception>
    public static string GetDescription(this SuppressionRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        return $"Suppression for rule '{record.RuleId}' in file '{record.TargetFile ?? "(unknown)"}' at '{record.TargetElement ?? "(unknown)"}'. Justification: '{record.Justification}'. Author: '{record.Author}'. Active: {record.IsActive}.";
    }

    /// <summary>
    /// Checks if a suppression record matches a specific rule and target file.
    /// </summary>
    /// <param name="record">The suppression record to check.</param>
    /// <param name="ruleId">The ID of the rule to match.</param>
    /// <param name="targetFile">The target file to match.</param>
    /// <returns>true if the suppression record matches; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="record"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="ruleId"/> or <paramref name="targetFile"/> is null or empty.</exception>
    public static bool MatchesRuleAndFile(this SuppressionRecord record, string ruleId, string targetFile)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentException.ThrowIfNullOrEmpty(ruleId);
        ArgumentException.ThrowIfNullOrEmpty(targetFile);
        return string.Equals(record.RuleId, ruleId, StringComparison.Ordinal) && string.Equals(record.TargetFile, targetFile, StringComparison.Ordinal);
    }
}