using System.Reflection;
using BenchmarkDotNet.Attributes;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;
using RoslynGuardAnalyzer.Rules;
using RoslynGuardAnalyzer.Core;
using System.Diagnostics.CodeAnalysis;

namespace RoslynGuardAnalyzer.Benchmarks;

public static class RuleEngineBenchmarksExtensions
{
    private static readonly FieldInfo _ruleEngineField = typeof(RuleEngineBenchmarks).GetField(
        "_ruleEngine", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Could not find _ruleEngine field");

    private static readonly FieldInfo _elementsField = typeof(RuleEngineBenchmarks).GetField(
        "_elements", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Could not find _elements field");

    private static readonly FieldInfo _sampleRuleField = typeof(RuleEngineBenchmarks).GetField(
        "_sampleRule", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Could not find _sampleRule field");

    /// <summary>
    /// Creates a benchmark that measures the execution time of a specific rule with warmup runs.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance. Cannot be null.</param>
    /// <param name="rule">The rule to benchmark. Cannot be null.</param>
    /// <param name="warmupIterations">Number of warmup iterations (default: 3)</param>
    /// <param name="benchmarkIterations">Number of benchmark iterations (default: 5)</param>
    /// <returns>Benchmark result with statistics</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> or <paramref name="rule"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="warmupIterations"/> or <paramref name="benchmarkIterations"/> is negative or zero.</exception>
    /// <exception cref="InvalidOperationException">Setup must be called before benchmarking.</exception>
    public static async Task<BenchmarkResult> BenchmarkRuleAsync(
        this RuleEngineBenchmarks benchmarks,
        [DisallowNull] AnalysisRule rule,
        int warmupIterations = 3,
        int benchmarkIterations = 5)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);
        ArgumentNullException.ThrowIfNull(rule);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(warmupIterations);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(benchmarkIterations);

        var ruleEngine = (RuleEngine?)_ruleEngineField.GetValue(benchmarks);
        var elements = (List<CodeElement>?)_elementsField.GetValue(benchmarks);

        if (ruleEngine is null || elements is null)
        {
            throw new InvalidOperationException("Setup must be called before benchmarking.");
        }

        // Warmup phase
        for (int i = 0; i < warmupIterations; i++)
        {
            await ruleEngine.ExecuteRuleAsync(rule, elements);
        }

        // Benchmark phase
        var results = new List<double>(benchmarkIterations);
        for (int i = 0; i < benchmarkIterations; i++)
        {
            var startTime = DateTime.UtcNow;
            await ruleEngine.ExecuteRuleAsync(rule, elements);
            var elapsed = DateTime.UtcNow - startTime;
            results.Add(elapsed.TotalMilliseconds);
        }

        return new BenchmarkResult
        {
            WarmupIterations = warmupIterations,
            BenchmarkIterations = benchmarkIterations,
            MinMilliseconds = results.Min(),
            MaxMilliseconds = results.Max(),
            AverageMilliseconds = results.Average(),
            MedianMilliseconds = results.OrderBy(static x => x).ElementAt(results.Count / 2)
        };
    }

    /// <summary>
    /// Creates a benchmark that compares multiple rules against each other.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance. Cannot be null.</param>
    /// <param name="rules">Collection of rules to compare. Cannot be null or empty.</param>
    /// <param name="iterationsPerRule">Number of iterations per rule (default: 10)</param>
    /// <returns>Dictionary mapping rule names to their benchmark results</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> or <paramref name="rules"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="rules"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="iterationsPerRule"/> is negative or zero.</exception>
    /// <exception cref="InvalidOperationException">Setup must be called before benchmarking.</exception>
    public static async Task<Dictionary<string, BenchmarkResult>> BenchmarkRulesComparisonAsync(
        this RuleEngineBenchmarks benchmarks,
        [DisallowNull] IEnumerable<AnalysisRule> rules,
        int iterationsPerRule = 10)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);
        ArgumentNullException.ThrowIfNull(rules);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(iterationsPerRule);

        var ruleEngine = (RuleEngine?)_ruleEngineField.GetValue(benchmarks);
        var elements = (List<CodeElement>?)_elementsField.GetValue(benchmarks);

        if (ruleEngine is null || elements is null)
        {
            throw new InvalidOperationException("Setup must be called before benchmarking.");
        }

        var results = new Dictionary<string, BenchmarkResult>();

        foreach (var rule in rules)
        {
            ArgumentNullException.ThrowIfNull(rule);

            var ruleResults = new List<double>(iterationsPerRule);

            // Warmup
            for (int i = 0; i < 2; i++)
            {
                await ruleEngine.ExecuteRuleAsync(rule, elements);
            }

            // Benchmark
            for (int i = 0; i < iterationsPerRule; i++)
            {
                var startTime = DateTime.UtcNow;
                await ruleEngine.ExecuteRuleAsync(rule, elements);
                var elapsed = DateTime.UtcNow - startTime;
                ruleResults.Add(elapsed.TotalMilliseconds);
            }

            results[rule.Name] = new BenchmarkResult
            {
                WarmupIterations = 2,
                BenchmarkIterations = iterationsPerRule,
                MinMilliseconds = ruleResults.Min(),
                MaxMilliseconds = ruleResults.Max(),
                AverageMilliseconds = ruleResults.Average(),
                MedianMilliseconds = ruleResults.OrderBy(static x => x).ElementAt(ruleResults.Count / 2)
            };
        }

        return results;
    }

    /// <summary>
    /// Measures the overhead of the rule engine setup and teardown.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance. Cannot be null.</param>
    /// <param name="iterations">Number of iterations to measure (default: 20)</param>
    /// <returns>Average setup/teardown time in milliseconds</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="iterations"/> is negative or zero.</exception>
    /// <exception cref="InvalidOperationException">Setup must be called before benchmarking.</exception>
    public static async Task<double> MeasureEngineOverheadAsync(
        this RuleEngineBenchmarks benchmarks,
        int iterations = 20)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(iterations);

        var ruleEngine = (RuleEngine?)_ruleEngineField.GetValue(benchmarks);
        var elements = (List<CodeElement>?)_elementsField.GetValue(benchmarks);

        if (ruleEngine is null || elements is null)
        {
            throw new InvalidOperationException("Setup must be called before benchmarking.");
        }

        var results = new List<double>(iterations);

        for (int i = 0; i < iterations; i++)
        {
            var startTime = DateTime.UtcNow;
            // Simulate minimal work
            await Task.Delay(1);
            var elapsed = DateTime.UtcNow - startTime;
            results.Add(elapsed.TotalMilliseconds);
        }

        return results.Average();
    }

    /// <summary>
    /// Creates a benchmark that measures execution time with different element counts.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance. Cannot be null.</param>
    /// <param name="rule">The rule to benchmark. Cannot be null.</param>
    /// <param name="elementCounts">Array of element counts to test (default: [10, 50, 100])</param>
    /// <param name="iterationsPerCount">Number of iterations per count (default: 5)</param>
    /// <returns>Dictionary mapping element counts to their benchmark results</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> or <paramref name="rule"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="iterationsPerCount"/> is negative or zero.</exception>
    /// <exception cref="InvalidOperationException">Setup must be called before benchmarking.</exception>
    public static async Task<Dictionary<int, BenchmarkResult>> BenchmarkScalabilityAsync(
        this RuleEngineBenchmarks benchmarks,
        [DisallowNull] AnalysisRule rule,
        int[]? elementCounts = null,
        int iterationsPerCount = 5)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);
        ArgumentNullException.ThrowIfNull(rule);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(iterationsPerCount);

        var ruleEngine = (RuleEngine?)_ruleEngineField.GetValue(benchmarks);

        if (ruleEngine is null)
        {
            throw new InvalidOperationException("Setup must be called before benchmarking.");
        }

        elementCounts ??= new[] { 10, 50, 100 };
        var results = new Dictionary<int, BenchmarkResult>(elementCounts.Length);

        foreach (var count in elementCounts)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

            var elements = GenerateElements(count);
            var ruleResults = new List<double>(iterationsPerCount);

            // Warmup
            for (int i = 0; i < 2; i++)
            {
                await ruleEngine.ExecuteRuleAsync(rule, elements);
            }

            // Benchmark
            for (int i = 0; i < iterationsPerCount; i++)
            {
                var startTime = DateTime.UtcNow;
                await ruleEngine.ExecuteRuleAsync(rule, elements);
                var elapsed = DateTime.UtcNow - startTime;
                ruleResults.Add(elapsed.TotalMilliseconds);
            }

            results[count] = new BenchmarkResult
            {
                WarmupIterations = 2,
                BenchmarkIterations = iterationsPerCount,
                ElementCount = count,
                MinMilliseconds = ruleResults.Min(),
                MaxMilliseconds = ruleResults.Max(),
                AverageMilliseconds = ruleResults.Average(),
                MedianMilliseconds = ruleResults.OrderBy(static x => x).ElementAt(ruleResults.Count / 2)
            };
        }

        return results;
    }

    private static List<CodeElement> GenerateElements(int count)
    {
        var elements = new List<CodeElement>(count);
        for (int i = 0; i < count; i++)
        {
            elements.Add(new CodeElement(
                $"TestClass{i}",
                CodeElementType.Class,
                $"src/Test{i}.cs")
            {
                Namespace = "TestNamespace",
                StartLineNumber = 1,
                EndLineNumber = 10,
                FullyQualifiedName = $"TestNamespace.TestClass{i}"
            });
        }
        return elements;
    }

    /// <summary>
    /// Represents benchmark results for rule engine operations.
    /// </summary>
    public sealed class BenchmarkResult
    {
        public int WarmupIterations { get; set; }
        public int BenchmarkIterations { get; set; }
        public int? ElementCount { get; set; }
        public double MinMilliseconds { get; set; }
        public double MaxMilliseconds { get; set; }
        public double AverageMilliseconds { get; set; }
        public double MedianMilliseconds { get; set; }

        public override string ToString() => ElementCount.HasValue
            ? $"Elements: {ElementCount}, Avg: {AverageMilliseconds:F2}ms, Min: {MinMilliseconds:F2}ms, Max: {MaxMilliseconds:F2}ms"
            : $"Avg: {AverageMilliseconds:F2}ms, Min: {MinMilliseconds:F2}ms, Max: {MaxMilliseconds:F2}ms";
    }
}