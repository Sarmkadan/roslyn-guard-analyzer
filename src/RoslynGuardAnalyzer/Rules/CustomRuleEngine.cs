#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Rules;

/// <summary>
/// Evaluates runtime-registered custom rules against code elements.
/// </summary>
public sealed class CustomRuleEngine
{
    private readonly ICustomRuleRegistry _customRuleRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomRuleEngine"/> class.
    /// </summary>
    public CustomRuleEngine(ICustomRuleRegistry customRuleRegistry)
    {
        _customRuleRegistry = customRuleRegistry ?? throw new ArgumentNullException(nameof(customRuleRegistry));
    }

    /// <summary>
    /// Evaluates one custom rule against the supplied elements.
    /// </summary>
    public Task<List<RuleViolation>> EvaluateRuleAsync(
        CustomAnalysisRule rule,
        IEnumerable<CodeElement> elements,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return rule.EvaluateAsync(elements);
    }

    /// <summary>
    /// Evaluates all registered custom rules against the supplied elements.
    /// </summary>
    public async Task<List<RuleViolation>> EvaluateAsync(
        IEnumerable<CodeElement> elements,
        CancellationToken cancellationToken = default)
    {
        if (elements is null)
            throw new ArgumentNullException(nameof(elements));

        var materializedElements = elements.ToList();
        var violations = new List<RuleViolation>();

        foreach (var rule in _customRuleRegistry.GetCustomRules())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var ruleViolations = await rule.EvaluateAsync(materializedElements).ConfigureAwait(false);
            violations.AddRange(ruleViolations);
        }

        return violations;
    }
}
