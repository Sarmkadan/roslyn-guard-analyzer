namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Provides additional convenience methods for <see cref="AnalysisRule"/> instances.
/// </summary>
public static class AnalysisRuleAdditionalExtensions
{
    /// <summary>
    /// Determines whether the rule has a documentation URL.
    /// </summary>
    /// <param name="rule">The rule to inspect.</param>
    /// <returns><see langword="true"/> when a non-empty documentation URL is present; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rule"/> is null.</exception>
    public static bool HasDocumentation(this AnalysisRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return !string.IsNullOrWhiteSpace(rule.DocumentationUrl);
    }
}
