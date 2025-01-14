# ConfigurationLoaderExtensions

The `ConfigurationLoaderExtensions` class provides a set of static utility methods designed to facilitate the loading, merging, and evaluation of analysis configurations within the Roslyn Guard Analyzer ecosystem. It serves as the primary entry point for retrieving `AnalysisConfig` instances from persistent storage, applying default values, and performing runtime checks to determine if specific rules or paths should be processed based on the current configuration state.

## API

### `LoadFromFileAsync`
```csharp
public static async Task<AnalysisConfig> LoadFromFileAsync(string filePath, CancellationToken cancellationToken = default)
```
Asynchronously reads and deserializes an analysis configuration from the specified file path.
*   **Parameters**:
    *   `filePath`: The absolute or relative path to the configuration file.
    *   `cancellationToken`: A token to cancel the asynchronous operation.
*   **Returns**: A task representing the asynchronous operation, containing the populated `AnalysisConfig` object.
*   **Throws**: Throws `FileNotFoundException` if the specified path does not exist, or `InvalidOperationException` if the file content is malformed or cannot be deserialized into a valid configuration.

### `MergeWithDefaultAsync`
```csharp
public static async Task<AnalysisConfig> MergeWithDefaultAsync(AnalysisConfig userConfig, CancellationToken cancellationToken = default)
```
Merges a user-provided configuration with the system's default settings, ensuring that any missing values in the user configuration are filled by defaults without overwriting explicit user definitions.
*   **Parameters**:
    *   `userConfig`: The base configuration loaded from user input or a file. If null, the method returns a pure default configuration.
    *   `cancellationToken`: A token to cancel the asynchronous operation.
*   **Returns**: A task containing a new `AnalysisConfig` instance representing the merged result.
*   **Throws**: Throws `ArgumentNullException` if internal default resources cannot be accessed.

### `IsRuleEnabled`
```csharp
public static bool IsRuleEnabled(AnalysisConfig config, string ruleId)
```
Determines whether a specific analyzer rule is active based on the provided configuration.
*   **Parameters**:
    *   `config`: The active analysis configuration.
    *   `ruleId`: The unique identifier of the rule to check (e.g., "RG001").
*   **Returns**: `true` if the rule is explicitly enabled or not explicitly disabled; `false` if the rule is explicitly disabled in the configuration.
*   **Throws**: Throws `ArgumentNullException` if `config` or `ruleId` is null.

### `IsPathExcluded`
```csharp
public static bool IsPathExcluded(AnalysisConfig config, string fullPath)
```
Checks if a given file path matches any exclusion patterns defined in the configuration.
*   **Parameters**:
    *   `config`: The active analysis configuration containing exclusion rules.
    *   `fullPath`: The absolute path of the file to evaluate.
*   **Returns**: `true` if the path matches an exclusion pattern; otherwise, `false`.
*   **Throws**: Throws `ArgumentNullException` if `config` or `fullPath` is null.

### `Clone`
```csharp
public static AnalysisConfig Clone(AnalysisConfig source)
```
Creates a deep copy of the provided `AnalysisConfig` instance.
*   **Parameters**:
    *   `source`: The configuration object to clone.
*   **Returns**: A new `AnalysisConfig` instance with identical property values to the source, ensuring nested collections are also copied.
*   **Throws**: Throws `ArgumentNullException` if `source` is null.

### `ShouldEnableCaching`
```csharp
public static bool ShouldEnableCaching(AnalysisConfig config)
```
Evaluates the configuration to determine if result caching mechanisms should be activated for the current analysis run.
*   **Parameters**:
    *   `config`: The active analysis configuration.
*   **Returns**: `true` if caching is permitted and configured; `false` otherwise.
*   **Throws**: Throws `ArgumentNullException` if `config` is null.

## Usage

### Loading and Merging Configuration
The following example demonstrates how to load a configuration file from disk, merge it with default settings to ensure completeness, and verify if caching is enabled before starting an analysis session.

```csharp
using RoslynGuardAnalyzer;

public async Task InitializeAnalysisAsync()
{
    var configPath = "./roslyn-guard.config.json";
    
    // Load user settings
    var userConfig = await ConfigurationLoaderExtensions.LoadFromFileAsync(configPath);
    
    // Merge with system defaults to handle missing optional fields
    var finalConfig = await ConfigurationLoaderExtensions.MergeWithDefaultAsync(userConfig);
    
    if (ConfigurationLoaderExtensions.ShouldEnableCaching(finalConfig))
    {
        Console.WriteLine("Analysis caching is enabled.");
        // Initialize cache provider...
    }
    
    // Proceed with analysis using finalConfig
}
```

### Runtime Rule and Path Evaluation
This example illustrates how to use the extension methods during the traversal of source files to dynamically decide whether to analyze a specific file or apply a specific rule.

```csharp
using RoslynGuardAnalyzer;

public void ProcessSourceFile(AnalysisConfig config, string filePath, string ruleId)
{
    // Skip processing if the file path is excluded
    if (ConfigurationLoaderExtensions.IsPathExcluded(config, filePath))
    {
        return;
    }

    // Skip specific rule logic if the rule is disabled
    if (!ConfigurationLoaderExtensions.IsRuleEnabled(config, ruleId))
    {
        return;
    }

    // Execute rule logic
    RunRuleAnalysis(ruleId, filePath);
}
```

## Notes

*   **Thread Safety**: All methods in `ConfigurationLoaderExtensions` are static and designed to be thread-safe. However, the `AnalysisConfig` instances returned by `LoadFromFileAsync`, `MergeWithDefaultAsync`, and `Clone` are mutable. If a single configuration instance is shared across multiple threads, external synchronization is required when modifying its properties. The `Clone` method should be used to create thread-local copies if concurrent modification is anticipated.
*   **Asynchronous Disposal**: The `LoadFromFileAsync` and `MergeWithDefaultAsync` methods utilize `CancellationToken` to support graceful cancellation. Callers should ensure the token is propagated correctly to avoid resource leaks during application shutdown.
*   **Path Normalization**: The `IsPathExcluded` method expects `fullPath` to be an absolute path. Passing relative paths may result in incorrect matching behavior depending on the current working directory at the time of execution.
*   **Null Handling**: While the boolean evaluation methods (`IsRuleEnabled`, `IsPathExcluded`, `ShouldEnableCaching`) throw `ArgumentNullException` for null inputs, they do not swallow internal configuration errors. If the `AnalysisConfig` object is in an invalid state (e.g., corrupted internal lists), these methods may propagate underlying collection exceptions.
