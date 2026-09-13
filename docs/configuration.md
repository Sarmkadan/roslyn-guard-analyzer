# Configuration

This document describes the configuration schema and validation rules for the Roslyn Guard Analyzer. Configuration is handled by three classes in the `RoslynGuardAnalyzer.Configuration` namespace:

- `RoslynGuardAnalyzerOptions` — strongly-typed options object using the `IOptions` pattern with DataAnnotations validation.
- `ConfigurationLoader` — loads and parses JSON configuration files into an `AnalysisConfig`.
- `ConfigurationValidator` — validates configuration objects for consistency and correctness.

## `RoslynGuardAnalyzerOptions`

The primary options object. It is a `sealed class` with default values, DataAnnotations validation attributes, and a `Validate()` method. It also supports merging with CLI options via `MergeWithCliOptions`.

### Properties

| Property | Type | Default | Validation |
| --- | --- | --- | --- |
| `ProjectPath` | `string` | `"./"` | — |
| `AnalysisTimeoutSeconds` | `int` | `600` | `[Range(1, int.MaxValue)]` — must be greater than 0 |
| `MaxViolationsToReport` | `int` | `1000` | `[Range(1, 100000)]` |
| `LogLevel` | `int` | `2` | `[Range(0, 4)]` — 0=none, 1=errors, 2=warnings, 3=info, 4=debug |
| `OutputFormat` | `string` | `"text"` | `[RegularExpression("^(text\|json\|csv\|html\|xml)$")]` |
| `OutputFile` | `string?` | `null` | — |
| `GenerateReport` | `bool` | `true` | — |
| `ReportType` | `string` | `"summary"` | `[RegularExpression("^(summary\|detailed\|full)$")]` |
| `FailOnViolations` | `bool` | `true` | — |
| `SkipCache` | `bool` | `false` | — |
| `MaxParallelThreads` | `int` | `Environment.ProcessorCount` | `[Range(1, 64)]` |
| `RuleFilter` | `List<string>` | `[]` (all rules) | — |
| `ExcludePatterns` | `List<string>` | `["**/bin/**", "**/obj/**", "**/*.Generated.cs", "**/*.Designer.cs"]` | — |
| `MinimumSeverity` | `string` | `"Low"` | `[RegularExpression("^(Low\|Medium\|High\|Critical)$")]` |
| `ConfigFile` | `string?` | `null` | — |
| `BaselineFile` | `string?` | `null` | — |
| `CreateBaseline` | `bool` | `false` | — |

### `Validate()`

Runs DataAnnotations validation (`Validator.TryValidateObject`) and collects error messages. In addition, it applies two custom checks that produce warnings (added to the returned error list but not gating validity):

- If `MaxViolationsToReport < 10`, adds *"Max violations is very low, may limit report completeness"*.
- If `MaxParallelThreads > Environment.ProcessorCount * 2`, adds a message that the thread count exceeds the recommended value.

Returns a `List<string>` of errors; an empty list means the configuration is valid.

### `MergeWithCliOptions(Cli.CliOptions cliOptions)`

Merges CLI options into the options object, giving CLI options priority. Each field is only overwritten when the CLI value differs from the CLI default (so an unset CLI option does not clobber a value loaded from a config file). Notable behaviors:

- `AnalysisTimeoutSeconds` is applied only when the CLI value differs from `300` (the CLI default differs from the options default of `600`).
- `MaxParallelThreads` is applied only when the CLI value differs from `Environment.ProcessorCount`.
- `OutputFormat` and `ReportType` are applied only when non-empty and not the CLI default (`"text"` / `"summary"`).
- `GenerateReport` and `FailOnViolations` are applied only when not `true`.
- `SkipCache` is applied only when not `false`.
- `RuleFilter` is applied only when the CLI list is non-empty.
- `ConfigFile`, `BaselineFile`, `OutputFile`, `ProjectPath` are applied when non-empty/non-null.
- `CreateBaseline` is applied when `true`.

### `ToString()`

Produces a summary string for logging, including `ProjectPath`, `AnalysisTimeoutSeconds`, `MaxViolationsToReport`, `LogLevel`, `OutputFormat`, `GenerateReport`, `MaxParallelThreads`, rule/exclude pattern counts, `BaselineFile`, and `CreateBaseline`.

## `ConfigurationLoader`

Loads and parses configuration files. It uses a lightweight, dependency-free JSON parser (no external NuGet packages). The default configuration file name is `.roslyn-guard.json`.

### `LoadFromFileAsync(string filePath)`

- Throws `ArgumentException` if `filePath` is null or whitespace.
- Throws `FileNotFoundException` if the file does not exist.
- Reads the file and parses it via `ParseConfigurationJson`.
- Wraps any parse/read failure in an `InvalidOperationException` with the file path as context.

### `TryLoadDefaultAsync(string projectPath)`

Walks up the directory tree from `projectPath`, looking for a `.roslyn-guard.json` file in each directory. Returns the first successfully loaded config, or `null` if none is found. If a found file fails to load, it logs a warning to stderr and continues searching upward (returns `null`).

### JSON schema

The parser recognizes the following keys (case-insensitive). Unknown keys are ignored.

| JSON key | Maps to | Type | Notes |
| --- | --- | --- | --- |
| `enabledRules` | `AnalysisConfig.EnabledRules` | array of strings | Each item is trimmed and surrounding quotes removed. |
| `excludePatterns` | `AnalysisConfig.ExcludePatterns` | array of strings | Same trimming as above. |
| `severity` | `AnalysisConfig.MinimumSeverity` | string | Surrounding quotes removed. |
| `maxViolations` | `AnalysisConfig.MaxViolationsToReport` | int | Parsed with `int.TryParse` (invariant culture); ignored if not a valid integer. |
| `enableCaching` | `AnalysisConfig.EnableCaching` | bool | `true` (case-insensitive) enables caching; anything else disables it. |
| `outputFormat` | `AnalysisConfig.OutputFormat` | string | Surrounding quotes removed. |

### `AnalysisConfig`

The parsed configuration data object produced by the loader. Its properties are:

| Property | Type | Default |
| --- | --- | --- |
| `EnabledRules` | `List<string>` | `[]` |
| `ExcludePatterns` | `List<string>` | `[]` |
| `MinimumSeverity` | `string` | `"Low"` |
| `MaxViolationsToReport` | `int` | `1000` |
| `EnableCaching` | `bool` | `true` |
| `OutputFormat` | `string` | `"text"` |

`AnalysisConfig.Validate(out List<string> errors)` checks:

- `MinimumSeverity` is one of `Low`, `Medium`, `High`, `Critical` (case-insensitive).
- `MaxViolationsToReport > 0`.
- `OutputFormat` is one of `text`, `json`, `csv`, `html`, `xml` (case-insensitive).

Returns `true` when no errors are collected.

## `ConfigurationValidator`

Provides static validation methods. All methods return a `ValidationResult` containing `IsValid`, `Errors`, and `Warnings`.

### `ValidationResult`

- `IsValid` (`bool`) — set to `false` when an error is added.
- `Errors` (`List<string>`) — hard failures.
- `Warnings` (`List<string>`) — non-fatal advisories.
- `AddError(string)` — appends an error and sets `IsValid = false`.
- `AddWarning(string)` — appends a warning.
- `ToString()` — renders a human-readable summary with ✓/✗ status and indented error/warning lists.

### `ValidateAnalysisConfig(AnalysisConfig config)`

- Throws `ArgumentNullException` if `config` is null.
- **Errors**:
  - Invalid `MinimumSeverity` (not one of `Low`, `Medium`, `High`, `Critical`, case-insensitive).
  - `MaxViolationsToReport <= 0`.
  - Invalid `OutputFormat` (not one of `text`, `json`, `csv`, `html`, `xml`, case-insensitive).
  - Any `ExcludePatterns` entry that is null or whitespace.
- **Warnings**:
  - `MaxViolationsToReport < 10` (may limit report completeness).
  - `EnabledRules` is empty (no rules explicitly enabled).
  - Duplicate rule names (compared case-insensitively).

### `ValidateCliOptions(Cli.CliOptions options)`

- Throws `ArgumentNullException` if `options` is null.
- **Errors**:
  - `ProjectPath` specified but does not exist as a file or directory.
  - `FilePath` specified but does not exist as a file.
  - `ConfigFile` specified but does not exist as a file.
  - `AnalysisTimeoutSeconds <= 0`.
  - `MaxParallelThreads <= 0`.
- **Warnings**:
  - `MaxParallelThreads > Environment.ProcessorCount * 2`.

### `ValidateRuleNames(IEnumerable<string> ruleNames, IEnumerable<string> supportedRules)`

- Throws `ArgumentNullException` if either argument is null.
- Adds an error for each rule name not present in `supportedRules` (compared case-insensitively).

### `ValidateComprehensive(AnalysisConfig? analysisConfig, Cli.CliOptions? cliOptions)`

Runs `ValidateAnalysisConfig` and `ValidateCliOptions` on whichever inputs are non-null and combines their errors and warnings into a single `ValidationResult`. The combined result is invalid if any constituent result is invalid.

## Validation rules summary

| Concern | Valid values | Failure type |
| --- | --- | --- |
| `MinimumSeverity` | `Low`, `Medium`, `High`, `Critical` | Error |
| `OutputFormat` | `text`, `json`, `csv`, `html`, `xml` | Error |
| `MaxViolationsToReport` | `> 0` | Error |
| `MaxViolationsToReport` | `< 10` | Warning |
| `AnalysisTimeoutSeconds` | `> 0` | Error |
| `MaxParallelThreads` | `1..64` (options) / `> 0` (CLI) | Error |
| `MaxParallelThreads` | `> ProcessorCount * 2` | Warning |
| `LogLevel` | `0..4` | Error |
| `ReportType` | `summary`, `detailed`, `full` | Error |
| `EnabledRules` | empty | Warning |
| Duplicate rules | — | Warning |
| `ExcludePatterns` | no empty entries | Error |
| Rule names | must be supported | Error |
| `ProjectPath` / `FilePath` / `ConfigFile` | must exist when specified | Error |