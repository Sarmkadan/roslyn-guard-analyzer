# ResultAggregator

The `ResultAggregator` class serves as a central collection and analysis hub for static analysis outcomes within the `roslyn-guard-analyzer` project. It accumulates individual `RuleViolation` instances and `AnalysisResult` objects generated during the scanning process, providing mechanisms to query aggregated data by various dimensions such as rule ID, severity level, or source file. Additionally, it offers statistical summaries and report generation capabilities to facilitate the final output of the analysis run.

## API

### `Add`
Adds a single rule violation to the aggregator.
*   **Parameters**: Accepts a `RuleViolation` instance representing a specific finding.
*   **Returns**: `void`.
*   **Throws**: May throw `ArgumentNullException` if the provided violation is null.

### `AddRange`
Adds a collection of rule violations to the aggregator in a single operation.
*   **Parameters**: Accepts an `IEnumerable<RuleViolation>` containing multiple violations.
*   **Returns**: `void`.
*   **Throws**: May throw `ArgumentNullException` if the collection is null, or `ArgumentException` if any element within the collection is null.

### `GetTotalViolations`
Retrieves the cumulative count of all violations currently stored in the aggregator.
*   **Parameters**: None.
*   **Returns**: `int` representing the total number of violations.
*   **Throws**: None.

### `GetAllViolations`
Returns a flat enumeration of every violation recorded.
*   **Parameters**: None.
*   **Returns**: `IEnumerable<RuleViolation>` containing all stored violations.
*   **Throws**: None.

### `GetViolationsByRule`
Groups all stored violations by their associated rule identifier.
*   **Parameters**: None.
*   **Returns**: `Dictionary<string, List<RuleViolation>>` where the key is the rule ID and the value is a list of violations for that rule.
*   **Throws**: None.

### `GetViolationsBySeverity`
Groups all stored violations by their severity level (e.g., Error, Warning, Info).
*   **Parameters**: None.
*   **Returns**: `Dictionary<string, List<RuleViolation>>` where the key is the severity name and the value is a list of violations matching that severity.
*   **Throws**: None.

### `GetViolationsByFile`
Groups all stored violations by the source file path in which they were detected.
*   **Parameters**: None.
*   **Returns**: `Dictionary<string, List<RuleViolation>>` where the key is the file path and the value is a list of violations found in that file.
*   **Throws**: None.

### `GetTotalFilesAnalyzed`
Returns the count of unique files that have been processed and contributed to the current aggregation.
*   **Parameters**: None.
*   **Returns**: `int` representing the number of unique files.
*   **Throws**: None.

### `GetTotalElementsAnalyzed`
Returns the total count of syntax elements or code constructs inspected during the analysis sessions added to this aggregator.
*   **Parameters**: None.
*   **Returns**: `int` representing the total element count.
*   **Throws**: None.

### `GenerateSummaryReport`
Constructs a comprehensive report object summarizing the current state of the aggregation.
*   **Parameters**: None.
*   **Returns**: `ViolationReport` containing formatted summary data, statistics, and grouped violations.
*   **Throws**: None.

### `GetStatistics`
Retrieves a dynamic set of statistical metrics regarding the analysis run.
*   **Parameters**: None.
*   **Returns**: `Dictionary<string, object>` containing key-value pairs of statistical data (e.g., average violations per file, distribution metrics).
*   **Throws**: None.

### `Clear`
Removes all stored violations, results, and reset statistical counters, returning the aggregator to its initial empty state.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: None.

### `GetResult`
Retrieves the primary or most recent consolidated analysis result if one exists.
*   **Parameters**: None.
*   **Returns**: `AnalysisResult?` (nullable); returns `null` if no results have been added.
*   **Throws**: None.

### `GetAllResults`
Returns a read-only list of all distinct `AnalysisResult` objects added to the aggregator.
*   **Parameters**: None.
*   **Returns**: `IReadOnlyList<AnalysisResult>` containing all stored results.
*   **Throws**: None.

## Usage

### Example 1: Accumulating Violations and Generating a Report
This example demonstrates initializing an aggregator, populating it with violations from multiple sources, and generating a final summary report.

```csharp
var aggregator = new ResultAggregator();

// Simulate adding violations from different analyzers
var violations = new List<RuleViolation>
{
    new RuleViolation("CA1001", "Types that own disposable fields should be disposable", Severity.Error, "Program.cs"),
    new RuleViolation("CA1303", "Do not pass literals as localized parameters", Severity.Warning, "Utils.cs")
};

aggregator.AddRange(violations);
aggregator.Add(new RuleViolation("CA1822", "Mark members as static", Severity.Info, "Program.cs"));

// Generate the final report
var report = aggregator.GenerateSummaryReport();

Console.WriteLine($"Total Violations: {aggregator.GetTotalViolations()}");
Console.WriteLine($"Files Analyzed: {aggregator.GetTotalFilesAnalyzed()}");
Console.WriteLine($"Report Generated: {report.Timestamp}");
```

### Example 2: Filtering and Statistical Analysis
This example illustrates how to query specific subsets of data and retrieve raw statistics for custom processing.

```csharp
var aggregator = new ResultAggregator();
// ... (assume aggregator is populated) ...

// Group violations by severity to process errors separately
var bySeverity = aggregator.GetViolationsBySeverity();
if (bySeverity.TryGetValue("Error", out var errors))
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Critical Issue in {error.FilePath}: {error.Message}");
    }
}

// Retrieve raw statistics for logging
var stats = aggregator.GetStatistics();
if (stats.ContainsKey("AverageViolationsPerFile"))
{
    Console.WriteLine($"Avg Violations/File: {stats["AverageViolationsPerFile"]}");
}

// Access specific analysis results
var allResults = aggregator.GetAllResults();
var latestResult = aggregator.GetResult();
```

## Notes

*   **Thread Safety**: The `ResultAggregator` is not thread-safe. Concurrent calls to modification methods (`Add`, `AddRange`, `Clear`) from multiple threads without external synchronization may result in data corruption or inconsistent state. Read operations should only be performed after all write operations are complete or protected by a lock.
*   **Empty State**: Calling `GetResult` on a newly instantiated or cleared aggregator returns `null`. Consumers must handle this nullable return value appropriately.
*   **Dictionary Keys**: The dictionaries returned by `GetViolationsByRule`, `GetViolationsBySeverity`, and `GetViolationsByFile` use string keys derived from the violation properties. If a violation has a null or empty identifier for these properties, behavior depends on the internal implementation of the grouping logic; typically, such items might be grouped under an empty string key or excluded.
*   **Memory Usage**: `GetAllViolations` and `GenerateSummaryReport` operate on the entire dataset held in memory. For extremely large analysis runs involving millions of violations, consumers should be aware of potential memory pressure when invoking these methods.
*   **Clear Behavior**: Invoking `Clear` resets counters for `GetTotalFilesAnalyzed` and `GetTotalElementsAnalyzed` to zero and empties all internal collections. It does not dispose of any `RuleViolation` or `AnalysisResult` objects passed in previously, but removes references to them within the aggregator.
