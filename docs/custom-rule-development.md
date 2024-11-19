# Custom Analyzer Rule Development Guide

This guide walks you through writing, configuring, registering, and testing your own architectural rules with Roslyn Guard Analyzer.

## Table of Contents

1. [Overview](#overview)
2. [Understanding the Rule Model](#understanding-the-rule-model)
3. [Writing Your First Rule](#writing-your-first-rule)
4. [Rule Categories](#rule-categories)
5. [Configuration Options](#configuration-options)
6. [Registering Your Rule](#registering-your-rule)
7. [Inline Suppression with GUARD_SKIP](#inline-suppression-with-guard_skip)
8. [Testing Your Rule](#testing-your-rule)
9. [Advanced Patterns](#advanced-patterns)
10. [Complete Example](#complete-example)

---

## Overview

Rules in Roslyn Guard Analyzer operate against **`CodeElement`** instances — lightweight snapshots of C# code artifacts (classes, methods, fields, etc.) collected during analysis. Each rule inspects those snapshots and returns zero or more **`RuleViolation`** objects.

The analysis pipeline is:

```
Source files
    ↓  (file scanner)
List<CodeElement>
    ↓  (RuleEngine)
List<RuleViolation>
    ↓  (formatters)
Report
```

---

## Understanding the Rule Model

### `AnalysisRule`

Every rule is an instance of `AnalysisRule` (a plain data class, not abstract). The key properties are:

| Property | Type | Description |
|---|---|---|
| `Id` | `string` | Unique short identifier, e.g. `"TEAM001"` |
| `Name` | `string` | Human-readable name |
| `Description` | `string` | What the rule enforces |
| `Category` | `RuleCategory` | Logical grouping (see below) |
| `DefaultSeverity` | `SeverityLevel` | `Info`, `Warning`, `Error`, or `Critical` |
| `IsEnabled` | `bool` | Whether the rule runs during analysis |
| `Configuration` | `Dictionary<string, object>` | Rule-specific options |

### `CodeElement`

Each element analyzed has:

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | Simple name of the element |
| `ElementType` | `CodeElementType` | `Class`, `Method`, `Property`, `Field`, etc. |
| `FilePath` | `string` | Absolute path to the source file |
| `StartLineNumber` | `int` | 1-based start line |
| `Namespace` | `string` | Containing namespace |
| `Attributes` | `List<string>` | Attribute names applied to the element |
| `SuppressDirectives` | `List<string>` | Inline `GUARD_SKIP` directives |
| `Dependencies` | `List<string>` | Types this element references |
| `IsAsync` | `bool` | Whether a method is async |
| `ReturnType` | `string?` | Return type for methods/properties |

### `RuleViolation`

A violation records:

| Property | Type | Description |
|---|---|---|
| `RuleId` | `string` | ID of the firing rule |
| `RuleName` | `string` | Name of the rule |
| `Message` | `string` | Explanation of the violation |
| `FilePath` | `string` | File containing the violation |
| `LineNumber` | `int` | Line number |
| `Severity` | `SeverityLevel` | Severity at time of violation |
| `Category` | `RuleCategory` | Category of the rule |

---

## Writing Your First Rule

Rules are created as `AnalysisRule` instances and their logic lives in the `RuleEngine`. The most ergonomic extension point is to **register a custom rule** and handle its category in a custom `RuleEngine` subclass, or to use the existing mechanism by mapping to an existing category.

Below is a self-contained example showing the full pattern:

### 1. Define the Rule

```csharp
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;

var maxDepsRule = new AnalysisRule("TEAM001", "Max Dependency Count", 
    "Classes must not exceed the configured maximum number of dependencies.", 
    RuleCategory.CodeStructure)
{
    DefaultSeverity = SeverityLevel.Warning,
    IsEnabled = true
};

// Store the limit as a typed configuration option.
maxDepsRule.SetConfigurationValue("maxDependencies", 10);
```

### 2. Implement the Validation Logic

```csharp
using RoslynGuardAnalyzer.Domain.Models;

List<RuleViolation> CheckMaxDependencies(AnalysisRule rule, List<CodeElement> elements)
{
    var max = rule.GetConfigurationValue<int>("maxDependencies", defaultValue: 10);
    var violations = new List<RuleViolation>();

    foreach (var element in elements.Where(e => e.ElementType == CodeElementType.Class))
    {
        if (element.Dependencies.Count > max)
        {
            violations.Add(new RuleViolation(
                rule.Id,
                rule.Name,
                $"Class '{element.Name}' has {element.Dependencies.Count} dependencies (limit: {max})",
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
```

---

## Rule Categories

Choose the category that best describes what your rule enforces:

| `RuleCategory` | Built-in example | When to use |
|---|---|---|
| `LayerDependency` | LYR001 | Boundaries between architectural layers |
| `NamingConvention` | NAM001 | Casing, prefixes, suffixes |
| `AsyncPattern` | ASY001 | `async`/`await` usage patterns |
| `NullSafety` | NUL001 | Nullable reference type handling |
| `CodeStructure` | — | Complexity, coupling, cohesion |

---

## Configuration Options

Rule-level options live in `AnalysisRule.Configuration` and are accessed with the typed helpers:

```csharp
// Writing options
rule.SetConfigurationValue("maxDependencies", 15);
rule.SetConfigurationValue("allowedPrefixes", new[] { "I", "Abstract" });

// Reading options (with default fallback)
int max = rule.GetConfigurationValue<int>("maxDependencies", defaultValue: 10);
string[]? prefixes = rule.GetConfigurationValue<string[]>("allowedPrefixes");
```

### `.roslyn-guard.json` Configuration

Per-project overrides are loaded from `.roslyn-guard.json` in the project root. Custom rules can be included by adding their IDs alongside the built-in ones:

```json
{
  "rules": {
    "TEAM001": {
      "enabled": true,
      "severity": "warning",
      "options": {
        "maxDependencies": 8
      }
    }
  }
}
```

---

## Registering Your Rule

Use `IRuleRegistry` to make the engine aware of your rule:

```csharp
using RoslynGuardAnalyzer.Services;

// Obtain the registry from the DI container.
var registry = serviceProvider.GetRequiredService<IRuleRegistry>();

registry.RegisterRule(maxDepsRule);

// Enable/disable at runtime.
registry.SetRuleEnabled("TEAM001", true);
```

### Registering via DI (Recommended)

For production setups, register rules inside your service configuration:

```csharp
using Microsoft.Extensions.DependencyInjection;
using RoslynGuardAnalyzer.Infrastructure;

var services = new ServiceCollection();
services.RegisterAnalyzerServices();

// Add your rules after the core services are registered.
services.AddSingleton(sp =>
{
    var registry = sp.GetRequiredService<IRuleRegistry>();
    registry.RegisterRule(maxDepsRule);
    return registry;
});
```

---

## Inline Suppression with GUARD_SKIP

Sometimes a specific location should be exempt from a rule without changing the global configuration. The `// GUARD_SKIP` comment directive lets you suppress violations inline.

### Suppress All Rules

Place `// GUARD_SKIP` on the line **immediately before** the declaration you want to exempt:

```csharp
// GUARD_SKIP
public class LegacyAdapter : IUserService, IOrderService, IProductService,
    IInventoryService, IShippingService, IPaymentService, // ...11 deps total
{
    // This class intentionally aggregates many dependencies.
}
```

### Suppress a Specific Rule

Use `// GUARD_SKIP:RULE_ID` to target a single rule while keeping all others active:

```csharp
// GUARD_SKIP:TEAM001
public class IntegrationFacade : IServiceA, IServiceB, IServiceC,
    IServiceD, IServiceE, IServiceF, IServiceG, IServiceH,
    IServiceI, IServiceJ, IServiceK
{
    // Over the dependency limit, but intentional for this facade.
}
```

### Programmatic Suppression

You can also set suppression directives on a `CodeElement` directly (useful in tests or custom parsers):

```csharp
var element = new CodeElement("LegacyAdapter", CodeElementType.Class, "src/Legacy.cs");

// Suppress all rules for this element.
element.SuppressDirectives.Add("GUARD_SKIP");

// Or suppress only a specific rule.
element.SuppressDirectives.Add("GUARD_SKIP:TEAM001");
```

### How It Works

The `RuleEngine` evaluates suppression before executing any rule check:

1. **Programmatic directives** — checks `CodeElement.SuppressDirectives`.
2. **Source-file comments** — reads the line at `StartLineNumber - 1` in the source file and matches `// GUARD_SKIP` or `// GUARD_SKIP:RULE_ID` (case-insensitive).

If either matches, the element is excluded from that rule's analysis entirely.

---

## Testing Your Rule

### Unit Test Setup

```csharp
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;
using RoslynGuardAnalyzer.Core;

[Fact]
public async Task MaxDependencies_ExceedsLimit_ReportsViolation()
{
    // Arrange
    var registry = new RuleRegistry();
    registry.RegisterRule(maxDepsRule);

    var engine = new RuleEngine(registry);

    var element = new CodeElement("HeavyClass", CodeElementType.Class, "src/Heavy.cs")
    {
        StartLineNumber = 10,
        EndLineNumber = 50
    };
    for (int i = 0; i < 15; i++)
        element.AddDependency($"Dep{i}");

    // Act
    var violations = await engine.ExecuteRuleAsync(maxDepsRule, new List<CodeElement> { element });

    // Assert
    Assert.Single(violations);
    Assert.Equal("TEAM001", violations[0].RuleId);
}
```

### Testing GUARD_SKIP Suppression

```csharp
[Fact]
public async Task MaxDependencies_WithGuardSkip_NoViolation()
{
    var registry = new RuleRegistry();
    registry.RegisterRule(maxDepsRule);
    var engine = new RuleEngine(registry);

    var element = new CodeElement("HeavyClass", CodeElementType.Class, "src/Heavy.cs")
    {
        StartLineNumber = 10,
        EndLineNumber = 50
    };
    for (int i = 0; i < 15; i++)
        element.AddDependency($"Dep{i}");

    // Suppress all rules via programmatic directive.
    element.SuppressDirectives.Add("GUARD_SKIP");

    var violations = await engine.ExecuteRuleAsync(maxDepsRule, new List<CodeElement> { element });

    Assert.Empty(violations);
}

[Fact]
public async Task MaxDependencies_WithGuardSkipSpecificRule_NoViolation()
{
    var registry = new RuleRegistry();
    registry.RegisterRule(maxDepsRule);
    var engine = new RuleEngine(registry);

    var element = new CodeElement("HeavyClass", CodeElementType.Class, "src/Heavy.cs")
    {
        StartLineNumber = 10,
        EndLineNumber = 50
    };
    for (int i = 0; i < 15; i++)
        element.AddDependency($"Dep{i}");

    // Suppress only TEAM001.
    element.SuppressDirectives.Add("GUARD_SKIP:TEAM001");

    var violations = await engine.ExecuteRuleAsync(maxDepsRule, new List<CodeElement> { element });

    Assert.Empty(violations);
}
```

---

## Advanced Patterns

### Accessing Rule Options Inside Checks

Pass the rule's `Configuration` values into your check method to make limits configurable:

```csharp
List<RuleViolation> CheckMethodLength(AnalysisRule rule, List<CodeElement> elements)
{
    int maxLines = rule.GetConfigurationValue<int>("maxLines", defaultValue: 50);
    var violations = new List<RuleViolation>();

    foreach (var element in elements.Where(e => e.ElementType == CodeElementType.Method))
    {
        int lineCount = element.EndLineNumber - element.StartLineNumber + 1;
        if (lineCount > maxLines)
        {
            violations.Add(new RuleViolation(
                rule.Id, rule.Name,
                $"Method '{element.Name}' is {lineCount} lines long (limit: {maxLines})",
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
```

### Checking Attribute Presence

Use `CodeElement.HasAttribute` to require or forbid specific attributes:

```csharp
// Enforce that all public API controller methods carry [Authorize].
foreach (var method in elements.Where(e =>
    e.ElementType == CodeElementType.Method &&
    e.IsPublic &&
    e.ParentName?.EndsWith("Controller", StringComparison.Ordinal) == true))
{
    if (!method.HasAttribute("Authorize"))
    {
        violations.Add(new RuleViolation(
            rule.Id, rule.Name,
            $"Public controller method '{method.Name}' is missing [Authorize]",
            method.FilePath)
        {
            LineNumber = method.StartLineNumber,
            Severity = SeverityLevel.Error,
            Category = RuleCategory.CodeStructure
        });
    }
}
```

### Combining with `SuppressRoslynGuardAttribute`

Your custom rules automatically respect the existing `[SuppressRoslynGuard("TEAM001")]` attribute because the `RuleEngine` filters elements carrying that attribute before executing any rule.

---

## Complete Example

Below is a full working example of a custom rule that forbids `public` fields in non-DTO classes.

```csharp
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;

// 1. Create the rule.
var noPublicFieldsRule = new AnalysisRule(
    "TEAM002",
    "No Public Fields",
    "Public fields are not allowed outside DTO classes. Use properties instead.",
    RuleCategory.CodeStructure)
{
    DefaultSeverity = SeverityLevel.Warning,
    IsEnabled = true
};

// 2. Implement check logic.
List<RuleViolation> CheckNoPublicFields(AnalysisRule rule, List<CodeElement> elements)
{
    var violations = new List<RuleViolation>();

    foreach (var element in elements.Where(e =>
        e.ElementType == CodeElementType.Field &&
        e.IsPublic))
    {
        // Skip DTO classes (convention: names ending with "Dto" or "Request"/"Response").
        var parentName = element.ParentName ?? string.Empty;
        if (parentName.EndsWith("Dto", StringComparison.OrdinalIgnoreCase) ||
            parentName.EndsWith("Request", StringComparison.OrdinalIgnoreCase) ||
            parentName.EndsWith("Response", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        violations.Add(new RuleViolation(
            rule.Id, rule.Name,
            $"Public field '{element.Name}' in '{parentName}' should be a property.",
            element.FilePath)
        {
            LineNumber = element.StartLineNumber,
            Severity = rule.DefaultSeverity,
            Category = rule.Category
        });
    }

    return violations;
}

// 3. Register and use.
var registry = serviceProvider.GetRequiredService<IRuleRegistry>();
registry.RegisterRule(noPublicFieldsRule);

var analysisService = serviceProvider.GetRequiredService<IAnalysisService>();
var result = await analysisService.AnalyzeProjectAsync("./src");

foreach (var violation in result.Violations.Where(v => v.RuleId == "TEAM002"))
{
    Console.WriteLine($"{violation.FilePath}:{violation.LineNumber} — {violation.Message}");
}
```

---

## See Also

- [Getting Started](./getting-started.md) — installation and first analysis
- [API Reference](./api-reference.md) — full interface documentation
- [Architecture Guide](./architecture.md) — internal design decisions
- [FAQ](./faq.md) — common questions
