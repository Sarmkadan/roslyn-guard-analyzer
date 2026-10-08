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
    private const string TestProjectSuffix = ".Tests";
    private const string GuardSkipCommentPrefix = "// ";
    private const string DiagnosticSeverityKey = ".severity";
    private const string DotNetDiagnosticPrefix = "dotnet_diagnostic";
    private const string TaskReturnType = "Task";
    private const string NullableAnnotation = "?";
    private const string SeverityNone = "none";
    private const string SeverityError = "error";
    private const string SeverityWarning = "warning";
    private const string SeveritySuggestion = "suggestion";
    private const string SeverityInfo = "info";
    private const string PropertyKind = "Property";
    private const string FieldKind = "Field";
    private const string PascalCaseNaming = "PascalCase";
    private const int LineIndexAdjustment = 2;
    private const int MinLineNumberForPreviousLine = 2;
    private const string IllegalDependencyMessage = "Repository '{0}' depends on layer '{1}' (illegal dependency)";
    private const string MethodReturnsTaskNotAsyncMessage = "Method '{0}' returns Task but is not marked as async";
    private const string AsyncMethodShouldEndWithSuffixMessage = "Async method '{0}' should end with '{1}' suffix";
    private const string ElementNotNullableAnnotatedMessage = "{0} '{1}' of reference type '{2}' is not nullable-annotated; mark it as nullable ('{3}') or ensure it is always initialized to a non-null value";
    private const string InterfaceShouldStartWithPrefixMessage = "Interface '{0}' should start with '{1}'";
    private const string MethodShouldUsePascalCaseMessage = "Method '{0}' should use {1} naming";
    private const string PropertyShouldUsePascalCaseMessage = "Property '{0}' should use {1} naming";
    private const string PrivateFieldShouldStartWithPrefixMessage = "Private field '{0}' should start with '{1}'";

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
                if (dependency.EndsWith(TestProjectSuffix, StringComparison.OrdinalIgnoreCase)) continue;

                var dependencyElement = elements
                    .FirstOrDefault(e => e.Name == dependency || e.GetFullyQualifiedName() == dependency);

                if (dependencyElement is null) continue;

                var depLayer = GetElementLayer(dependencyElement, layerPatterns);

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
                            string.Format(IllegalDependencyMessage, element.Name, dependency),
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

    // These caches are read and written from Parallel.ForEachAsync in
    // ExecuteAllRulesAsync, so they must be thread-safe.
    private readonly ConcurrentDictionary<string, string[]> _editorConfigCache = new();
    private readonly ConcurrentDictionary<string, string[]> _fileLineCache = new();

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
        if (string.IsNullOrEmpty(element.FilePath) || element.StartLineNumber <= MinLineNumberForPreviousLine || !System.IO.File.Exists(element.FilePath))
            return false;

        var lines = _fileLineCache.GetOrAdd(element.FilePath, static path => System.IO.File.ReadAllLines(path));

        var prevLineIndex = element.StartLineNumber - LineIndexAdjustment; // convert 1-based to 0-based, then go back one line
        if (prevLineIndex < 0 || prevLineIndex >= lines.Length)
            return false;

        var prevLine = lines[prevLineIndex].Trim();

        return prevLine.Equals($"{GuardSkipCommentPrefix}{AnalyzerConstants.Suppression.GuardSkipAll}", StringComparison.OrdinalIgnoreCase) ||
               prevLine.StartsWith($"{GuardSkipCommentPrefix}{AnalyzerConstants.Suppression.GuardSkipPrefix}{ruleId}", StringComparison.OrdinalIgnoreCase);
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
                var lines = _editorConfigCache.GetOrAdd(editorConfigPath, static path => System.IO.File.ReadAllLines(path));

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith($"{DotNetDiagnosticPrefix}.{rule.Id}{DiagnosticSeverityKey}", StringComparison.OrdinalIgnoreCase))
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
            if (element.ReturnType?.Contains(TaskReturnType, StringComparison.OrdinalIgnoreCase) == true
                && !element.IsAsync)
            {
                var sev = GetSeverity(rule, element.FilePath);
                if (sev.HasValue)
                {
                    violations.Add(new RuleViolation(
                        rule.Id,
                        rule.Name,
                        string.Format(MethodReturnsTaskNotAsyncMessage, element.Name),
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
                        string.Format(AsyncMethodShouldEndWithSuffixMessage, element.Name, AnalyzerConstants.Naming.AsyncSuffix),
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

            var kind = element.ElementType == CodeElementType.Property ? PropertyKind : FieldKind;
            violations.Add(new RuleViolation(
                rule.Id,
                rule.Name,
                string.Format(ElementNotNullableAnnotatedMessage, kind, element.Name, element.ReturnType, NullableAnnotation),
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
                new() { string.Format(InterfaceShouldStartWithPrefixMessage, element.Name, AnalyzerConstants.Naming.InterfacePrefix) },

            CodeElementType.Method when !char.IsUpper(element.Name[0]) =>
                new() { string.Format(MethodShouldUsePascalCaseMessage, element.Name, PascalCaseNaming) },

            CodeElementType.Property when !char.IsUpper(element.Name[0]) =>
                new() { string.Format(PropertyShouldUsePascalCaseMessage, element.Name, PascalCaseNaming) },

            CodeElementType.Field when !element.IsPublic && !element.Name.StartsWith(AnalyzerConstants.Naming.PrivateFieldPrefix) =>
                new() { string.Format(PrivateFieldShouldStartWithPrefixMessage, element.Name, AnalyzerConstants.Naming.PrivateFieldPrefix) },

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
