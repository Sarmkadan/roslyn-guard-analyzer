#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Services;

/// <summary>
/// Service interface for orchestrating code analysis workflow.
/// </summary>
public interface IAnalysisService
{
    /// <summary>
    /// Analyzes a project asynchronously.
    /// </summary>
    /// <param name="projectPath">The path to the project to analyze.</param>
    /// <returns>A task that resolves to the <see cref="AnalysisResult"/> for the project.</returns>
    Task<AnalysisResult> AnalyzeProjectAsync(string projectPath);

    /// <summary>
    /// Analyzes a single file asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the file to analyze.</param>
    /// <returns>A task that resolves to the <see cref="AnalysisResult"/> for the file.</returns>
    Task<AnalysisResult> AnalyzeFileAsync(string filePath);
}

/// <summary>
/// Service interface for managing architectural rules.
/// </summary>
public interface IRuleRegistry
{
    /// <summary>
    /// Registers a new rule.
    /// </summary>
    /// <param name="rule">The rule to register.</param>
    void RegisterRule(AnalysisRule rule);

    /// <summary>
    /// Gets a rule by ID.
    /// </summary>
    /// <param name="ruleId">The identifier of the rule to look up.</param>
    /// <returns>The rule with the given ID, or <c>null</c> if no such rule is registered.</returns>
    AnalysisRule? GetRule(string ruleId);

    /// <summary>
    /// Gets all registered rules.
    /// </summary>
    /// <returns>A read-only list of all registered rules.</returns>
    IReadOnlyList<AnalysisRule> GetAllRules();

    /// <summary>
    /// Gets rules by category.
    /// </summary>
    /// <param name="category">The category name to filter rules by.</param>
    /// <returns>A read-only list of the rules that belong to the given category.</returns>
    IReadOnlyList<AnalysisRule> GetRulesByCategory(string category);

    /// <summary>
    /// Removes a rule by ID.
    /// </summary>
    /// <param name="ruleId">The identifier of the rule to remove.</param>
    /// <returns><c>true</c> if a rule with the given ID was removed; otherwise, <c>false</c>.</returns>
    bool RemoveRule(string ruleId);
}

/// <summary>
/// Service interface for generating analysis reports.
/// </summary>
public interface IReportingService
{
    /// <summary>
    /// Generates a report from analysis results.
    /// </summary>
    /// <param name="result">The analysis result to report on.</param>
    /// <returns>The generated report as a string.</returns>
    string GenerateReport(AnalysisResult result);

    /// <summary>
    /// Generates a report in a specific format.
    /// </summary>
    /// <param name="result">The analysis result to report on.</param>
    /// <param name="format">The output format name (for example, JSON, CSV, or XML).</param>
    /// <returns>The generated report as a string in the requested format.</returns>
    string GenerateFormattedReport(AnalysisResult result, string format);

    /// <summary>
    /// Saves the report to a file.
    /// </summary>
    /// <param name="report">The violation report to save.</param>
    /// <param name="filePath">The path of the file to write the report to.</param>
    /// <returns>A task that completes when the report has been saved.</returns>
    Task SaveReportAsync(ViolationReport report, string filePath);
}

/// <summary>
/// Service interface for executing analysis rules.
/// </summary>
public interface IRuleEngine
{
    /// <summary>
    /// Executes a rule against code elements.
    /// </summary>
    /// <param name="rule">The rule to execute.</param>
    /// <param name="elements">The code elements to evaluate the rule against.</param>
    /// <returns>A task that resolves to the list of violations found by the rule.</returns>
    Task<List<RuleViolation>> ExecuteRuleAsync(AnalysisRule rule, List<CodeElement> elements);

    /// <summary>
    /// Executes all enabled rules.
    /// </summary>
    /// <param name="elements">The code elements to evaluate the rules against.</param>
    /// <returns>A task that resolves to the combined list of violations from all enabled rules.</returns>
    Task<List<RuleViolation>> ExecuteAllRulesAsync(List<CodeElement> elements);
}

/// <summary>
/// Service interface for configuration validation.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates a rule configuration.
    /// </summary>
    /// <param name="config">The rule configuration to validate.</param>
    /// <returns>
    /// A tuple whose <c>IsValid</c> value indicates whether the configuration is valid,
    /// and whose <c>Errors</c> value lists the validation error messages.
    /// </returns>
    (bool IsValid, List<string> Errors) ValidateRuleConfiguration(RuleConfiguration config);

    /// <summary>
    /// Validates an analysis rule.
    /// </summary>
    /// <param name="rule">The analysis rule to validate.</param>
    /// <returns>
    /// A tuple whose <c>IsValid</c> value indicates whether the rule is valid,
    /// and whose <c>Errors</c> value lists the validation error messages.
    /// </returns>
    (bool IsValid, List<string> Errors) ValidateRule(AnalysisRule rule);

    /// <summary>
    /// Validates a project path.
    /// </summary>
    /// <param name="projectPath">The project path to validate.</param>
    /// <returns>
    /// A tuple whose <c>IsValid</c> value indicates whether the path is valid,
    /// and whose <c>Error</c> value contains the error message when the path is invalid, or <c>null</c> otherwise.
    /// </returns>
    (bool IsValid, string? Error) ValidateProjectPath(string projectPath);
}

/// <summary>
/// Service interface for managing baseline files.
/// </summary>
public interface IBaselineService
{
    /// <summary>
    /// Loads a baseline from file.
    /// </summary>
    /// <param name="filePath">The path of the baseline file to load.</param>
    /// <returns>A task that resolves to the loaded <see cref="Baseline"/>, or <c>null</c> if no baseline could be loaded.</returns>
    Task<Baseline?> LoadBaselineAsync(string filePath);

    /// <summary>
    /// Saves a baseline to file.
    /// </summary>
    /// <param name="baseline">The baseline to save.</param>
    /// <param name="filePath">The path of the file to write the baseline to.</param>
    /// <returns>A task that completes when the baseline has been saved.</returns>
    Task SaveBaselineAsync(Baseline baseline, string filePath);

    /// <summary>
    /// Filters violations to only return new violations not in baseline.
    /// </summary>
    /// <param name="violations">The violations to filter.</param>
    /// <param name="baseline">The baseline whose violations are excluded. When <c>null</c> or empty, all violations are returned.</param>
    /// <param name="baselineExpiration">
    /// The age after which baseline entries expire. Expired entries are removed from the baseline before filtering.
    /// When left at its default value, no expiration is applied.
    /// </param>
    /// <returns>A list of the violations that are not present in the baseline.</returns>
    List<RuleViolation> FilterNewViolations(
        List<RuleViolation> violations,
        Baseline? baseline,
        TimeSpan baselineExpiration = default);

    /// <summary>
    /// Creates a baseline from analysis results.
    /// </summary>
    /// <param name="result">The analysis result whose violations are recorded in the baseline.</param>
    /// <returns>A new <see cref="Baseline"/> containing the violations from the result.</returns>
    Baseline CreateBaseline(AnalysisResult result);

    /// <summary>
    /// Creates a baseline from violations.
    /// </summary>
    /// <param name="projectName">The name of the project the baseline belongs to.</param>
    /// <param name="violations">The violations to record in the baseline.</param>
    /// <returns>A new <see cref="Baseline"/> containing the given violations.</returns>
    Baseline CreateBaseline(string projectName, List<RuleViolation> violations);
}
