# RuleConfigurationBuilder

`RuleConfigurationBuilder` provides a fluent API for constructing immutable `RuleConfiguration` instances that define how Roslyn analyzer rules behave. It supports enabling or disabling rules, setting diagnostic severities, attaching key-value parameters, and supplying human-readable descriptions. Factory methods for common rule categories—naming conventions, layer dependencies, async patterns, and null safety—offer pre-configured starting points.

## API

### `public RuleConfigurationBuilder()`
Default constructor. Creates a new builder with no initial configuration. All settings must be supplied via the fluent methods before calling `Build`.

### `public RuleConfigurationBuilder WithEnabled(bool enabled)`
Sets whether the rule is enabled.
- **Parameters:** `enabled` — `true` to activate the rule, `false` to suppress it.
- **Returns:** The same builder instance for chaining.
- **Throws:** Nothing.

### `public RuleConfigurationBuilder WithSeverity(DiagnosticSeverity severity)`
Assigns the diagnostic severity the rule will report.
- **Parameters:** `severity` — a `DiagnosticSeverity` value (e.g., `Error`, `Warning`, `Info`, `Hidden`).
- **Returns:** The same builder instance for chaining.
- **Throws:** Nothing.

### `public RuleConfigurationBuilder WithParameter(string key, string value)`
Adds a single named parameter to the rule configuration. If a parameter with the same key already exists, it is overwritten.
- **Parameters:**
  - `key` — non-null parameter name.
  - `value` — parameter value; may be `null`.
- **Returns:** The same builder instance for chaining.
- **Throws:** `ArgumentNullException` if `key` is `null`.

### `public RuleConfigurationBuilder WithParameters(IEnumerable<KeyValuePair<string, string>> parameters)`
Adds multiple parameters at once. Existing parameters with matching keys are overwritten; non-matching keys are appended.
- **Parameters:** `parameters` — a collection of key-value pairs. May be empty but must not be `null`.
- **Returns:** The same builder instance for chaining.
- **Throws:** `ArgumentNullException` if `parameters` is `null`. Individual keys within the collection must not be `null`; behavior on null keys is undefined.

### `public RuleConfigurationBuilder WithDescription(string description)`
Sets a human-readable description for the rule, typically shown in editor tooltips or configuration files.
- **Parameters:** `description` — the description text; may be `null` or empty.
- **Returns:** The same builder instance for chaining.
- **Throws:** Nothing.

### `public RuleConfiguration Build()`
Finalizes and produces an immutable `RuleConfiguration` object from the current builder state.
- **Returns:** A new `RuleConfiguration` instance.
- **Throws:** Nothing. Calling `Build` does not reset the builder; subsequent calls produce independent snapshots of the same state.

### `public static RuleConfigurationBuilder CreateNamingConvention()`
Creates a builder pre-configured with sensible defaults for naming-convention rules (e.g., enabled, severity `Warning`).
- **Returns:** A new `RuleConfigurationBuilder` instance with naming-convention defaults.

### `public static RuleConfigurationBuilder CreateLayerDependency()`
Creates a builder pre-configured for layer-dependency validation rules.
- **Returns:** A new `RuleConfigurationBuilder` instance with layer-dependency defaults.

### `public static RuleConfigurationBuilder CreateAsyncPatterns()`
Creates a builder pre-configured for async/await pattern rules.
- **Returns:** A new `RuleConfigurationBuilder` instance with async-pattern defaults.

### `public static RuleConfigurationBuilder CreateNullSafety()`
Creates a builder pre-configured for null-safety analysis rules.
- **Returns:** A new `RuleConfigurationBuilder` instance with null-safety defaults.

## Usage

### Example 1: Customizing a naming convention rule
```csharp
var namingConfig = RuleConfigurationBuilder
    .CreateNamingConvention()
    .WithSeverity(DiagnosticSeverity.Error)
    .WithParameter("allowedPrefix", "I")
    .WithParameter("allowedSuffix", "Async")
    .WithDescription("Interface names must start with 'I'; async methods must end with 'Async'.")
    .Build();

// namingConfig is now an immutable RuleConfiguration ready for registration.
```

### Example 2: Disabling a null-safety rule with parameters
```csharp
var nullSafetyConfig = RuleConfigurationBuilder
    .CreateNullSafety()
    .WithEnabled(false)
    .WithParameters(new Dictionary<string, string>
    {
        ["strictMode"] = "true",
        ["excludedNamespaces"] = "System.Runtime.CompilerServices"
    })
    .Build();

// Even though the rule is disabled, the parameters are preserved in the configuration.
```

## Notes

- The builder is **not thread-safe**. If multiple threads mutate the same builder instance concurrently, the resulting state is unpredictable. Create separate builder instances per thread or serialize access.
- Calling `Build()` does **not** clear the builder’s internal state. Repeated calls to `Build()` return distinct `RuleConfiguration` objects that reflect the builder’s state at the moment of each call. This allows producing multiple variants from a single base configuration, but also means later mutations affect future `Build()` outputs.
- The static factory methods (`CreateNamingConvention`, etc.) return builders with opinionated defaults. These defaults may include a specific severity, enabled state, and possibly pre-populated parameters. Consult the implementation or project documentation for the exact defaults of each factory.
- `WithParameter` overwrites by key. If you need to remove a parameter, you must rebuild the configuration without calling `WithParameter` for that key; there is no explicit removal method.
- `WithParameters` accepts any `IEnumerable<KeyValuePair<string, string>>`, including dictionaries, lists, or LINQ query results. Duplicate keys in the input collection result in the last value winning; the order of processing follows the enumeration order.
