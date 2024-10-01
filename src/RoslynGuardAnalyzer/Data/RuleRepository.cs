// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System;
using RoslynGuardAnalyzer.Domain.Models;

using RoslynGuardAnalyzer.Core;
namespace RoslynGuardAnalyzer.Data;

/// <summary>
/// Repository for managing persistence of analysis rules.
/// </summary>
public sealed class RuleRepository : RepositoryBase<AnalysisRule>
{
    private const string RulesFileName = "rules.json";
    private readonly string _dataDirectory;

    public RuleRepository(string? dataDirectory = null)
    {
        _dataDirectory = dataDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AnalyzerConstants.ApplicationName,
            "Data");

        EnsureDataDirectoryExists();
    }

    /// <summary>
    /// Gets rules by category.
    /// </summary>
    public IReadOnlyList<AnalysisRule> GetByCategory(RuleCategory category)
    {
        return Find(r => r.Category == category);
    }

    /// <summary>
    /// Gets enabled rules only.
    /// </summary>
    public IReadOnlyList<AnalysisRule> GetEnabledRules()
    {
        return Find(r => r.IsEnabled);
    }

    /// <summary>
    /// Gets rules by severity.
    /// </summary>
    public IReadOnlyList<AnalysisRule> GetBySeverity(SeverityLevel severity)
    {
        return Find(r => r.DefaultSeverity == severity);
    }

    /// <summary>
    /// Gets rules created after a specific date.
    /// </summary>
    public IReadOnlyList<AnalysisRule> GetCreatedAfter(DateTime date)
    {
        return Find(r => r.CreatedAt > date);
    }

    /// <summary>
    /// Disables a rule by ID.
    /// </summary>
    public bool DisableRule(string ruleId)
    {
        var rule = GetById(ruleId);
        if (rule == null)
            return false;

        rule.IsEnabled = false;
        rule.MarkAsModified();
        Update(ruleId, rule);
        return true;
    }

    /// <summary>
    /// Enables a rule by ID.
    /// </summary>
    public bool EnableRule(string ruleId)
    {
        var rule = GetById(ruleId);
        if (rule == null)
            return false;

        rule.IsEnabled = true;
        rule.MarkAsModified();
        Update(ruleId, rule);
        return true;
    }

    /// <summary>
    /// Saves all rules to disk asynchronously.
    /// </summary>
    public async Task SaveAsync()
    {
        var rulesPath = Path.Combine(_dataDirectory, RulesFileName);

        try
        {
            var rules = GetAll();
            var json = System.Text.Json.JsonSerializer.Serialize(
                rules,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(rulesPath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save rules: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads rules from disk asynchronously.
    /// </summary>
    public async Task LoadAsync()
    {
        var rulesPath = Path.Combine(_dataDirectory, RulesFileName);

        if (!File.Exists(rulesPath))
            return;

        try
        {
            var json = await File.ReadAllTextAsync(rulesPath);
            var rules = System.Text.Json.JsonSerializer.Deserialize<List<AnalysisRule>>(json);

            Clear();

            if (rules != null)
            {
                foreach (var rule in rules)
                {
                    Add(rule.Id, rule);
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load rules: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Exports rules to a JSON file.
    /// </summary>
    public async Task ExportAsync(string filePath)
    {
        try
        {
            var rules = GetAll();
            var json = System.Text.Json.JsonSerializer.Serialize(
                rules,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to export rules: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Imports rules from a JSON file.
    /// </summary>
    public async Task ImportAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Import file not found: {filePath}");

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var rules = System.Text.Json.JsonSerializer.Deserialize<List<AnalysisRule>>(json);

            if (rules != null)
            {
                foreach (var rule in rules)
                {
                    if (Exists(rule.Id))
                    {
                        Update(rule.Id, rule);
                    }
                    else
                    {
                        Add(rule.Id, rule);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to import rules: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Ensures the data directory exists.
    /// </summary>
    private void EnsureDataDirectoryExists()
    {
        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }
    }

    /// <summary>
    /// Gets the data directory path.
    /// </summary>
    public string GetDataDirectory()
    {
        return _dataDirectory;
    }

    /// <summary>
    /// Gets statistics about stored rules.
    /// </summary>
    public RuleRepositoryStatistics GetStatistics()
    {
        var allRules = GetAll();

        return new RuleRepositoryStatistics
        {
            TotalRules = allRules.Count,
            EnabledRules = allRules.Count(r => r.IsEnabled),
            DisabledRules = allRules.Count(r => !r.IsEnabled),
            RulesByCategory = allRules
                .GroupBy(r => r.Category)
                .ToDictionary(g => g.Key.ToString(), g => g.Count())
        };
    }
}

/// <summary>
/// Statistics about the rules repository.
/// </summary>
public sealed class RuleRepositoryStatistics
{
    public int TotalRules { get; set; }
    public int EnabledRules { get; set; }
    public int DisabledRules { get; set; }
    public Dictionary<string, int> RulesByCategory { get; set; } = new();

    public double GetEnabledPercentage()
    {
        if (TotalRules == 0)
            return 0;

        return (EnabledRules / (double)TotalRules) * 100;
    }
}
