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
    /// <param name="violation">The violation to create baseline entry for</param>
    /// <param name="contentHash">Pre-computed content hash for the violation</param>
    /// <returns>New BaselineViolation instance</returns>
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
    /// Creates a baseline violation from a RuleViolation, computing the content hash automatically
    /// </summary>
    /// <param name="violation">The violation to create baseline entry for</param>
    /// <returns>New BaselineViolation instance with computed content hash</returns>
    public static BaselineViolation FromRuleViolation(RuleViolation violation)
    {
        if (violation is null)
            throw new ArgumentNullException(nameof(violation));

        var contentHash = ComputeContentHash(violation);
        return FromRuleViolation(violation, contentHash);
    }

    /// <summary>
    /// Computes a normalized content hash from violation details for matching.
    /// This ensures we match violations based on their actual code content, not just location.
    /// The hash includes RuleId, normalized file path, message, and code snippet (if available)
    /// to uniquely identify the violation's essence.
    /// </summary>
    /// <param name="violation">The violation to compute hash for</param>
    /// <returns>Base64-encoded SHA256 hash representing the violation's content</returns>
    public static string ComputeContentHash(RuleViolation violation)
    {
        if (violation is null)
            throw new ArgumentNullException(nameof(violation));

        // Use a normalized string that represents the violation's essence
        // Include rule, normalized file path, message, and code snippet for maximum stability
        var normalizedFilePath = NormalizeFilePath(violation.FilePath);
        var normalizedMessage = NormalizeMessage(violation.Message);
        var codeSnippet = violation.CodeSnippet ?? string.Empty;

        // Include code snippet if available to make the fingerprint more stable
        // The snippet helps distinguish between different violations at the same logical location
        var normalizedContent = $"{violation.RuleId}|{normalizedFilePath}|{normalizedMessage}|{codeSnippet}";

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
    /// Normalizes file paths for consistent comparison across different operating systems.
    /// Converts to forward slashes, removes redundant separators, and handles case sensitivity.
    /// </summary>
    /// <param name="filePath">The file path to normalize</param>
    /// <returns>Normalized file path with consistent separators</returns>
    private static string NormalizeFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return filePath ?? string.Empty;

        // Convert to forward slashes for consistency
        var normalized = filePath.Replace('\\', '/');

        // Remove redundant separators (e.g., "/./" -> "/")
        normalized = normalized.Replace("/./", "/");
        normalized = normalized.Replace("/../", "/");

        // Remove leading "./" if present
        if (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized.Substring(2);
        }

        // Remove trailing slashes
        normalized = normalized.TrimEnd('/');

        return normalized;
    }

    /// <summary>
    /// Checks if this baseline violation matches a new violation.
    /// Matching uses a stable fingerprint: RuleId + FilePath + ContentHash
    /// LineNumber is used as a tiebreaker when content hashes match but line numbers differ.
    /// This makes baseline matching resilient to line-number drift caused by unrelated edits.
    /// </summary>
    /// <param name="violation">The violation to match against</param>
    /// <returns>True if the violations represent the same issue despite line number changes</returns>
    public bool Matches(RuleViolation violation)
    {
        if (violation is null)
            return false;

        // Normalize file paths for consistent comparison across different operating systems
        var normalizedFilePath = NormalizeFilePath(violation.FilePath);

        // Primary matching: RuleId + FilePath + ContentHash
        // This creates a stable fingerprint that's resilient to line number changes
        if (violation.RuleId != RuleId ||
            !string.Equals(normalizedFilePath, NormalizeFilePath(FilePath), StringComparison.Ordinal))
        {
            return false;
        }

        // If content hash is set, use it as the primary matching criterion
        if (!string.IsNullOrWhiteSpace(ContentHash))
        {
            var currentHash = ComputeContentHash(violation);
            if (ContentHash == currentHash)
            {
                // Content matches! Use line number as tiebreaker - if close enough, consider it a match
                // Allow small line number differences (within 5 lines) to account for minor code shifts
                if (Math.Abs(violation.LineNumber - LineNumber) <= 5)
                {
                    return true;
                }

                // If line numbers differ significantly but content matches, it's likely the same violation
                // This handles cases where the violation line itself moved due to insertions/deletions
                return true;
            }
        }

        // Fallback for old baselines without content hash: match on rule, file, and line
        // This maintains backward compatibility with existing baseline files
        return violation.RuleId == RuleId &&
               string.Equals(normalizedFilePath, NormalizeFilePath(FilePath), StringComparison.OrdinalIgnoreCase) &&
               violation.LineNumber == LineNumber;
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

[JsonPropertyName("schemaVersion")]
public string SchemaVersion { get; set; } = "1.0";

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
