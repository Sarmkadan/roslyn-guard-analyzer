#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Represents a violation stored in a baseline file for comparison.
/// Used to track violations that should be ignored in future runs.
/// </summary>
public sealed class BaselineViolation : IEquatable<BaselineViolation>
{
    /// <summary>
    /// Unique identifier for the violation (matches RuleViolation.Id)
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// Rule ID that generated this violation
    /// </summary>
    [JsonPropertyName("ruleId")]
    public string RuleId { get; set; }

    /// <summary>
    /// File path where the violation occurred
    /// </summary>
    [JsonPropertyName("filePath")]
    public string FilePath { get; set; }

    /// <summary>
    /// Line number where the violation occurred
    /// </summary>
    [JsonPropertyName("lineNumber")]
    public int LineNumber { get; set; }

    /// <summary>
    /// Normalized content hash for the violation (for matching identical violations)
    /// </summary>
    [JsonPropertyName("contentHash")]
    public string ContentHash { get; set; }

    /// <summary>
    /// When the baseline entry was created
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Optional description of why this violation is in baseline
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    public BaselineViolation()
    {
        Id = Guid.NewGuid().ToString();
        RuleId = string.Empty;
        FilePath = string.Empty;
        ContentHash = string.Empty;
        CreatedAt = DateTime.UtcNow;
    }

    public BaselineViolation(string ruleId, string filePath, int lineNumber, string contentHash, string? description = null)
    {
        Id = Guid.NewGuid().ToString();
        RuleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        LineNumber = lineNumber;
        ContentHash = contentHash ?? throw new ArgumentNullException(nameof(contentHash));
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a baseline violation from a RuleViolation
    /// </summary>
    public static BaselineViolation FromRuleViolation(RuleViolation violation, string contentHash)
    {
        if (violation is null)
            throw new ArgumentNullException(nameof(violation));

        if (string.IsNullOrWhiteSpace(contentHash))
            throw new ArgumentException("Content hash cannot be null or empty", nameof(contentHash));

        return new BaselineViolation(
            ruleId: violation.RuleId,
            filePath: violation.FilePath,
            lineNumber: violation.LineNumber,
            contentHash: contentHash,
            description: violation.Message
        );
    }

    /// <summary>
    /// Computes a normalized content hash from violation details for matching.
    /// This ensures we match violations based on their actual code content, not just location.
    /// </summary>
    public static string ComputeContentHash(RuleViolation violation)
    {
        if (violation is null)
            throw new ArgumentNullException(nameof(violation));

        // Use a normalized string that represents the violation's essence
        // This includes rule, file, line, and message to uniquely identify the violation
        var normalizedContent = $"{violation.RuleId}|{violation.FilePath}|{violation.LineNumber}|{NormalizeMessage(violation.Message)}";

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(normalizedContent);
        var hashBytes = sha256.ComputeHash(bytes);

        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Normalizes a message for consistent hashing (removes variable parts like timestamps, paths)
    /// </summary>
    private static string NormalizeMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return string.Empty;

        // Remove common variable patterns that don't affect the violation's essence
        var normalized = message
            .Replace("\r\n", " ")
            .Replace("\n", " ")
            .Replace("\r", " ")
            .Trim();

        // Remove file paths that might vary between runs
        normalized = System.Text.RegularExpressions.Regex.Replace(
            normalized,
            @"[a-zA-Z]:\\?[\w\\/.-]*|/[\w./-]*",
            "<path>",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        // Remove timestamps
        normalized = System.Text.RegularExpressions.Regex.Replace(
            normalized,
            @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}",
            "<timestamp>"
        );

        // Remove numbers that might be line numbers or IDs
        normalized = System.Text.RegularExpressions.Regex.Replace(
            normalized,
            @"\d+",
            "<num>"
        );

        return normalized.Trim();
    }

    /// <summary>
    /// Checks if this baseline violation matches a new violation.
    /// Matching is based on: RuleId + FilePath + LineNumber + ContentHash
    /// </summary>
    public bool Matches(RuleViolation violation)
    {
        if (violation is null)
            return false;

        // Must match rule, file, and line
        if (violation.RuleId != RuleId ||
            !string.Equals(violation.FilePath, FilePath, StringComparison.OrdinalIgnoreCase) ||
            violation.LineNumber != LineNumber)
        {
            return false;
        }

        // If content hash is set, use it for matching
        if (!string.IsNullOrWhiteSpace(ContentHash))
        {
            var currentHash = ComputeContentHash(violation);
            return ContentHash == currentHash;
        }

        // Fallback: match without content hash (less precise)
        return true;
    }

    /// <summary>
    /// Determines if this baseline violation is still valid (not expired).
    /// </summary>
    public bool IsValid(TimeSpan maxAge)
    {
        var age = DateTime.UtcNow - CreatedAt;
        return age <= maxAge;
    }

    public bool Equals(BaselineViolation? other)
    {
        if (other is null) return false;
        return Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as BaselineViolation);

    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString() =>
        $"BaselineViolation {{ RuleId={RuleId}, File={FilePath}, Line={LineNumber}, Hash={ContentHash[..8]}... }}";
}

/// <summary>
/// Collection of baseline violations for a project
/// </summary>
public sealed class Baseline
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; } = string.Empty;

    [JsonPropertyName("baselineCreatedAt")]
    public DateTime BaselineCreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("violations")]
    public List<BaselineViolation> Violations { get; set; } = [];

    [JsonIgnore]
    public int ViolationCount => Violations.Count;

    public Baseline() { }

    public Baseline(string projectName)
    {
        ProjectName = projectName ?? throw new ArgumentNullException(nameof(projectName));
    }

    /// <summary>
    /// Adds a violation to the baseline
    /// </summary>
    public void AddViolation(BaselineViolation violation)
    {
        if (violation is null)
            throw new ArgumentNullException(nameof(violation));

        Violations.Add(violation);
    }

    /// <summary>
    /// Checks if a violation exists in the baseline
    /// </summary>
    public bool Contains(RuleViolation violation)
    {
        if (violation is null)
            return false;

        return Violations.Any(v => v.Matches(violation));
    }

    /// <summary>
    /// Gets violations that are still valid (not expired)
    /// </summary>
    public List<BaselineViolation> GetValidViolations(TimeSpan maxAge)
    {
        return Violations
            .Where(v => v.IsValid(maxAge))
            .ToList();
    }

    /// <summary>
    /// Removes expired violations from the baseline
    /// </summary>
    public void RemoveExpired(TimeSpan maxAge)
    {
        var valid = GetValidViolations(maxAge);
        Violations = valid;
    }

    /// <summary>
    /// Serializes baseline to JSON
    /// </summary>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Deserializes baseline from JSON
    /// </summary>
    public static Baseline? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        return JsonSerializer.Deserialize<Baseline>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
