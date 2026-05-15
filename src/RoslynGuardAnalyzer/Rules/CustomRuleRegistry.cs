#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RoslynGuardAnalyzer.Rules;

/// <summary>
/// Stores runtime-defined custom rules in a thread-safe registry.
/// </summary>
public sealed class CustomRuleRegistry : ICustomRuleRegistry
{
    private readonly ConcurrentDictionary<string, CustomAnalysisRule> _rules = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public void RegisterCustomRule(CustomAnalysisRule rule)
    {
        if (rule is null)
            throw new ArgumentNullException(nameof(rule));

        if (!_rules.TryAdd(rule.Id, rule))
            throw new InvalidOperationException($"A custom rule with ID '{rule.Id}' is already registered.");
    }

    /// <inheritdoc/>
    public IReadOnlyList<CustomAnalysisRule> GetCustomRules()
    {
        return _rules.Values.OrderBy(rule => rule.Id).ToList().AsReadOnly();
    }
}
