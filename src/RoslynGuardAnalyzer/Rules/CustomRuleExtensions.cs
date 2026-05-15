#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Rules;

/// <summary>
/// Provides standalone helpers for evaluating custom rules.
/// </summary>
public static class CustomRuleExtensions
{
    /// <summary>
    /// Evaluates a custom rule against the supplied code elements.
    /// </summary>
    public static Task<List<RuleViolation>> EvaluateAsync(this CustomAnalysisRule rule, IEnumerable<CodeElement> elements)
    {
        if (rule is null)
            throw new ArgumentNullException(nameof(rule));

        if (elements is null)
            throw new ArgumentNullException(nameof(elements));

        var violations = elements
            .Where(rule.ViolationPredicate)
            .Select(element =>
            {
                var violation = new RuleViolation(rule.Id, rule.Name, rule.MessageFactory(element), element.FilePath)
                {
                    LineNumber = element.StartLineNumber,
                    Severity = rule.DefaultSeverity,
                    Category = rule.Category
                };

                violation.AddMetadata("ElementName", element.Name);
                violation.AddMetadata("FullyQualifiedName", element.GetFullyQualifiedName());
                return violation;
            })
            .ToList();

        return Task.FromResult(violations);
    }
}
