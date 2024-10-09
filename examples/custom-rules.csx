#!/usr/bin/env dotnet

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
// Example: Implementing Custom Rules
// This script demonstrates how to create and register custom analysis rules
// that extend the default rule set with organization-specific patterns.
// =============================================================================

#r "nuget: Microsoft.Extensions.DependencyInjection, 10.0.0"

using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;
using Microsoft.Extensions.DependencyInjection;

// Example 1: Simple custom rule - No TODO comments in production code
public class NoTodoCommentsRule : AnalysisRule
{
    public override string Id => "CUSTOM001";
    public override string Category => "Code Quality";
    public override string Description => "Prevents TODO comments in production code";
    public override RuleSeverity DefaultSeverity => RuleSeverity.Warning;

    public override async Task<IEnumerable<RuleViolation>> ValidateAsync(
        CodeElement element,
        RuleConfiguration config)
    {
        var violations = new List<RuleViolation>();

        if (element.SourceCode?.Contains("TODO", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Find the line with TODO
            var lines = element.SourceCode.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("TODO", StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add(new RuleViolation
                    {
                        RuleId = Id,
                        FilePath = element.FilePath,
                        Line = element.Line + i,
                        Column = lines[i].IndexOf("TODO", StringComparison.OrdinalIgnoreCase) + 1,
                        Message = "TODO comment found in production code - should be completed or tracked in issue tracker",
                        Severity = config.Severity,
                        Suggestion = "Create a GitHub issue instead of leaving TODO comments"
                    });
                }
            }
        }

        return violations;
    }
}

// Example 2: Dependency validation - No circular dependencies
public class NoCircularDependenciesRule : AnalysisRule
{
    public override string Id => "CUSTOM002";
    public override string Category => "Architecture";
    public override string Description => "Prevents circular dependencies between modules";
    public override RuleSeverity DefaultSeverity => RuleSeverity.Error;

    public override async Task<IEnumerable<RuleViolation>> ValidateAsync(
        CodeElement element,
        RuleConfiguration config)
    {
        var violations = new List<RuleViolation>();

        // Check if any of this element's dependencies depend back on it
        var isCircular = element.Dependencies.Any(dep =>
            element.Name.EndsWith("Service") &&
            dep.Contains("Service") &&
            dep.StartsWith("I") // Assuming interfaces mark service contracts
        );

        if (isCircular)
        {
            violations.Add(new RuleViolation
            {
                RuleId = Id,
                FilePath = element.FilePath,
                Line = element.Line,
                Column = 1,
                Message = $"Potential circular dependency detected in {element.Name}",
                Severity = config.Severity,
                Suggestion = "Refactor dependencies to remove cycles - consider using dependency inversion"
            });
        }

        return violations;
    }
}

// Example 3: Performance rule - No large method complexity
public class LargeMethodComplexityRule : AnalysisRule
{
    public override string Id => "CUSTOM003";
    public override string Category => "Performance";
    public override string Description => "Flags methods with high cyclomatic complexity";
    public override RuleSeverity DefaultSeverity => RuleSeverity.Warning;

    public override async Task<IEnumerable<RuleViolation>> ValidateAsync(
        CodeElement element,
        RuleConfiguration config)
    {
        var violations = new List<RuleViolation>();

        if (element.ElementType != CodeElementType.Method)
            return violations;

        var maxComplexity = config.GetOption<int>("maxComplexity") ?? 10;

        // Rough estimate of complexity by counting control flow keywords
        if (element.SourceCode != null)
        {
            var complexity = 1; // Base complexity
            complexity += CountOccurrences(element.SourceCode, "if");
            complexity += CountOccurrences(element.SourceCode, "else");
            complexity += CountOccurrences(element.SourceCode, "switch");
            complexity += CountOccurrences(element.SourceCode, "case");
            complexity += CountOccurrences(element.SourceCode, "for");
            complexity += CountOccurrences(element.SourceCode, "foreach");
            complexity += CountOccurrences(element.SourceCode, "while");
            complexity += CountOccurrences(element.SourceCode, "&&");
            complexity += CountOccurrences(element.SourceCode, "||");

            if (complexity > maxComplexity)
            {
                violations.Add(new RuleViolation
                {
                    RuleId = Id,
                    FilePath = element.FilePath,
                    Line = element.Line,
                    Column = 1,
                    Message = $"Method {element.Name} has high complexity: {complexity} (max: {maxComplexity})",
                    Severity = config.Severity,
                    Suggestion = "Consider breaking this method into smaller, more focused methods"
                });
            }
        }

        return violations;
    }

    private static int CountOccurrences(string text, string pattern)
    {
        return text.Split(new[] { pattern }, StringSplitOptions.None).Length - 1;
    }
}

// Usage example
var services = new ServiceCollection();
services.RegisterAnalyzerServices();

// Register custom rules
services.AddSingleton<AnalysisRule, NoTodoCommentsRule>();
services.AddSingleton<AnalysisRule, NoCircularDependenciesRule>();
services.AddSingleton<AnalysisRule, LargeMethodComplexityRule>();

var provider = services.BuildServiceProvider();
var ruleRegistry = provider.GetRequiredService<IRuleRegistry>();

Console.WriteLine("=== Custom Rules Registration Example ===");
Console.WriteLine();

// Verify custom rules are registered
var allRules = ruleRegistry.GetAllRules();
Console.WriteLine($"Total rules registered: {allRules.Count()}");
Console.WriteLine();

foreach (var rule in allRules.Where(r => r.Id.StartsWith("CUSTOM")))
{
    Console.WriteLine($"Rule: {rule.Id} ({rule.Category})");
    Console.WriteLine($"Description: {rule.Description}");
    Console.WriteLine($"Default Severity: {rule.DefaultSeverity}");
    Console.WriteLine();
}

Console.WriteLine("Custom rules are now available for analysis!");
