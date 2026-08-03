using BenchmarkDotNet.Attributes;
using RoslynGuardAnalyzer.Integration;
using System;

namespace RoslynGuardAnalyzer.Benchmarks;

[MemoryDiagnoser]
public class HttpClientFactoryBenchmarks
{
    private HttpClientFactory? _factory;
    private string[]? _uniqueUrls;
    private const string SingleUrl = "https://example.com/api";

    // Params for input size
    [Params(10, 100, 1000)]
    public int N { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _factory = new HttpClientFactory();
        _uniqueUrls = new string[N];
        
        for (int i = 0; i < N; i++)
        {
            _uniqueUrls[i] = $"https://example.com/api/endpoint{i}";
        }
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _factory?.Dispose();
    }

    [Benchmark]
    public void CreateClient_CacheMiss()
    {
        if (_factory == null || _uniqueUrls == null) return;

        for (int i = 0; i < N; i++)
        {
            _factory.CreateClient(_uniqueUrls[i]);
        }
    }

    [Benchmark]
    public void CreateClient_CacheHit()
    {
        if (_factory == null) return;

        for (int i = 0; i < N; i++)
        {
            _factory.CreateClient(SingleUrl);
        }
    }

    // Separate factory instance for ClearCache to ensure it runs on a populated cache every iteration
    private HttpClientFactory? _factoryForClear;

    [IterationSetup(Target = nameof(ClearCache))]
    public void SetupClearCache()
    {
        _factoryForClear = new HttpClientFactory();
        for (int i = 0; i < N; i++)
        {
            _factoryForClear.CreateClient($"https://example.com/api/{i}");
        }
    }

    [IterationCleanup(Target = nameof(ClearCache))]
    public void CleanupClearCache()
    {
        _factoryForClear?.Dispose();
    }

    [Benchmark]
    public void ClearCache()
    {
        _factoryForClear?.ClearCache();
    }
}
