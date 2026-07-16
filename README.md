# Roslyn Guard Analyzer

...

## Architecture

...

## DiagnosticsService

The `DiagnosticsService` class provides diagnostic information about the analyzer and analysis runs. It tracks performance, errors, and usage statistics.

### Usage Example

```csharp
var diagnostics = new DiagnosticsService();

// Record some analysis runs
diagnostics.RecordAnalysis(1500, 5);
diagnostics.RecordAnalysis(2000, 3);
diagnostics.RecordError("Error during analysis");

// Get diagnostic information
Console.WriteLine($"Analysis count: {diagnostics.GetAnalysisCount()}");
Console.WriteLine($"Total violations found: {diagnostics.GetTotalViolationsFound()}");
Console.WriteLine($"Total analysis time: {diagnostics.TotalAnalysisTimeMs}ms");
Console.WriteLine($"Error count: {diagnostics.GetErrorCount()}");
Console.WriteLine($"Recent errors: {string.Join(", ", diagnostics.GetRecentErrors())}");
Console.WriteLine(diagnostics.GenerateDiagnosticReport());

// Reset statistics
diagnostics.Reset();
Console.WriteLine($"Analysis count after reset: {diagnostics.GetAnalysisCount()}");
```

## ResultAggregator

`ResultAggregator` collects `AnalysisResult` instances from one or many analysis runs and provides convenient aggregation operations such as counting violations, grouping them by rule, severity or file, and generating a consolidated `ViolationReport`. It is useful when you need a single view over multiple projects or files.

### Usage Example

```csharp
using RoslynGuardAnalyzer.Services;
using RoslynGuardAnalyzer.Domain.Models;

// Create the aggregator
var aggregator = new ResultAggregator();

// Add individual results
aggregator.Add(new AnalysisResult { /* populate result */ });

// Or add a collection of results
IEnumerable<AnalysisResult> results = GetResultsFromSomewhere();
aggregator.AddRange(results);

// Query aggregated data
int totalViolations = aggregator.GetTotalViolations();
IEnumerable<RuleViolation> allViolations = aggregator.GetAllViolations();
var byRule = aggregator.GetViolationsByRule();
var bySeverity = aggregator.GetViolationsBySeverity();
var byFile = aggregator.GetViolationsByFile();

int totalFiles = aggregator.GetTotalFilesAnalyzed();
int totalElements = aggregator.GetTotalElementsAnalyzed();

// Generate a summary report
ViolationReport report = aggregator.GenerateSummaryReport();
Console.WriteLine(report);

// Get detailed statistics
var stats = aggregator.GetStatistics();
foreach (var kvp in stats)
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
```

...

