#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Exceptions;

namespace RoslynGuardAnalyzer.Services;

/// <summary>
/// Manages registration and retrieval of architectural analysis rules.
/// </summary>
public sealed class RuleRegistry : IRuleRegistry
{
    private readonly Dictionary<string, AnalysisRule> _rules = new();
    private readonly ILogger<RuleRegistry>? _logger;

    /// <summary>
    /// Initializes a new instance of the RuleRegistry with default rules.
    /// </summary>
    /// <param name="logger">The optional logger for registry operations.</param>
    public RuleRegistry(ILogger<RuleRegistry>? logger = null)
    {
        _logger = logger;
        InitializeDefaultRules();
    }

    /// <summary>
    /// Registers a new rule in the registry.
    /// </summary>
    /// <param name="rule">The analysis rule to register.</param>
    /// <exception cref="ArgumentNullException">Thrown if rule is null.</exception>
    /// <exception cref="ConfigurationException">Thrown if rule is invalid or already registered.</exception>
    public void RegisterRule(AnalysisRule rule)
    {
        if (rule is null)
        {
            _logger?.LogWarning("Cannot register invalid rule with ID {RuleId}", (object?)null);
            throw new ArgumentNullException(nameof(rule));
        }

        if (!rule.IsValid())
        {
            _logger?.LogWarning("Cannot register invalid rule with ID {RuleId}", rule.Id);
            throw new ConfigurationException($"Rule {rule.Id} is not valid.");
        }

        if (_rules.ContainsKey(rule.Id))
        {
            _logger?.LogWarning("Rule with ID {RuleId} is already registered", rule.Id);
            throw new ConfigurationException($"Rule with ID '{rule.Id}' is already registered.");
        }

        _rules[rule.Id] = rule;
        _logger?.LogDebug("Registered rule with ID {RuleId}", rule.Id);
    }

    /// <summary>
    /// Retrieves a rule by its ID.
    /// </summary>
    public AnalysisRule? GetRule(string ruleId)
    {
        if (string.IsNullOrWhiteSpace(ruleId))
        {
            _logger?.LogDebug("Rule lookup missed for ID {RuleId}", ruleId);
            return null;
        }

        _rules.TryGetValue(ruleId, out var rule);

        if (rule is null)
            _logger?.LogDebug("Rule lookup missed for ID {RuleId}", ruleId);

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
        {
            _logger?.LogDebug("Rule removal missed for ID {RuleId}", ruleId);
            return false;
        }

        var removed = _rules.Remove(ruleId);

        if (!removed)
            _logger?.LogDebug("Rule removal missed for ID {RuleId}", ruleId);

        return removed;
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

        _logger?.LogInformation("Initialized registry with {RuleCount} default rules", _rules.Count);
    }
}
