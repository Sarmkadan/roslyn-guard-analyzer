# Roslyn Guard Analyzer

...

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
