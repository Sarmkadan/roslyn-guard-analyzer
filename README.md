# Roslyn Guard Analyzer

...

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
