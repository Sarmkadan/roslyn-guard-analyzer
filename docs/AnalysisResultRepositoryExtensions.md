# AnalysisResultRepositoryExtensions
The `AnalysisResultRepositoryExtensions` class provides a set of extension methods for working with analysis results in the context of the roslyn-guard-analyzer project. These methods enable efficient retrieval and filtering of analysis results, allowing for more streamlined processing and decision-making based on the outcomes of analyses.

## API
* `public static AnalysisResult? GetLatestSuccessfulForProject`: Retrieves the most recent successful analysis result for a given project. This method returns an `AnalysisResult` object if a successful analysis is found; otherwise, it returns `null`.
* `public static IReadOnlyList<AnalysisResult> GetWithCriticalViolations`: Returns a list of analysis results that contain critical violations. The method does not take any parameters and returns a read-only list of `AnalysisResult` objects.
* `public static IReadOnlyDictionary<string, AnalysisResult?> GetLatestAnalysesByProject`: Provides a dictionary where the keys are project identifiers and the values are the latest analysis results for each project. The method returns a read-only dictionary, and the value for each project can be `null` if no analysis result is available.
* `public static IReadOnlyList<AnalysisResult> GetWithMultipleViolationCategories`: Retrieves a list of analysis results that have violations across multiple categories. This method returns a read-only list of `AnalysisResult` objects.

## Usage
The following examples demonstrate how to use the `AnalysisResultRepositoryExtensions` methods in practical scenarios:
```csharp
// Example 1: Retrieving the latest successful analysis for a project
var latestSuccessfulAnalysis = AnalysisResultRepositoryExtensions.GetLatestSuccessfulForProject(projectId);
if (latestSuccessfulAnalysis != null)
{
    Console.WriteLine($"Latest successful analysis for project {projectId}: {latestSuccessfulAnalysis}");
}

// Example 2: Filtering analysis results with critical violations
var criticalViolations = AnalysisResultRepositoryExtensions.GetWithCriticalViolations();
foreach (var analysisResult in criticalViolations)
{
    Console.WriteLine($"Analysis result with critical violations: {analysisResult}");
}
```

## Notes
When using these extension methods, consider the following:
- The `GetLatestSuccessfulForProject` method may return `null` if no successful analysis is found for the specified project.
- The `GetWithCriticalViolations` and `GetWithMultipleViolationCategories` methods return read-only lists, which cannot be modified directly.
- The `GetLatestAnalysesByProject` method returns a dictionary where each value can be `null`, indicating that no analysis result is available for the corresponding project.
- These methods are designed to be thread-safe, allowing for concurrent access to analysis results without compromising data integrity. However, the underlying data storage and retrieval mechanisms should also be designed with thread-safety in mind to ensure consistent behavior.
