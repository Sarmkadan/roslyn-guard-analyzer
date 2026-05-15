#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;

namespace RoslynGuardAnalyzer.Rules;

/// <summary>
/// Defines storage operations for custom rules registered at runtime.
/// </summary>
public interface ICustomRuleRegistry
{
    /// <summary>
    /// Registers a custom rule instance.
    /// </summary>
    void RegisterCustomRule(CustomAnalysisRule rule);

    /// <summary>
    /// Returns all registered custom rules.
    /// </summary>
    IReadOnlyList<CustomAnalysisRule> GetCustomRules();
}
