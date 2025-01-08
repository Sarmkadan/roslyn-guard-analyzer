# PerformanceAnalyzer

`PerformanceAnalyzer` aggregates and analyzes execution timings for named components within a diagnostic or code-analysis pipeline. It records individual timing samples, computes summary statistics (minimum, maximum, average, total), tracks execution counts, and exposes methods to identify bottlenecks, generate textual reports, and retrieve metrics for specific components or the entire session.

## API

### Properties

#### `public required string ComponentName`
The unique identifier of the component whose performance is being tracked. This name is used as the key when storing, retrieving, and reporting metrics.

#### `public required long TotalTimeMs`
The cumulative elapsed time in milliseconds across all recorded executions for this component.

#### `public required long MinTimeMs`
The smallest single-execution time in milliseconds recorded for this component.

#### `public required long MaxTimeMs`
The largest single-execution time in milliseconds recorded for this component.

#### `public required long AverageTimeMs`
The arithmetic mean of all recorded execution times in milliseconds. Computed as `TotalTimeMs / ExecutionCount` when `ExecutionCount` is greater than zero.

#### `public required int ExecutionCount`
The total number of times `RecordTiming` has been called for this component.

#### `public double PercentageOfTotal`
The proportion of this component’s `TotalTimeMs` relative to the sum of `TotalTimeMs` across all tracked components, expressed as a value between 0.0 and 100.0. Returns 0.0 when no timings have been recorded globally.

### Methods

#### `public void RecordTiming(long elapsedMs)`
Records a single timing observation for the component identified by `ComponentName`.

| Parameter  | Type   | Description                                      |
|------------|--------|--------------------------------------------------|
| `elapsedMs` | `long` | The elapsed time in milliseconds to record.      |

Updates `TotalTimeMs`, `MinTimeMs`, `MaxTimeMs`, `AverageTimeMs`, and increments `ExecutionCount`. If `elapsedMs` is negative, an `ArgumentOutOfRangeException` is thrown.

#### `public PerformanceMetrics? GetMetricsForComponent(string componentName)`
Retrieves the aggregated metrics for a specified component.

| Parameter       | Type     | Description                                    |
|-----------------|----------|------------------------------------------------|
| `componentName` | `string` | The name of the component to look up.          |

Returns a `PerformanceMetrics` instance if the component exists; otherwise returns `null`. Throws `ArgumentNullException` when `componentName` is `null`.

#### `public List<PerformanceMetrics> GetAllMetrics()`
Returns a list containing `PerformanceMetrics` for every component that has been registered and has at least one recorded timing. The list is a snapshot; subsequent recordings do not modify it.

#### `public List<PerformanceMetrics> GetBottlenecks(double thresholdPercentage)`
Returns components whose `PercentageOfTotal` meets or exceeds the given threshold.

| Parameter            | Type     | Description                                                              |
|----------------------|----------|--------------------------------------------------------------------------|
| `thresholdPercentage` | `double` | Minimum percentage (0.0–100.0) a component must reach to be included.    |

Throws `ArgumentOutOfRangeException` if `thresholdPercentage` is negative or greater than 100.0.

#### `public long GetTotalTimeMs()`
Returns the sum of `TotalTimeMs` across all tracked components. This is the denominator used when computing each component’s `PercentageOfTotal`.

#### `public string GenerateReport()`
Produces a formatted, human-readable report containing component names, execution counts, total, average, min, max times, and percentage of total for every tracked component. The report layout is deterministic and suitable for logging or diagnostic output.

#### `public void Clear()`
Removes all recorded timings and metrics for every component. After calling `Clear`, `HasComponent` returns `false` for any previously tracked name, and `GetAllMetrics` returns an empty list.

#### `public bool HasComponent(string componentName)`
Indicates whether a component with the given name has been registered and has at least one recorded timing.

| Parameter       | Type     | Description                              |
|-----------------|----------|------------------------------------------|
| `componentName` | `string` | The component name to check.             |

Returns `true` if the component exists with `ExecutionCount > 0`; otherwise `false`. Throws `ArgumentNullException` when `componentName` is `null`.

## Usage

### Example 1: Basic recording and reporting

```csharp
var analyzer = new PerformanceAnalyzer
{
    ComponentName = "SyntaxWalker",
    TotalTimeMs = 0,
    MinTimeMs = 0,
    MaxTimeMs = 0,
    AverageTimeMs = 0,
    ExecutionCount = 0
};

// Simulate multiple runs
analyzer.RecordTiming(120);
analyzer.RecordTiming(95);
analyzer.RecordTiming(140);

Console.WriteLine(analyzer.GenerateReport());
// Output includes SyntaxWalker with ExecutionCount=3, TotalTimeMs=355, etc.
```

### Example 2: Multi-component bottleneck detection

```csharp
var parserAnalyzer = new PerformanceAnalyzer
{
    ComponentName = "Parser",
    TotalTimeMs = 0, MinTimeMs = 0, MaxTimeMs = 0,
    AverageTimeMs = 0, ExecutionCount = 0
};

var binderAnalyzer = new PerformanceAnalyzer
{
    ComponentName = "Binder",
    TotalTimeMs = 0, MinTimeMs = 0, MaxTimeMs = 0,
    AverageTimeMs = 0, ExecutionCount = 0
};

parserAnalyzer.RecordTiming(200);
parserAnalyzer.RecordTiming(220);
binderAnalyzer.RecordTiming(800);

// Retrieve bottlenecks above 60% of total time
var bottlenecks = binderAnalyzer.GetBottlenecks(60.0);
// bottlenecks contains Binder metrics since its share is ~65.5%

var metrics = binderAnalyzer.GetMetricsForComponent("Parser");
// metrics is null because Parser was recorded in a different instance
```

## Notes

- **Instance scope**: Each `PerformanceAnalyzer` instance tracks only its own `ComponentName`. Cross-component queries such as `GetBottlenecks`, `GetTotalTimeMs`, and `PercentageOfTotal` operate on a shared static store keyed by component name. Consequently, two instances with the same `ComponentName` contribute to the same aggregate metrics.
- **Thread safety**: `RecordTiming`, `Clear`, and all read methods access shared state without internal synchronization. In multi-threaded scenarios, callers must provide external locking to prevent race conditions when recording timings or generating reports concurrently.
- **Empty state**: Before any timing is recorded, `MinTimeMs`, `MaxTimeMs`, and `AverageTimeMs` remain at their initialized values (typically zero). `GetMetricsForComponent` returns `null` for a name that has never been recorded. `GenerateReport` produces a header-only or empty-body report when no components have data.
- **Negative input**: Passing a negative value to `RecordTiming` always throws `ArgumentOutOfRangeException`. The analyzer assumes all elapsed times are non-negative.
- **Clear behavior**: `Clear` resets the global store, affecting all instances that share component names. After clearing, previously returned `PerformanceMetrics` snapshots remain unchanged, but new queries reflect the empty state.
