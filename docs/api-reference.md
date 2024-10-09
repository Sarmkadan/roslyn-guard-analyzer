# API Reference

Complete API documentation for Roslyn Guard Analyzer's public interfaces and classes.

## Core Interfaces

### IAnalysisService

Main service for orchestrating analysis operations.

```csharp
namespace RoslynGuardAnalyzer.Services;

public interface IAnalysisService
{
    /// <summary>
    /// Analyzes a project or individual file asynchronously.
    /// </summary>
    /// <param name="projectPath">
    /// Path to project.csproj file or directory containing .cs files
    /// </param>
    /// <returns>
    /// AnalysisResult containing all violations found
    /// </returns>
    /// <exception cref="RoslynGuardException">
    /// Thrown when analysis fails due to configuration or I/O errors
    /// </exception>
    Task<AnalysisResult> AnalyzeProjectAsync(string projectPath);

    /// <summary>
    /// Analyzes a project with custom configuration.
    /// </summary>
    /// <param name="projectPath">Path to analyze</param>
    /// <param name="configuration">Custom rule configuration</param>
    /// <returns>Analysis results</returns>
    Task<AnalysisResult> AnalyzeWithConfigAsync(
        string projectPath,
        RuleConfiguration configuration);
}
```

### IRuleRegistry

Manages rule registration and configuration.

```csharp
namespace RoslynGuardAnalyzer.Services;

public interface IRuleRegistry
{
    /// <summary>
    /// Gets all registered rules.
    /// </summary>
    /// <returns>Enumerable of AnalysisRule instances</returns>
    IEnumerable<AnalysisRule> GetAllRules();

    /// <summary>
    /// Gets enabled rules only.
    /// </summary>
    /// <returns>Enumerable of enabled rules</returns>
    IEnumerable<AnalysisRule> GetEnabledRules();

    /// <summary>
    /// Registers a new rule.
    /// </summary>
    /// <param name="rule">Rule instance to register</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if rule with same ID already registered
    /// </exception>
    void RegisterRule(AnalysisRule rule);

    /// <summary>
    /// Gets a specific rule by ID.
    /// </summary>
    /// <param name="ruleId">The rule ID (e.g., "LYR001")</param>
    /// <returns>Rule instance or null if not found</returns>
    AnalysisRule? GetRule(string ruleId);

    /// <summary>
    /// Enables or disables a rule.
    /// </summary>
    /// <param name="ruleId">Rule ID to modify</param>
    /// <param name="enabled">Whether rule should be enabled</param>
    void SetRuleEnabled(string ruleId, bool enabled);

    /// <summary>
    /// Unregisters a rule.
    /// </summary>
    /// <param name="ruleId">Rule ID to remove</param>
    /// <returns>True if removed, false if not found</returns>
    bool UnregisterRule(string ruleId);
}
```

### IRuleEngine

Executes rules against code elements.

```csharp
namespace RoslynGuardAnalyzer.Services;

public interface IRuleEngine
{
    /// <summary>
    /// Executes all enabled rules against a code element.
    /// </summary>
    /// <param name="element">Code element to analyze</param>
    /// <returns>Violations found by all rules</returns>
    Task<IEnumerable<RuleViolation>> ExecuteRulesAsync(
        CodeElement element);

    /// <summary>
    /// Executes a specific rule.
    /// </summary>
    /// <param name="ruleId">ID of rule to execute</param>
    /// <param name="element">Code element to analyze</param>
    /// <returns>Violations found by specified rule</returns>
    /// <exception cref="RoslynGuardException">
    /// Thrown if rule not found
    /// </exception>
    Task<IEnumerable<RuleViolation>> ExecuteRuleAsync(
        string ruleId,
        CodeElement element);

    /// <summary>
    /// Executes multiple specific rules.
    /// </summary>
    /// <param name="ruleIds">IDs of rules to execute</param>
    /// <param name="element">Code element to analyze</param>
    /// <returns>Combined violations from all rules</returns>
    Task<IEnumerable<RuleViolation>> ExecuteRulesAsync(
        IEnumerable<string> ruleIds,
        CodeElement element);
}
```

### IReportingService

Generates formatted reports from analysis results.

```csharp
namespace RoslynGuardAnalyzer.Services;

public interface IReportingService
{
    /// <summary>
    /// Generates a human-readable text report.
    /// </summary>
    /// <param name="result">Analysis results</param>
    /// <returns>Formatted text report</returns>
    string GenerateReport(AnalysisResult result);

    /// <summary>
    /// Generates a JSON report for tool integration.
    /// </summary>
    /// <param name="result">Analysis results</param>
    /// <returns>JSON-formatted report</returns>
    string GenerateJsonReport(AnalysisResult result);

    /// <summary>
    /// Generates a CSV report for spreadsheet analysis.
    /// </summary>
    /// <param name="result">Analysis results</param>
    /// <returns>CSV-formatted report</returns>
    string GenerateCsvReport(AnalysisResult result);

    /// <summary>
    /// Generates an HTML report for viewing in browser.
    /// </summary>
    /// <param name="result">Analysis results</param>
    /// <returns>HTML-formatted report</returns>
    string GenerateHtmlReport(AnalysisResult result);

    /// <summary>
    /// Generates report in specified format.
    /// </summary>
    /// <param name="result">Analysis results</param>
    /// <param name="format">Output format (text, json, csv, xml, html)</param>
    /// <returns>Formatted report</returns>
    string GenerateReport(AnalysisResult result, string format);
}
```

### IValidationService

Validates configurations and code elements.

```csharp
namespace RoslynGuardAnalyzer.Services;

public interface IValidationService
{
    /// <summary>
    /// Validates a rule configuration.
    /// </summary>
    /// <param name="config">Configuration to validate</param>
    /// <returns>List of validation errors (empty if valid)</returns>
    IEnumerable<string> ValidateConfiguration(RuleConfiguration config);

    /// <summary>
    /// Validates a code element.
    /// </summary>
    /// <param name="element">Code element to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool IsValidCodeElement(CodeElement element);

    /// <summary>
    /// Gets detailed validation errors for code element.
    /// </summary>
    /// <param name="element">Code element to validate</param>
    /// <returns>List of validation errors</returns>
    IEnumerable<string> ValidateCodeElement(CodeElement element);
}
```

## Domain Models

### AnalysisRule

Abstract base class for implementing analysis rules.

```csharp
namespace RoslynGuardAnalyzer.Domain.Models;

public abstract class AnalysisRule
{
    /// <summary>
    /// Unique identifier for this rule (e.g., "LYR001")
    /// </summary>
    public abstract string Id { get; }

    /// <summary>
    /// Category for grouping related rules
    /// </summary>
    public abstract string Category { get; }

    /// <summary>
    /// Human-readable description of rule purpose
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Default severity level for violations
    /// </summary>
    public virtual RuleSeverity DefaultSeverity => RuleSeverity.Error;

    /// <summary>
    /// Whether this rule is currently enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Executes the rule against a code element.
    /// </summary>
    /// <param name="element">Code element to analyze</param>
    /// <param name="config">Rule configuration</param>
    /// <returns>Violations found</returns>
    public abstract Task<IEnumerable<RuleViolation>> ValidateAsync(
        CodeElement element,
        RuleConfiguration config);
}
```

### RuleViolation

Represents a single violation instance.

```csharp
namespace RoslynGuardAnalyzer.Domain.Models;

public class RuleViolation
{
    /// <summary>
    /// ID of the rule that detected this violation
    /// </summary>
    public string RuleId { get; set; } = string.Empty;

    /// <summary>
    /// Path to file containing violation
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Line number (1-based)
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// Column number (1-based)
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// Human-readable violation description
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Severity level of violation
    /// </summary>
    public RuleSeverity Severity { get; set; }

    /// <summary>
    /// The code element that triggered this violation
    /// </summary>
    public CodeElement? Element { get; set; }

    /// <summary>
    /// Suggested fix or remediation (optional)
    /// </summary>
    public string? Suggestion { get; set; }
}
```

### CodeElement

Represents a code artifact being analyzed.

```csharp
namespace RoslynGuardAnalyzer.Domain.Models;

public class CodeElement
{
    /// <summary>
    /// Name of the code element (class, method, field, etc.)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of code element
    /// </summary>
    public CodeElementType ElementType { get; set; }

    /// <summary>
    /// Full file path
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Line number where element is defined
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// Column number where element is defined
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// Fully qualified name (namespace.typename)
    /// </summary>
    public string? FullyQualifiedName { get; set; }

    /// <summary>
    /// Access modifier (public, private, protected, internal)
    /// </summary>
    public string? AccessModifier { get; set; }

    /// <summary>
    /// Names of types this element depends on
    /// </summary>
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// Attributes applied to this element
    /// </summary>
    public List<string> Attributes { get; set; } = new();

    /// <summary>
    /// Raw source code of element
    /// </summary>
    public string? SourceCode { get; set; }
}
```

### AnalysisResult

Complete results from an analysis execution.

```csharp
namespace RoslynGuardAnalyzer.Domain.Models;

public class AnalysisResult
{
    /// <summary>
    /// Path to analyzed project
    /// </summary>
    public string ProjectPath { get; set; } = string.Empty;

    /// <summary>
    /// When analysis completed (UTC)
    /// </summary>
    public DateTime TimestampUtc { get; set; }

    /// <summary>
    /// Total number of C# files analyzed
    /// </summary>
    public int TotalFilesAnalyzed { get; set; }

    /// <summary>
    /// All violations found
    /// </summary>
    public List<RuleViolation> Violations { get; set; } = new();

    /// <summary>
    /// Total number of violations
    /// </summary>
    public int ViolationCount => Violations.Count;

    /// <summary>
    /// Violations grouped by severity
    /// </summary>
    public IReadOnlyDictionary<RuleSeverity, int> ViolationsBySeverity
    {
        get => Violations
            .GroupBy(v => v.Severity)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Analysis statistics and metrics
    /// </summary>
    public AnalysisStatistics? Statistics { get; set; }
}
```

### RuleConfiguration

Configuration for a specific rule.

```csharp
namespace RoslynGuardAnalyzer.Domain.Models;

public class RuleConfiguration
{
    /// <summary>
    /// Whether this rule is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Severity level for violations
    /// </summary>
    public RuleSeverity Severity { get; set; } = RuleSeverity.Error;

    /// <summary>
    /// Rule-specific configuration options
    /// </summary>
    public Dictionary<string, object> Options { get; set; } = new();

    /// <summary>
    /// Gets a configuration option by key
    /// </summary>
    public T? GetOption<T>(string key)
    {
        return Options.TryGetValue(key, out var value)
            ? (T?)value
            : default;
    }

    /// <summary>
    /// Sets a configuration option
    /// </summary>
    public void SetOption<T>(string key, T value)
    {
        Options[key] = value!;
    }
}
```

## Enums

### RuleSeverity

```csharp
namespace RoslynGuardAnalyzer.Core;

public enum RuleSeverity
{
    /// <summary>
    /// Lowest severity - informational only
    /// </summary>
    Info = 0,

    /// <summary>
    /// Warning severity - should be fixed but doesn't fail build
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error severity - should fail build/CI
    /// </summary>
    Error = 2,

    /// <summary>
    /// Critical severity - requires immediate attention
    /// </summary>
    Critical = 3
}
```

### CodeElementType

```csharp
namespace RoslynGuardAnalyzer.Core;

public enum CodeElementType
{
    Class,
    Interface,
    Struct,
    Enum,
    Record,
    Method,
    Property,
    Field,
    Event,
    Constructor,
    Delegate,
    Namespace
}
```

## Exceptions

### RoslynGuardException

Base exception for all analyzer-specific errors.

```csharp
namespace RoslynGuardAnalyzer.Exceptions;

public class RoslynGuardException : Exception
{
    public RoslynGuardException(string message)
        : base(message) { }

    public RoslynGuardException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

### Derived Exceptions

```csharp
// Configuration errors
public class ConfigurationException : RoslynGuardException { }

// Analysis execution errors
public class AnalysisException : RoslynGuardException { }

// Rule execution errors
public class RuleExecutionException : RoslynGuardException { }

// Report formatting errors
public class FormattingException : RoslynGuardException { }

// Validation errors
public class ValidationException : RoslynGuardException { }
```

## Extension Points

### Implementing Custom Rules

```csharp
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;

public class MyCustomRule : AnalysisRule
{
    public override string Id => "CUSTOM001";
    public override string Category => "Custom";
    public override string Description => "My custom architectural rule";
    public override RuleSeverity DefaultSeverity => RuleSeverity.Warning;

    public override async Task<IEnumerable<RuleViolation>> ValidateAsync(
        CodeElement element,
        RuleConfiguration config)
    {
        var violations = new List<RuleViolation>();

        // Example: Flag classes with more than 10 dependencies
        if (element.ElementType == CodeElementType.Class &&
            element.Dependencies.Count > 10)
        {
            violations.Add(new RuleViolation
            {
                RuleId = Id,
                FilePath = element.FilePath,
                Line = element.Line,
                Message = $"Class has {element.Dependencies.Count} dependencies (max 10)",
                Severity = config.Severity,
                Element = element
            });
        }

        return violations;
    }
}

// Register the rule
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<AnalysisRule, MyCustomRule>();
}
```

### Implementing Custom Formatters

```csharp
using RoslynGuardAnalyzer.Formatters;

public class YamlFormatter : IOutputFormatter
{
    public string Name => "yaml";

    public string Format(AnalysisResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("analysis:");
        sb.AppendLine($"  project: {result.ProjectPath}");
        sb.AppendLine($"  timestamp: {result.TimestampUtc:O}");
        sb.AppendLine($"  files_analyzed: {result.TotalFilesAnalyzed}");
        sb.AppendLine($"  violations: {result.ViolationCount}");
        sb.AppendLine("  items:");

        foreach (var violation in result.Violations)
        {
            sb.AppendLine($"    - rule_id: {violation.RuleId}");
            sb.AppendLine($"      file: {violation.FilePath}");
            sb.AppendLine($"      line: {violation.Line}");
            sb.AppendLine($"      message: {violation.Message}");
        }

        return sb.ToString();
    }
}

// Register the formatter
var registry = new FormatterRegistry();
registry.Register(new YamlFormatter());
```

## Usage Examples

### Example 1: Basic Analysis

```csharp
var services = new ServiceCollection();
services.RegisterAnalyzerServices();
var provider = services.BuildServiceProvider();

var analysisService = provider.GetRequiredService<IAnalysisService>();
var result = await analysisService.AnalyzeProjectAsync("./src");

Console.WriteLine($"Found {result.ViolationCount} violations");
foreach (var violation in result.Violations)
{
    Console.WriteLine($"{violation.RuleId}: {violation.Message}");
}
```

### Example 2: Custom Rule Registration

```csharp
var registry = provider.GetRequiredService<IRuleRegistry>();
registry.RegisterRule(new MyCustomRule());
registry.SetRuleEnabled("CUSTOM001", true);

var result = await analysisService.AnalyzeProjectAsync("./src");
```

### Example 3: Rule Configuration

```csharp
var config = new RuleConfiguration
{
    Enabled = true,
    Severity = RuleSeverity.Warning
};
config.SetOption("maxDependencies", 15);

var result = await analysisService.AnalyzeWithConfigAsync("./src", config);
```

## Versioning

API is versioned semantically:
- **Major**: Breaking changes to interfaces
- **Minor**: New interfaces/methods (backward compatible)
- **Patch**: Bug fixes and internal improvements

Current version: 1.2.0
