// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

using RoslynGuardAnalyzer.Core;
namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Contains configuration settings for running analysis with specific rules.
/// </summary>
public sealed class RuleConfiguration
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<AnalysisRule> EnabledRules { get; set; }
    public List<string> ExcludedNamespaces { get; set; }
    public List<string> ExcludedFiles { get; set; }
    public int MaxViolationsToReport { get; set; }
    public int AnalysisTimeoutSeconds { get; set; }
    public SeverityLevel MinimumReportedSeverity { get; set; }
    public bool FailOnError { get; set; }
    public bool GenerateDetailedReport { get; set; }
    public Dictionary<string, string> CustomSettings { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public RuleConfiguration()
    {
        Id = Guid.NewGuid().ToString();
        Name = string.Empty;
        Description = string.Empty;
        EnabledRules = new List<AnalysisRule>();
        ExcludedNamespaces = new List<string>();
        ExcludedFiles = new List<string>();
        MaxViolationsToReport = AnalyzerConstants.Analysis.DefaultMaxViolationsToReport;
        AnalysisTimeoutSeconds = AnalyzerConstants.Analysis.DefaultTimeoutSeconds;
        MinimumReportedSeverity = SeverityLevel.Warning;
        FailOnError = false;
        GenerateDetailedReport = true;
        CustomSettings = new Dictionary<string, string>();
        CreatedAt = DateTime.UtcNow;
    }

    public RuleConfiguration(string name, string description)
        : this()
    {
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Adds a rule to the enabled rules list.
    /// </summary>
    public void AddRule(AnalysisRule rule)
    {
        if (rule != null && !EnabledRules.Any(r => r.Id == rule.Id))
        {
            EnabledRules.Add(rule);
        }
    }

    /// <summary>
    /// Removes a rule by ID.
    /// </summary>
    public bool RemoveRule(string ruleId)
    {
        var rule = EnabledRules.FirstOrDefault(r => r.Id == ruleId);
        if (rule != null)
        {
            EnabledRules.Remove(rule);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets a rule by ID.
    /// </summary>
    public AnalysisRule? GetRule(string ruleId)
    {
        return EnabledRules.FirstOrDefault(r => r.Id == ruleId);
    }

    /// <summary>
    /// Adds a namespace to the exclusion list.
    /// </summary>
    public void ExcludeNamespace(string namespaceName)
    {
        if (!string.IsNullOrWhiteSpace(namespaceName) && !ExcludedNamespaces.Contains(namespaceName))
        {
            ExcludedNamespaces.Add(namespaceName);
        }
    }

    /// <summary>
    /// Adds a file pattern to the exclusion list.
    /// </summary>
    public void ExcludeFile(string filePattern)
    {
        if (!string.IsNullOrWhiteSpace(filePattern) && !ExcludedFiles.Contains(filePattern))
        {
            ExcludedFiles.Add(filePattern);
        }
    }

    /// <summary>
    /// Checks if a file should be analyzed based on exclusion rules.
    /// </summary>
    public bool ShouldAnalyzeFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        foreach (var pattern in ExcludedFiles)
        {
            if (filePath.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a namespace should be analyzed.
    /// </summary>
    public bool ShouldAnalyzeNamespace(string namespaceName)
    {
        if (string.IsNullOrWhiteSpace(namespaceName))
            return true;

        return !ExcludedNamespaces.Any(n =>
            namespaceName.StartsWith(n, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Sets a custom setting value.
    /// </summary>
    public void SetCustomSetting(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Setting key cannot be null or empty.", nameof(key));

        CustomSettings[key] = value ?? string.Empty;
    }

    /// <summary>
    /// Gets a custom setting with optional default.
    /// </summary>
    public string? GetCustomSetting(string key, string? defaultValue = null)
    {
        return CustomSettings.TryGetValue(key, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Validates the configuration is valid and consistent.
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Length < 3)
            return false;

        if (MaxViolationsToReport <= 0 || AnalysisTimeoutSeconds <= 0)
            return false;

        if (!EnabledRules.Any())
            return false;

        return true;
    }

    /// <summary>
    /// Gets the count of enabled rules.
    /// </summary>
    public int GetEnabledRuleCount()
    {
        return EnabledRules.Count(r => r.IsEnabled);
    }

    /// <summary>
    /// Creates a copy of this configuration.
    /// </summary>
    public RuleConfiguration CreateCopy()
    {
        var copy = new RuleConfiguration
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"{Name} (Copy)",
            Description = Description,
            MaxViolationsToReport = MaxViolationsToReport,
            AnalysisTimeoutSeconds = AnalysisTimeoutSeconds,
            MinimumReportedSeverity = MinimumReportedSeverity,
            FailOnError = FailOnError,
            GenerateDetailedReport = GenerateDetailedReport
        };

        foreach (var rule in EnabledRules)
            copy.AddRule(rule);

        foreach (var ns in ExcludedNamespaces)
            copy.ExcludeNamespace(ns);

        foreach (var file in ExcludedFiles)
            copy.ExcludeFile(file);

        foreach (var setting in CustomSettings)
            copy.SetCustomSetting(setting.Key, setting.Value);

        return copy;
    }

    /// <summary>
    /// Marks the configuration as updated.
    /// </summary>
    public void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
