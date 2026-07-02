using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;
using RoslynGuardAnalyzer.Rules;

namespace RoslynGuardAnalyzer.Benchmarks;

[MemoryDiagnoser]
public class RuleEngineBenchmarks
{
    private RuleEngine? _ruleEngine;
    private List<CodeElement>? _elements;
    private AnalysisRule? _sampleRule;

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

    [Benchmark]
    public async Task ExecuteRuleAsync()
    {
        await _ruleEngine!.ExecuteRuleAsync(_sampleRule!, _elements!);
    }

    [Benchmark]
    public async Task ExecuteAllRulesAsync()
    {
        await _ruleEngine!.ExecuteAllRulesAsync(_elements!);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<RuleEngineBenchmarks>();
    }
}
