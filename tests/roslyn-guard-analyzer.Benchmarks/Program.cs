using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;
using RoslynGuardAnalyzer.Rules;

namespace RoslynGuardAnalyzer.Benchmarks;

/// <summary>
/// Benchmark class for RuleEngine performance.
/// </summary>
[MemoryDiagnoser]
public class RuleEngineBenchmarks
{
    private RuleEngine? _ruleEngine;
    private List<CodeElement>? _elements;
    private AnalysisRule? _sampleRule;

    /// <summary>
    /// Initializes the benchmark setup.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        var registry = new RuleRegistry(); // Assuming a default implementation
        _ruleEngine = new RuleEngine(registry);
        
        _elements = new List<CodeElement>
        {
            new CodeElement("TestRepository", CodeElementType.Class, "src/Test.cs")
            {
                Namespace = "TestNamespace",
                StartLineNumber = 1,
                EndLineNumber = 2,
                FullyQualifiedName = "TestNamespace.TestRepository"
            },
            new CodeElement("TestService", CodeElementType.Class, "src/Test.cs")
            {
                Namespace = "TestNamespace",
                StartLineNumber = 10,
                EndLineNumber = 11,
                FullyQualifiedName = "TestNamespace.TestService"
            }
        };
        _elements[0].AddDependency("TestNamespace.TestService");

        _sampleRule = new AnalysisRule("R001", "NamingRule", "Description", RuleCategory.NamingConvention);
    }

    /// <summary>
    /// Executes a single rule asynchronously.
    /// </summary>
    /// <returns>A task representing the execution of the rule.</returns>
    [Benchmark]
    public async Task ExecuteRuleAsync()
    {
        await _ruleEngine!.ExecuteRuleAsync(_sampleRule!, _elements!);
    }

    /// <summary>
    /// Executes all rules asynchronously.
    /// </summary>
    /// <returns>A task representing the execution of all rules.</returns>
    [Benchmark]
    public async Task ExecuteAllRulesAsync()
    {
        await _ruleEngine!.ExecuteAllRulesAsync(_elements!);
    }
}

/// <summary>
/// Program entry point for benchmarking.
/// </summary>
public class Program
{
    /// <summary>
    /// Runs the benchmark.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<RuleEngineBenchmarks>();
    }
}
