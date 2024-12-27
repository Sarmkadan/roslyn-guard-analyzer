# ConfigurationLoader

`ConfigurationLoader` provides the central mechanism for loading, parsing, and validating analysis configuration within the `roslyn-guard-analyzer` project. It supports loading configuration from explicit file paths as well as automatic discovery of default configuration files, and exposes properties that govern rule enablement, severity thresholds, output formatting, and caching behavior.

## API

### `public static async Task<AnalysisConfig> LoadFromFileAsync(string filePath)`

Loads analysis configuration from a specified file path.

- **Purpose**: Reads and deserializes a configuration file into an `AnalysisConfig` object. The method performs full validation if the `Validate` property on the resulting instance is `true`.
- **Parameters**:
  - `filePath` (`string`): The absolute or relative path to the configuration file.
- **Return Value**: A fully populated and optionally validated `AnalysisConfig` instance.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `filePath` is `null`.
  - `FileNotFoundException`: Thrown when the specified file does not exist.
  - `InvalidOperationException`: Thrown when the file content cannot be deserialized or validation fails.

### `public static async Task<AnalysisConfig?> TryLoadDefaultAsync()`

Attempts to locate and load a default configuration file from predefined locations.

- **Purpose**: Searches standard locations (current directory, user profile directory, and application base directory) for a file named `roslyn-guard.json` or `.roslyn-guard.json`. Returns `null` if no default file is found rather than throwing.
- **Return Value**: An `AnalysisConfig` instance if a default file is found and successfully loaded; `null` otherwise.
- **Exceptions**: Does not throw for missing files. May throw `InvalidOperationException` if a file is found but contains invalid content.

### `public List<string> EnabledRules`

- **Purpose**: Specifies the collection of rule identifiers that are active during analysis. Rules not present in this list are suppressed.
- **Type**: `List<string>`

### `public List<string> ExcludePatterns`

- **Purpose**: Defines glob-style patterns for files and directories to exclude from analysis. Supports standard wildcard characters (`*`, `**`, `?`).
- **Type**: `List<string>`

### `public string MinimumSeverity`

- **Purpose**: Sets the minimum diagnostic severity required for a violation to be reported. Violations below this threshold are filtered out.
- **Type**: `string` (expected values: `"Hidden"`, `"Info"`, `"Warning"`, `"Error"`).

### `public int MaxViolationsToReport`

- **Purpose**: Caps the total number of violations emitted per analysis run. When the limit is reached, remaining violations are suppressed.
- **Type**: `int`

### `public bool EnableCaching`

- **Purpose**: Controls whether analysis results are cached between runs. When enabled, unchanged files may reuse previous results to improve performance.
- **Type**: `bool`

### `public string OutputFormat`

- **Purpose**: Determines the format used for outputting analysis results.
- **Type**: `string` (expected values: `"Console"`, `"Json"`, `"Sarif"`).

### `public bool Validate`

- **Purpose**: When set to `true`, the configuration is validated immediately upon loading. Validation checks for rule ID conflicts, severity string validity, and pattern syntax correctness.
- **Type**: `bool`

## Usage

### Example 1: Loading from an explicit file path

```csharp
using System;
using System.Threading.Tasks;

public async Task AnalyzeWithCustomConfig()
{
    string configPath = @"C:\Projects\GuardConfig\analysis-config.json";

    AnalysisConfig config = await ConfigurationLoader.LoadFromFileAsync(configPath);

    Console.WriteLine($"Loaded {config.EnabledRules.Count} enabled rules");
    Console.WriteLine($"Minimum severity: {config.MinimumSeverity}");
    Console.WriteLine($"Output format: {config.OutputFormat}");

    // Proceed with analysis using the loaded configuration
}
```

### Example 2: Attempting default configuration with fallback

```csharp
using System;
using System.Threading.Tasks;

public async Task AnalyzeWithDefaultOrFallback()
{
    AnalysisConfig? config = await ConfigurationLoader.TryLoadDefaultAsync();

    if (config is null)
    {
        Console.WriteLine("No default configuration found; using built-in defaults.");
        config = new AnalysisConfig
        {
            EnabledRules = new List<string> { "RG0001", "RG0002" },
            MinimumSeverity = "Warning",
            OutputFormat = "Console",
            MaxViolationsToReport = 100
        };
    }

    if (config.Validate)
    {
        Console.WriteLine("Configuration validation is active.");
    }

    // Proceed with analysis
}
```

## Notes

- **File discovery for `TryLoadDefaultAsync`**: The method searches in order: the current working directory, the user profile directory (`%USERPROFILE%` on Windows, `$HOME` on Unix), and the directory containing the executing assembly. The first matching file is used.
- **Validation behavior**: When `Validate` is `true`, `LoadFromFileAsync` performs validation synchronously after deserialization. Validation failures result in an `InvalidOperationException` with details about the specific errors encountered.
- **Thread safety**: The static methods `LoadFromFileAsync` and `TryLoadDefaultAsync` are safe to call from multiple threads concurrently. Instance properties on the returned `AnalysisConfig` object are not synchronized; external synchronization is required if an instance is mutated across threads.
- **Empty collections**: `EnabledRules` and `ExcludePatterns` may be empty. An empty `EnabledRules` list means no rules are active, effectively disabling all analysis. An empty `ExcludePatterns` list means no files are excluded.
- **Severity comparison**: The `MinimumSeverity` string is compared ordinally against the predefined severity hierarchy. Values outside the recognized set cause validation errors when `Validate` is enabled.
- **Caching scope**: When `EnableCaching` is `true`, the caching scope is limited to the current analysis session. Caches are not persisted across process invocations unless explicitly implemented by the consuming code.
