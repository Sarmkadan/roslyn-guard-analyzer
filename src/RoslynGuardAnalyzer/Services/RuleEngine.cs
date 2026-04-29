#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Services;

/// <summary>
/// Executes architectural rules against code elements to detect violations.
/// </summary>
public sealed class RuleEngine : IRuleEngine
{
    private readonly IRuleRegistry _ruleRegistry;

    public RuleEngine(IRuleRegistry ruleRegistry)
    {
        _ruleRegistry = ruleRegistry ?? throw new ArgumentNullException(nameof(ruleRegistry));
    }

    /// <summary>
    /// Executes a specific rule against code elements.
    /// All checks are synchronous CPU-bound work - no need for Task.Run
    /// since callers (BackgroundTaskQueue, AnalysisService) already run
    /// on background threads.
    /// </summary>
    public Task<List<RuleViolation>> ExecuteRuleAsync(AnalysisRule rule, List<CodeElement> elements)
    {
        if (rule is null)
            throw new ArgumentNullException(nameof(rule));

        if (!rule.IsEnabled || elements is null || !elements.Any())
            return Task.FromResult(new List<RuleViolation>());

        var activeElements = elements.Where(e => !e.Attributes.Any(a => 
            a.Contains("SuppressRoslynGuard", StringComparison.OrdinalIgnoreCase) && 
            a.Contains(rule.Id, StringComparison.OrdinalIgnoreCase))).ToList();

        var violations = rule.Category switch
        {
            RuleCategory.LayerDependency => CheckLayerDependencies(rule, activeElements),
            RuleCategory.NamingConvention => CheckNamingConventions(rule, activeElements),
            RuleCategory.AsyncPattern => CheckAsyncPatterns(rule, activeElements),
            RuleCategory.NullSafety => CheckNullSafety(rule, activeElements),
            _ => new List<RuleViolation>()
        };

        return Task.FromResult(violations);
    }

    /// <summary>
    /// Executes all enabled rules against code elements.
    /// </summary>
    public async Task<List<RuleViolation>> ExecuteAllRulesAsync(List<CodeElement> elements)
    {
        if (elements is null || !elements.Any())
            return new List<RuleViolation>();

        var violations = new List<RuleViolation>();
        var enabledRules = _ruleRegistry.GetEnabledRules();

        foreach (var rule in enabledRules)
        {
            var ruleViolations = await ExecuteRuleAsync(rule, elements);
            violations.AddRange(ruleViolations);
        }

        return violations;
    }

    /// <summary>
    /// Checks for layer dependency violations.
    /// </summary>
    private List<RuleViolation> CheckLayerDependencies(AnalysisRule rule, List<CodeElement> elements)
    {
        var violations = new List<RuleViolation>();

        var layerPatterns = new[]
        {
            (AnalyzerConstants.LayerPatterns.RepositoryLayerSuffix, 0),
            (AnalyzerConstants.LayerPatterns.ServiceLayerSuffix, 1),
            (AnalyzerConstants.LayerPatterns.ControllerLayerSuffix, 2)
        };

        foreach (var element in elements.Where(e => e.IsContainer()))
        {
            var elementLayer = GetElementLayer(element, layerPatterns);
            if (elementLayer < 0) continue;

            foreach (var dependency in element.Dependencies)
            {
                if (dependency.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase)) continue;

                var dependencyLayer = elements
                    .FirstOrDefault(e => e.Name == dependency || e.FullyQualifiedName == dependency)?
                    .GetFullyQualifiedName();

                if (dependencyLayer is null) continue;

                var depLayer = GetElementLayer(
                    elements.First(e => e.FullyQualifiedName == dependencyLayer),
                    layerPatterns);

                if (depLayer < 0) continue;

                // Repositories can't depend on services or controllers
                if (elementLayer == 0 && (depLayer == 1 || depLayer == 2))
                {
                    violations.Add(new RuleViolation(
                        rule.Id,
                        rule.Name,
                        $"Repository '{element.Name}' depends on layer '{dependency}' (illegal dependency)",
                        element.FilePath)
                    {
                        LineNumber = element.StartLineNumber,
                        Severity = rule.DefaultSeverity,
                        Category = rule.Category
                    });
                }
            }
        }

        return violations;
    }

    /// <summary>
    /// Checks for naming convention violations.
    /// </summary>
    private List<RuleViolation> CheckNamingConventions(AnalysisRule rule, List<CodeElement> elements)
    {
        var violations = new List<RuleViolation>();

        foreach (var element in elements)
        {
            var issues = ValidateNaming(element);

            foreach (var issue in issues)
            {
                violations.Add(new RuleViolation(
                    rule.Id,
                    rule.Name,
                    issue,
                    element.FilePath)
                {
                    LineNumber = element.StartLineNumber,
                    Severity = rule.DefaultSeverity,
                    Category = rule.Category
                });
            }
        }

        return violations;
    }

    /// <summary>
    /// Checks for async pattern violations.
    /// </summary>
    private List<RuleViolation> CheckAsyncPatterns(AnalysisRule rule, List<CodeElement> elements)
    {
        var violations = new List<RuleViolation>();

        foreach (var element in elements.Where(e => e.ElementType == CodeElementType.Method))
        {
            // Methods returning Task should be async
            if (element.ReturnType?.Contains("Task", StringComparison.OrdinalIgnoreCase) == true
                && !element.IsAsync)
            {
                violations.Add(new RuleViolation(
                    rule.Id,
                    rule.Name,
                    $"Method '{element.Name}' returns Task but is not marked as async",
                    element.FilePath)
                {
                    LineNumber = element.StartLineNumber,
                    Severity = rule.DefaultSeverity,
                    Category = rule.Category
                });
            }

            // Async methods should end with "Async" suffix
            if (element.IsAsync && !element.Name.EndsWith(AnalyzerConstants.Naming.AsyncSuffix))
            {
                violations.Add(new RuleViolation(
                    rule.Id,
                    rule.Name,
                    $"Async method '{element.Name}' should end with '{AnalyzerConstants.Naming.AsyncSuffix}' suffix",
                    element.FilePath)
                {
                    LineNumber = element.StartLineNumber,
                    Severity = rule.DefaultSeverity,
                    Category = rule.Category
                });
            }
        }

        return violations;
    }

    /// <summary>
    /// Checks for null safety violations.
    /// </summary>
    private List<RuleViolation> CheckNullSafety(AnalysisRule rule, List<CodeElement> elements)
    {
        var violations = new List<RuleViolation>();

        foreach (var element in elements)
        {
            // Check if non-nullable types are properly handled
            if (element.ElementType == CodeElementType.Property || element.ElementType == CodeElementType.Field)
            {
                if (!string.IsNullOrEmpty(element.ReturnType) && !element.ReturnType.Contains("?", StringComparison.Ordinal))
                {
                    // Non-nullable type without proper initialization detection
                    // Simplified check - would need deeper semantic analysis in production
                }
            }
        }

        return violations;
    }

    /// <summary>
    /// Validates naming conventions for an element.
    /// </summary>
    private List<string> ValidateNaming(CodeElement element)
    {
        var issues = new List<string>();

        return element.ElementType switch
        {
            CodeElementType.Interface when !element.Name.StartsWith(AnalyzerConstants.Naming.InterfacePrefix) =>
                new() { $"Interface '{element.Name}' should start with '{AnalyzerConstants.Naming.InterfacePrefix}'" },

            CodeElementType.Method when !char.IsUpper(element.Name[0]) =>
                new() { $"Method '{element.Name}' should use PascalCase naming" },

            CodeElementType.Property when !char.IsUpper(element.Name[0]) =>
                new() { $"Property '{element.Name}' should use PascalCase naming" },

            CodeElementType.Field when !element.IsPublic && !element.Name.StartsWith(AnalyzerConstants.Naming.PrivateFieldPrefix) =>
                new() { $"Private field '{element.Name}' should start with '{AnalyzerConstants.Naming.PrivateFieldPrefix}'" },

            _ => issues
        };
    }

    /// <summary>
    /// Determines the architectural layer of an element based on naming patterns.
    /// </summary>
    private int GetElementLayer(CodeElement element, (string suffix, int layer)[] patterns)
    {
        foreach (var (suffix, layer) in patterns)
        {
            if (element.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return layer;
        }

        return -1;
    }
}
