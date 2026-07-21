#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Services;

/// <summary>
/// Service for managing baseline files that store known violations.
/// </summary>
public sealed class BaselineService : IBaselineService
{
    private readonly ILogger<BaselineService> _logger;

    public BaselineService(ILogger<BaselineService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Loads a baseline from file.
    /// </summary>
    public async Task<Baseline?> LoadBaselineAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Baseline file not found: {FilePath}", filePath);
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var baseline = Baseline.FromJson(json);

            if (baseline is null)
            {
                _logger.LogError("Failed to parse baseline file: {FilePath}", filePath);
                return null;
            }

            _logger.LogInformation(
                "Loaded baseline with {ViolationCount} violations from {FilePath}",
                baseline.ViolationCount,
                filePath
            );

            return baseline;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading baseline file: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// Saves a baseline to file.
    /// </summary>
    public async Task SaveBaselineAsync(Baseline baseline, string filePath)
    {
        if (baseline is null)
            throw new ArgumentNullException(nameof(baseline));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = baseline.ToJson();
            await File.WriteAllTextAsync(filePath, json);

            _logger.LogInformation(
                "Saved baseline with {ViolationCount} violations to {FilePath}",
                baseline.ViolationCount,
                filePath
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving baseline file: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// Filters violations to only return new violations not in baseline.
    /// </summary>
    public List<RuleViolation> FilterNewViolations(
        List<RuleViolation> violations,
        Baseline? baseline,
        TimeSpan baselineExpiration = default)
    {
        if (violations is null || violations.Count == 0)
            return violations ?? [];

        if (baseline is null || baseline.ViolationCount == 0)
            return violations;

        // Remove expired violations if expiration is set
        if (baselineExpiration != default)
        {
            baseline.RemoveExpired(baselineExpiration);
        }

        var newViolations = new List<RuleViolation>();

        foreach (var violation in violations)
        {
            if (!baseline.Contains(violation))
            {
                newViolations.Add(violation);
            }
            else
            {
                _logger.LogDebug(
                    "Ignoring violation from baseline: {RuleId} at {FilePath}:{LineNumber}",
                    violation.RuleId,
                    Path.GetFileName(violation.FilePath),
                    violation.LineNumber
                );
            }
        }

        _logger.LogInformation(
            "Filtered violations: {TotalViolations} total, {NewViolations} new, {IgnoredViolations} ignored from baseline",
            violations.Count,
            newViolations.Count,
            violations.Count - newViolations.Count
        );

        return newViolations;
    }

    /// <summary>
    /// Creates a baseline from analysis results.
    /// </summary>
    public Baseline CreateBaseline(AnalysisResult result)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        var baseline = new Baseline(result.ProjectName);

        foreach (var violation in result.Violations)
        {
            var contentHash = BaselineViolation.ComputeContentHash(violation);
            var baselineViolation = BaselineViolation.FromRuleViolation(violation, contentHash);
            baseline.AddViolation(baselineViolation);
        }

        _logger.LogInformation(
            "Created baseline with {ViolationCount} violations for project {ProjectName}",
            baseline.ViolationCount,
            result.ProjectName
        );

        return baseline;
    }

    /// <summary>
    /// Creates a baseline from violations.
    /// </summary>
    public Baseline CreateBaseline(string projectName, List<RuleViolation> violations)
    {
        if (string.IsNullOrWhiteSpace(projectName))
            throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

        if (violations is null)
            throw new ArgumentNullException(nameof(violations));

        var baseline = new Baseline(projectName);

        foreach (var violation in violations)
        {
            var contentHash = BaselineViolation.ComputeContentHash(violation);
            var baselineViolation = BaselineViolation.FromRuleViolation(violation, contentHash);
            baseline.AddViolation(baselineViolation);
        }

        _logger.LogInformation(
            "Created baseline with {ViolationCount} violations for {ProjectName}",
            baseline.ViolationCount,
            projectName
        );

        return baseline;
    }
}
