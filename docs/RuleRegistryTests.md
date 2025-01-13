# RuleRegistryTests

Overview of the test suite for the `RuleRegistry` class in the roslyn‑guard‑analyzer project. These tests verify the core behavior of rule registration, retrieval, modification, and execution within the analyzer’s rule management system.

## API

### `public void RuleRegistry_DefaultInitialization_RegistersFourBuiltInRules`
- **Purpose:** Confirms that a newly created `RuleRegistry` instance automatically registers the four built‑in rules defined by the analyzer.
- **Parameters:** None.
- **Return value:** None (void).
- **Throws:** No exceptions are expected; the test fails if the registry does not contain exactly four rules after construction.

### `public void RegisterRule_DuplicateRuleId_ThrowsConfigurationException`
- **Purpose:** Ensures that attempting to register a rule with an identifier that already exists in the registry throws a `ConfigurationException`.
- **Parameters:** None.
- **Return value:** None (void).
- **Throws:** The test expects a `ConfigurationException` to be thrown; any other outcome causes the test to fail.

### `public void RuleViolation_IsCritical_ReturnsTrueOnlyForErrorAndCriticalSeverity`
- **Purpose:** Validates the `IsCritical` property of `RuleViolation` returns `true` only for `Error` and `Critical` severity levels, and `false` for `Warning` or `Info`.
- **Parameters:** None.
- **Return value:** None (void).
- **Throws:** No exceptions are expected; the test fails if the property does not match the specified severity mapping.

### `public void RegisterRule_ValidRule_RegistersSuccessfully`
- **Purpose:** Checks that a rule with a unique identifier and valid properties can be added to the registry without error and is subsequently retrievable.
- **Parameters:** None.
- **Return value:** None (void).
- **Throws:** No exceptions are expected; the test fails if registration fails or the rule cannot be retrieved afterward.

### `public void RegisterRule_NullRule_ThrowsArgumentNullException`
- **Purpose:** Verifies that passing a `null` rule to the registration method throws an `ArgumentNullException`.
- **Parameters:** None.
- **Return value:** None (void).
- **Throws:** The test expects an `ArgumentNullException`; any other result marks the test as failed.

### `public void GetRule_NonExistentRuleId_ReturnsNull`
- **Purpose:** Ensures that querying the registry for a rule identifier that has not been registered returns `null`.
- **Parameters:** None.
- **Return value:** None (void).
- **Throws:** No exceptions are expected; the test fails if a non‑null value is returned.

### `public void GetRulesByCategory_MatchingCategory_ReturnsCorrectRules`
- **Purpose:** Confirms that `GetRulesByCategory` returns exactly the set of rules whose `Category` property matches the supplied category string.
- **Parameters:** None.
- **Return value:** None (void).
- **Throws:** No exceptions are expected; the test fails if the returned collection does not match the expected rules.

### `public void RemoveRule_ExistingRule_ReturnsTrueAndRemovesRule`
- **Purpose:** Checks that removing a rule that exists in the registry returns `true` and that the rule is no longer present afterward.
- **Parameters:** None.
- **Return value:** None (void).
- **Throws:** No exceptions are expected; the test fails if the return value is not `true` or the rule remains in the registry.

### `public void GetEnabledRules_ReturnsOnlyEnabledRules`
- **Purpose:** Validates that `GetEnabledRules` filters out disabled rules and returns only those whose `IsEnabled` property is `true`.
- **Parameters:** None.
- **Return value:** None (void).
- **Throws:** No exceptions are expected; the test fails if any disabled rule is included or an enabled rule is omitted.

### `public void Clear_RemovesAllRules`
- **Purpose:** Ensures that invoking `Clear` removes all rules from the registry, leaving it empty.
- **Parameters:** None.
- **Return value:** None (void).
- **Throws:** No exceptions are expected; the test fails if any rules remain after the call.

### `public async System.Threading.Tasks.Task ExecuteRuleAsync_MockedEngine_ReturnsConfiguredViolationsAndVerifiesInteraction`
- **Purpose:** Asynchronously tests that executing a rule against a mocked analysis engine produces the pre‑configured violations and that the engine’s expected methods are called.
- **Parameters:** None.
- **Return value:** A `Task` representing the asynchronous operation.
- **Throws:** No exceptions are expected; the test fails if the returned violations do not match the configuration or if the mock interactions are not verified.

## Usage

### Example 1: Registering and retrieving a custom rule
```csharp
using RoslynGuardAnalyzer.Rules;
using RoslynGuardAnalyzer.Registry;

// Create a registry (defaults load the four built‑in rules)
var registry = new RuleRegistry();

// Define a custom rule
var customRule = new Rule(
    id: "RG0005",
    title: "Avoid magic numbers",
    description: "Numeric literals should be named constants.",
    category: "Maintainability",
    severity: DiagnosticSeverity.Warning,
    isEnabled: true);

// Register the rule
registry.RegisterRule(customRule);

// Retrieve the rule by its ID
Rule? retrieved = registry.GetRule("RG0005");
System.Diagnostics.Debug.Assert(retrieved != null && retrieved.Id == "RG0005");
```

### Example 2: Getting enabled rules and executing a rule
```csharp
using RoslynGuardAnalyzer.Rules;
using RoslynGuardAnalyzer.Registry;
using RoslynGuardAnalyzer.Analysis;

// Assume a registry already populated with rules
var registry = new RuleRegistry();
// (rules have been added elsewhere)

// Obtain only the rules that are currently enabled
IEnumerable<Rule> enabled = registry.GetEnabledRules();

foreach (var rule in enabled)
{
    // Execute each rule against a syntax tree (mocked or real engine)
    var violations = await rule.ExecuteAsync(syntaxTree, cancellationToken);
    // Process violations...
}
```

## Notes

- The registry is **not thread‑safe**; concurrent calls to `RegisterRule`, `RemoveRule`, `Clear`, or query methods from multiple threads may result in undefined behavior. External synchronization is required when the registry is accessed from more than one thread.
- Registering a rule with an identifier that differs only in case is treated as a duplicate; the registry performs a case‑sensitive comparison.
- The `Clear` method does not dispose of individual rule instances; callers remain responsible for managing the lifetime of any rule objects they retain after removal.
- `ExecuteRuleAsync` is intended to be overridden or mocked in tests; the production implementation expects a concrete `IAnalysisEngine` that returns `IEnumerable<RuleViolation>`.
- After a rule is removed via `RemoveRule`, any cached references to that rule instance will still exist but will no longer be accessible through the registry.
