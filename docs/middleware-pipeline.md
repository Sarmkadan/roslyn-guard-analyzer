# Analysis middleware pipeline

The middleware types in `src/RoslynGuardAnalyzer/Middleware` provide a standalone,
composable wrapper around an analysis handler. They are not part of the default CLI
execution path; a caller must construct an `AnalysisPipeline`, register middleware,
and execute it with a `PipelineContext`.

## Pipeline construction and execution

`AnalysisPipeline` stores middleware in the order supplied to `Use`. At execution
time, it builds the delegate chain in reverse so that the first registered
middleware is the outermost component and therefore the first one entered. Each
middleware can run code before and after `next(context)`, omit that call to
short-circuit the pipeline, or allow an exception to propagate toward earlier
middleware.

For example:

```csharp
var pipeline = new AnalysisPipeline()
    .Use(new ErrorHandlingMiddleware(failOnError: false))
    .Use(new LoggingMiddleware(logLevel: 3))
    .Use(new PerformanceMetricsMiddleware())
    .UseHandler(async context =>
    {
        // Perform the analysis.
        await AnalyzeAsync(context.ProjectPath);
    });

await pipeline.ExecuteAsync(new PipelineContext
{
    AnalysisId = Guid.NewGuid().ToString("N"),
    ProjectPath = projectPath
});
```

The request path for that registration is:

```text
ErrorHandling -> Logging -> PerformanceMetrics -> Handler
```

Completion and exception handling unwind in the opposite direction:

```text
Handler -> PerformanceMetrics -> Logging -> ErrorHandling
```

This order lets performance metrics finalize even when the handler throws, lets
logging observe and rethrow the original failure, and then lets the outer error
handler decide whether the caller receives that failure. Order is configured by
the caller; the library does not install a default sequence.

`Use` and `UseHandler` reject `null`. If no handler is configured, execution ends
in a no-op delegate after the last middleware. An empty pipeline is consequently
a successful no-op. `Middlewares` exposes a read-only view of the registrations,
and `GetChainDescription` reports their `Name` values in registration order.

## Shared context

Every component receives the same `PipelineContext`. Its required `ProjectPath`
and `AnalysisId` identify the operation. Middleware can also update lifecycle
timestamps, `ErrorMessage`, and `IsCancelled`, or exchange reference-type data
through `Items`, `SetItem`, and `GetItem`.

The context is mutable and the pipeline does not synchronize access. A caller
should avoid executing the same context concurrently unless it provides its own
synchronization.

## Middleware responsibilities

### `ErrorHandlingMiddleware`

This component controls whether selected analysis failures are propagated.

- With the default `failOnError: true`, none of its filtered catch blocks run, so
  exceptions pass through unchanged.
- With `failOnError: false`, it catches `RoslynGuardException`,
  `FileAccessException`, `ConfigurationException`, `TimeoutException`, and
  `OperationCanceledException` from downstream components.
- For handled failures it sets a descriptive `ErrorMessage` and, except for
  cancellation, writes a warning to standard error. Cancellation additionally
  sets `IsCancelled` to `true`.
- It does not call another handler after catching a failure. “Continue” means
  returning successfully to the caller after suppressing the exception, not
  resuming the downstream operation at the failure point.
- Exception types outside the listed set continue to propagate.

Place it before components whose failures it should handle. If it is placed inside
logging, a suppressed exception will not reach the logging middleware's failure
path.

### `LoggingMiddleware`

This component records the overall lifecycle of downstream execution.

- On entry, it writes the current Unix timestamp in milliseconds to
  `StartTimeMilliseconds` and starts a stopwatch.
- At log level 3 (`info`) or higher, it writes start and successful-completion
  messages to standard output.
- At log level 4 (`debug`), it also reports the item count on success or the stack
  trace on failure.
- When downstream code throws, it records `EndTimeMilliseconds`, copies the
  exception message to `ErrorMessage`, writes an error at log level 1 or higher,
  and rethrows the same exception.
- Log level 0 is silent. Level 2 currently adds no messages beyond the level 1
  error output.

Because successful completion is logged only after `next` returns, an inner error
handler that suppresses an exception makes the operation appear successful to an
outer logging middleware. Registering error handling outside logging, as in the
example, preserves the failure log before suppression.

### `PerformanceMetricsMiddleware`

This component measures downstream execution and publishes a
`PerformanceMetrics` object under the `"PerformanceMetrics"` context item key.
Its `finally` block always records the data, whether downstream execution succeeds
or throws.

The captured values are:

- elapsed stopwatch time in `TotalMilliseconds`;
- UTC `StartTime` and `EndTime`;
- `Environment.ProcessorCount`; and
- the non-negative change in managed memory reported by `GC.GetTotalMemory(false)`.

Despite the property name `PeakMemoryBytes`, the value is a before/after managed
memory delta, not a sampled peak working-set measurement.

`GetMetrics(context)` returns the stored object, or `null` before it is available.
The middleware stores the new object only while unwinding from downstream work,
so the handler cannot retrieve that invocation's metrics during its own execution.
Similarly, `RecordComponentTiming` only updates a metrics object that is already
present in the context; repeated timings for a component are accumulated.
`GenerateReport` formats totals and orders component timings from longest to
shortest.

## Ordering considerations

Registration order determines which behavior observes an exception:

| Registration order (outer to inner) | Result when the handler throws |
| --- | --- |
| Error handling, logging, metrics | Metrics finalize, logging records failure, then configured error handling may suppress it. |
| Logging, error handling, metrics | Metrics finalize; if error handling suppresses the exception, logging records successful completion. |
| Metrics, logging, error handling | If error handling suppresses the exception, both logging and metrics complete normally. |

Choose the order based on which components must observe the original failure. The
first sequence is a useful general-purpose arrangement when failures should be
measured and logged even if selected exceptions are ultimately suppressed.
