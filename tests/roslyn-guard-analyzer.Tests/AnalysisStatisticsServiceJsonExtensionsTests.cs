using System;
using System.Text.Json;
using RoslynGuardAnalyzer.Services;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class AnalysisStatisticsServiceJsonExtensionsTests
{
    // Helper to create an instance of ViolationStatistics via deserialization.
    private static AnalysisStatisticsService.ViolationStatistics CreateEmptyStatistics()
    {
        // Deserialize an empty JSON object to get a default instance.
        return JsonSerializer.Deserialize<AnalysisStatisticsService.ViolationStatistics>("{}")!;
    }

    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        var stats = CreateEmptyStatistics();

        var json = stats.ToJson();

        Assert.False(string.IsNullOrWhiteSpace(json));
        // The default empty instance should serialize to "{}" or similar.
        Assert.Contains("{", json);
        Assert.Contains("}", json);
    }

    [Fact]
    public void ToJson_IndentedTrue_IncludesNewLine()
    {
        var stats = CreateEmptyStatistics();

        var json = stats.ToJson(indented: true);

        Assert.Contains("\n", json);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        AnalysisStatisticsService.ViolationStatistics? stats = null;

        Assert.Throws<ArgumentNullException>(() => stats!.ToJson());
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsInstance()
    {
        var stats = CreateEmptyStatistics();
        var json = stats.ToJson();

        var result = AnalysisStatisticsServiceJsonExtensions.FromJson(json);

        Assert.NotNull(result);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => AnalysisStatisticsServiceJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void FromJson_EmptyOrWhiteSpace_ReturnsNull()
    {
        var empty = AnalysisStatisticsServiceJsonExtensions.FromJson("");
        var whitespace = AnalysisStatisticsServiceJsonExtensions.FromJson("   ");

        Assert.Null(empty);
        Assert.Null(whitespace);
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndInstance()
    {
        var stats = CreateEmptyStatistics();
        var json = stats.ToJson();

        var success = AnalysisStatisticsServiceJsonExtensions.TryFromJson(json, out var result);

        Assert.True(success);
        Assert.NotNull(result);
    }

    [Fact]
    public void TryFromJson_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => AnalysisStatisticsServiceJsonExtensions.TryFromJson(null!, out _));
    }

    [Fact]
    public void TryFromJson_EmptyOrWhiteSpace_ReturnsFalse()
    {
        var successEmpty = AnalysisStatisticsServiceJsonExtensions.TryFromJson("", out var resultEmpty);
        var successWhitespace = AnalysisStatisticsServiceJsonExtensions.TryFromJson("   ", out var resultWhitespace);

        Assert.False(successEmpty);
        Assert.False(successWhitespace);
        Assert.Null(resultEmpty);
        Assert.Null(resultWhitespace);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        var success = AnalysisStatisticsServiceJsonExtensions.TryFromJson("{invalid json}", out var result);

        Assert.False(success);
        Assert.Null(result);
    }
}
