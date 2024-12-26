# ConfigurationValidator

`ConfigurationValidator` provides a structured mechanism for validating configuration inputs used by Roslyn-based guard analyzers. It aggregates errors and warnings into separate collections, exposes a boolean validity flag, and offers static factory methods that perform targeted or comprehensive validation of analysis configuration, CLI options, and rule names. The type is designed to be used both programmatically and in diagnostic reporting workflows.

## API

### `public bool IsValid`

Gets a value indicating whether the current validation state contains no errors. Warnings do not affect this property; only the presence of at least one error renders the configuration invalid.

- **Return value**: `true` if the `Errors` list is empty; otherwise `false`.
- **Exceptions**: None.

### `public List<string> Errors`

The list of error messages accumulated during validation. Each entry describes a condition that prevents the configuration from being considered valid.

- **Return value**: A mutable `List<string>` of error descriptions. Consumers may read or enumerate this list.
- **Exceptions**: None.

### `public List<string> Warnings`

The list of warning messages accumulated during validation. Warnings indicate non-fatal concerns that do not invalidate the configuration but may merit attention.

- **Return value**: A mutable `List<string>` of warning descriptions. Consumers may read or enumerate this list.
- **Exceptions**: None.

### `public void AddError(string message)`

Appends an error message to the `Errors` list. After this call, `IsValid` will return `false`.

- **Parameters**:
  - `message` (`string`): The error description to record. A `null` or empty value is stored as-is; no validation is performed on the content.
- **Return value**: None.
- **Exceptions**: None.

### `public void AddWarning(string message)`

Appends a warning message to the `Warnings` list. This does not affect the value of `IsValid`.

- **Parameters**:
  - `message` (`string`): The warning description to record. A `null` or empty value is stored as-is; no validation is performed on the content.
- **Return value**: None.
- **Exceptions**: None.

### `public override string ToString()`

Returns a string that represents the current validation state, typically including the counts of errors and warnings and their messages. The exact format is implementation-defined and intended for diagnostic output.

- **Return value**: A `string` summarizing the validation result.
- **Exceptions**: None.

### `public static ValidationResult ValidateAnalysisConfig(...)`

Performs validation specific to analysis configuration settings. The exact parameters are determined by the configuration model used in the roslyn-guard-analyzer project.

- **Return value**: A `ValidationResult` containing the accumulated errors and warnings for the analysis configuration.
- **Exceptions**: May throw `ArgumentNullException` if required configuration objects are `null`, depending on internal parameter validation.

### `public static ValidationResult ValidateCliOptions(...)`

Validates command-line interface options supplied to the analyzer. This covers argument formats, mutual exclusivity constraints, and value ranges where applicable.

- **Return value**: A `ValidationResult` with errors and warnings pertaining to CLI options.
- **Exceptions**: May throw `ArgumentNullException` if the options argument is `null`.

### `public static ValidationResult ValidateRuleNames(...)`

Validates rule name identifiers against known rule sets, checking for existence, correct formatting, and potential duplicates.

- **Return value**: A `ValidationResult` describing any issues with the supplied rule names.
- **Exceptions**: May throw `ArgumentNullException` if the rule name collection is `null`.

### `public static ValidationResult ValidateComprehensive(...)`

Runs all relevant validation checks (analysis configuration, CLI options, rule names) in a single operation and returns a combined result.

- **Return value**: A `ValidationResult` aggregating errors and warnings from all validation scopes.
- **Exceptions**: May throw `ArgumentNullException` if any required input is `null`.

## Usage

### Example 1: Incremental validation with a `ConfigurationValidator` instance

```csharp
var validator = new ConfigurationValidator();

// Validate individual settings
if (string.IsNullOrWhiteSpace(config.OutputPath))
{
    validator.AddError("Output path must be specified.");
}

if (config.MaxViolations < 0)
{
    validator.AddError("MaxViolations cannot be negative.");
}

if (config.AnalyzerTimeout.TotalSeconds > 300)
{
    validator.AddWarning("Analyzer timeout exceeds recommended maximum of 300 seconds.");
}

if (!validator.IsValid)
{
    foreach (var error in validator.Errors)
    {
        Console.WriteLine($"ERROR: {error}");
    }
    return;
}

// Proceed with valid configuration
```

### Example 2: Using static validation methods for comprehensive checks

```csharp
var analysisConfig = new AnalysisConfig
{
    RuleSetPath = "rules/security.ruleset",
    SeverityOverrides = new Dictionary<string, DiagnosticSeverity>
    {
        ["CA1000"] = DiagnosticSeverity.Error
    }
};

var cliOptions = new CliOptions
{
    TreatWarningsAsErrors = true,
    ReportFormat = "json"
};

var ruleNames = new List<string> { "CA1000", "CA1001", "CA9999" };

// Run comprehensive validation across all inputs
ValidationResult result = ConfigurationValidator.ValidateComprehensive(
    analysisConfig, cliOptions, ruleNames);

if (!result.IsValid)
{
    Console.WriteLine(result.ToString());
    Environment.Exit(1);
}

// Configuration is valid; continue with analysis
```

## Notes

- **Thread safety**: Instance members (`AddError`, `AddWarning`, `Errors`, `Warnings`, `IsValid`, `ToString`) are not thread-safe. Concurrent calls to mutate or read state on the same instance must be externally synchronized. The static validation methods are stateless and safe for concurrent invocation provided their arguments are not mutated during the call.
- **Edge cases**: Adding an error to an instance that already contains errors keeps `IsValid` as `false`; removing items from the `Errors` list externally will not automatically recompute `IsValid` unless the list becomes empty (the property reflects the current state of the list). `AddError` and `AddWarning` accept `null` or empty strings without rejection—consumers should guard against meaningless entries if downstream display logic requires non-empty messages.
- **`ValidationResult`**: The static methods return a `ValidationResult` type (not shown here) that presumably mirrors the instance structure with `IsValid`, `Errors`, and `Warnings` members. The instance-based `ConfigurationValidator` may itself implement or wrap `ValidationResult`; consult the project source for the exact relationship.
- **ToString format**: The output of `ToString()` is intended for human-readable diagnostics and should not be parsed programmatically. Its format may change across versions.
