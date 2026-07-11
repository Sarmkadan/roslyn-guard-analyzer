#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Services;

/// <summary>
/// Provides extension methods for <see cref="RuleRegistry"/> to enhance rule management functionality.
/// </summary>
public static class RuleRegistryExtensions
{
    /// <summary>
    /// Attempts to get a rule by its ID, throwing a descriptive exception if not found.
    /// </summary>
    /// <param name="registry">The rule registry instance.</param>
    /// <param name="ruleId">The ID of the rule to retrieve.</param>
    /// <returns>The found analysis rule.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="registry"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="ruleId"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    /// <exception cref="KeyNotFoundException">The rule ID is not found in the registry.</exception>
    public static AnalysisRule GetRequiredRule(this RuleRegistry registry, string ruleId)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (string.IsNullOrWhiteSpace(ruleId))
        {
            throw new ArgumentException("Rule ID cannot be null or whitespace.", nameof(ruleId));
        }

        var rule = registry.GetRule(ruleId);
        return rule ?? throw new KeyNotFoundException($"Rule with ID '{ruleId}' was not found in the registry.");
    }

    /// <summary>
    /// Checks if a rule with the specified ID exists in the registry.
    /// </summary>
    /// <param name="registry">The rule registry instance.</param>
    /// <param name="ruleId">The ID of the rule to check.</param>
    /// <returns>True if the rule exists; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="registry"/> is <see langword="null"/>.</exception>
    public static bool ContainsRule(this RuleRegistry registry, string ruleId)
    {
        ArgumentNullException.ThrowIfNull(registry);

        return !string.IsNullOrWhiteSpace(ruleId) && registry.GetRule(ruleId) is not null;
    }

    /// <summary>
    /// Gets the number of rules in the specified category.
    /// </summary>
    /// <param name="registry">The rule registry instance.</param>
    /// <param name="category">The category to count rules for.</param>
    /// <returns>The count of rules in the specified category.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="registry"/> is <see langword="null"/>.</exception>
    public static int GetRuleCountByCategory(this RuleRegistry registry, string category)
    {
        ArgumentNullException.ThrowIfNull(registry);

        return string.IsNullOrWhiteSpace(category)
            ? 0
            : registry.GetRulesByCategory(category).Count;
    }

    /// <summary>
    /// Gets all rule IDs currently registered in the registry.
    /// </summary>
    /// <param name="registry">The rule registry instance.</param>
    /// <returns>A read-only list of all registered rule IDs.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="registry"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> GetAllRuleIds(this RuleRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        return registry.GetAllRules()
            .Select(r => r.Id)
            .ToList()
            .AsReadOnly();
    }
}