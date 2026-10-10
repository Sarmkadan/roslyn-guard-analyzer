#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

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
    private const string EmptyFilePathMessage = "File path cannot be null or empty";
    private const string EmptyProjectNameMessage = "Project name cannot be null or empty";
    private const string TraversalSegment = "..";
    private const string TraversalMessageSuffix =
        "' contains directory traversal sequence '..'. " +
        "Paths must stay within the expected directory structure.";
    private const string InvalidPathCharsMessageSuffix = "' contains invalid path characters.";

    private const string BaselineNotFoundLog = "Baseline file not found: {FilePath}";
    private const string BaselineParseFailedLog = "Failed to parse baseline file: {FilePath}";
    private const string BaselineLoadedLog =
        "Loaded baseline with {ViolationCount} violations from {FilePath}";
    private const string BaselineLoadErrorLog = "Error loading baseline file: {FilePath}";
    private const string BaselineSavedLog =
        "Saved baseline with {ViolationCount} violations to {FilePath}";
    private const string BaselineSaveErrorLog = "Error saving baseline file: {FilePath}";
    private const string IgnoredViolationLog =
        "Ignoring violation from baseline: {RuleId} at {FilePath}:{LineNumber}";
    private const string FilteredViolationsLog =
        "Filtered violations: {TotalViolations} total, {NewViolations} new, {IgnoredViolations} ignored from baseline";
    private const string CreatedBaselineForProjectLog =
        "Created baseline with {ViolationCount} violations for project {ProjectName}";
    private const string CreatedBaselineForNameLog =
        "Created baseline with {ViolationCount} violations for {ProjectName}";

    private readonly ILogger<BaselineService> _logger;

    public BaselineService(ILogger<BaselineService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates that a file path contains no traversal segments or invalid path characters.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the path is invalid.</exception>
    private void ValidateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException(EmptyFilePathMessage, nameof(filePath));

        if (filePath.Split('/', '\\').Any(segment => segment == TraversalSegment))
        {
            throw new ArgumentException(
                $"File path '{filePath}{TraversalMessageSuffix}",
                nameof(filePath));
        }

        if (filePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            throw new ArgumentException(
                $"File path '{filePath}{InvalidPathCharsMessageSuffix}",
                nameof(filePath));
        }

        _ = Path.GetFullPath(filePath);
    }

    /// <summary>
    /// Loads a baseline from file.
    /// </summary>
    public async Task<Baseline?> LoadBaselineAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException(EmptyFilePathMessage, nameof(filePath));

        // Validate the file path to prevent directory traversal
        ValidateFilePath(filePath);

        if (!File.Exists(filePath))
        {
            _logger.LogWarning(BaselineNotFoundLog, filePath);
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var baseline = Baseline.FromJson(json);

            if (baseline is null)
            {
                _logger.LogError(BaselineParseFailedLog, filePath);
                return null;
            }

            _logger.LogInformation(
                BaselineLoadedLog,
                baseline.ViolationCount,
                filePath
            );

            return baseline;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, BaselineLoadErrorLog, filePath);
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
            throw new ArgumentException(EmptyFilePathMessage, nameof(filePath));

        // Validate the file path to prevent directory traversal
        ValidateFilePath(filePath);

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
                BaselineSavedLog,
                baseline.ViolationCount,
                filePath
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, BaselineSaveErrorLog, filePath);
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
                    IgnoredViolationLog,
                    violation.RuleId,
                    Path.GetFileName(violation.FilePath),
                    violation.LineNumber
                );
            }
        }

        _logger.LogInformation(
            FilteredViolationsLog,
            violations.Count,
            newViolations.Count,
            violations.Count - newViolations.Count
        );

        return newViolations;
    }

    /// <summary>
    /// Creates a baseline from analysis results.
    /// </summary>
    /// <param name="result">Analysis results containing violations to baseline</param>
    /// <returns>Baseline containing all violations</returns>
    public Baseline CreateBaseline(AnalysisResult result)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        var baseline = new Baseline(result.ProjectName);

        foreach (var violation in result.Violations)
        {
            var baselineViolation = BaselineViolation.FromRuleViolation(violation);
            baseline.AddViolation(baselineViolation);
        }

        _logger.LogInformation(
            CreatedBaselineForProjectLog,
            baseline.ViolationCount,
            result.ProjectName
        );

        return baseline;
    }

    /// <summary>
    /// Creates a baseline from violations.
    /// </summary>
    /// <param name="projectName">Name of the project being baselined</param>
    /// <param name="violations">List of violations to include in baseline</param>
    /// <returns>Baseline containing all violations</returns>
    public Baseline CreateBaseline(string projectName, List<RuleViolation> violations)
    {
        if (string.IsNullOrWhiteSpace(projectName))
            throw new ArgumentException(EmptyProjectNameMessage, nameof(projectName));

        if (violations is null)
            throw new ArgumentNullException(nameof(violations));

        var baseline = new Baseline(projectName);

        foreach (var violation in violations)
        {
            var baselineViolation = BaselineViolation.FromRuleViolation(violation);
            baseline.AddViolation(baselineViolation);
        }

        _logger.LogInformation(
            CreatedBaselineForNameLog,
            baseline.ViolationCount,
            projectName
        );

        return baseline;
    }
}
