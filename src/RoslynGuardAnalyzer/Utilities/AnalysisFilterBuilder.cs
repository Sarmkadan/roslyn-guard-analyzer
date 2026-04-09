#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Utilities;

/// <summary>
/// Builder for creating filters to apply to analysis results.
/// Allows filtering by severity, rule, file, and other criteria.
/// </summary>
public sealed class AnalysisFilterBuilder
{
    private readonly List<Func<RuleViolation, bool>> _predicates = [];

    /// <summary>
    /// Filters violations by severity level (inclusive).
    /// </summary>
    public AnalysisFilterBuilder MinimumSeverity(string severity)
    {
        var severityOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Low"] = 1,
            ["Medium"] = 2,
            ["High"] = 3,
            ["Critical"] = 4
        };

        if (!severityOrder.TryGetValue(severity, out var minLevel))
            throw new ArgumentException($"Unknown severity: {severity}");

        _predicates.Add(v =>
        {
            severityOrder.TryGetValue(v.Severity, out var level);
            return level >= minLevel;
        });

        return this;
    }

    /// <summary>
    /// Filters violations by specific severity level.
    /// </summary>
    public AnalysisFilterBuilder BySeverity(string severity)
    {
        _predicates.Add(v => v.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase));
        return this;
    }

    /// <summary>
    /// Filters violations by rule name.
    /// </summary>
    public AnalysisFilterBuilder ByRule(string ruleName)
    {
        _predicates.Add(v => v.RuleName.Equals(ruleName, StringComparison.OrdinalIgnoreCase));
        return this;
    }

    /// <summary>
    /// Filters violations by any of the specified rules.
    /// </summary>
    public AnalysisFilterBuilder ByAnyRule(params string[] ruleNames)
    {
        var rules = new HashSet<string>(ruleNames, StringComparer.OrdinalIgnoreCase);
        _predicates.Add(v => rules.Contains(v.RuleName));
        return this;
    }

    /// <summary>
    /// Filters violations by file path (supports wildcards).
    /// </summary>
    public AnalysisFilterBuilder ByFile(string filePath)
    {
        var matcher = new TypeNameMatcher(filePath); // Reuse pattern matching
        _predicates.Add(v => v.FilePath.Contains(filePath, StringComparison.OrdinalIgnoreCase));
        return this;
    }

    /// <summary>
    /// Filters violations that occurred on or after a specific line.
    /// </summary>
    public AnalysisFilterBuilder FromLine(int lineNumber)
    {
        _predicates.Add(v => v.LineNumber >= lineNumber);
        return this;
    }

    /// <summary>
    /// Filters violations that occurred on or before a specific line.
    /// </summary>
    public AnalysisFilterBuilder ToLine(int lineNumber)
    {
        _predicates.Add(v => v.LineNumber <= lineNumber);
        return this;
    }

    /// <summary>
    /// Filters violations by message content (case-insensitive).
    /// </summary>
    public AnalysisFilterBuilder ContainsMessage(string text)
    {
        _predicates.Add(v => v.Message.Contains(text, StringComparison.OrdinalIgnoreCase));
        return this;
    }

    /// <summary>
    /// Adds a custom filter predicate.
    /// </summary>
    public AnalysisFilterBuilder Where(Func<RuleViolation, bool> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        _predicates.Add(predicate);
        return this;
    }

    /// <summary>
    /// Builds the combined filter function.
    /// </summary>
    public Func<RuleViolation, bool> Build()
    {
        if (_predicates.Count == 0)
            return v => true; // No filters, include all

        return violation => _predicates.All(p => p(violation));
    }

    /// <summary>
    /// Applies the filter to a collection of violations.
    /// </summary>
    public IEnumerable<RuleViolation> Apply(IEnumerable<RuleViolation> violations)
    {
        var filter = Build();
        return violations.Where(filter);
    }
}
