using BenchmarkDotNet.Attributes;
using RoslynGuardAnalyzer.CodeFixes;

namespace RoslynGuardAnalyzer.Benchmarks;

[MemoryDiagnoser]
public class CodeFixBenchmarks
{
    private CodeFix _codeFix = null!;
    private int _size;

    [Params(10, 100, 1000)]
    public int Size
    {
        get => _size;
        set
        {
            _size = value;
            // Recreate the CodeFix when size changes
            _codeFix = new CodeFix
            {
                RuleId = new string('a', value),
                FilePath = new string('b', value),
                OriginalCode = new string('c', value),
                StartLine = value
            };
        }
    }

    [GlobalSetup]
    public void Setup()
    {
        // Initialize with default size (first param value)
        Size = 10;
    }

    [Benchmark]
    public CodeFix CreateCodeFix() => new CodeFix
    {
        RuleId = new string('a', _size),
        FilePath = new string('b', _size),
        OriginalCode = new string('c', _size),
        StartLine = _size
    };

    [Benchmark]
    public string GetSummary() => _codeFix.GetSummary();

    [Benchmark]
    public bool IsValid() => _codeFix.IsValid();
}