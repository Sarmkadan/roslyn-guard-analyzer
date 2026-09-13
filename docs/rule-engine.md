# Rule engine

`RuleEngine` executes registered analysis rules against the `CodeElement` model produced by the analysis pipeline. It supports executing one supplied rule or all enabled rules in the registry. The engine does not parse source code, register rules, or apply suppressions after violations are produced; its responsibility is to select eligible elements, run the appropriate check, and return `RuleViolation` objects.

The implementation is in `src/RoslynGuardAnalyzer/Services/RuleEngine.cs` and implements `IRuleEngine` from `Services/Interfaces.cs`.

## Dependencies

The constructor requires an `IRuleRegistry`:

```csharp
var registry = new RuleRegistry();
IRuleEngine engine = new RuleEngine(registry);
```

Passing `null` for the registry throws `ArgumentNullException`. The default dependency-injection registration in `ServiceCollectionExtensions` registers both `IRuleRegistry`/`RuleRegistry` and `IRuleEngine`/`RuleEngine` as singletons.

The engine also relies on these project types:

| Dependency | Role |
| --- | --- |
| `IRuleRegistry` | Supplies the rules used by `ExecuteAllRulesAsync`. |
| `AnalysisRule` and `RuleCategory` | Describe whether a rule is enabled, how it is dispatched, and its default severity. |
| `CustomAnalysisRule` | Provides the evaluator used for custom rules. |
| `CodeElement` | Represents the types, methods, properties, fields, dependencies, source locations, attributes, and suppression directives inspected by rules. |
| `RuleViolation` | Represents each reported finding. |
| `AnalyzerConstants` | Defines layer suffixes, naming conventions, default rule IDs, and suppression tokens. |
| `ParallelAnalysisConfig.MaxRuleParallelism` | Bounds concurrent rule execution. Its default is the smaller of the processor count and four. |

The engine maintains thread-safe caches for source-file lines and `.editorconfig` lines because all-rules execution may invoke checks concurrently.

## Executing one rule

`ExecuteRuleAsync(AnalysisRule rule, List<CodeElement> elements)` follows this sequence:

1. Reject a `null` rule with `ArgumentNullException`.
2. Return an empty list when the rule is disabled, the element list is `null`, or the list is empty.
3. Remove elements suppressed for that rule.
4. If the rule is a `CustomAnalysisRule`, call `EvaluateAsync` with the remaining elements and return its task.
5. Otherwise, dispatch by `RuleCategory` to the matching built-in check.
6. Return the resulting violations in an already-completed task. An unsupported category produces an empty list.

Although the method is asynchronous in shape, built-in checks are synchronous CPU-bound operations and are not wrapped in `Task.Run`.

### Element suppression

Suppression is applied before either a built-in or custom rule runs. An element is excluded when either of these mechanisms targets the current rule:

- An attribute string contains both `SuppressRoslynGuard` and the rule ID. Both comparisons are case-insensitive.
- `CodeElement.SuppressDirectives` contains `GUARD_SKIP` or `GUARD_SKIP:<rule ID>`, using case-insensitive equality.
- The source line immediately before the element declaration is exactly `// GUARD_SKIP`, or starts with `// GUARD_SKIP:<rule ID>`, using case-insensitive comparison.

The source-line fallback requires an existing file path and a declaration after line 1. File contents are cached by path for the lifetime of the engine. The rule-specific inline check uses `StartsWith`, so text after the rule ID also matches.

### Severity resolution

Before a built-in check adds a violation, it resolves severity for the element's file. Starting in the file's directory, the engine walks upward and examines every `.editorconfig` it finds for:

```ini
dotnet_diagnostic.<rule ID>.severity = warning
```

The recognized values are `error`, `warning`, `suggestion`, `info`, and `none`. `suggestion` and `info` map to `SeverityLevel.Info`; `none` suppresses the prospective violation. The first recognized matching setting encountered while walking upward wins. If no recognized override exists, or the file path is empty, the rule's `DefaultSeverity` is used. `.editorconfig` contents are cached by path.

This is a direct line-based lookup. It does not evaluate `.editorconfig` section patterns or the complete EditorConfig specification.

## Built-in rule execution

Built-in rules are selected by category:

| Category | Elements inspected | Violation condition |
| --- | --- | --- |
| `LayerDependency` | Container elements and their dependencies | A name ending in the repository-layer suffix depends on a known element ending in the service- or controller-layer suffix. Dependencies ending in `.Tests`, unknown dependencies, and elements outside the known suffixes are ignored. |
| `NamingConvention` | All active elements | Interfaces must have the configured interface prefix; methods and properties must begin with an uppercase character; non-public fields must have the configured private-field prefix. At most one naming issue is returned per element. |
| `AsyncPattern` | Methods | A return type containing `Task` belongs to a method not marked async, or an async method lacks the configured `Async` suffix. A method may produce both violations. |
| `NullSafety` | Public properties and fields with a return type | A reference-like type is not nullable-annotated. Nullable types, known value-type keywords, `Guid`, arrays whose base type is a known value type, non-public members, and missing return types are skipped. |

Each built-in violation contains the rule ID, name, category, resolved severity, element file path, and declaration start line. The check supplies a category-specific message.

Custom rules do not use the category switch or the engine's `.editorconfig` severity resolution. After the engine filters suppressed elements, `CustomAnalysisRule.EvaluateAsync` applies its configured predicate and message factory and creates its own violations.

## Executing all rules

`ExecuteAllRulesAsync(List<CodeElement> elements)` obtains every rule from the registry, filters to `IsEnabled`, and executes those rules with `Parallel.ForEachAsync`. Concurrency is bounded by `ParallelAnalysisConfig.MaxRuleParallelism`; elements within an individual built-in rule are processed sequentially.

The method returns an empty list without querying or running rules when the element list is `null` or empty, and returns an empty list when the registry has no enabled rules.

Violations from concurrent executions are collected in a `ConcurrentBag`. Before being returned, they are sorted deterministically by:

1. `FilePath`
2. `LineNumber`
3. `RuleId`

If a rule throws an exception other than `OperationCanceledException`, all-rules execution wraps it in `RuleExecutionException`, records it, continues running other rules, and writes a warning to standard output after processing. Those failures are not returned to the caller. `OperationCanceledException` is not caught by that handler and can end the parallel operation. The cancellation token supplied by `Parallel.ForEachAsync` is not passed into `ExecuteRuleAsync` or custom rule evaluation.

## Example

```csharp
var registry = new RuleRegistry();
var engine = new RuleEngine(registry);

List<CodeElement> elements = GetCodeElements();

// Runs the four enabled defaults plus any other enabled registered rules.
List<RuleViolation> allViolations =
    await engine.ExecuteAllRulesAsync(elements);

// A registered rule can also be executed directly.
AnalysisRule? namingRule = registry.GetRule(
    AnalyzerConstants.DefaultRules.NamingConventionRule);

if (namingRule is not null)
{
    List<RuleViolation> namingViolations =
        await engine.ExecuteRuleAsync(namingRule, elements);
}
```

Calling `ExecuteRuleAsync` directly does not require the supplied rule to be registered. In contrast, `ExecuteAllRulesAsync` only sees rules returned by the injected registry.
