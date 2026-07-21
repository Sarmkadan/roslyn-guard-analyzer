#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Rules;

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
            a.Contains(rule.Id, StringComparison.OrdinalIgnoreCase)) &&
            !IsGuardSkipped(e, rule.Id)).ToList();

        if (rule is CustomAnalysisRule customRule)
            return customRule.EvaluateAsync(activeElements);

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
    /// Executes all enabled rules against code elements using parallel processing.
    /// </summary>
    public async Task<List<RuleViolation>> ExecuteAllRulesAsync(List<CodeElement> elements)
    {
        if (elements is null || !elements.Any())
            return new List<RuleViolation>();

        var enabledRules = _ruleRegistry.GetAllRules().Where(r => r.IsEnabled).ToList();

        if (!enabledRules.Any())
            return new List<RuleViolation>();

        // Use thread-safe collection for violations
        var violations = new ConcurrentBag<RuleViolation>();
        var exceptions = new ConcurrentBag<Exception>();

        // Process rules in parallel with bounded degree of parallelism
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = ParallelAnalysisConfig.MaxRuleParallelism
        };

        await Parallel.ForEachAsync(enabledRules, parallelOptions, async (rule, cancellationToken) =>
        {
            try
            {
                var ruleViolations = await ExecuteRuleAsync(rule, elements);
                foreach (var violation in ruleViolations)
                {
                    violations.Add(violation);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                exceptions.Add(new RuleExecutionException($"Failed to execute rule {rule.Id}", ex));
            }
        });

        // Log any rule execution exceptions
        foreach (var ex in exceptions)
        {
            Console.WriteLine($"Warning: {ex.Message}");
        }

        // Return violations in deterministic order (sorted by file path and line number)
        return violations
            .OrderBy(v => v.FilePath)
            .ThenBy(v => v.LineNumber)
            .ThenBy(v => v.RuleId)
            .ToList();
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
                    var sev = GetSeverity(rule, element.FilePath);
                    if (sev.HasValue)
                    {
                        violations.Add(new RuleViolation(
                            rule.Id,
                            rule.Name,
                            $"Repository '{element.Name}' depends on layer '{dependency}' (illegal dependency)",
                            element.FilePath)
                        {
                            LineNumber = element.StartLineNumber,
                            Severity = sev.Value,
                            Category = rule.Category
                        });
                    }
                }
            }
        }

        return violations;
    }

    private readonly Dictionary<string, string[]> _editorConfigCache = new();
    private readonly Dictionary<string, string[]> _fileLineCache = new();

    /// <summary>
    /// Checks whether a code element carries a GUARD_SKIP inline suppression directive
    /// for the given rule. Looks at:
    /// 1. <see cref="CodeElement.SuppressDirectives"/> set programmatically by parsers.
    /// 2. The line immediately preceding the element's declaration in its source file,
    /// which may contain <c>// GUARD_SKIP</c> (all rules) or
    /// <c>// GUARD_SKIP:RULE_ID</c> (specific rule).
    /// </summary>
    private bool IsGuardSkipped(CodeElement element, string ruleId)
    {
        // Check programmatically-set suppression directives first.
        if (element.SuppressDirectives.Any(d =>
            d.Equals(AnalyzerConstants.Suppression.GuardSkipAll, StringComparison.OrdinalIgnoreCase) ||
            d.Equals($"{AnalyzerConstants.Suppression.GuardSkipPrefix}{ruleId}", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Fall back to reading the source file for inline comment directives.
        if (string.IsNullOrEmpty(element.FilePath) || element.StartLineNumber <= 1 || !System.IO.File.Exists(element.FilePath))
            return false;

        if (!_fileLineCache.TryGetValue(element.FilePath, out var lines))
        {
            lines = System.IO.File.ReadAllLines(element.FilePath);
            _fileLineCache[element.FilePath] = lines;
        }

        var prevLineIndex = element.StartLineNumber - 2; // convert 1-based to 0-based, then go back one line
        if (prevLineIndex < 0 || prevLineIndex >= lines.Length)
            return false;

        var prevLine = lines[prevLineIndex].Trim();

        return prevLine.Equals($"// {AnalyzerConstants.Suppression.GuardSkipAll}", StringComparison.OrdinalIgnoreCase) ||
               prevLine.StartsWith($"// {AnalyzerConstants.Suppression.GuardSkipPrefix}{ruleId}", StringComparison.OrdinalIgnoreCase);
    }

    private SeverityLevel? GetSeverity(AnalysisRule rule, string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return rule.DefaultSeverity;

        var dir = System.IO.Path.GetDirectoryName(filePath);
        while (!string.IsNullOrEmpty(dir))
        {
            var editorConfigPath = System.IO.Path.Combine(dir, ".editorconfig");
            if (System.IO.File.Exists(editorConfigPath))
            {
                if (!_editorConfigCache.TryGetValue(editorConfigPath, out var lines))
                {
                    lines = System.IO.File.ReadAllLines(editorConfigPath);
                    _editorConfigCache[editorConfigPath] = lines;
                }

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith($"dotnet_diagnostic.{rule.Id}.severity", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = trimmed.Split('=');
                        if (parts.Length == 2)
                        {
                            var severityStr = parts[1].Trim().ToLowerInvariant();
                            if (severityStr == "none") return null;
                            if (severityStr == "error") return SeverityLevel.Error;
                            if (severityStr == "warning") return SeverityLevel.Warning;
                            if (severityStr == "suggestion" || severityStr == "info") return SeverityLevel.Info;
                        }
                    }
                }
            }
            dir = System.IO.Path.GetDirectoryName(dir);
        }
        return rule.DefaultSeverity;
    }

    private List<RuleViolation> CheckNamingConventions(AnalysisRule rule, List<CodeElement> elements)
    {
        var violations = new List<RuleViolation>();

        foreach (var element in elements)
        {
            var issues = ValidateNaming(element);

            var sev = GetSeverity(rule, element.FilePath);
            if (!sev.HasValue) continue;

            foreach (var issue in issues)
            {
                violations.Add(new RuleViolation(
                    rule.Id,
                    rule.Name,
                    issue,
                    element.FilePath)
                {
                    LineNumber = element.StartLineNumber,
                    Severity = sev.Value,
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
                var sev = GetSeverity(rule, element.FilePath);
                if (sev.HasValue)
                {
                    violations.Add(new RuleViolation(
                        rule.Id,
                        rule.Name,
                        $"Method '{element.Name}' returns Task but is not marked as async",
                        element.FilePath)
                    {
                        LineNumber = element.StartLineNumber,
                        Severity = sev.Value,
                        Category = rule.Category
                    });
                }
            }

            // Async methods should end with "Async" suffix
            if (element.IsAsync && !element.Name.EndsWith(AnalyzerConstants.Naming.AsyncSuffix))
            {
                var sev = GetSeverity(rule, element.FilePath);
                if (sev.HasValue)
                {
                    violations.Add(new RuleViolation(
                        rule.Id,
                        rule.Name,
                        $"Async method '{element.Name}' should end with '{AnalyzerConstants.Naming.AsyncSuffix}' suffix",
                        element.FilePath)
                    {
                        LineNumber = element.StartLineNumber,
                        Severity = sev.Value,
                        Category = rule.Category
                    });
                }
            }
        }

        return violations;
    }

    private static readonly HashSet<string> ValueTypeKeywords = new(StringComparer.Ordinal)
    {
        "bool", "byte", "sbyte", "char", "decimal", "double", "float", "int", "uint",
        "long", "ulong", "short", "ushort", "void", "nint", "nuint", "System.Guid", "Guid"
    };

    /// <summary>
    /// Checks for null safety violations. Flags public non-nullable reference-type properties and fields that are not
    /// value types, since they cannot be guaranteed to hold a non-null value without
    /// explicit initialization or nullable annotation.
    /// </summary>
    private List<RuleViolation> CheckNullSafety(AnalysisRule rule, List<CodeElement> elements)
    {
        var violations = new List<RuleViolation>();

        foreach (var element in elements)
        {
            if (element.ElementType != CodeElementType.Property && element.ElementType != CodeElementType.Field)
                continue;

            if (string.IsNullOrEmpty(element.ReturnType))
                continue;

            var baseType = element.ReturnType.TrimEnd('[', ']');

            if (baseType.Contains('?', StringComparison.Ordinal))
                continue;

            if (ValueTypeKeywords.Contains(baseType))
                continue;

            if (!element.IsPublic)
                continue;

            var sev = GetSeverity(rule, element.FilePath);
            if (!sev.HasValue)
                continue;

            var kind = element.ElementType == CodeElementType.Property ? "Property" : "Field";
            violations.Add(new RuleViolation(
                rule.Id,
                rule.Name,
                $"{kind} '{element.Name}' of reference type '{element.ReturnType}' is not nullable-annotated; " +
                "mark it as nullable ('?') or ensure it is always initialized to a non-null value",
                element.FilePath)
            {
                LineNumber = element.StartLineNumber,
                Severity = sev.Value,
                Category = rule.Category
            });
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

/// <summary>
/// Custom exception for rule execution failures
/// </summary>
public class RuleExecutionException : Exception
{
    public RuleExecutionException(string message, Exception innerException) : base(message, innerException) { }
}
