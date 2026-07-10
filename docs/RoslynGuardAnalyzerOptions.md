# RoslynGuardAnalyzerOptions

`RoslynGuardAnalyzerOptions` encapsulates all configurable parameters that govern a Roslyn-based guard analysis run. It holds project paths, execution constraints, output preferences, filtering rules, and severity thresholds. Instances are typically populated from command-line arguments, configuration files, or programmatic setup, and they can be merged with CLI overrides via `MergeWithCliOptions`.

## API

### `public string ProjectPath`
Gets or sets the absolute or relative path to the project or solution file to analyze. This value is required for any analysis run; an empty or null string will cause downstream validation to fail.

### `public int AnalysisTimeoutSeconds`
Specifies the maximum wall-clock time, in seconds, that the entire analysis is allowed to run. A value of zero or less disables the timeout. When the timeout is exceeded, the analyzer terminates and reports whatever violations were collected up to that point.

### `public int MaxViolationsToReport`
Limits the total number of violations emitted in the output. Once this count is reached, the analyzer stops reporting additional diagnostics. A value of zero suppresses all violation output; a negative value is treated as unlimited.

### `public int LogLevel`
Controls the verbosity of internal logging. Higher values produce more detailed diagnostic output. Typical ranges follow standard logging frameworks (e.g., 0 = errors only, 1 = warnings, 2 = informational, 3 = debug).

### `public string OutputFormat`
Defines the format for violation output. Common values include `"Console"`, `"Json"`, `"Xml"`, or `"Sarif"`. The value is case-insensitive. An unrecognized format causes the analyzer to fall back to `"Console"` and emit a warning.

### `public string? OutputFile`
Optional path to a file where the analysis report is written. When `null`, output is directed to the console or standard output stream. If a relative path is provided, it is resolved against the current working directory.

### `public bool GenerateReport`
When `true`, a structured report is produced after analysis completes. The report format is determined by `ReportType`. When `false`, only raw violation data is emitted according to `OutputFormat`.

### `public string ReportType`
Specifies the type of structured report to generate when `GenerateReport` is `true`. Supported values include `"Summary"`, `"Detailed"`, and `"Sarif"`. The value is case-insensitive. An unrecognized type defaults to `"Summary"`.

### `public bool FailOnViolations`
If `true`, the analysis process returns a non-zero exit code when any violations are detected. This enables integration with CI/CD pipelines that gate on analysis results.

### `public bool SkipCache`
When `true`, the analyzer ignores any cached intermediate results and performs a full re-analysis. This guarantees fresh results at the cost of increased execution time.

### `public int MaxParallelThreads`
Sets the maximum number of worker threads used during analysis. A value of `1` forces single-threaded execution. A value of `0` or less lets the analyzer auto-select a degree of parallelism based on available processors.

### `public List<string> RuleFilter`
A list of rule identifiers to include in the analysis. When non-empty, only rules whose IDs match an entry in this list are executed. Matching is case-insensitive. An empty list means all available rules are active.

### `public List<string> ExcludePatterns`
A list of glob-style patterns for files or directories to exclude from analysis. Patterns are matched against project-relative paths. Entries follow standard glob syntax (e.g., `**/Generated/**`, `*.g.cs`).

### `public string MinimumSeverity`
Sets the lowest diagnostic severity that is reported. Accepted values are `"Hidden"`, `"Info"`, `"Warning"`, `"Error"`. Violations below this severity are silently dropped. The value is case-insensitive; an unrecognized string defaults to `"Warning"`.

### `public string? ConfigFile`
Optional path to a JSON or XML configuration file that supplies default values for all options. When provided, the file is read before CLI arguments are applied. A `null` value means no external configuration file is used.

### `public List<string> Validate`
Returns a list of validation error messages. After all options are set and merged, callers should inspect this list. An empty list indicates the options are valid; any entries describe missing required fields, incompatible combinations, or malformed values.

### `public override string ToString`
Returns a string representation of the current options, including all property values. Sensitive fields such as file paths are included verbatim. This is intended for diagnostic logging and debugging.

### `public void MergeWithCliOptions`
Merges the current instance with a set of command-line overrides. CLI-specified values take precedence over existing property values. After merging, the `Validate` list is repopulated to reflect the combined state.

**Parameters:**  
- `string[] args` – An array of command-line arguments in the form `--PropertyName value` or `-p value`. Boolean properties can be set with `--Flag` (true) or `--no-Flag` (false).

**Exceptions:**  
- `ArgumentException` – Thrown when an unrecognized argument name is encountered.  
- `FormatException` – Thrown when a value cannot be parsed into the target property’s type (e.g., a non-integer string for `MaxParallelThreads`).

## Usage

### Example 1: Programmatic setup and validation

```csharp
var options = new RoslynGuardAnalyzerOptions
{
    ProjectPath = @"C:\src\MySolution.sln",
    AnalysisTimeoutSeconds = 120,
    OutputFormat = "Sarif",
    OutputFile = @"C:\reports\analysis.sarif",
    MinimumSeverity = "Warning",
    RuleFilter = new List<string> { "SA0001", "SA1200" },
    ExcludePatterns = new List<string> { "**/Migrations/**" },
    MaxParallelThreads = 4
};

var validationErrors = options.Validate;
if (validationErrors.Count > 0)
{
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"Configuration error: {error}");
    }
    return;
}

// Pass options to the analyzer runner
var runner = new GuardAnalyzerRunner(options);
await runner.RunAsync();
```

### Example 2: Loading from config file with CLI overrides

```csharp
var options = new RoslynGuardAnalyzerOptions
{
    ConfigFile = @"C:\configs\guard-defaults.json"
};

// Simulate CLI arguments: override timeout, enable fail-on-violations, add an exclude pattern
string[] cliArgs = new[]
{
    "--AnalysisTimeoutSeconds", "300",
    "--FailOnViolations",
    "--ExcludePatterns", "**/obj/**"
};

options.MergeWithCliOptions(cliArgs);

if (options.Validate.Count > 0)
{
    throw new InvalidOperationException(
        $"Invalid options: {string.Join("; ", options.Validate)}");
}

Console.WriteLine(options.ToString());
```

## Notes

- **Validation lifecycle:** The `Validate` list is populated after construction and again after each call to `MergeWithCliOptions`. Callers must check it before starting analysis. An empty list is the only indicator of a valid state.
- **Thread safety:** Instance members are not synchronized. Concurrent reads and writes to the same `RoslynGuardAnalyzerOptions` object, or concurrent calls to `MergeWithCliOptions`, produce undefined behavior. Create separate instances per thread or synchronize externally.
- **`MinimumSeverity` ordering:** Severity values follow the Roslyn `DiagnosticSeverity` enum ordering: `Hidden` < `Info` < `Warning` < `Error`. Setting `MinimumSeverity` to `"Error"` suppresses everything except errors.
- **`MaxViolationsToReport` interaction with `FailOnViolations`:** Even when `MaxViolationsToReport` is zero, `FailOnViolations` still evaluates the actual violation count. If violations exist but are suppressed from output, the process can still fail.
- **`SkipCache` and timeout:** When `SkipCache` is `true`, the analysis runs from scratch, which may make it more likely to hit the `AnalysisTimeoutSeconds` limit on large codebases.
- **`OutputFile` and `GenerateReport`:** When `GenerateReport` is `true` and `OutputFile` is `null`, the structured report is written to the console, which may produce large output. Prefer specifying an `OutputFile` for detailed reports.
- **`RuleFilter` and `ExcludePatterns` interaction:** Files excluded by `ExcludePatterns` are not analyzed even if they contain code that would trigger a rule in `RuleFilter`. Exclusion takes precedence.
- **`MergeWithCliOptions` argument format:** Boolean properties use `--PropertyName` for `true` and `--no-PropertyName` for `false`. List properties can be specified multiple times to accumulate values.
