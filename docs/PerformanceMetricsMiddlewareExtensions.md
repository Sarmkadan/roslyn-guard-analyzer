# PerformanceMetricsMiddlewareExtensions

The `PerformanceMetricsMiddlewareExtensions` class provides a comprehensive set of static utility methods for retrieving and formatting performance data collected during analysis execution within the `roslyn-guard-analyzer` pipeline. This extension class serves as the primary read-only interface for accessing timing metrics, memory consumption statistics, and component-level performance breakdowns, enabling developers to generate reports, diagnose bottlenecks, and monitor the efficiency of the analysis process without directly interacting with the underlying middleware state.

## API

### GetElapsedFormatted
Retrieves the total elapsed time of the analysis session formatted as a human-readable string (e.g., "00:01:23.456").
*   **Returns**: `string` representing the formatted duration.
*   **Throws**: None.

### GetPeakMemoryFormatted
Retrieves the peak memory usage recorded during the session formatted as a human-readable string with appropriate units (e.g., "128.5 MB").
*   **Returns**: `string` representing the formatted memory size.
*   **Throws**: None.

### GetComponentTimings
Returns a read-only dictionary mapping component names to their respective execution times in milliseconds.
*   **Returns**: `IReadOnlyDictionary<string, long>` where keys are component identifiers and values are durations in milliseconds.
*   **Throws**: None.

### GetPeakMemoryBytes
Retrieves the exact peak memory usage recorded during the session in bytes.
*   **Returns**: `long` representing the memory in bytes.
*   **Throws**: None.

### GetTotalMilliseconds
Retrieves the total elapsed time of the analysis session in milliseconds.
*   **Returns**: `long` representing the total duration.
*   **Throws**: None.

### GetProcessorCount
Retrieves the number of logical processors available to the runtime during the analysis session.
*   **Returns**: `int` representing the processor count.
*   **Throws**: None.

### GetStartTime
Retrieves the precise timestamp marking the beginning of the analysis session.
*   **Returns**: `DateTime` indicating the start time.
*   **Throws**: None.

### GetEndTime
Retrieves the precise timestamp marking the completion of the analysis session.
*   **Returns**: `DateTime` indicating the end time.
*   **Throws**: None.

### GetSlowestComponent
Identifies the name of the single component that consumed the most execution time.
*   **Returns**: `string?` containing the component name, or `null` if no components were timed.
*   **Throws**: None.

### GetComponentTime
Retrieves the execution time in milliseconds for a specific named component.
*   **Parameters**:
    *   `componentName` (`string`): The identifier of the component to query.
*   **Returns**: `long` representing the duration in milliseconds. Returns 0 if the component is not found.
*   **Throws**: None.

### GetTimedComponents
Returns an enumerable collection of all component names that have recorded timing data.
*   **Returns**: `IEnumerable<string>` containing component identifiers.
*   **Throws**: None.

### HasComponentTimings
Determines whether any component timing data has been recorded for the current session.
*   **Returns**: `bool` indicating the presence of timing data.
*   **Throws**: None.

### GetComponentPercentage
Calculates the percentage of the total execution time attributed to a specific component.
*   **Parameters**:
    *   `componentName` (`string`): The identifier of the component to evaluate.
*   **Returns**: `double` representing the percentage (0.0 to 100.0). Returns 0.0 if the component is not found or total time is zero.
*   **Throws**: None.

### GetTopSlowestComponents
Returns a collection of components ordered by execution time in descending order, limited to the slowest performers.
*   **Returns**: `IEnumerable<KeyValuePair<string, long>>` where pairs consist of the component name and its duration.
*   **Throws**: None.

### GetMetricsSummary
Generates a comprehensive summary of all available metrics, returning both keys and pre-formatted string values suitable for logging or display.
*   **Returns**: `IReadOnlyDictionary<string, string>` containing metric labels and their formatted values.
*   **Throws**: None.

### GetAverageComponentTime
Calculates the arithmetic mean of execution times across all recorded components.
*   **Returns**: `double` representing the average duration in milliseconds. Returns 0.0 if no components exist.
*   **Throws**: None.

### GetTotalComponentTime
Sums the execution times of all recorded components to provide the aggregate processing time.
*   **Returns**: `long` representing the total duration in milliseconds.
*   **Throws**: None.

## Usage

### Generating a Performance Report
The following example demonstrates how to retrieve a full summary of metrics and output them to the console, including specific details about the slowest performing component.

```csharp
using RoslynGuard.Analyzer.Extensions;

// Check if timing data exists before attempting detailed analysis
if (PerformanceMetricsMiddlewareExtensions.HasComponentTimings)
{
    var summary = PerformanceMetricsMiddlewareExtensions.GetMetricsSummary();
    
    Console.WriteLine("=== Analysis Performance Report ===");
    foreach (var metric in summary)
    {
        Console.WriteLine($"{metric.Key}: {metric.Value}");
    }

    var slowest = PerformanceMetricsMiddlewareExtensions.GetSlowestComponent();
    if (slowest != null)
    {
        var time = PerformanceMetricsMiddlewareExtensions.GetComponentTime(slowest);
        var percent = PerformanceMetricsMiddlewareExtensions.GetComponentPercentage(slowest);
        Console.WriteLine($"\nBottleneck Detected: '{slowest}' took {time}ms ({percent:F2}% of total time).");
    }
}
```

### Programmatic Threshold Monitoring
This example illustrates how to access raw numeric values to enforce performance thresholds, such as failing a build if memory usage exceeds a limit or if a specific component takes too long.

```csharp
using RoslynGuard.Analyzer.Extensions;

const long MaxAllowedMemoryBytes = 512 * 1024 * 1024; // 512 MB
const int MaxAllowedComponentMs = 5000; // 5 seconds

long peakMemory = PerformanceMetricsMiddlewareExtensions.GetPeakMemoryBytes();
if (peakMemory > MaxAllowedMemoryBytes)
{
    throw new InvalidOperationException(
        $"Memory limit exceeded. Peak: {PerformanceMetricsMiddlewareExtensions.GetPeakMemoryFormatted()}");
}

var slowComponents = PerformanceMetricsMiddlewareExtensions.GetTopSlowestComponents();
foreach (var component in slowComponents)
{
    if (component.Value > MaxAllowedComponentMs)
    {
        Console.WriteLine(
            $"Warning: Component '{component.Key}' exceeded time threshold ({component.Value}ms).");
    }
}
```

## Notes

*   **Thread Safety**: As this class exposes static methods accessing shared middleware state, it is assumed that the underlying metrics collection is finalized before these methods are invoked. Calling these methods concurrently while the middleware is actively recording metrics may result in inconsistent snapshots of the data.
*   **Empty State Handling**: Methods returning collections (`GetTimedComponents`, `GetTopSlowestComponents`) will return empty enumerables if no data is present, rather than throwing exceptions. Similarly, `GetSlowestComponent` returns `null` if no components have been timed.
*   **Division by Zero**: Methods calculating percentages (`GetComponentPercentage`) or averages (`GetAverageComponentTime`) internally handle cases where the total time is zero to prevent division-by-zero errors, returning `0.0` in such scenarios.
*   **Data Granularity**: Time-based methods return values in milliseconds (`long`), while formatted methods (`GetElapsedFormatted`, `GetPeakMemoryFormatted`) handle the conversion to human-readable units internally. For precise calculations, use the raw numeric getters; for logging, use the formatted variants.
