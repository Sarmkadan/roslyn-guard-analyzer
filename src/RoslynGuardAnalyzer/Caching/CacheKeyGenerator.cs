// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Security.Cryptography;
using System.Text;

namespace RoslynGuardAnalyzer.Caching;

/// <summary>
/// Generates cache keys for analysis results based on project/file characteristics.
/// Uses content hashing to detect changes and invalidate stale cache entries.
/// </summary>
public static class CacheKeyGenerator
{
    private const string AnalysisPrefix = "analysis";
    private const string ResultPrefix = "result";
    private const string FilePrefix = "file";
    private const string ProjectPrefix = "project";

    /// <summary>
    /// Generates a cache key for a project analysis.
    /// </summary>
    public static string GenerateProjectAnalysisKey(string projectPath, string configHash = "")
    {
        var pathHash = ComputeHash(projectPath);
        var config = string.IsNullOrEmpty(configHash) ? "" : $"_{configHash}";
        return $"{AnalysisPrefix}_{ProjectPrefix}_{pathHash}{config}";
    }

    /// <summary>
    /// Generates a cache key for a file analysis.
    /// </summary>
    public static string GenerateFileAnalysisKey(string filePath, string? fileContentHash = null)
    {
        var pathHash = ComputeHash(filePath);
        var content = fileContentHash ?? "";
        return $"{AnalysisPrefix}_{FilePrefix}_{pathHash}_{content}";
    }

    /// <summary>
    /// Generates a cache key for analysis result.
    /// </summary>
    public static string GenerateResultKey(string analysisId)
    {
        return $"{ResultPrefix}_{analysisId}";
    }

    /// <summary>
    /// Generates a cache key for rule execution cache.
    /// </summary>
    public static string GenerateRuleExecutionKey(string ruleName, string targetName)
    {
        var ruleHash = ComputeHash(ruleName);
        var targetHash = ComputeHash(targetName);
        return $"rule_exec_{ruleHash}_{targetHash}";
    }

    /// <summary>
    /// Generates a cache key for code element analysis.
    /// </summary>
    public static string GenerateCodeElementKey(string fullTypeName, string memberName = "")
    {
        var typeHash = ComputeHash(fullTypeName);
        var memberPart = string.IsNullOrEmpty(memberName) ? "" : $"_{ComputeHash(memberName)}";
        return $"element_{typeHash}{memberPart}";
    }

    /// <summary>
    /// Computes a SHA256 hash of a string (for consistency checking).
    /// </summary>
    public static string ComputeHash(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "empty";

        using (var sha = SHA256.Create())
        {
            var hashedBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashedBytes, 0, 8).Replace("-", "").ToLowerInvariant();
        }
    }

    /// <summary>
    /// Computes a hash from file contents.
    /// </summary>
    public static string ComputeFileHash(string filePath)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
                return "not_found";

            using (var sha = SHA256.Create())
            using (var stream = System.IO.File.OpenRead(filePath))
            {
                var hashedBytes = sha.ComputeHash(stream);
                return BitConverter.ToString(hashedBytes, 0, 8).Replace("-", "").ToLowerInvariant();
            }
        }
        catch
        {
            return "error";
        }
    }

    /// <summary>
    /// Creates a composite cache key from multiple components.
    /// </summary>
    public static string CreateCompositeKey(params string[] components)
    {
        if (components == null || components.Length == 0)
            throw new ArgumentException("At least one component required");

        var combined = string.Join("|", components);
        return ComputeHash(combined);
    }

    /// <summary>
    /// Generates a cache key pattern for prefix-based cache invalidation.
    /// </summary>
    public static string GeneratePatternKey(string prefix)
    {
        return $"{prefix}_*";
    }
}
