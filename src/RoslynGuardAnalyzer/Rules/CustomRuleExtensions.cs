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
    /// <param name="rule">The custom analysis rule to evaluate.</param>
    /// <param name="elements">The code elements to analyze.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of rule violations found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="rule"/> or <paramref name="elements"/> is <see langword="null"/>.</exception>
    public static Task<List<RuleViolation>> EvaluateAsync(this CustomAnalysisRule rule, IEnumerable<CodeElement> elements)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(elements);

        var violations = elements
            .Where(rule.ViolationPredicate ?? throw new InvalidOperationException("ViolationPredicate cannot be null"))
            .Select(element =>
            {
                ArgumentNullException.ThrowIfNull(element);

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