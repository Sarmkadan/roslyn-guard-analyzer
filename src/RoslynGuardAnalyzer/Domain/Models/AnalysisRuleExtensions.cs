namespace RoslynGuardAnalyzer.Domain.Models;

public static class AnalysisRuleExtensions
{
    /// <summary>
    /// Determines whether two <see cref="AnalysisRule"/> instances are equivalent.
    /// </summary>
    /// <param name="rule">The rule to compare.</param>
    /// <param name="other">The other rule to compare.</param>
    /// <returns>true if the rules are equivalent; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="rule"/> or <paramref name="other"/> is null.</exception>
    public static bool IsEquivalentTo(this AnalysisRule? rule, AnalysisRule? other)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(other);

        return rule.Id == other.Id &&
               rule.Name == other.Name &&
               rule.Description == other.Description &&
               rule.Category == other.Category &&
               rule.DefaultSeverity == other.DefaultSeverity &&
               rule.IsEnabled == other.IsEnabled &&
               rule.RulePattern == other.RulePattern &&
               rule.Configuration.Keys.All(k => other.Configuration.ContainsKey(k) && other.Configuration[k].Equals(rule.Configuration[k]));
    }

    /// <summary>
    /// Gets a human-readable string representation of the rule's category and severity.
    /// </summary>
    /// <param name="rule">The rule to get the string representation for.</param>
    /// <returns>A string representation of the rule's category and severity.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="rule"/> is null.</exception>
    public static string GetCategoryAndSeverityString(this AnalysisRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return $"{rule.Category} - {rule.DefaultSeverity}";
    }

    /// <summary>
    /// Determines whether a rule is a subset of another rule (i.e., its pattern is a subset of the other rule's pattern).
    /// </summary>
    /// <param name="rule">The rule to check.</param>
    /// <param name="other">The other rule to check.</param>
    /// <returns>true if the rule is a subset of the other rule; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="rule"/> or <paramref name="other"/> is null.</exception>
    public static bool IsSubsetOf(this AnalysisRule? rule, AnalysisRule? other)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(other);

        if (string.IsNullOrEmpty(rule.RulePattern) || string.IsNullOrEmpty(other.RulePattern))
        {
            return false;
        }

        // Simple substring check; consider improving with more sophisticated pattern matching logic
        return other.RulePattern.Contains(rule.RulePattern, StringComparison.OrdinalIgnoreCase);
    }
}
