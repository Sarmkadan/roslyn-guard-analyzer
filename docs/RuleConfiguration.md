# RuleConfiguration

`RuleConfiguration` represents the central configuration object for the Roslyn guard analyzer, holding all settings that govern how analysis rules are applied, which files and namespaces are excluded, severity thresholds, reporting behavior, and custom extension data. It serves as both a mutable configuration container during setup and a serializable snapshot for persistence or sharing across analysis sessions.

## API

### Properties

#### `public string Id`
A unique identifier for this configuration instance. Typically a GUID or user-assigned key used to distinguish configurations when multiple sets are stored or compared.

#### `public string Name`
A human-readable label for the configuration. Intended for display in tooling, logs, and configuration management interfaces.

#### `public string Description`
A free-text description of the configuration’s purpose, scope, or intended usage. May contain markdown or plain text depending on the consuming tool.

#### `public List<AnalysisRule> EnabledRules`
The collection of analysis rules that are currently active in this configuration. Rules not present in this list are considered disabled and will not be evaluated during analysis. The list preserves insertion order, which may affect rule execution sequence.

#### `public List<string> ExcludedNamespaces`
A list of namespace prefixes to skip during analysis. Any type or member whose fully qualified name starts with one of these entries is excluded from all rule evaluations. Matching is case-sensitive and prefix-based.

#### `public List<string> ExcludedFiles`
A list of file path patterns or exact paths to exclude from analysis. Supports glob-style patterns (e.g., `**/Generated/*.cs`). Files matching any entry are completely ignored by the analyzer.

#### `public int MaxViolationsToReport`
The maximum number of violations the analyzer will report before stopping further rule evaluation. A value of `0` or negative means no limit. Useful for large codebases where only a sample of issues is needed.

#### `public int AnalysisTimeoutSeconds`
The maximum time in seconds allowed for a single analysis pass. If the analysis exceeds this duration, it is cancelled and partial results may be returned depending on `FailOnError`. A value of `0` disables the timeout.

#### `public SeverityLevel MinimumReportedSeverity`
The severity threshold below which violations are suppressed. Violations with a severity lower than this value are not included in the output. Typical values follow the Roslyn diagnostic severity scale (Hidden, Info, Warning, Error).

#### `public bool FailOnError`
When `true`, any unhandled exception during rule evaluation causes the entire analysis to fail immediately with an error result. When `false`, exceptions are logged and the offending rule is skipped, allowing other rules to continue.

#### `public bool GenerateDetailedReport`
When `true`, the analyzer produces extended output including rule execution times, per-file violation counts, and diagnostic metadata. When `false`, only a summary or minimal violation list is produced.

#### `public Dictionary<string, string> CustomSettings`
An extensible key-value store for arbitrary configuration data. Rules can read custom parameters from this dictionary to adjust their behavior without requiring changes to the `RuleConfiguration` type itself. Keys are case-sensitive.

#### `public DateTime CreatedAt`
The UTC timestamp when this configuration was first created. Set automatically by the parameterless constructor and preserved through serialization.

#### `public DateTime? UpdatedAt`
The UTC timestamp of the last modification to this configuration, or `null` if it has never been updated after creation. Callers are responsible for setting this value when mutating the configuration.

### Constructors

#### `public RuleConfiguration()`
Parameterless constructor. Initializes all collections to empty lists, sets `Id` to a new GUID, `CreatedAt` to `DateTime.UtcNow`, `UpdatedAt` to `null`, and applies default values: `MaxViolationsToReport = 100`, `AnalysisTimeoutSeconds = 300`, `MinimumReportedSeverity = SeverityLevel.Warning`, `FailOnError = true`, `GenerateDetailedReport = false`.

#### `public RuleConfiguration(string id, string name)`
Parameterized constructor. Initializes the configuration with the given `id` and `name`. All other properties receive the same defaults as the parameterless constructor. `CreatedAt` is set to `DateTime.UtcNow`. Throws `ArgumentNullException` if `id` is null or empty.

### Methods

#### `public void AddRule(AnalysisRule rule)`
Adds a rule to the `EnabledRules` list. If the rule is already present (determined by reference equality or rule ID equality), the method does nothing—no duplicate is added. Throws `ArgumentNullException` if `rule` is null.

#### `public bool RemoveRule(string ruleId)`
Removes the rule with the specified `ruleId` from `EnabledRules`. Returns `true` if a rule was found and removed; returns `false` if no rule with that ID exists in the list. Throws `ArgumentNullException` if `ruleId` is null or empty.

#### `public AnalysisRule? GetRule(string ruleId)`
Returns the rule with the specified `ruleId` from `EnabledRules`, or `null` if no matching rule is found. Throws `ArgumentNullException` if `ruleId` is null or empty.

#### `public void ExcludeNamespace(string namespacePrefix)`
Adds a namespace prefix to the `ExcludedNamespaces` list. If the prefix is already present, the method does nothing. The prefix should not include a trailing dot—matching is performed by appending a dot internally. Throws `ArgumentNullException` if `namespacePrefix` is null or empty.

## Usage

### Example 1: Building a configuration programmatically

```csharp
var config = new RuleConfiguration("guard-default", "Default Guard Rules");

config.AddRule(new AnalysisRule("GUARD001", "NullCheck", SeverityLevel.Error));
config.AddRule(new AnalysisRule("GUARD002", "ArgumentValidation", SeverityLevel.Warning));

config.ExcludeNamespace("System.Runtime.CompilerServices");
config.ExcludeNamespace("Microsoft.CodeAnalysis.Generated");

config.MinimumReportedSeverity = SeverityLevel.Warning;
config.MaxViolationsToReport = 50;
config.GenerateDetailedReport = true;
config.FailOnError = false;

config.CustomSettings["NullCheck.StrictMode"] = "true";
config.CustomSettings["ArgumentValidation.AllowAsyncVoid"] = "false";

config.UpdatedAt = DateTime.UtcNow;
```

### Example 2: Loading, modifying, and querying a persisted configuration

```csharp
// Assume 'loadedConfig' was deserialized from JSON or a database
var loadedConfig = LoadConfiguration("config.json");

// Disable a specific rule by ID
if (loadedConfig.RemoveRule("GUARD003"))
{
    Console.WriteLine("Rule GUARD003 disabled.");
}

// Check if a critical rule is still active
var nullCheckRule = loadedConfig.GetRule("GUARD001");
if (nullCheckRule == null || nullCheckRule.Severity < SeverityLevel.Error)
{
    throw new InvalidOperationException("Critical null-check rule is missing or downgraded.");
}

// Add a new exclusion for generated code
loadedConfig.ExcludeNamespace("MyProject.Generated");

// Adjust timeout for a large codebase
loadedConfig.AnalysisTimeoutSeconds = 600;

loadedConfig.UpdatedAt = DateTime.UtcNow;
SaveConfiguration(loadedConfig, "config.json");
```

## Notes

- **Collection mutability**: `EnabledRules`, `ExcludedNamespaces`, `ExcludedFiles`, and `CustomSettings` are mutable reference types. Changes to these collections are visible to any code holding a reference to the same `RuleConfiguration` instance. No internal synchronization is performed—concurrent mutations from multiple threads will cause undefined behavior unless externally synchronized.
- **Rule identity**: `AddRule`, `RemoveRule`, and `GetRule` use rule ID for identity checks. Two distinct `AnalysisRule` instances with the same ID are considered the same rule. Adding a rule with an ID already present is a no-op.
- **Namespace exclusion matching**: The `ExcludeNamespace` method expects a prefix without a trailing dot. The analyzer internally appends a dot before comparing against fully qualified type names. Supplying a prefix that already ends with a dot may cause double-dot matching and unintended exclusion failures.
- **Timeout behavior**: When `AnalysisTimeoutSeconds` is exceeded, the behavior depends on `FailOnError`. If `FailOnError` is `true`, the analysis throws or returns an error result. If `false`, partial results collected up to the timeout are returned.
- **`UpdatedAt` management**: The type does not automatically update `UpdatedAt` when properties or collections change. Callers must explicitly set `UpdatedAt` to track modifications. This design allows batch updates without intermediate timestamps.
- **`MaxViolationsToReport` interaction with `MinimumReportedSeverity`**: The violation limit applies after severity filtering. Only violations meeting the severity threshold count toward the limit.
- **Serialization**: `DateTime` values are stored in UTC. Consumers deserializing configurations should expect `UpdatedAt` to potentially be `null` and handle accordingly.
