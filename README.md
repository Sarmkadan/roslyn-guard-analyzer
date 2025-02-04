# Roslyn Guard Analyzer

...

## AnalysisStartedEvent

The `AnalysisStartedEvent` is a notification event that is triggered when an analysis is initiated. It provides information about the project being analyzed, including the project path, analysis ID, and configuration file path.

### Usage Example
```csharp
var eventBus = new EventBus();
eventBus.Subscribe<AnalysisStartedEvent>(handler);

// Subscribe to events of type AnalysisStartedEvent
eventBus.Subscribe<AnalysisStartedEvent>(handler);

// Usage example:
eventBus.Subscribe<AnalysisStartedEvent>(handler => {
    var event = handler.GetEvent();
    Console.WriteLine($"Analysis started for project: {event.ProjectPath}");
    Console.WriteLine($"Analysis ID: {event.AnalysisId}");
    Console.WriteLine($"Config file path: {event.ConfigFilePath}");
});
```

## EventBus

The `EventBus` class is a publish-subscribe event bus that enables decoupling of event producers and consumers. It allows for the registration of event handlers and the publication of events to all subscribed handlers.

### Usage Example
```csharp
var eventBus = new EventBus();

// Subscribe to events of type MyEvent
eventBus.Subscribe<MyEvent>(handler);

// Publish an event
await eventBus.PublishAsync(new MyEvent());

// Unsubscribe from events of type MyEvent
eventBus.Unsubscribe<MyEvent>(handler);

// Clear all subscriptions
eventBus.ClearSubscriptions();
```

## ServiceCollectionExtensions

The `ServiceCollectionExtensions` class provides utility methods for registering and configuring services related to the Roslyn Guard Analyzer. It enables registration of analyzer services, validation-only services, and reporting-only services, as well as configuration of various settings such as data directory, analysis timeout, and log level.

### Usage Example
```csharp
var services = new ServiceCollection();
ServiceCollectionExtensions.RegisterAnalyzerServices(services);
ServiceCollectionExtensions.RegisterValidationOnly(services);
ServiceCollectionExtensions.RegisterReportingOnly(services);

var serviceProvider = services.BuildServiceProvider();
var analyzer = serviceProvider.GetService<Analyzer>();
var validationService = serviceProvider.GetService<ValidationService>();
var reportingService = serviceProvider.GetService<ReportingService>();

await ServiceCollectionExtensions.InitializeAnalyzerAsync(services);
```

## SuppressionManagerExtensions

The `SuppressionManagerExtensions` class provides utility methods to easily interact with suppression records. These extensions enable you to add, remove, and query suppressions for rule violations.

### Usage Example
```csharp
var suppressionManager = new SuppressionManager();

// Add a suppression
var record = suppressionManager.AddSuppression(
    new SuppressionRecord
    {
        RuleId = "LYR001",
        TargetFile = "src/Domain/UserRepository.cs",
        Justification = "Legacy dependency scheduled for refactor",
        Author = "team-maintainer",
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    });

// Check if there are suppressions for a rule
if (suppressionManager.HasActiveSuppressionsForRule("LYR001"))
{
    Console.WriteLine("There are active suppressions for LYR001");
}

// Export active suppressions
var activeSuppressions = suppressionManager.ExportActiveSuppressions();
foreach (var suppression in activeSuppressions)
{
    Console.WriteLine($"Rule {suppression.RuleId} suppressed in {suppression.TargetFile}");
}

// Cleanup expired suppressions
var removedCount = suppressionManager.CleanupExpiredSuppressions();
Console.WriteLine($"Removed {removedCount} expired suppressions");

// Get suppression count
var count = suppressionManager.GetSuppressionCount();
Console.WriteLine($"Total suppressions: {count}");
```

## ResultAggregatorExtensions

The `ResultAggregatorExtensions` class provides utility methods to aggregate and categorize analysis results. It enables summarizing violations by file, rule, and severity, while supporting bulk additions of analysis results.

### Usage Example
```csharp
var aggregator = new ResultAggregator();
var results = new List<AnalysisResult>
{
    new AnalysisResult { Violations = new List<RuleViolation> { new RuleViolation { RuleId = "LYR001", Severity = "High" } } },
    new AnalysisResult { Violations = new List<RuleViolation> { new RuleViolation { RuleId = "LYR002", Severity = "Medium" } } }
};

// Add results in bulk
ResultAggregatorExtensions.AddRange(aggregator, results);

// Get total violations
int total = ResultAggregatorExtensions.GetTotalViolations(aggregator);
Console.WriteLine($"Total violations: {total}");

// Group violations by file
var violationsByFile = ResultAggregatorExtensions.GetViolationsByFile(aggregator);
foreach (var file in violationsByFile)
{
    Console.WriteLine($"File: {file.Key}, Violations: {file.Value.Count}");
}

// Group violations by rule
var violationsByRule = ResultAggregatorExtensions.GetViolationsByRule(aggregator);
foreach (var rule in violationsByRule)
{
    Console.WriteLine($"Rule: {rule.Key}, Violations: {rule.Value.Count}");
}

// Group violations by severity
var violationsBySeverity = ResultAggregatorExtensions.GetViolationsBySeverity(aggregator);
foreach (var severity in violationsBySeverity)
{
    Console.WriteLine($"Severity: {severity.Key}, Violations: {severity.Value.Count}");
}
```

## RuleEngineBenchmarksExtensions

The `RuleEngineBenchmarksExtensions` class provides benchmarking utilities to measure performance characteristics of the rule engine. It supports individual rule benchmarking, scalability testing, and overhead measurement through configurable iterations and element counts.

### Usage Example
```csharp
var benchmarks = new RuleEngineBenchmarksExtensions
{
    WarmupIterations = 5,
    BenchmarkIterations = 10,
    ElementCount = 1000
};

// Benchmark single rule performance
var ruleResult = await benchmarks.BenchmarkRuleAsync("LYR001", 500);
Console.WriteLine($"Rule LYR001: {ruleResult}");

// Benchmark scalability across element counts
var scalabilityResults = await benchmarks.BenchmarkScalabilityAsync(100, 1000);
foreach (var result in scalabilityResults)
{
    Console.WriteLine($"Elements: {result.Key}, {result.Value}");
}
```

## ConfigurationLoaderExtensions

The `ConfigurationLoaderExtensions` class provides utility methods for loading, merging, and querying analysis configurations. It enables rule enablement checks, path exclusion validation, and caching configuration evaluation.

### Usage Example
```csharp
var config = await ConfigurationLoaderExtensions.LoadFromFileAsync("analysis-config.json");
var mergedConfig = await ConfigurationLoaderExtensions.MergeWithDefaultAsync(config);

if (ConfigurationLoaderExtensions.IsRuleEnabled(mergedConfig, "LYR001"))
{
    Console.WriteLine("Rule LYR001 is enabled");
}

if (ConfigurationLoaderExtensions.IsPathExcluded(mergedConfig, "src/Domain/ExcludedFile.cs"))
{
    Console.WriteLine("Path is excluded from analysis");
}

if (ConfigurationLoaderExtensions.ShouldEnableCaching(mergedConfig))
{
    Console.WriteLine("Caching is enabled for this configuration");
}

var clonedConfig = ConfigurationLoaderExtensions.Clone(mergedConfig);
```

## RoslynGuardException

The `RoslynGuardException` is the base exception class for all Roslyn Guard Analyzer errors. It provides standardized error handling with error codes, timestamps, and formatted exception messages. All analyzer-specific exceptions inherit from this base class, ensuring consistent error reporting across the entire codebase.

### Key Properties

- `ErrorCode` - A unique identifier for the error type
- `OccurredAt` - The UTC timestamp when the exception occurred
- `ToString()` - Returns a formatted string including error code, message, and timestamp

### Usage Example

```csharp
try
{
    var analyzer = new Analyzer();
    await analyzer.AnalyzeProjectAsync("src/MyProject.csproj");
}
catch (RoslynGuardException ex) when (ex.ErrorCode == "ERR001")
{
    // Handle specific error code
    Console.WriteLine($"Error occurred: {ex.ToString()}");
    Console.WriteLine($"Error code: {ex.ErrorCode}");
    Console.WriteLine($"Timestamp: {ex.OccurredAt:yyyy-MM-dd HH:mm:ss}");
}
catch (RuleNotFoundException ruleEx)
{
    // Handle rule not found scenario
    Console.WriteLine($"Rule '{ruleEx.RuleId}' was not found in configuration.");
    Console.WriteLine($"Full error: {ruleEx.ToString()}");
}
catch (AnalysisException analysisEx)
{
    // Handle analysis failure with project details
    Console.WriteLine($"Analysis failed for project: {analysisEx.ProjectPath}");
    foreach (var detail in analysisEx.Details)
    {
        Console.WriteLine($"Detail: {detail}");
    }
    Console.WriteLine($"Error: {analysisEx.ToString()}");
}
catch (ConfigurationException configEx)
{
    // Handle configuration issues
    if (configEx.ConfigKey != null)
    {
        Console.WriteLine($"Invalid configuration key: {configEx.ConfigKey}");
    }
    Console.WriteLine($"Configuration error: {configEx.ToString()}");
}
catch (FileAccessException fileEx)
{
    // Handle file I/O errors
    Console.WriteLine($"File access error in: {fileEx.FilePath}");
    Console.WriteLine($"Error: {fileEx.ToString()}");
}
catch (ParseException parseEx)
{
    // Handle parsing failures
    Console.WriteLine($"Parse error in file: {parseEx.FilePath}");
    Console.WriteLine($"Error: {parseEx.ToString()}");
}
```