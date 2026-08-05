#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="ReportingService"/> functionality.
/// Tests report generation, formatted output and asynchronous saving.
/// </summary>
public class ReportingServiceTests
{
    private static AnalysisResult CreateResultWithViolation()
    {
        var result = new AnalysisResult("SampleProject", "/src/SampleProject");
        result.AnalysisEndTime = result.AnalysisStartTime.AddSeconds(5);
        result.TotalFilesAnalyzed = 3;
        result.TotalElementsAnalyzed = 10;

        result.AddViolation(new RuleViolation
        {
            RuleId = "RG001",
            RuleName = "EmptyCatch",
            Message = "Empty catch block detected",
            Severity = SeverityLevel.Error,
            FilePath = "Foo.cs",
            LineNumber = 42,
            Category = RuleCategory.CodeStructure
        });

        return result;
    }

    [Fact]
    public void GenerateReport_WithViolations_ContainsProjectAndViolationDetails()
    {
        var service = new ReportingService();
        var result = CreateResultWithViolation();

        var report = service.GenerateReport(result);

        report.Should().Contain("SampleProject");
        report.Should().Contain("RG001");
        report.Should().Contain("Empty catch block detected");
        report.Should().Contain("Total Violations: 1");
    }

    [Fact]
    public void GenerateReport_WithNoViolations_ReportsNoViolationsFound()
    {
        var service = new ReportingService();
        var result = new AnalysisResult("CleanProject", "/src/CleanProject");
        result.AnalysisEndTime = result.AnalysisStartTime;

        var report = service.GenerateReport(result);

        report.Should().Contain("No violations found");
        report.Should().Contain("Total Violations: 0");
    }

    [Fact]
    public void GenerateReport_NullResult_ThrowsArgumentNullException()
    {
        var service = new ReportingService();

        Action act = () => service.GenerateReport(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("JSON")]
    [InlineData("CSV")]
    [InlineData("XML")]
    public void GenerateFormattedReport_KnownFormats_ProducesFormatSpecificContent(string format)
    {
        var service = new ReportingService();
        var result = CreateResultWithViolation();

        var report = service.GenerateFormattedReport(result, format);

        report.Should().Contain("RG001");
    }

    [Fact]
    public void GenerateFormattedReport_UnknownFormat_FallsBackToTextReport()
    {
        var service = new ReportingService();
        var result = CreateResultWithViolation();

        var report = service.GenerateFormattedReport(result, "UNKNOWN");

        report.Should().Contain("ROSLYN GUARD ANALYZER");
    }

    [Fact]
    public async Task SaveReportAsync_ValidPath_WritesContentToFile()
    {
        var service = new ReportingService();
        var report = new ViolationReport("Test Title", "TestProject")
        {
            DetailedContent = "Some detailed content"
        };
        var filePath = Path.Combine(Path.GetTempPath(), $"report_{Guid.NewGuid():N}.txt");

        try
        {
            await service.SaveReportAsync(report, filePath);

            File.Exists(filePath).Should().BeTrue();
            var content = await File.ReadAllTextAsync(filePath);
            content.Should().Be("Some detailed content");
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task SaveReportAsync_NullReport_ThrowsArgumentNullException()
    {
        var service = new ReportingService();

        Func<Task> act = () => service.SaveReportAsync(null!, "somefile.txt");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SaveReportAsync_EmptyFilePath_ThrowsArgumentException()
    {
        var service = new ReportingService();
        var report = new ViolationReport();

        Func<Task> act = () => service.SaveReportAsync(report, string.Empty);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
