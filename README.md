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

## IFixAllProvider

The `IFixAllProvider` interface coordinates bulk preview and application of code fixes for collections of rule violations. It enables applying fixes to multiple violations at once with configurable filtering options such as severity thresholds, rule selection, and breaking change handling. The provider returns detailed results including execution metrics, success status, and any violations that could not be fixed.

### Usage Example

```csharp
// Create required services
var codeFixService = new CodeFixService();
var logger = new Logger<FixAllProvider>(new LoggerFactory());
var fixAllProvider = new FixAllProvider(codeFixService, logger);

// Define violations to fix (typically from analysis results)
var violations = new List<RuleViolation>
{
    new RuleViolation
    {
        Id = "LYR001-001",
        RuleId = "LYR001",
        FilePath = "src/Domain/UserRepository.cs",
        LineNumber = 42,
        Message = "Consider using nullable reference types",
        Severity = SeverityLevel.Warning
    },
    new RuleViolation
    {
        Id = "LYR002-002",
        RuleId = "LYR002",
        FilePath = "src/Application/Services/UserService.cs",
        LineNumber = 87,
        Message = "Exception should not be caught in application layer",
        Severity = SeverityLevel.Error
    }
};

// Configure fix options
var options = new FixAllOptions
{
    DryRun = false, // Set to true to preview changes without applying
    MinimumSeverity = SeverityLevel.Warning, // Only fix warnings and errors
    RuleIds = new List<string> { "LYR001" }, // Only fix specific rules
    SkipBreakingChanges = true, // Skip fixes marked as breaking changes
    MaxFixes = 10 // Limit number of fixes applied
};

// Apply all fixes
var result = await fixAllProvider.ApplyAllAsync(violations, options);

// Inspect results
Console.WriteLine($"Total violations: {result.TotalViolations}");
Console.WriteLine($"Fixable violations: {result.FixableViolations}");
Console.WriteLine($"Fixes applied: {result.FixResult.AppliedFixes.Count}");
Console.WriteLine($"Duration: {result.Duration.TotalMilliseconds}ms");
Console.WriteLine($"Success: {result.IsSuccess}");

if (result.UnfixableViolations.Any())
{
    Console.WriteLine($"Could not fix {result.UnfixableViolations.Count} violations:");
    foreach (var violation in result.UnfixableViolations)
    {
        Console.WriteLine($"  - {violation.RuleId} at {violation.FilePath}:{violation.LineNumber}");
    }
}

foreach (var message in result.Messages)
{
    Console.WriteLine(message);
}
```

## CodeFix

The `CodeFix` class represents an auto-fix action that can be applied to a source file to resolve a detected architectural rule violation. It contains all the information necessary to identify where the violation occurred, what needs to be changed, and how to make the change safely. Code fixes are generated by analyzers and can be applied individually or in bulk using the `IFixAllProvider` interface.



### Key Properties

- `Id` - Unique identifier for this specific fix instance
- `ViolationId` - Identifier of the rule violation this fix addresses
- `RuleId` - Rule identifier that produced this fix
- `Title` - Human-readable title describing the fix action
- `Description` - Detailed description of what this fix will change
- `FilePath` - Absolute path to the file where the fix should be applied
- `StartLine` - 1-based line number where the fix begins
- `EndLine` - 1-based line number where the fix ends
- `OriginalCode` - Exact code token that will be replaced
- `ReplacementCode` - Replacement code that resolves the violation
- `Severity` - Severity level inherited from the source violation
- `GeneratedAt` - UTC timestamp when this fix was generated
- `IsBreakingChange` - Whether applying this fix may alter observable behavior
- `IsValid()` - Method to validate if the fix has all required data
- `GetSummary()` - Returns a formatted summary string for display

### Usage Example

```csharp
// Create a code fix for a violation where a public method should be made internal
var fix = new CodeFix
{
    Id = Guid.NewGuid().ToString(),
    ViolationId = "ARCH-001",
    RuleId = "LYR001",
    Title = "Make method internal to reduce API surface",
    Description = "This public method should be made internal as it's only used within this assembly",
    FilePath = "/home/user/project/src/Domain/UserService.cs",
    StartLine = 42,
    EndLine = 42,
    OriginalCode = "public void ProcessUser(User user)",
    ReplacementCode = "internal void ProcessUser(User user)",
    Severity = SeverityLevel.Warning,
    IsBreakingChange = false,
    GeneratedAt = DateTime.UtcNow
};

// Validate the fix has all required data
if (fix.IsValid())
{
    Console.WriteLine($"Fix is valid: {fix.GetSummary()}");
    
    // Apply the fix to the source file
    Console.WriteLine($"Applying fix to {fix.FilePath} at line {fix.StartLine}");
    Console.WriteLine($"Replacing: {fix.OriginalCode}");
    Console.WriteLine($"With: {fix.ReplacementCode}");
}
else
{
    Console.WriteLine("Fix is missing required data and cannot be applied");
}

// Example with a breaking change fix (renaming a public API)
var breakingFix = new CodeFix
{
    Id = Guid.NewGuid().ToString(),
    ViolationId = "API-002",
    RuleId = "LYR002",
    Title = "Rename public method to follow naming conventions",
    Description = "Public API method name should be PascalCase to follow .NET conventions",
    FilePath = "/home/user/project/src/Application/UserController.cs",
    StartLine = 87,
    EndLine = 87,
    OriginalCode = "public void getUserById(int id)",
    ReplacementCode = "public User GetUserById(int id)",
    Severity = SeverityLevel.Error,
    IsBreakingChange = true, // This is a breaking change as it affects public API
    GeneratedAt = DateTime.UtcNow
};

Console.WriteLine($"Breaking change fix generated: {breakingFix.GetSummary()}");
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