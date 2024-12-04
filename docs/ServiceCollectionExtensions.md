# ServiceCollectionExtensions

Provides extension methods for registering and configuring Roslyn Guard Analyzer services within an `IServiceCollection`, along with configuration properties and methods for analyzer behavior customization.

## API

### `public static void RegisterAnalyzerServices(IServiceCollection services)`
Registers the core analyzer services including diagnostics, reporting, and validation components into the dependency injection container.

**Parameters**
- `services`: The `IServiceCollection` to add services to.

**Throws**
- `ArgumentNullException` if `services` is null.

---

### `public static void RegisterAnalyzerServices(IServiceCollection services, Action<AnalyzerConfiguration> configure)`
Registers the core analyzer services and applies a configuration delegate to customize analyzer behavior.

**Parameters**
- `services`: The `IServiceCollection` to add services to.
- `configure`: An action to configure the `AnalyzerConfiguration` instance.

**Throws**
- `ArgumentNullException` if `services` or `configure` is null.

---

### `public static async Task InitializeAnalyzerAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)`
Initializes the analyzer engine asynchronously, loading rules and preparing the analysis pipeline.

**Parameters**
- `serviceProvider`: The `IServiceProvider` to resolve registered analyzer services.
- `cancellationToken`: A token to observe while waiting for initialization to complete.

**Returns**
- A `Task` representing the asynchronous initialization operation.

**Throws**
- `ArgumentNullException` if `serviceProvider` is null.
- `InvalidOperationException` if analyzer services have not been registered.
- `OperationCanceledException` if the operation is canceled via `cancellationToken`.

---

### `public static void RegisterValidationOnly(IServiceCollection services)`
Registers only the validation services, excluding reporting and detailed diagnostics components.

**Parameters**
- `services`: The `IServiceCollection` to add services to.

**Throws**
- `ArgumentNullException` if `services` is null.

---

### `public static void RegisterReportingOnly(IServiceCollection services)`
Registers only the reporting services, excluding validation and analysis execution components.

**Parameters**
- `services`: The `IServiceCollection` to add services to.

**Throws**
- `ArgumentNullException` if `services` is null.

---

### `public static void ConfigureAnalyzer(IServiceCollection services, Action<AnalyzerConfiguration> configure)`
Applies a configuration delegate to an existing `AnalyzerConfiguration` instance in the service collection.

**Parameters**
- `services`: The `IServiceCollection` containing the analyzer configuration.
- `configure`: An action to modify the `AnalyzerConfiguration` instance.

**Throws**
- `ArgumentNullException` if `services` or `configure` is null.
- `InvalidOperationException` if `AnalyzerConfiguration` has not been registered.

---

### `public string DataDirectory`
Gets or sets the directory path where analyzer data, caches, and temporary files are stored.

**Remarks**
Defaults to a subdirectory in the application's base directory. Must be a valid writable path.

---

### `public int MaxViolationsToReport`
Gets or sets the maximum number of violations to include in a single report.

**Remarks**
Defaults to 1000. Values less than 1 are treated as unlimited.

---

### `public int AnalysisTimeoutSeconds`
Gets or sets the timeout in seconds for a single analysis operation.

**Remarks**
Defaults to 300 seconds. Values less than 1 disable the timeout.

---

### `public bool FailOnError`
Gets or sets a value indicating whether the analyzer should treat errors as fatal and halt execution.

**Remarks**
Defaults to `false`. When `true`, analysis exceptions will propagate rather than being logged.

---

### `public bool GenerateDetailedReports`
Gets or sets a value indicating whether detailed diagnostic reports should be generated.

**Remarks**
Defaults to `false`. Enabling this increases memory and disk usage.

---

### `public string DefaultReportFormat`
Gets or sets the default output format for generated reports.

**Remarks**
Supported values: "json", "xml", "html", "sarif". Defaults to "json".

---

### `public int LogLevel`
Gets or sets the verbosity level for analyzer logging.

**Remarks**
Values: 0 (None), 1 (Error), 2 (Warning), 3 (Info), 4 (Debug), 5 (Trace). Defaults to 2 (Warning).

---

### `public bool UseParallelAnalysis`
Gets or sets a value indicating whether parallel analysis should be used when processing multiple projects.

**Remarks**
Defaults to `true`. When enabled, `MaxParallelThreads` controls concurrency.

---

### `public int MaxParallelThreads`
Gets or sets the maximum number of parallel threads for analysis operations.

**Remarks**
Defaults to `Environment.ProcessorCount`. Values less than 1 use the processor count.

---

### `public bool IsValid`
Gets a value indicating whether the current configuration is valid for analysis operations.

**Returns**
`true` if all required paths exist and settings are within acceptable ranges; otherwise, `false`.

---

### `public void EnsureDataDirectoryExists()`
Creates the data directory specified by `DataDirectory` if it does not already exist.

**Throws**
- `UnauthorizedAccessException` if the process lacks permission to create the directory.
- `IOException` if a file exists with the same path or other I/O error occurs.

---

### `public AnalyzerConfiguration Clone()`
Creates a deep copy of the current configuration instance.

**Returns**
A new `AnalyzerConfiguration` instance with identical property values.

## Usage

### Basic Service Registration and Initialization

```csharp
using Microsoft.Extensions.DependencyInjection;
using RoslynGuard.Analyzer;

var services = new ServiceCollection();

services.RegisterAnalyzerServices(configure: config =>
{
    config.DataDirectory = @"C:\RoslynGuard\Data";
    config.MaxViolationsToReport = 500;
    config.AnalysisTimeoutSeconds = 120;
    config.FailOnError = true;
    config.GenerateDetailedReports = true;
    config.DefaultReportFormat = "sarif";
    config.LogLevel = 3;
    config.UseParallelAnalysis = true;
    config.MaxParallelThreads = 4;
});

var serviceProvider = services.BuildServiceProvider();

await ServiceCollectionExtensions.InitializeAnalyzerAsync(serviceProvider);
```

### Selective Registration for Validation-Only Pipeline

```csharp
using Microsoft.Extensions.DependencyInjection;
using RoslynGuard.Analyzer;

var services = new ServiceCollection();

services.RegisterValidationOnly();

var config = new AnalyzerConfiguration
{
    DataDirectory = Path.Combine(AppContext.BaseDirectory, "guard-data"),
    MaxViolationsToReport = 200,
    AnalysisTimeoutSeconds = 60,
    FailOnError = false,
    GenerateDetailedReports = false,
    DefaultReportFormat = "json",
    LogLevel = 2,
    UseParallelAnalysis = false
};

config.EnsureDataDirectoryExists();

services.ConfigureAnalyzer(c =>
{
    c.DataDirectory = config.DataDirectory;
    c.MaxViolationsToReport = config.MaxViolationsToReport;
    c.AnalysisTimeoutSeconds = config.AnalysisTimeoutSeconds;
    c.FailOnError = config.FailOnError;
    c.GenerateDetailedReports = config.GenerateDetailedReports;
    c.DefaultReportFormat = config.DefaultReportFormat;
    c.LogLevel = config.LogLevel;
    c.UseParallelAnalysis = config.UseParallelAnalysis;
    c.MaxParallelThreads = config.MaxParallelThreads;
});

var provider = services.BuildServiceProvider();
await ServiceCollectionExtensions.InitializeAnalyzerAsync(provider);
```

## Notes

- **Thread Safety**: The static registration methods are thread-safe for concurrent calls during application startup. The `AnalyzerConfiguration` instance properties are not thread-safe; modifications should occur before `BuildServiceProvider` or within a synchronized block.
- **Configuration Validation**: `IsValid` performs synchronous checks on path accessibility and numeric bounds. Call it after setting all properties to verify readiness before analysis.
- **Data Directory**: `EnsureDataDirectoryExists` should be called explicitly if `DataDirectory` is set to a non-default path before `InitializeAnalyzerAsync`, as the initializer does not automatically create directories.
- **Timeout Behavior**: `AnalysisTimeoutSeconds` applies per-analysis-operation. Long-running solutions with many projects may exceed this if `UseParallelAnalysis` is false; consider increasing the value or enabling parallelism.
- **Cloning**: `Clone` produces a fully independent copy suitable for scenarios requiring multiple analyzer configurations (e.g., different rule sets per project). The cloned instance shares no mutable state with the original.
- **Service Lifetime**: Services registered via `RegisterAnalyzerServices` are scoped by default. `RegisterValidationOnly` and `RegisterReportingOnly` register transient services to minimize memory footprint in limited contexts.
- **Initialization Order**: `InitializeAnalyzerAsync` must be called after `BuildServiceProvider` and before any analysis operations. Calling it multiple times is idempotent but incurs overhead on subsequent calls.
