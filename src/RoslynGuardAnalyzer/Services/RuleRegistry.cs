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
/// Manages registration and retrieval of architectural analysis rules.
/// </summary>
public sealed class RuleRegistry : IRuleRegistry
{
    private readonly Dictionary<string, AnalysisRule> _rules = new();

    /// <summary>
    /// Initializes a new instance of the RuleRegistry with default rules.
    /// </summary>
    public RuleRegistry()
    {
        InitializeDefaultRules();
    }

    /// <summary>
    /// Registers a new rule in the registry.
    /// </summary>
    public void RegisterRule(AnalysisRule rule)
    {
        if (rule is null)
            throw new ArgumentNullException(nameof(rule));

        if (!rule.IsValid())
            throw new ConfigurationException($"Rule {rule.Id} is not valid.");

        if (_rules.ContainsKey(rule.Id))
            throw new ConfigurationException($"Rule with ID '{rule.Id}' is already registered.");

        _rules[rule.Id] = rule;
    }

    /// <summary>
    /// Retrieves a rule by its ID.
    /// </summary>
    public AnalysisRule? GetRule(string ruleId)
    {
        if (string.IsNullOrWhiteSpace(ruleId))
            return null;

        _rules.TryGetValue(ruleId, out var rule);
        return rule;
    }

    /// <summary>
    /// Retrieves all registered rules.
    /// </summary>
    public IReadOnlyList<AnalysisRule> GetAllRules()
    {
        return _rules.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Retrieves rules filtered by category.
    /// </summary>
    public IReadOnlyList<AnalysisRule> GetRulesByCategory(string category)
    {
        return _rules.Values
            .Where(r => r.Category.ToString() == category)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Removes a rule from the registry.
    /// </summary>
    public bool RemoveRule(string ruleId)
    {
        if (string.IsNullOrWhiteSpace(ruleId))
            return false;

        return _rules.Remove(ruleId);
    }

    /// <summary>
    /// Gets the total count of registered rules.
    /// </summary>
    public int GetRuleCount()
    {
        return _rules.Count;
    }

    /// <summary>
    /// Gets enabled rules only.
    /// </summary>
    public IReadOnlyList<AnalysisRule> GetEnabledRules()
    {
        return _rules.Values
            .Where(r => r.IsEnabled)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Clears all registered rules.
    /// </summary>
    public void Clear()
    {
        _rules.Clear();
    }

    /// <summary>
    /// Initializes the registry with default rules.
    /// </summary>
    private void InitializeDefaultRules()
    {
        var layerRule = new AnalysisRule(
            AnalyzerConstants.DefaultRules.LayerDependencyRule,
            "Layer Dependency Rule",
            "Enforces proper layer dependencies and prevents illegal cross-layer references",
            RuleCategory.LayerDependency)
        {
            DefaultSeverity = SeverityLevel.Error,
            Author = AnalyzerConstants.Author,
            Version = new Version(1, 0, 0)
        };

        var namingRule = new AnalysisRule(
            AnalyzerConstants.DefaultRules.NamingConventionRule,
            "Naming Convention Rule",
            "Enforces consistent naming conventions across the codebase",
            RuleCategory.NamingConvention)
        {
            DefaultSeverity = SeverityLevel.Warning,
            Author = AnalyzerConstants.Author,
            Version = new Version(1, 0, 0)
        };

        var asyncRule = new AnalysisRule(
            AnalyzerConstants.DefaultRules.AsyncPatternRule,
            "Async Pattern Rule",
            "Enforces proper async/await patterns and detects blocking calls",
            RuleCategory.AsyncPattern)
        {
            DefaultSeverity = SeverityLevel.Warning,
            Author = AnalyzerConstants.Author,
            Version = new Version(1, 0, 0)
        };

        var nullSafetyRule = new AnalysisRule(
            AnalyzerConstants.DefaultRules.NullSafetyRule,
            "Null Safety Rule",
            "Enforces null safety and proper nullable reference handling",
            RuleCategory.NullSafety)
        {
            DefaultSeverity = SeverityLevel.Warning,
            Author = AnalyzerConstants.Author,
            Version = new Version(1, 0, 0)
        };

        RegisterRule(layerRule);
        RegisterRule(namingRule);
        RegisterRule(asyncRule);
        RegisterRule(nullSafetyRule);
    }
}
