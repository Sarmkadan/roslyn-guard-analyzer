# AnalysisResultRepository

`AnalysisResultRepository` manages the persistence, retrieval, and statistical aggregation of `AnalysisResult` objects produced by the Roslyn Guard Analyzer. It serves as the central data access layer for stored analysis outcomes, offering both query methods that filter results by various criteria and administrative operations such as saving, loading, exporting, and purging historical data.

## API

### Constructors

- **`public AnalysisResultRepository`**
  Initializes a new instance of the repository. The specific storage backend (in-memory, file-based, or database) is determined by the implementation details of the constructor and is not exposed through the public surface.

### Query Methods

- **`public IReadOnlyList<AnalysisResult> GetByProject(string projectName)`**
  Returns all analysis results associated with the specified project.
  - **Parameters**: `projectName` — the name of the project to query.
  - **Returns**: a read-only list of matching `AnalysisResult` instances, or an empty list if none exist.
  - **Throws**: `ArgumentNullException` when `projectName` is null.

- **`public IReadOnlyList<AnalysisResult> GetAnalyzedAfter(DateTime cutoff)`**
  Returns all analysis results whose analysis timestamp is strictly later than the given cutoff.
  - **Parameters**: `cutoff` — the exclusive lower bound for the analysis timestamp.
  - **Returns**: a read-only list of results analyzed after the cutoff, ordered chronologically by default.

- **`public IReadOnlyList<AnalysisResult> GetFailedAnalyses()`**
  Returns all analysis results that did not complete successfully (e.g., due to exceptions, cancellation, or internal errors).
  - **Returns**: a read-only list of failed results.

- **`public IReadOnlyList<AnalysisResult> GetSuccessfulAnalyses()`**
  Returns all analysis results that completed without errors.
  - **Returns**: a read-only list of successful results.

- **`public IReadOnlyList<AnalysisResult> GetWithViolationsInCategory(string category)`**
  Returns all analysis results that contain at least one violation belonging to the specified diagnostic category.
  - **Parameters**: `category` — the diagnostic category identifier (e.g., `"Security"`, `"Performance"`).
  - **Returns**: a read-only list of results with violations in the given category.
  - **Throws**: `ArgumentNullException` when `category` is null.

- **`public AnalysisResult? GetLatestForProject(string projectName)`**
  Returns the most recent analysis result for the given project, or `null` if the project has never been analyzed.
  - **Parameters**: `projectName` — the name of the project.
  - **Returns**: the latest `AnalysisResult`, or `null`.
  - **Throws**: `ArgumentNullException` when `projectName` is null.

- **`public IReadOnlyList<AnalysisResult> GetWithViolationCountGreaterThan(int threshold)`**
  Returns all analysis results whose total violation count exceeds the specified threshold.
  - **Parameters**: `threshold` — the minimum violation count (exclusive).
  - **Returns**: a read-only list of results with violation counts strictly greater than `threshold`.

### Persistence and Export

- **`public async Task SaveAsync(AnalysisResult result)`**
  Persists a single `AnalysisResult` to the underlying storage asynchronously.
  - **Parameters**: `result` — the analysis result to save.
  - **Throws**: `ArgumentNullException` when `result` is null; may throw storage-specific exceptions on I/O failure.

- **`public async Task LoadAllAsync()`**
  Loads all previously persisted analysis results from storage into the repository’s internal collection asynchronously. Subsequent queries will reflect the loaded data.
  - **Throws**: storage-specific exceptions if the data source is unavailable or corrupted.

- **`public async Task ExportToCsvAsync(string filePath)`**
  Exports all currently loaded analysis results to a CSV file at the specified path asynchronously.
  - **Parameters**: `filePath` — the destination file path. Existing files are overwritten.
  - **Throws**: `ArgumentNullException` when `filePath` is null; `DirectoryNotFoundException` or `IOException` on invalid paths or write failures.

### Statistics

- **`public AnalysisResultStatistics GetStatistics()`**
  Computes and returns a snapshot of aggregate statistics derived from all currently loaded analysis results.
  - **Returns**: an `AnalysisResultStatistics` object populated with the following properties exposed directly on the repository:
    - **`public int TotalAnalyses`** — total number of loaded results.
    - **`public int SuccessfulAnalyses`** — count of successful results.
    - **`public int FailedAnalyses`** — count of failed results.
    - **`public double AverageViolationCount`** — mean number of violations across all results.
    - **`public int TotalViolations`** — sum of all violations across all results.
    - **`public double AverageAnalysisDurationSeconds`** — mean duration of analyses in seconds.
    - **`public int ProjectsAnalyzed`** — distinct number of projects represented in the results.

### Maintenance

- **`public async Task ClearOldResultsAsync(DateTime olderThan)`**
  Removes all analysis results with a timestamp earlier than the specified date from both the in-memory collection and the underlying persistent storage asynchronously.
  - **Parameters**: `olderThan` — the exclusive upper bound; results with timestamps before this value are deleted.
  - **Throws**: storage-specific exceptions if the deletion operation fails.

## Usage

### Example 1: Saving, Loading, and Querying Results

```csharp
var repository = new AnalysisResultRepository();

// Load historical data
await repository.LoadAllAsync();

// Save a new result
var newResult = new AnalysisResult
{
    ProjectName = "MyProject",
    AnalyzedAt = DateTime.UtcNow,
    Success = true,
    Violations = new List<Violation>
    {
        new Violation { Category = "Security", Description = "SQL injection risk" }
    }
};
await repository.SaveAsync(newResult);

// Query the latest result for a project
AnalysisResult? latest = repository.GetLatestForProject("MyProject");
if (latest is not null)
{
    Console.WriteLine($"Latest analysis: {latest.AnalyzedAt}");
}

// Get all results with security violations
var securityViolations = repository.GetWithViolationsInCategory("Security");
Console.WriteLine($"Results with security issues: {securityViolations.Count}");
```

### Example 2: Exporting and Using Statistics

```csharp
var repository = new AnalysisResultRepository();
await repository.LoadAllAsync();

// Compute and display statistics
var stats = repository.GetStatistics();
Console.WriteLine($"Projects analyzed: {stats.ProjectsAnalyzed}");
Console.WriteLine($"Average violations: {stats.AverageViolationCount:F2}");
Console.WriteLine($"Average duration: {stats.AverageAnalysisDurationSeconds:F2}s");

// Export all data for reporting
await repository.ExportToCsvAsync(@"C:\Reports\analysis_results.csv");

// Purge results older than 90 days
await repository.ClearOldResultsAsync(DateTime.UtcNow.AddDays(-90));
```

## Notes

- **Empty data sets**: All query methods return empty collections rather than null when no results match the criteria. `GetLatestForProject` returns `null` when no results exist for the project.
- **Statistics on empty data**: When no results are loaded, `GetStatistics` returns an object with zero counts and `NaN` or zero for averages depending on the implementation’s handling of division by zero. Callers should check `TotalAnalyses > 0` before interpreting averages.
- **Thread safety**: The public API is not guaranteed to be thread-safe. Concurrent calls to `SaveAsync`, `LoadAllAsync`, or `ClearOldResultsAsync` while queries are executing may produce inconsistent results. External synchronization is recommended for multi-threaded scenarios.
- **`LoadAllAsync` behavior**: Calling `LoadAllAsync` replaces the current in-memory collection with data from storage. Any unsaved results added since the last load or save will be lost.
- **`ClearOldResultsAsync` scope**: This method affects both the in-memory state and persistent storage. Results removed by this operation cannot be recovered through `LoadAllAsync`.
- **Export format**: `ExportToCsvAsync` writes all currently loaded results. The CSV schema is determined by the `AnalysisResult` structure and includes all publicly serializable fields.
