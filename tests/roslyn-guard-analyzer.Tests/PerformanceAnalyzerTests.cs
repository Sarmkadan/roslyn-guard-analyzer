using System;
using System.Linq;
using RoslynGuardAnalyzer.Utilities;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public sealed class PerformanceAnalyzerTests
{
    [Fact]
    public void RecordTiming_ValidInput_AddsTiming()
    {
        var analyzer = new PerformanceAnalyzer();
        analyzer.RecordTiming("ComponentA", 120);

        var metrics = analyzer.GetMetricsForComponent("ComponentA");
        Assert.NotNull(metrics);
        Assert.Equal("ComponentA", metrics!.ComponentName);
        Assert.Equal(120, metrics.TotalTimeMs);
        Assert.Equal(120, metrics.MinTimeMs);
        Assert.Equal(120, metrics.MaxTimeMs);
        Assert.Equal(120, metrics.AverageTimeMs);
        Assert.Equal(1, metrics.ExecutionCount);
    }

    [Fact]
    public void RecordTiming_NullOrWhiteSpaceComponent_ThrowsArgumentException()
    {
        var analyzer = new PerformanceAnalyzer();
        Assert.Throws<ArgumentException>(() => analyzer.RecordTiming(null!, 50));
        Assert.Throws<ArgumentException>(() => analyzer.RecordTiming("", 50));
        Assert.Throws<ArgumentException>(() => analyzer.RecordTiming("   ", 50));
    }

    [Fact]
    public void RecordTiming_NegativeMilliseconds_ThrowsArgumentException()
    {
        var analyzer = new PerformanceAnalyzer();
        Assert.Throws<ArgumentException>(() => analyzer.RecordTiming("ComponentB", -10));
    }

    [Fact]
    public void GetMetricsForComponent_NoTimings_ReturnsNull()
    {
        var analyzer = new PerformanceAnalyzer();
        var metrics = analyzer.GetMetricsForComponent("NonExistent");
        Assert.Null(metrics);
    }

    [Fact]
    public void GetAllMetrics_SortedDescendingByTotalTime()
    {
        var analyzer = new PerformanceAnalyzer();
        analyzer.RecordTiming("A", 100);
        analyzer.RecordTiming("B", 300);
        analyzer.RecordTiming("C", 200);

        var all = analyzer.GetAllMetrics();
        Assert.Equal(3, all.Count);
        Assert.Equal("B", all[0].ComponentName);
        Assert.Equal("C", all[1].ComponentName);
        Assert.Equal("A", all[2].ComponentName);
    }

    [Fact]
    public void GetBottlenecks_ReturnsTopNComponents()
    {
        var analyzer = new PerformanceAnalyzer();
        analyzer.RecordTiming("Fast", 10);
        analyzer.RecordTiming("Slow1", 500);
        analyzer.RecordTiming("Slow2", 400);
        analyzer.RecordTiming("Slow3", 300);

        var bottlenecks = analyzer.GetBottlenecks(2);
        Assert.Equal(2, bottlenecks.Count);
        Assert.Equal("Slow1", bottlenecks[0].ComponentName);
        Assert.Equal("Slow2", bottlenecks[1].ComponentName);
    }

    [Fact]
    public void Clear_RemovesAllTimings()
    {
        var analyzer = new PerformanceAnalyzer();
        analyzer.RecordTiming("Comp1", 50);
        analyzer.RecordTiming("Comp2", 70);

        analyzer.Clear();

        Assert.Empty(analyzer.GetAllMetrics());
        Assert.Equal(0, analyzer.ComponentCount);
        Assert.False(analyzer.HasComponent("Comp1"));
        Assert.False(analyzer.HasComponent("Comp2"));
    }

    [Fact]
    public void PercentageOfTotal_IsCalculatedCorrectly()
    {
        var analyzer = new PerformanceAnalyzer();
        analyzer.RecordTiming("X", 200);
        analyzer.RecordTiming("Y", 300);
        analyzer.RecordTiming("Z", 500);

        var all = analyzer.GetAllMetrics();
        var total = all.Sum(m => m.TotalTimeMs);

        foreach (var metric in all)
        {
            var expected = total > 0 ? (metric.TotalTimeMs * 100.0) / total : 0;
            Assert.Equal(expected, metric.PercentageOfTotal, precision: 5);
        }
    }
}
