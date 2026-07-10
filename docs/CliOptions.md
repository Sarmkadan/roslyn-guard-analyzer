# CliOptions

Configuration container for command-line arguments used by the Roslyn Guard Analyzer tool. Encapsulates project analysis settings, output preferences, and runtime behavior controls.

## API

### `public string? ProjectPath`
Gets or sets the path to the project file (`.csproj`) or solution file (`.sln`) to analyze. If `null`, the tool may attempt to infer the target from the current directory or fail with an error.

### `public string? FilePath`
Gets or sets the path to a single source file (`.cs`) to analyze. When specified, overrides `ProjectPath` and restricts analysis to the given file. If `null`, analysis proceeds at the project or solution level.

### `public string OutputFormat`
Gets or sets the output format for analysis results. Must be one of the supported formats (e.g., `"text"`, `"json"`, `"sarif"`). Defaults to `"text"` if not explicitly set. Invalid values may cause the tool to terminate with an error.

### `public string? OutputFile`
Gets or sets the file path where analysis results should be written. If `null`, results are written to standard output. Parent directories must exist or the tool will fail unless `Verbose` is enabled.

### `public bool Verbose`
Gets or sets a value indicating whether verbose logging is enabled. When `true`, additional diagnostic information is printed to standard error. Does not affect result output unless `OutputFile` is also set.

### `public bool ShowHelp`
Gets or sets a value indicating whether to display usage instructions and exit. When `true`, the tool prints help text and terminates immediately without performing analysis.

### `public bool ShowVersion`
Gets or sets a value indicating whether to display version information and exit. When `true`, the tool prints version details and terminates immediately without performing analysis.

### `public int MaxParallelThreads`
Gets or sets the maximum number of parallel threads to use during analysis. Must be a positive integer. Defaults to the number of logical processors if not specified. Values less than `1` are treated as `1`.

### `public int AnalysisTimeoutSeconds`
Gets or sets the maximum duration (in seconds) allowed for the analysis phase. Must be a non-negative integer. Defaults to `300` (5 minutes). A value of `0` indicates no timeout.

### `public List<string> RuleFilter`
Gets the list of rule identifiers to include or exclude during analysis. Each entry may be a rule ID, category, or wildcard pattern. Empty list implies all rules are enabled. Mutations to the list affect subsequent analysis behavior.

### `public bool FailOnViolations`
Gets or sets a value indicating whether the tool should exit with a non-zero status code if any violations are detected. When `false`, the tool exits with `0` regardless of findings unless other errors occur.

### `public string? ConfigFile`
Gets or sets the path to a configuration file (e.g., `.editorconfig`, JSON, or XML) containing additional analysis settings. If `null`, only command-line and default settings are used. The file must exist and be readable or the tool will fail.

### `public bool GenerateReport`
Gets or sets a value indicating whether to generate a summary report after analysis. When `true`, a report is written to `OutputFile` (or standard output) in the format specified by `ReportType`. Ignored if `OutputFile` is `null` and standard output is not redirected.

### `public string ReportType`
Gets or sets the format of the generated report. Must be one of `"text"`, `"json"`, `"html"`, or `"md"`. Defaults to `"text"` if not specified. Invalid values may cause the report generation to be skipped.

### `public bool SkipCache`
Gets or sets a value indicating whether to bypass any cached analysis results. When `true`, all files are re-analyzed regardless of prior results. Useful for ensuring up-to-date findings but increases runtime.

### `public int LogLevel`
Gets or sets the minimum severity level for log messages. Must be an integer between `0` (trace) and `5` (critical). Defaults to `3` (info). Messages below the specified level are suppressed.

### `public bool Validate`
Gets or sets a value indicating whether to validate configuration and arguments without performing analysis. When `true`, the tool checks all settings, file paths, and rule filters, then exits with success or failure accordingly.

### `public string? GetTargetPath()`
Returns the effective target path for analysis, derived from `ProjectPath` or `FilePath`. If both are `null`, returns `null`. Otherwise, returns the first non-null value. This method does not throw; it always returns a string or `null`.

### `public override string ToString()`
Returns a human-readable string representation of the current configuration, suitable for logging or debugging. Includes all public properties and their current values. Does not include sensitive data (e.g., file paths are truncated). The format is implementation-defined and may change between versions.

## Usage
