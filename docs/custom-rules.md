# Custom Rules

The custom rules subsystem allows defining and registering analysis rules at runtime without recompiling the analyzer. This enables teams to create project-specific architectural constraints that can be modified independently of the core analyzer.

## Overview

Custom rules consist of three cooperating components:

1. **`CustomAnalysisRule`** - The rule definition containing metadata and evaluation logic
2. **`CustomRuleBuilder`** - Fluent API for constructing `CustomAnalysisRule` instances  
3. **`CustomRuleRegistry`** - Thread-safe storage for runtime-registered rules
4. **`CustomRuleEngine`** - Executes registered custom rules against code elements

Custom rules integrate seamlessly with the existing analysis pipeline and support all standard features including severity configuration, inline suppression via `GUARD_SKIP`, and `.editorconfig` severity overrides.

## CustomRuleEngine

The `CustomRuleEngine` evaluates runtime-registered custom rules against code elements. It serves as the execution engine for rules created via the `CustomRuleBuilder`.

### Responsibilities

- Evaluates individual custom rules via their configured predicate and message factory
- Executes all registered custom rules in sequence against a set of code elements
- Handles cancellation tokens for long-running analyses
- Provides async evaluation while maintaining compatibility with synchronous built-in rules

### Usage

```csharp
using RoslynGuardAnalyzer.Rules;
using RoslynGuardAnalyzer.Domain.Models;

// Obtain a registry with registered rules
var registry = new CustomRuleRegistry();
// ... register rules via registry or builder ...

// Create the engine
var engine = new CustomRuleRegistry(registry);

// Evaluate a specific rule
var violations = await engine.EvaluateRuleAsync(
    customRule, 
    codeElements, 
    cancellationToken);

// Evaluate all registered rules
var allViolations = await engine.EvaluateAsync(
    codeElements, 
    cancellationToken);
```

### Thread Safety

The engine is safe for concurrent use when the underlying `ICustomRuleRegistry` implementation is thread-safe (the provided `CustomRuleRegistry` is thread-safe).

## CustomRuleBuilder

The `CustomRuleBuilder` provides a fluent interface for creating `CustomAnalysisRule` instances with strongly-typed configuration.

### Fluent Interface Pattern

All setter methods return the builder instance, allowing method chaining:

```csharp
var rule = CustomRuleBuilder.Create("TEAM001", "Max Dependency Count")
    .For(RuleCategory.CodeStructure)
    .WithSeverity(SeverityLevel.Warning)
    .WithDescription("Classes must not exceed the configured maximum number of dependencies.")
    .When(element => element.Dependencies.Count > 10)
    .WithMessage(element => $"Class '{element.Name}' has too many dependencies.")
    .Build();
```

### Configuration Methods

| Method | Description |
|--------|-------------|
| `Create(id, name)` | Starts a new rule definition (required) |
| `For(category)` | Sets the rule category |
| `WithSeverity(severity)` | Sets the default severity for violations |
| `WithDescription(description)` | Sets the rule description |
| `When(predicate)` | **Required**: Defines the violation detection logic |
| `WithMessage(message)` | Uses a constant message for all violations |
| `WithMessage(factory)` | Uses a factory to generate dynamic messages per element |
| `Build()` | **Required**: Creates the immutable `CustomAnalysisRule` |

### Required Configuration

A valid custom rule must have:
1. A non-empty identifier (`id`)
2. A non-empty name (`name`) 
3. A violation predicate (`When` clause)

### Message Generation

If no explicit message is configured, the builder provides a default message format:
```
"Rule '{rule name}' was violated by '{element name}'."
```

## CustomRuleRegistry

The `CustomRuleRegistry` provides thread-safe storage for custom rules registered at runtime. It implements `ICustomRuleRegistry` and serves as the primary registration mechanism for custom rules.

### Features

- **Thread-safe**: Uses `ConcurrentDictionary` for safe concurrent access
- **Case-insensitive ID lookup**: Rule IDs are compared ignoring case
- **Duplicate prevention**: Prevents registering multiple rules with the same ID
- **Built-in rule initialization**: Automatically registers predefined rules on construction
- **Ordered retrieval**: `GetCustomRules()` returns rules sorted by ID

### Built-in Rules

The registry automatically registers these built-in custom rules during initialization:

| Rule ID | Rule Title | Category | Description |
|---------|------------|----------|-------------|
| `AV001` | Async Void Methods Must Be Event Handlers | AsyncPattern | Detects async void methods that are not event handlers |
| `AVW001` | Async Void Methods Should Be Avoided | AsyncPattern | Warns about async void usage that could cause unobserved exceptions |
| `EC001` | Empty Catch Blocks Must Be Removed Or Handle Exception | CodeStructure | Flags empty catch blocks that swallow exceptions |
| `ECB001` | Empty Catch Blocks Must Be Removed Or Handle Exception | CodeStructure | Flags catch blocks with no statements |
| `MTN001` | Public Types Must Have Meaningful Names | NamingConvention | Enforces descriptive type names for public types |

### Usage

```csharp
using RoslynGuardAnalyzer.Rules;
using RoslynGuardAnalyzer.Domain.Models;

// Create registry (automatically loads built-in rules)
var registry = new CustomRuleRegistry();

// Register a custom rule
var customRule = CustomRuleBuilder.Create("TEAM001", "No Public Fields")
    .For(RuleCategory.CodeStructure)
    .WithSeverity(SeverityLevel.Warning)
    .WithDescription("Public fields are not allowed outside DTO classes.")
    .When(element => 
        element.ElementType == CodeElementType.Field && 
        element.IsPublic &&
        !(element.ParentName?.EndsWith("Dto", StringComparison.OrdinalIgnoreCase) ?? false))
    .WithMessage(element => $"Public field '{element.Name}' should be a property.")
    .Build();

registry.RegisterCustomRule(customRule);

// Retrieve all registered rules
IReadOnlyList<CustomAnalysisRule> rules = registry.GetCustomRules();

// Check if a rule exists (case-insensitive)
bool hasRule = registry.GetCustomRules().Any(r => r.Id.Equals("TEAM001", StringComparison.OrdinalIgnoreCase));
```

### Integration with RuleEngine

The registry works directly with `CustomRuleEngine`:

```csharp
var registry = new CustomRuleRegistry();
// ... register rules ...
var engine = new CustomRuleEngine(registry);
var violations = await engine.EvaluateAsync(codeElements);
```

## Complete Example

Here's a complete example showing how to create, register, and use a custom rule:

```csharp
using RoslynGuardAnalyzer.Rules;
using RoslynGuardAnalyzer.Domain.Models;

// 1. Create the rule using the fluent builder
var noPublicFieldsRule = CustomRuleBuilder.Create("TEAM002", "No Public Fields")
    .For(RuleCategory.CodeStructure)
    .WithSeverity(SeverityLevel.Warning)
    .WithDescription("Public fields are not allowed outside DTO classes. Use properties instead.")
    .When(element => 
        element.ElementType == CodeElementType.Field &&
        element.IsPublic &&
        !(element.ParentName?.EndsWith("Dto", StringComparison.OrdinalIgnoreCase) ?? false) &&
        !(element.ParentName?.EndsWith("Request", StringComparison.OrdinalIgnoreCase) ?? false) &&
        !(element.ParentName?.EndsWith("Response", StringComparison.OrdinalIgnoreCase) ?? false))
    .WithMessage(element => $"Public field '{element.Name}' in '{element.ParentName ?? "<unknown>"}' should be a property.")
    .Build();

// 2. Register the rule
var registry = new CustomRuleRegistry();
registry.RegisterCustomRule(noPublicFieldsRule);

// 3. Create the engine and execute
var engine = new CustomRuleEngine(registry);
var elements = GetCodeElementsFromSomewhere(); // Your code element source
var violations = await engine.EvaluateAsync(elements);

// 4. Process results
foreach (var violation in violations)
{
    Console.WriteLine($"{violation.FilePath}:{violation.LineNumber} - {violation.Message}");
}
```

## Rule Evaluation Process

When `CustomRuleEngine.EvaluateAsync` is called:

1. **Null checking**: Validates input parameters
2. **Materialization**: Converts elements to list to prevent multiple enumeration
3. **Iteration**: Processes each registered rule from the registry
4. **Cancellation**: Checks cancellation token before each rule evaluation
5. **Delegation**: Calls `rule.EvaluateAsync(elements)` on each `CustomAnalysisRule`
6. **Aggregation**: Combines violations from all rules into a single list
7. **Return**: Returns the combined list of `RuleViolation` objects

Each `CustomAnalysisRule` handles its own evaluation logic via:
- The configured `ViolationPredicate` to detect violations
- The configured `MessageFactory` to generate violation messages
- Automatic population of standard violation properties (`RuleId`, `RuleName`, `FilePath`, `LineNumber`, `Severity`, `Category`)
- Enrichment of each violation with `ElementName` and `FullyQualifiedName` metadata

## Best Practices

### Rule Design

1. **Focused scope**: Each rule should address a single architectural concern
2. **Clear messaging**: Violation messages should clearly state the problem and solution
3. **Appropriate severity**: Use `Error` for blocking issues, `Warning` for improvements, `Info` for suggestions
4. **Performance**: Keep predicates efficient as they run against every code element

### Naming Conventions

1. **IDs**: Use PascalCase with numeric suffix (e.g., `TEAM001`, `PROJ005`)
2. **Names**: Use descriptive PascalCase names (e.g., "No Public Fields", "Max Dependency Count")
3. **Descriptions**: Complete sentences ending in periods

### Configuration

1. **Use builder methods**: Prefer fluent interface over manual property setting
2. **Provide defaults**: Supply meaningful default values where applicable
3. **Document options**: If your rule needs configuration, document it in the description

### Testing

1. **Unit test predicates**: Test your violation logic in isolation
2. **Test message generation**: Verify messages are clear and accurate
3. **Test integration**: Test rule registration and execution through the engine
4. **Test suppression**: Verify `GUARD_SKIP` directives work correctly

## See Also

- [Getting Started](./getting-started.md) — installation and first analysis
- [Rule Engine](./rule-engine.md) — details on how rules are executed
- [Custom Analyzer Rule Development Guide](./custom-rule-development.md) — traditional AnalysisRule approach
- [API Reference](./api-reference.md) — full interface documentation