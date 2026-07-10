# AnalysisStatisticsService

The `AnalysisStatisticsService` type aggregates and exposes quantitative information about code analysis violations produced by the Roslyn Guard analyzer. It provides both mutable counters for incremental updates and static helper methods for computing derived statistics, generating reports, and assessing overall code health.

## API

### Instance Fields

| Member | Type | Description |
|--------|------|-------------|
| `TotalCount` | `int` | Total number of violations recorded across all rules and files. |
| `CriticalCount` | `int` | Number of violations with severity **Critical**. |
| `HighCount` | `int` | Number of violations with severity **High**. |
| `MediumCount` | `int` | Number of violations with severity **Medium**. |
| `LowCount` | `int` | Number of violations with severity **Low**. |
| `ByRule` | `Dictionary<string, int>` | Mapping from rule identifier to the count of violations produced by that rule. |
| `ByFile` | `Dictionary<string, int>` | Mapping from file path to the count of violations found in that file. |
| `BySeverity` | `Dictionary<SeverityLevel, int>` | Mapping from `SeverityLevel` enum value to the count of violations of that severity. |
| `AffectedFiles` | `int` | Number of distinct files that contain at least one violation. |
| `AffectedRules` | `int` | Number of distinct rules that have produced at least one violation. |

### Static Methods

| Member | Signature (inferred) | Description | Parameters | Return Value | Exceptions |
|--------|----------------------|-------------|------------|--------------|------------|
| `CalculateStatistics` | `public static ViolationStatistics CalculateStatistics()` | Computes a snapshot of violation statistics from the current internal state of the service. | None | A `ViolationStatistics` object containing aggregated counts. | Throws `InvalidOperationException` if the service has not been initialized with any violation data. |
| `CalculateStatistics` | `public static ViolationStatistics CalculateStatistics(IEnumerable<Violation> violations)` | Computes violation statistics from an explicit collection of `Violation` objects. | `violations`: The violations to analyze. Must not be `null`. | A `ViolationStatistics` object representing the supplied violations. | Throws `ArgumentNullException` if `violations` is `null`. |
| `GetTopRulesByViolations` | `public static List<(string Rule, int Count)> GetTopRulesByViolations(int count)` | Returns the most frequently violated rules, ordered descending by violation count. | `count`: Maximum number of entries to return; must be greater than zero. | A list of tuples where each tuple contains the rule identifier and its violation count. | Throws `ArgumentOutOfRangeException` if `count` ≤ 0. |
| `GetTopFilesByViolations` | `public static List<(string File, int Count)> GetTopFilesByViolations(int count)` | Returns the files with the highest number of violations, ordered descending. | `count`: Maximum number of entries to return; must be greater than zero. | A list of tuples where each tuple contains the file path and its violation count. | Throws `ArgumentOutOfRangeException` if `count` ≤ 0. |
| `GetSeverityDistribution` | `public static Dictionary<string, double> GetSeverityDistribution()` | Provides the proportion of each severity level relative to the total violation count. | None | A dictionary mapping severity names (`"Critical"`, `"High"`, `"Medium"`, `"Low"`) to their percentage (0.0‑100.0). | Throws `InvalidOperationException` if no violations have been recorded. |
| `GenerateSummaryReport` | `public static string GenerateSummaryReport()` | Creates a human‑readable multi‑line summary of the current violation statistics. | None | A formatted string suitable for logging or console output. | Throws `InvalidOperationException` if the service contains no violation data. |
| `CalculateRiskScore` | `public static int CalculateRiskScore()` | Calculates an integer risk score based on weighted severity counts (higher scores indicate greater risk). | None | An integer risk score; the exact weighting algorithm is internal but deterministic. | Throws `InvalidOperationException` if no violations are present. |
| `GetHealthAssessment` | `public static string GetHealthAssessment()` | Returns a qualitative assessment of code health (e.g., `"Good"`, `"Moderate"`, `"Poor"`). | None | A string describing the overall health based on the calculated risk score. | Throws `InvalidOperationException` if the service lacks violation data. |

## Usage

### Example 1: Incremental aggregation and reporting
```csharp
var stats = new AnalysisStatisticsService();

// Simulate processing violations from analysis
foreach (var violation in analyzer.GetViolations())
{
    stats.TotalCount++;
    switch (violation.Severity)
    {
        case SeverityLevel.Critical: stats.CriticalCount++; break;
        case SeverityLevel.High:     stats.HighCount++; break;
        case SeverityLevel.Medium:   stats.MediumCount++; break;
        case SeverityLevel.Low:      stats.LowCount++; break;
    }

    stats.ByRule[violation.RuleId] = stats.ByRule.GetValueOrDefault(violation.RuleId) + 1;
    stats.ByFile[violation.FilePath] = stats.ByFile.GetValueOrDefault(violation.FilePath) + 1;
    stats.BySeverity[violation.Severity] = stats.BySeverity.GetValueOrDefault(violation.Severity) + 1;
}

// Generate a report for the CI pipeline
string report = AnalysisStatisticsService.GenerateSummaryReport();
Console.WriteLine(report);
```

### Example 2: Ad‑hoc statistics from a violation collection
```csharp
IEnumerable<Violation> recentViolations = GetRecentViolations(since: DateTime.UtcNow.AddDays(-7));

// Compute statistics for the last week only
ViolationStatistics weeklyStats = AnalysisStatisticsService.CalculateStatistics(recentViolations);

// Identify the top 5 problematic rules
var topRules = AnalysisStatisticsService.GetTopRulesByViolations(5);
foreach (var (rule, count) in topRules)
{
    Console.WriteLine($"{rule}: {count} violations");
}

// Obtain a severity distribution as percentages
var distribution = AnalysisStatisticsService.GetSeverityDistribution();
Console.WriteLine($"High severity: {distribution["High"]:F1}%");
```

## Notes

- The instance fields are intended to be updated directly by the analyzer; the type does not provide methods to reset or clear the counters. Consumers should instantiate a new `AnalysisStatisticsService` for each independent analysis run if a clean state is required.
- All static methods that operate on the service’s internal state (`GenerateSummaryReport`, `CalculateRiskScore`, `GetHealthAssessment`, the parameter‑less `CalculateStatistics`, and `GetSeverityDistribution`) will throw an `InvalidOperationException` if invoked before any violation data has been recorded.
- The two overloads of `CalculateStatistics` allow either a snapshot of the accumulated state or an ad‑hoc calculation on an arbitrary `IEnumerable<Violation>`; the latter does not modify the instance fields.
- The service itself contains no synchronization primitives. If multiple threads update the same instance concurrently, external locking is required to avoid race conditions on the counters and dictionaries.
- The dictionaries (`ByRule`, `ByFile`, `BySeverity`) are not guaranteed to be sorted; callers needing ordered results should apply `OrderBy` or similar LINQ operations after retrieval.
- The `SeverityLevel` enum is assumed to contain the four values `Critical`, `High`, `Medium`, and `Low`; any additional values will be ignored by the predefined counters and may appear only in `BySeverity`.
