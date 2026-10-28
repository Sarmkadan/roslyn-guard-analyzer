using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using RoslynGuardAnalyzer.Services;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Tests for DiagnosticsService run statistics, error history, and reporting.
/// </summary>
public sealed class DiagnosticsServiceTests
{
    [Fact]
    public void RecordAnalysis_MultipleRuns_AccumulatesCountTimeAndViolations()
    {
        // Arrange
        var service = new DiagnosticsService();

        // Act
        service.RecordAnalysis(100, 3);
        service.RecordAnalysis(50, 7);

        // Assert
        service.GetAnalysisCount().Should().Be(2);
        service.GetAverageAnalysisTime().Should().Be(75);
        service.GetTotalViolationsFound().Should().Be(10);
    }

    [Fact]
    public void GetAverageAnalysisTime_NoRuns_ReturnsZeroWithoutDividingByZero()
    {
        // Arrange
        var service = new DiagnosticsService();

        // Act
        var average = service.GetAverageAnalysisTime();

        // Assert
        average.Should().Be(0);
        service.GetAnalysisCount().Should().Be(0);
    }

    [Fact]
    public void GetAverageAnalysisTime_NonDivisibleTotal_TruncatesToWholeMilliseconds()
    {
        // Arrange
        var service = new DiagnosticsService();
        service.RecordAnalysis(1, 0);
        service.RecordAnalysis(2, 0);

        // Act
        var average = service.GetAverageAnalysisTime();

        // Assert
        average.Should().Be(1); // 3 / 2 with integer division
    }

    [Fact]
    public void RecordError_MoreThanTenErrors_KeepsOnlyLastTenButCountsAll()
    {
        // Arrange
        var service = new DiagnosticsService();

        // Act
        for (var i = 0; i < 12; i++)
        {
            service.RecordError($"error-{i}");
        }

        // Assert
        service.GetErrorCount().Should().Be(12);
        service.GetRecentErrors(100).Should().Equal(Enumerable.Range(2, 10).Select(i => $"error-{i}").ToArray());
        service.GetRecentErrors().Should().Equal("error-7", "error-8", "error-9", "error-10", "error-11");
        service.GetRecentErrors(0).Should().BeEmpty();
    }

    [Fact]
    public void RecordError_NullMessage_IsStoredWithoutThrowing()
    {
        // Arrange
        var service = new DiagnosticsService();

        // Act
        Action act = () => service.RecordError(null!);

        // Assert
        act.Should().NotThrow();
        service.GetErrorCount().Should().Be(1);
        service.GetRecentErrors().Should().ContainSingle().Which.Should().BeNull();
    }

    [Fact]
    public void Reset_AfterRecordingData_ClearsAllStatistics()
    {
        // Arrange
        var service = new DiagnosticsService();
        service.RecordAnalysis(10, 1);
        service.RecordError("failure");

        // Act
        service.Reset();

        // Assert
        service.GetAnalysisCount().Should().Be(0);
        service.GetTotalViolationsFound().Should().Be(0);
        service.GetAverageAnalysisTime().Should().Be(0);
        service.GetErrorCount().Should().Be(0);
        service.GetRecentErrors().Should().BeEmpty();
    }

    [Fact]
    public void GenerateDiagnosticReport_RecordedData_ReflectsStatisticsAndErrors()
    {
        // Arrange
        var service = new DiagnosticsService();
        service.RecordAnalysis(40, 5);
        service.RecordError("boom");

        // Act
        var report = service.GenerateDiagnosticReport();

        // Assert
        report.Should().Contain("Total Runs: 1");
        report.Should().Contain("Avg Time: 40ms");
        report.Should().Contain("Total Violations: 5");
        report.Should().Contain("Total Errors: 1");
        report.Should().Contain("- boom");
        report.Should().Contain("AnalysesCompleted: 1");
    }

    [Fact]
    public void RecordAnalysis_ConcurrentCalls_DoesNotLoseUpdates()
    {
        // Arrange
        var service = new DiagnosticsService();

        // Act
        Parallel.For(0, 1000, _ => service.RecordAnalysis(2, 3));

        // Assert
        service.GetAnalysisCount().Should().Be(1000);
        service.GetTotalViolationsFound().Should().Be(3000);
        service.GetAverageAnalysisTime().Should().Be(2);
    }
}
