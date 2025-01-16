# RuleEngineBenchmarksExtensions

Provides asynchronous benchmarking utilities for measuring the performance of rule‑engine executions within the Roslyn Guard Analyzer project. The type exposes static methods to run individual rule benchmarks, compare multiple rules, measure engine overhead, and assess scalability, along with properties that capture benchmark configuration and result statistics.

## API

### BenchmarkRuleAsync
**Purpose**  
Asynchronously benchmarks a single rule execution and returns a detailed result.

**Parameters**  
- `rule`: The rule object to be benchmarked.  
- `cancellationToken` (optional): A token to observe for cancellation requests.

**Return Value**  
A `Task<BenchmarkResult>` that completes with the benchmark measurements for the supplied rule.

**Exceptions**  
- `ArgumentNullException` if `rule` is `null`.  
- `OperationCanceledException` if the operation is canceled via `cancellationToken`.  
- `InvalidOperationException` if the rule cannot be executed in the current context.

### BenchmarkRulesComparisonAsync
**Purpose**  
Asynchronously benchmarks a collection of rules and returns a mapping from rule identifiers to their respective benchmark results.

**Parameters**  
- `rules`: An enumerable of rule objects to benchmark.  
- `cancellationToken` (optional): A token to observe for cancellation requests.

**Return Value**  
A `Task<Dictionary<string, BenchmarkResult>>` where each key is a rule identifier (typically the rule’s name or fully qualified name) and each value is the corresponding `BenchmarkResult`.

**Exceptions**  
- `ArgumentNullException` if `rules` is `null`.  
- `OperationCanceledException` if the operation is canceled via `cancellationToken`.  
- `InvalidOperationException` if any rule in the collection cannot be executed.

### MeasureEngineOverheadAsync
**Purpose**  
Asynchronously measures the baseline overhead of the rule engine itself, excluding any rule‑specific logic.

**Parameters**  
- `cancellationToken` (optional): A token to observe for cancellation requests.

**Return Value**  
A `Task<double>` representing the average overhead time in milliseconds per engine invocation.

**Exceptions**  
- `OperationCanceledException` if the operation is canceled via `cancellationToken`.  
- `InvalidOperationException` if the engine cannot be initialized for measurement.

### BenchmarkScalabilityAsync
**Purpose**  
Asynchronously benchmarks how benchmark execution time scales with varying input element counts, returning a mapping from element count to benchmark result.

**Parameters**  
- `rule`: The rule object to benchmark at different scales.  
- `elementCounts`: A collection of integer values representing the numbers of elements to feed into the rule for each benchmark run.  
- `cancellationToken` (optional): A token to observe for cancellation requests.

**Return Value**  
A `Task<Dictionary<int, BenchmarkResult>>` where each key is an element count from `elementCounts` and each value is the corresponding `BenchmarkResult`.

**Exceptions**  
- `ArgumentNullException` if `rule` or `elementCounts` is `null`.  
- `OperationCanceledException` if the operation is canceled via `cancellationToken`.  
- `InvalidOperationException` if the rule cannot be executed for any of the supplied element counts.

### WarmupIterations
**Purpose**  
Gets or sets the number of warm‑up iterations performed before measurement collections begin.

**Property Value**  
An `int` indicating how many preliminary executions are discarded to stabilize JIT and caching effects. Defaults to a sensible value if not explicitly set.

### BenchmarkIterations
**Purpose**  
Gets or sets the number of timed iterations used to compute benchmark statistics.

**Property Value**  
An `int` indicating how many measurement runs are performed for each benchmark. Higher values improve statistical confidence at the cost of longer execution time.

### ElementCount
**Purpose**  
Gets or sets the number of elements processed by the rule during a single benchmark iteration when the benchmark is not explicitly scaling over a range.

**Property Value**  
A nullable `int` (`int?`) representing the element count; `null` indicates that the rule’s default input size should be used.

### MinMilliseconds
**Purpose**  
Gets the minimum observed execution time (in milliseconds) across all measured iterations.

**Property Value**  
A `double` representing the lowest timing value recorded.

### MaxMilliseconds
**Purpose**  
Gets the maximum observed execution time (in milliseconds) across all measured iterations.

**Property Value**  
A `double` representing the highest timing value recorded.

### AverageMilliseconds
**Purpose**  
Gets the arithmetic mean of the observed execution times (in milliseconds).

**Property Value**  
A `double` representing the average timing value.

### MedianMilliseconds
**Purpose**  
Gets the median of the observed execution times (in milliseconds).

**Property Value**  
A `double` representing the middle timing value when all measurements are sorted.

### ToString
**Purpose**  
Returns a human‑readable summary of the benchmark result or configuration.

**Return Value**  
A `string` containing formatted fields such as iteration counts, element count, and timing statistics.

## Usage

```csharp
using RoslynGuardAnalyzer.Benchmarks;
using System.Threading.Tasks;

// Example 1: Benchmark a single rule
public async Task<BenchmarkResult> RunSingleRuleBenchmarkAsync(MyRule rule)
{
    var extensions = new RuleEngineBenchmarksExtensions
    {
        WarmupIterations = 5,
        BenchmarkIterations = 20,
        ElementCount = 1000
    };

    return await extensions.BenchmarkRuleAsync(rule);
}
```

```csharp
using RoslynGuardAnalyzer.Benchmarks;
using System.Collections.Generic;
using System.Threading.Tasks;

// Example 2: Compare multiple rules and assess scalability
public async Task PerformComparisonAndScalabilityAsync(IEnumerable<IRule> rules)
{
    var extensions = new RuleEngineBenchmarksExtensions
    {
        WarmupIterations = 3,
        BenchmarkIterations = 15
    };

    // Benchmark each rule individually and collect results
    var comparison = await extensions.BenchmarkRulesComparisonAsync(rules);

    // Determine how a specific rule scales with input size
    var targetRule = rules.First();
    var scalability = await extensions.BenchmarkScalabilityAsync(
        targetRule,
        new List<int> { 100, 500, 1000, 5000 });

    // Further processing of comparison and scalability results …
}
```

## Notes

- The benchmarking methods are **asynchronous** and should be awaited; calling them without `await` will return a hot `Task` that may complete after the caller has moved on, leading to inaccurate measurements.
- All methods accept an optional `CancellationToken`. If cancellation is requested, the operation will stop as soon as safely possible and throw `OperationCanceledException`.
- Setting `WarmupIterations` to a value lower than 1 is ineffective; the implementation treats non‑positive values as 1 to ensure at least one warm‑up run.
- `BenchmarkIterations` must be greater than 0; otherwise the methods will throw an `ArgumentOutOfRangeException` (inherited from argument validation).
- When `ElementCount` is `null`, the benchmark uses the rule’s default input size, which may vary between rule implementations; for reproducible results, specify an explicit count.
- The timing properties (`MinMilliseconds`, `MaxMilliseconds`, `AverageMilliseconds`, `MedianMilliseconds`) are only populated after a benchmark method has completed successfully; accessing them prior to execution yields default values (zero).
- The type itself is **not thread‑safe** for concurrent mutation of its configuration properties (`WarmupIterations`, `BenchmarkIterations`, `ElementCount`). If multiple threads share the same instance, synchronize access to these properties or create separate instances per thread.
- The static benchmarking methods (`BenchmarkRuleAsync`, `BenchmarkRulesComparisonAsync`, `MeasureEngineOverheadAsync`, `BenchmarkScalabilityAsync`) do not rely on mutable state and can be invoked concurrently from multiple threads without additional synchronization, provided each call receives its own cancellation token if needed.
