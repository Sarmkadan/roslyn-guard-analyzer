#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for CliArgumentParserJsonExtensions
// =====================================================================

using System;
using System.Text.Json;
using FluentAssertions;
using RoslynGuardAnalyzer.Cli;
using Xunit;

namespace RoslynGuardAnalyzer.Tests.Cli;

public class CliArgumentParserJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithDefaultParser_ReturnsValidJson()
    {
        // Arrange
        var parser = new CliArgumentParser([]);

        // Act
        var json = parser.ToJson();

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("showHelp");
        json.Should().Contain("false");
        json.Should().Contain("showVersion");
        json.Should().Contain("false");
        json.Should().Contain("outputFormat");
        json.Should().Contain("text");
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var parser = new CliArgumentParser(["--verbose", "--project=test.csproj"]);

        // Act
        var json = parser.ToJson(indented: true);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("{");
        json.Should().Contain("}");
        json.Should().Contain("\n"); // Should have newlines for formatting
    }

    [Fact]
    public void ToJson_WithIndentedFalse_ReturnsCompactJson()
    {
        // Arrange
        var parser = new CliArgumentParser(["--verbose"]);

        // Act
        var json = parser.ToJson(indented: false);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().NotContain("\n"); // Should be compact
        json.Should().StartWith("{");
        json.Should().EndWith("}");
    }

    [Fact]
    public void ToJson_NullParser_ThrowsArgumentNullException()
    {
        // Arrange
        CliArgumentParser? parser = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => parser!.ToJson());
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsParserInstance()
    {
        // Arrange
        var originalParser = new CliArgumentParser([
            "--verbose",
            "--project=/path/to/project.csproj",
            "--format=json",
            "--timeout=120",
            "--threads=8"
        ]);
        var json = originalParser.ToJson();

        // Act
        var parsedParser = CliArgumentParserJsonExtensions.FromJson(json);

        // Assert
        parsedParser.Should().NotBeNull();
        var parsedOptions = parsedParser!.Parse();
        var originalOptions = originalParser.Parse();

        parsedOptions.Verbose.Should().Be(originalOptions.Verbose);
        parsedOptions.ProjectPath.Should().Be(originalOptions.ProjectPath);
        parsedOptions.OutputFormat.Should().Be(originalOptions.OutputFormat);
        parsedOptions.AnalysisTimeoutSeconds.Should().Be(originalOptions.AnalysisTimeoutSeconds);
        parsedOptions.MaxParallelThreads.Should().Be(originalOptions.MaxParallelThreads);
    }

    [Fact]
    public void FromJson_JsonWithAllOptions_ReturnsParserWithAllOptions()
    {
        // Arrange
        var originalParser = new CliArgumentParser([
            "--project=/workspace/project.csproj",
            "--file=/path/to/file.cs",
            "--output=/output/report.txt",
            "--format=csv",
            "--config=/config/settings.json",
            "--verbose",
            "--timeout=180",
            "--threads=4",
            "--log-level=3",
            "--rule-filter=Rule1,Rule2,Rule3",
            "--no-fail-on-violations",
            "--no-report",
            "--report-type=html",
            "--skip-cache"
        ]);
        var json = originalParser.ToJson();

        // Act
        var parsedParser = CliArgumentParserJsonExtensions.FromJson(json);

        // Assert
        parsedParser.Should().NotBeNull();
        var parsedOptions = parsedParser!.Parse();
        var originalOptions = originalParser.Parse();

        parsedOptions.ProjectPath.Should().Be(originalOptions.ProjectPath);
        parsedOptions.FilePath.Should().Be(originalOptions.FilePath);
        parsedOptions.OutputFile.Should().Be(originalOptions.OutputFile);
        parsedOptions.OutputFormat.Should().Be(originalOptions.OutputFormat);
        parsedOptions.ConfigFile.Should().Be(originalOptions.ConfigFile);
        parsedOptions.Verbose.Should().Be(originalOptions.Verbose);
        parsedOptions.AnalysisTimeoutSeconds.Should().Be(originalOptions.AnalysisTimeoutSeconds);
        parsedOptions.MaxParallelThreads.Should().Be(originalOptions.MaxParallelThreads);
        parsedOptions.LogLevel.Should().Be(originalOptions.LogLevel);
        parsedOptions.RuleFilter.Should().BeEquivalentTo(originalOptions.RuleFilter);
        parsedOptions.FailOnViolations.Should().Be(originalOptions.FailOnViolations);
        parsedOptions.GenerateReport.Should().Be(originalOptions.GenerateReport);
        parsedOptions.ReportType.Should().Be(originalOptions.ReportType);
        parsedOptions.SkipCache.Should().Be(originalOptions.SkipCache);
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CliArgumentParserJsonExtensions.FromJson(json!));
    }

    [Fact]
    public void FromJson_EmptyJson_ReturnsParserWithDefaultValues()
    {
        // Arrange
        var json = "{}";

        // Act
        var result = CliArgumentParserJsonExtensions.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        var parsedOptions = result!.Parse();
        parsedOptions.ShowHelp.Should().BeFalse();
        parsedOptions.ShowVersion.Should().BeFalse();
        parsedOptions.OutputFormat.Should().Be("text");
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var json = "invalid json {{";

        // Act & Assert
        Assert.Throws<JsonException>(() => CliArgumentParserJsonExtensions.FromJson(json));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndParser()
    {
        // Arrange
        var originalParser = new CliArgumentParser([
            "--verbose",
            "--project=/test/project.csproj",
            "--timeout=90"
        ]);
        var json = originalParser.ToJson();

        // Act
        var result = CliArgumentParserJsonExtensions.TryFromJson(json, out var parsedParser);

        // Assert
        result.Should().BeTrue();
        parsedParser.Should().NotBeNull();
        var parsedOptions = parsedParser!.Parse();
        var originalOptions = originalParser.Parse();

        parsedOptions.Verbose.Should().Be(originalOptions.Verbose);
        parsedOptions.ProjectPath.Should().Be(originalOptions.ProjectPath);
        parsedOptions.AnalysisTimeoutSeconds.Should().Be(originalOptions.AnalysisTimeoutSeconds);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var json = "invalid json {{";

        // Act
        var result = CliArgumentParserJsonExtensions.TryFromJson(json, out var parsedParser);

        // Assert
        result.Should().BeFalse();
        parsedParser.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_NullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => CliArgumentParserJsonExtensions.TryFromJson(json!, out _)
        );
    }

    [Fact]
    public void TryFromJson_EmptyJson_ReturnsTrueAndParser()
    {
        // Arrange
        var json = "{}";

        // Act
        var result = CliArgumentParserJsonExtensions.TryFromJson(json, out var parsedParser);

        // Assert
        result.Should().BeTrue();
        parsedParser.Should().NotBeNull();
        var parsedOptions = parsedParser!.Parse();
        parsedOptions.ShowHelp.Should().BeFalse();
    }

    [Fact]
    public void RoundTrip_SerializeAndDeserialize_ReturnsEquivalentParser()
    {
        // Arrange
        var originalParser = new CliArgumentParser([
            "--project=/workspace/myproject.csproj",
            "--format=json",
            "--output=/reports/output.json",
            "--verbose",
            "--timeout=240",
            "--threads=16",
            "--log-level=1",
            "--rule-filter=Security,Performance,Maintainability"
        ]);

        // Act
        var json = originalParser.ToJson();
        var deserializedParser = CliArgumentParserJsonExtensions.FromJson(json);

        // Assert
        deserializedParser.Should().NotBeNull();
        var deserializedOptions = deserializedParser!.Parse();
        var originalOptions = originalParser.Parse();

        deserializedOptions.ProjectPath.Should().Be(originalOptions.ProjectPath);
        deserializedOptions.OutputFormat.Should().Be(originalOptions.OutputFormat);
        deserializedOptions.OutputFile.Should().Be(originalOptions.OutputFile);
        deserializedOptions.Verbose.Should().Be(originalOptions.Verbose);
        deserializedOptions.AnalysisTimeoutSeconds.Should().Be(originalOptions.AnalysisTimeoutSeconds);
        deserializedOptions.MaxParallelThreads.Should().Be(originalOptions.MaxParallelThreads);
        deserializedOptions.LogLevel.Should().Be(originalOptions.LogLevel);
        deserializedOptions.RuleFilter.Should().BeEquivalentTo(originalOptions.RuleFilter);
    }

    [Fact]
    public void RoundTrip_WithAllBooleanFlags_RoundTripsCorrectly()
    {
        // Arrange
        var originalParser = new CliArgumentParser([
            "--verbose",
            "--skip-cache",
            "--no-fail-on-violations",
            "--no-report"
        ]);

        // Act
        var json = originalParser.ToJson();
        var deserializedParser = CliArgumentParserJsonExtensions.FromJson(json);

        // Assert
        deserializedParser.Should().NotBeNull();
        var deserializedOptions = deserializedParser!.Parse();
        var originalOptions = originalParser.Parse();

        deserializedOptions.Verbose.Should().Be(originalOptions.Verbose);
        deserializedOptions.SkipCache.Should().Be(originalOptions.SkipCache);
        deserializedOptions.FailOnViolations.Should().Be(originalOptions.FailOnViolations);
        deserializedOptions.GenerateReport.Should().Be(originalOptions.GenerateReport);
    }

    [Fact]
    public void RoundTrip_WithEnumValues_RoundTripsCorrectly()
    {
        // Arrange
        var originalParser = new CliArgumentParser([
            "--log-level=0",
            "--report-type=sarif"
        ]);

        // Act
        var json = originalParser.ToJson();
        var deserializedParser = CliArgumentParserJsonExtensions.FromJson(json);

        // Assert
        deserializedParser.Should().NotBeNull();
        var deserializedOptions = deserializedParser!.Parse();
        var originalOptions = originalParser.Parse();

        deserializedOptions.LogLevel.Should().Be(originalOptions.LogLevel);
        deserializedOptions.ReportType.Should().Be(originalOptions.ReportType);
    }

    [Fact]
    public void ToJson_ProducesCamelCasePropertyNames()
    {
        // Arrange
        var parser = new CliArgumentParser(["--project=test.csproj"]);

        // Act
        var json = parser.ToJson();

        // Assert
        json.Should().Contain("projectPath");
        json.Should().Contain("outputFormat");
        json.Should().Contain("analysisTimeoutSeconds");
        json.Should().NotContain("ProjectPath"); // PascalCase should not be present
    }

    [Fact]
    public void FromJson_ProducesParserThatParsesToSameOptions()
    {
        // Arrange
        var originalParser = new CliArgumentParser([
            "--project=/test/project.csproj",
            "--format=json",
            "--timeout=120",
            "--threads=8",
            "--log-level=3",
            "--rule-filter=Rule1,Rule2"
        ]);
        var json = originalParser.ToJson();
        var deserializedParser = CliArgumentParserJsonExtensions.FromJson(json);

        // Act - parse both
        var originalOptions = originalParser.Parse();
        var deserializedOptions = deserializedParser!.Parse();

        // Assert
        deserializedOptions.Should().BeEquivalentTo(originalOptions);
    }
}