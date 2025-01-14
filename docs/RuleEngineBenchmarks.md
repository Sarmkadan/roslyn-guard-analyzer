# RuleEngineBenchmarks

The `RuleEngineBenchmarks` class provides a harness for measuring the performance of rule execution in the `roslyn-guard-analyzer`. It is designed to be used with a benchmarking framework (e.g., BenchmarkDotNet) to collect timing and resource-usage metrics for individual rules and for the full rule set. The class exposes methods to initialize the engine, run a single rule asynchronously, run all rules asynchronously, and a static entry point for invoking the benchmarks from the command line.

## API

### `public void Setup()`

- **Purpose**: Prepares the rule engine for benchmarking. This method must be called once before any `ExecuteRuleAsync` or `ExecuteAllRulesAsync` call. Typical initialization includes loading rule definitions, compiling analyzers, and setting up the test code context.
- **Parameters**: None.
- **Return value**: None.
- **Throws**: `InvalidOperationException` if the engine is already initialized or if required configuration files are missing. `FileNotFoundException` if referenced analyzer assemblies cannot be located.

### `public async Task ExecuteRuleAsync()`

- **Purpose**: Executes a single, pre‑selected rule against the test code and measures its performance. The specific rule is determined by the benchmark configuration (e.g., a `[Params]` attribute in a derived class). This method is intended to be called after `Setup`.
- **Parameters**: None.
- **Return value**: A `Task` that completes when the rule execution finishes.
- **Throws**: `InvalidOperationException` if `Setup` has not been called. `OperationCanceledException` if the operation is cancelled via the default cancellation token (if supported by the underlying engine).

### `public async Task ExecuteAllRulesAsync()`

- **Purpose**: Executes all registered rules against the test code and measures the aggregate performance. This method is intended to be called after `Setup`.
- **Parameters**: None.
- **Return value**: A `Task` that completes when all rules have been executed.
- **Throws**: `InvalidOperationException` if `Setup` has not been called. `AggregateException` if one or more rules fail during execution.

### `public static void Main()`

- **Purpose**: Entry point for running the benchmarks from the command line. Typically delegates to a benchmarking runner (e.g., `BenchmarkRunner.Run<RuleEngineBenchmarks>()`).
- **Parameters**: None.
- **Return value**: None.
- **Throws**: `InvalidOperationException` if the benchmarking framework fails to initialize or if no benchmarks are configured.

## Usage

The following examples assume the class is used with BenchmarkDotNet. The first example shows a minimal benchmark configuration; the second demonstrates how to run the benchmarks programmatically.

### Example 1: Basic Benchmark Class

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

public class RuleEngineBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Initialize the rule engine with test code and rule definitions
    }

    [Benchmark]
    public async Task ExecuteRuleAsync()
    {
        // Runs a single rule (rule selection handled by [Params] in a real scenario)
    }

    [Benchmark]
    public async Task ExecuteAllRulesAsync()
    {
        // Runs all rules
    }

    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<RuleEngineBenchmarks>();
    }
}
```

### Example 2: Programmatic Invocation Without BenchmarkDotNet

```csharp
public class Program
{
    public static async Task Main()
    {
        var benchmarks = new RuleEngineBenchmarks();
        benchmarks.Setup();

        // Measure single rule execution
        var sw = Stopwatch.StartNew();
        await benchmarks.ExecuteRuleAsync();
        sw.Stop();
        Console.WriteLine($"Single rule: {sw.ElapsedMilliseconds} ms");

        // Measure all rules execution
        sw.Restart();
        await benchmarks.ExecuteAllRulesAsync();
        sw.Stop();
        Console.WriteLine($"All rules: {sw.ElapsedMilliseconds} ms");
    }
}
```

## Notes

- **Initialization requirement**: `Setup` must be called exactly once before any `ExecuteRuleAsync` or `ExecuteAllRulesAsync` invocation. Calling these methods without prior setup will result in an `InvalidOperationException`.
- **Thread safety**: The instance members are **not thread‑safe**. Concurrent calls to `ExecuteRuleAsync` or `ExecuteAllRulesAsync` from multiple threads may produce undefined behavior or corrupt internal state. The class is intended for single‑threaded benchmarking scenarios.
- **Idempotency**: `Setup` is not idempotent; calling it more than once will throw an exception. If re‑initialization is required, create a new instance of `RuleEngineBenchmarks`.
- **Cancellation**: The asynchronous methods do not expose a cancellation token parameter in their signatures. If cancellation support is needed, it must be implemented by the underlying rule engine (e.g., via a default token). The methods may throw `OperationCanceledException` if the engine’s internal token is cancelled.
- **Static `Main`**: The `Main` method is intended for direct execution. When used with a benchmarking framework, it typically invokes the runner and exits. It does not return a value; any errors are written to standard error or logged by the framework.
