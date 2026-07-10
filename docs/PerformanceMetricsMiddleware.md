# PerformanceMetricsMiddleware

The `PerformanceMetricsMiddleware` component serves as a diagnostic utility within the `roslyn-guard-analyzer` pipeline, designed to capture, aggregate, and report runtime performance data during analysis execution. It tracks temporal metrics such as total elapsed time and component-specific durations, alongside resource consumption indicators like peak memory usage and processor availability. By exposing both instance-level data and static reporting mechanisms, this middleware facilitates detailed performance profiling and bottleneck identification without interrupting the primary analysis workflow.

## API

### Instance Properties

#### `TotalMilliseconds`
*   **Type:** `public long`
*   **Description:** Represents the total duration of the monitored operation in milliseconds. This value is typically populated after the middleware has completed its invocation cycle.
*   **Remarks:** Returns `0` if the operation has not yet completed or if timing data was not successfully captured.

#### `PeakMemoryBytes`
*   **Type:** `public long`
*   **Description:** Indicates the maximum amount of memory allocated by the process during the monitored interval, measured in bytes.
*   **Remarks:** This metric relies on underlying system diagnostics; values may vary depending on the garbage collection state at the time of measurement.

#### `ProcessorCount`
*   **Type:** `public int`
*   **Description:** Captures the number of logical processors available to the runtime environment at the start of the monitoring session.
*   **Remarks:** This is a snapshot value taken at initialization and does not reflect dynamic changes to CPU affinity or availability during execution.

#### `ComponentTimingsMs`
*   **Type:** `public Dictionary<string, long>`
*   **Description:** A collection mapping specific component names to their respective execution times in milliseconds. This allows for granular performance breakdowns of sub-tasks within the analyzer.
*   **Remarks:** The dictionary is thread-safe for writes via the `RecordComponentTiming` method. Keys are case-sensitive.

#### `StartTime`
*   **Type:** `public DateTime`
*   **Description:** The precise timestamp marking the beginning of the middleware's monitoring scope.
*   **Remarks:** Initialized upon the construction or first invocation of the middleware.

#### `EndTime`
*   **Type:** `public DateTime`
*   **Description:** The precise timestamp marking the conclusion of the middleware's monitoring scope.
*   **Remarks:** Remains unset until the `InvokeAsync` method completes execution.

#### `GetElapsed`
*   **Type:** `public TimeSpan`
*   **Description:** A computed property that returns the difference between `EndTime` and `StartTime` as a `TimeSpan` object.
*   **Remarks:** If `EndTime` has not been set (i.e., the operation is ongoing), this property typically returns the duration from `StartTime` to the current moment or `TimeSpan.Zero`, depending on implementation state.

### Instance Methods

#### `InvokeAsync`
*   **Signature:** `public async Task InvokeAsync`
*   **Description:** Executes the middleware logic, initiating the performance tracking, invoking the next delegate in the pipeline, and finalizing metric collection upon completion.
*   **Parameters:** None (assumes standard middleware signature context handled internally or via closure).
*   **Return Value:** A `Task` representing the asynchronous operation. The task completes when the inner pipeline execution and metric finalization are finished.
*   **Exceptions:** May throw exceptions propagated from the inner pipeline delegate or if system resources required for metric collection (e.g., performance counters) are unavailable.

### Static Methods

#### `RecordComponentTiming`
*   **Signature:** `public static void RecordComponentTiming`
*   **Description:** Records a specific duration for a named component into the global or context-specific timing collection.
*   **Parameters:** Implicitly requires a component name (string) and a duration (long/ms), though specific parameter names are inferred from usage patterns associated with the `ComponentTimingsMs` dictionary.
*   **Return Value:** `void`.
*   **Exceptions:** May throw `ArgumentNullException` if the component name is null, or `InvalidOperationException` if called outside a valid monitoring context.

#### `GetMetrics`
*   **Signature:** `public static PerformanceMetrics? GetMetrics`
*   **Description:** Retrieves the current instance of performance metrics associated with the active execution context.
*   **Return Value:** Returns a `PerformanceMetrics` object if a context is active; otherwise, returns `null`.
*   **Remarks:** Useful for external tools or logging sinks to access real-time or post-execution data without direct reference to the middleware instance.

#### `GenerateReport`
*   **Signature:** `public static string GenerateReport`
*   **Description:** Aggregates current metric data into a formatted human-readable string report.
*   **Return Value:** A `string` containing the formatted report, including total time, memory usage, and component breakdowns.
*   **Exceptions:** May throw if the underlying metrics collection is in an inconsistent state or if formatting resources are unavailable.

## Usage

### Example 1: Integrating into the Analysis Pipeline
The following example demonstrates how to register and utilize the middleware within a standard Roslyn analyzer host configuration to automatically capture metrics for a full analysis run.

```csharp
using RoslynGuardAnalyzer.Middleware;
using System;
using System.Threading.Tasks;

public class AnalysisHost
{
    public async Task RunAnalysisAsync()
    {
        var middleware = new PerformanceMetricsMiddleware();
        
        try 
        {
            // Invoke the middleware which wraps the actual analysis logic
            await middleware.InvokeAsync();
            
            // Access instance properties after completion
            Console.WriteLine($"Analysis completed in {middleware.TotalMilliseconds}ms");
            Console.WriteLine($"Peak Memory: {middleware.PeakMemoryBytes / 1024.0 / 1024.0:F2} MB");
            
            // Generate and display the detailed report
            string report = PerformanceMetricsMiddleware.GenerateReport();
            Console.WriteLine(report);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Analysis failed: {ex.Message}");
            throw;
        }
    }
}
```

### Example 2: Recording Granular Component Timings
This example illustrates how to manually record timings for specific sub-tasks (e.g., parsing, semantic analysis) using the static recording method, allowing for detailed breakdowns in the final report.

```csharp
using RoslynGuardAnalyzer.Middleware;
using System.Diagnostics;

public class ComponentAnalyzer
{
    public void AnalyzeSyntaxTree()
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Simulate complex syntax analysis
        PerformHeavyParsing();
        
        stopwatch.Stop();
        
        // Record the timing for the "SyntaxParsing" component
        PerformanceMetricsMiddleware.RecordComponentTiming("SyntaxParsing", stopwatch.ElapsedMilliseconds);
    }

    public void AnalyzeSemantics()
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Simulate semantic validation
        PerformSemanticValidation();
        
        stopwatch.Stop();
        
        // Record the timing for the "SemanticValidation" component
        PerformanceMetricsMiddleware.RecordComponentTiming("SemanticValidation", stopwatch.ElapsedMilliseconds);
    }

    private void PerformHeavyParsing() { /* Implementation */ }
    private void PerformSemanticValidation() { /* Implementation */ }
}
```

## Notes

*   **Thread Safety:** The `ComponentTimingsMs` dictionary is designed to be thread-safe for write operations when accessed via `RecordComponentTiming`. However, direct modification of the `ComponentTimingsMs` property from external threads is not recommended and may lead to race conditions.
*   **Context Dependency:** The static methods `GetMetrics` and `RecordComponentTiming` rely on an active execution context (likely `AsyncLocal` or similar). Calling these outside the scope of an active `InvokeAsync` call may result in `null` returns or silent failures where data is not aggregated.
*   **Resource Availability:** `PeakMemoryBytes` depends on the ability of the runtime to query process memory statistics. In restricted environments (e.g., certain containerized setups with limited permissions), this value may default to `0` or an estimate.
*   **Timing Precision:** `TotalMilliseconds` and component timings are recorded using system high-resolution timers where available, but resolution is ultimately bound by the underlying operating system and hardware capabilities.
*   **Report Generation:** `GenerateReport` creates a snapshot of the data at the moment of invocation. If called while `InvokeAsync` is still running, the report will reflect partial data up to that point.
