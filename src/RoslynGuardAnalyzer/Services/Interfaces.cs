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
    Task<AnalysisResult> AnalyzeProjectAsync(string projectPath);

    /// <summary>
    /// Analyzes a single file asynchronously.
    /// </summary>
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
    void RegisterRule(AnalysisRule rule);

    /// <summary>
    /// Gets a rule by ID.
    /// </summary>
    AnalysisRule? GetRule(string ruleId);

    /// <summary>
    /// Gets all registered rules.
    /// </summary>
    IReadOnlyList<AnalysisRule> GetAllRules();

    /// <summary>
    /// Gets rules by category.
    /// </summary>
    IReadOnlyList<AnalysisRule> GetRulesByCategory(string category);

    /// <summary>
    /// Removes a rule by ID.
    /// </summary>
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
    string GenerateReport(AnalysisResult result);

    /// <summary>
    /// Generates a report in a specific format.
    /// </summary>
    string GenerateFormattedReport(AnalysisResult result, string format);

    /// <summary>
    /// Saves the report to a file.
    /// </summary>
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
    Task<List<RuleViolation>> ExecuteRuleAsync(AnalysisRule rule, List<CodeElement> elements);

    /// <summary>
    /// Executes all enabled rules.
    /// </summary>
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
    (bool IsValid, List<string> Errors) ValidateRuleConfiguration(RuleConfiguration config);

    /// <summary>
    /// Validates an analysis rule.
    /// </summary>
    (bool IsValid, List<string> Errors) ValidateRule(AnalysisRule rule);

    /// <summary>
    /// Validates a project path.
    /// </summary>
    (bool IsValid, string? Error) ValidateProjectPath(string projectPath);
}
