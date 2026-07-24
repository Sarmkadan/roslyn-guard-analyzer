using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;
using RoslynGuardAnalyzer.Rules;
using RoslynGuardAnalyzer.Utilities;

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
/// Benchmark class for string conversion methods.
/// </summary>
[MemoryDiagnoser]
public class StringExtensionsBenchmarks
{
    // Already converted cases (should be fast with early-out)
    private const string AlreadyConvertedPascal = "AlreadyPascalCase";
    private const string AlreadyConvertedCamel = "alreadyCamelCase";
    private const string AlreadyConvertedSnake = "already_snake_case";
    private const string AlreadyConvertedKebab = "already-kebab-case";

    // Simple cases that need conversion
    private const string PascalCaseInput = "HelloWorld";
    private const string camelCaseInput = "helloWorld";
    private const string snakeCaseInput = "hello_world_foo_bar";
    private const string kebabCaseInput = "hello-world-foo-bar";
    private const string MixedSeparatorsInput = "hello_world-foo bar";

    // Edge cases
    private const string EmptyInput = "";
    private const string SingleCharInput = "a";
    private const string TwoCharInput = "aB";
    private const string LongMixedInput = "ThisIsALongMixedCaseStringWithVariousSeparators_AndMoreParts-ToTest";

    // Real-world patterns from analyzer usage
    private const string TypeNameInput = "IMyInterface";
    private const string MethodNameInput = "GetUserById";
    private const string PropertyNameInput = "IsValid";
    private const string ComplexIdentifierInput = "HttpClientFactoryOptions";

    // Very long strings that test allocation patterns
    private const string VeryLongPascalInput = "ThisIsAVeryLongPascalCaseStringThatMightBeEncounteredInRealAnalyzerScenarios";
    private const string VeryLongSnakeInput = "this_is_a_very_long_snake_case_string_that_might_be_encountered_in_real_analyzer_scenarios";

    /// <summary>
    /// Initializes the benchmark setup.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        // Setup is handled by individual benchmarks
    }

    #region ToPascalCase Benchmarks

    [Benchmark]
    public string ToPascalCase_AlreadyPascalCase()
    {
        return AlreadyConvertedPascal.ToPascalCase();
    }

    [Benchmark]
    public string ToPascalCase_SnakeCase()
    {
        return snakeCaseInput.ToPascalCase();
    }

    [Benchmark]
    public string ToPascalCase_CamelCase()
    {
        return camelCaseInput.ToPascalCase();
    }

    [Benchmark]
    public string ToPascalCase_MixedSeparators()
    {
        return MixedSeparatorsInput.ToPascalCase();
    }

    [Benchmark]
    public string ToPascalCase_SingleChar()
    {
        return SingleCharInput.ToPascalCase();
    }

    [Benchmark]
    public string ToPascalCase_TwoChars()
    {
        return TwoCharInput.ToPascalCase();
    }

    [Benchmark]
    public string ToPascalCase_Empty()
    {
        return EmptyInput.ToPascalCase();
    }

    [Benchmark]
    public string ToPascalCase_TypeName()
    {
        return TypeNameInput.ToPascalCase();
    }

    [Benchmark]
    public string ToPascalCase_MethodName()
    {
        return MethodNameInput.ToPascalCase();
    }

    [Benchmark]
    public string ToPascalCase_VeryLong()
    {
        return VeryLongPascalInput.ToPascalCase();
    }

    #endregion

    #region ToCamelCase Benchmarks

    [Benchmark]
    public string ToCamelCase_AlreadyCamelCase()
    {
        return AlreadyConvertedCamel.ToCamelCase();
    }

    [Benchmark]
    public string ToCamelCase_PascalCase()
    {
        return PascalCaseInput.ToCamelCase();
    }

    [Benchmark]
    public string ToCamelCase_SnakeCase()
    {
        return snakeCaseInput.ToCamelCase();
    }

    [Benchmark]
    public string ToCamelCase_MixedSeparators()
    {
        return MixedSeparatorsInput.ToCamelCase();
    }

    [Benchmark]
    public string ToCamelCase_TypeName()
    {
        return TypeNameInput.ToCamelCase();
    }

    [Benchmark]
    public string ToCamelCase_MethodName()
    {
        return MethodNameInput.ToCamelCase();
    }

    #endregion

    #region ToSnakeCase Benchmarks

    [Benchmark]
    public string ToSnakeCase_AlreadySnakeCase()
    {
        return AlreadyConvertedSnake.ToSnakeCase();
    }

    [Benchmark]
    public string ToSnakeCase_PascalCase()
    {
        return PascalCaseInput.ToSnakeCase();
    }

    [Benchmark]
    public string ToSnakeCase_CamelCase()
    {
        return camelCaseInput.ToSnakeCase();
    }

    [Benchmark]
    public string ToSnakeCase_PropertyName()
    {
        return PropertyNameInput.ToSnakeCase();
    }

    [Benchmark]
    public string ToSnakeCase_LongMixed()
    {
        return LongMixedInput.ToSnakeCase();
    }

    [Benchmark]
    public string ToSnakeCase_VeryLong()
    {
        return VeryLongSnakeInput.ToSnakeCase();
    }

    [Benchmark]
    public string ToSnakeCase_TypeName()
    {
        return TypeNameInput.ToSnakeCase();
    }

    [Benchmark]
    public string ToSnakeCase_ComplexIdentifier()
    {
        return ComplexIdentifierInput.ToSnakeCase();
    }

    #endregion

    #region ToKebabCase Benchmarks

    [Benchmark]
    public string ToKebabCase_AlreadyKebabCase()
    {
        return AlreadyConvertedKebab.ToKebabCase();
    }

    [Benchmark]
    public string ToKebabCase_PascalCase()
    {
        return PascalCaseInput.ToKebabCase();
    }

    [Benchmark]
    public string ToKebabCase_SnakeCase()
    {
        return snakeCaseInput.ToKebabCase();
    }

    [Benchmark]
    public string ToKebabCase_MixedSeparators()
    {
        return MixedSeparatorsInput.ToKebabCase();
    }

    [Benchmark]
    public string ToKebabCase_LongMixed()
    {
        return LongMixedInput.ToKebabCase();
    }

    [Benchmark]
    public string ToKebabCase_TypeName()
    {
        return TypeNameInput.ToKebabCase();
    }

    #endregion

    /// <summary>
    /// Benchmark that simulates real analyzer workload with many identifiers.
    /// </summary>
    [Benchmark]
    public string[] AnalyzerIdentifierConversionWorkload()
    {
        string[] inputs = {
            "GetUserById", "IsValid", "HttpClientFactory", "AnalysisService",
            "RuleEngine", "CodeElement", "ViolationReport", "ConfigurationOption",
            "NamingConvention", "PerformanceMetric", "MemoryUsage", "ThreadSafety"
        };

        string[] results = new string[inputs.Length];
        for (int i = 0; i < inputs.Length; i++)
        {
            results[i] = inputs[i].ToSnakeCase();
        }

        return results;
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
        BenchmarkRunner.Run<StringExtensionsBenchmarks>();
        BenchmarkRunner.Run<RuleEngineBenchmarks>();
    }
}
