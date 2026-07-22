#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for CliArgumentParser
// =============================================================================

using System;
using FluentAssertions;
using RoslynGuardAnalyzer.Cli;
using RoslynGuardAnalyzer.Core;
using Xunit;

namespace RoslynGuardAnalyzer.Tests.Cli;

public class CliArgumentParserTests
{
    [Fact]
    public void Parse_EmptyArgs_ReturnsDefaultOptions()
    {
        // Arrange
        var parser = new CliArgumentParser([]);

        // Act
        var options = parser.Parse();

        // Assert
        options.Should().NotBeNull();
        options.ShowHelp.Should().BeFalse();
        options.ShowVersion.Should().BeFalse();
        options.Verbose.Should().BeFalse();
        options.SkipCache.Should().BeFalse();
        options.ProjectPath.Should().BeNull();
        options.FilePath.Should().BeNull();
        options.OutputFile.Should().BeNull();
        options.OutputFormat.Should().Be("text");
        options.ConfigFile.Should().BeNull();
        options.MaxParallelThreads.Should().BeGreaterThan(0);
        options.AnalysisTimeoutSeconds.Should().Be(300);
        options.RuleFilter.Should().BeEmpty();
        options.FailOnViolations.Should().BeTrue();
        options.GenerateReport.Should().BeTrue();
        options.ReportType.Should().Be("summary");
        options.LogLevel.Should().Be(2);
        options.BaselineFile.Should().BeNull();
        options.CreateBaseline.Should().BeFalse();
        options.FailOnSeverity.Should().BeNull();
    }

    [Fact]
    public void Parse_HelpFlag_ShowsHelp()
    {
        // Arrange
        var parser = new CliArgumentParser(["--help"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.ShowHelp.Should().BeTrue();
        options.ShowVersion.Should().BeFalse();
    }

    [Fact]
    public void Parse_HelpShortFlag_ShowsHelp()
    {
        // Arrange
        var parser = new CliArgumentParser(["-h"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.ShowHelp.Should().BeTrue();
    }

    [Fact]
    public void Parse_VersionFlag_ShowsVersion()
    {
        // Arrange
        var parser = new CliArgumentParser(["--version"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.ShowVersion.Should().BeTrue();
        options.ShowHelp.Should().BeFalse();
    }

    [Fact]
    public void Parse_VersionShortFlag_ShowsVersion()
    {
        // Arrange
        var parser = new CliArgumentParser(["-v"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.ShowVersion.Should().BeTrue();
    }

    [Fact]
    public void Parse_VerboseFlag_SetsVerbose()
    {
        // Arrange
        var parser = new CliArgumentParser(["--verbose"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.Verbose.Should().BeTrue();
    }

    [Fact]
    public void Parse_SkipCacheFlag_SetsSkipCache()
    {
        // Arrange
        var parser = new CliArgumentParser(["--skip-cache"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.SkipCache.Should().BeTrue();
    }

    [Fact]
    public void Parse_ProjectPathWithEquals_SetsProjectPath()
    {
        // Arrange
        var parser = new CliArgumentParser(["--project=/path/to/project.csproj"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.ProjectPath.Should().Be("/path/to/project.csproj");
        options.FilePath.Should().BeNull();
    }

    [Fact]
    public void Parse_ProjectPathWithSpace_SetsProjectPath()
    {
        // Arrange
        var parser = new CliArgumentParser(["--project", "/path/to/project.csproj"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.ProjectPath.Should().Be("/path/to/project.csproj");
        options.FilePath.Should().BeNull();
    }

    [Fact]
    public void Parse_FilePathWithEquals_SetsFilePath()
    {
        // Arrange
        var parser = new CliArgumentParser(["--file=/path/to/file.cs"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.FilePath.Should().Be("/path/to/file.cs");
        options.ProjectPath.Should().BeNull();
    }

    [Fact]
    public void Parse_FilePathWithSpace_SetsFilePath()
    {
        // Arrange
        var parser = new CliArgumentParser(["--file", "/path/to/file.cs"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.FilePath.Should().Be("/path/to/file.cs");
        options.ProjectPath.Should().BeNull();
    }

    [Fact]
    public void Parse_OutputFileWithEquals_SetsOutputFile()
    {
        // Arrange
        var parser = new CliArgumentParser(["--output=/path/to/output.txt"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.OutputFile.Should().Be("/path/to/output.txt");
    }

    [Fact]
    public void Parse_OutputFileWithSpace_SetsOutputFile()
    {
        // Arrange
        var parser = new CliArgumentParser(["--output", "/path/to/output.txt"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.OutputFile.Should().Be("/path/to/output.txt");
    }

    [Fact]
    public void Parse_OutputFormatWithEquals_SetsOutputFormat()
    {
        // Arrange
        var parser = new CliArgumentParser(["--format=json"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.OutputFormat.Should().Be("json");
    }

    [Fact]
    public void Parse_OutputFormatWithSpace_SetsOutputFormat()
    {
        // Arrange
        var parser = new CliArgumentParser(["--format", "csv"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.OutputFormat.Should().Be("csv");
    }

    [Fact]
    public void Parse_ConfigFileWithEquals_SetsConfigFile()
    {
        // Arrange
        var parser = new CliArgumentParser(["--config=/path/to/config.json"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.ConfigFile.Should().Be("/path/to/config.json");
    }

    [Fact]
    public void Parse_ConfigFileWithSpace_SetsConfigFile()
    {
        // Arrange
        var parser = new CliArgumentParser(["--config", "/path/to/config.json"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.ConfigFile.Should().Be("/path/to/config.json");
    }

    [Fact]
    public void Parse_TimeoutWithEquals_SetsTimeout()
    {
        // Arrange
        var parser = new CliArgumentParser(["--timeout=60"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.AnalysisTimeoutSeconds.Should().Be(60);
    }

    [Fact]
    public void Parse_TimeoutWithSpace_SetsTimeout()
    {
        // Arrange
        var parser = new CliArgumentParser(["--timeout", "120"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.AnalysisTimeoutSeconds.Should().Be(120);
    }

    [Fact]
    public void Parse_TimeoutInvalidValue_UsesDefault()
    {
        // Arrange
        var parser = new CliArgumentParser(["--timeout", "invalid"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.AnalysisTimeoutSeconds.Should().Be(300); // default value
    }

    [Fact]
    public void Parse_ThreadsWithEquals_SetsThreads()
    {
        // Arrange
        var parser = new CliArgumentParser(["--threads=8"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.MaxParallelThreads.Should().Be(8);
    }

    [Fact]
    public void Parse_ThreadsWithSpace_SetsThreads()
    {
        // Arrange
        var parser = new CliArgumentParser(["--threads", "4"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.MaxParallelThreads.Should().Be(4);
    }

    [Fact]
    public void Parse_ThreadsInvalidValue_UsesDefault()
    {
        // Arrange
        var parser = new CliArgumentParser(["--threads", "invalid"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.MaxParallelThreads.Should().BeGreaterThan(0); // default value
    }

    [Fact]
    public void Parse_LogLevelWithEquals_SetsLogLevel()
    {
        // Arrange
        var parser = new CliArgumentParser(["--log-level=3"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.LogLevel.Should().Be(3);
    }

    [Fact]
    public void Parse_LogLevelWithSpace_SetsLogLevel()
    {
        // Arrange
        var parser = new CliArgumentParser(["--log-level", "1"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.LogLevel.Should().Be(1);
    }

    [Fact]
    public void Parse_LogLevelInvalidValue_UsesDefault()
    {
        // Arrange
        var parser = new CliArgumentParser(["--log-level", "invalid"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.LogLevel.Should().Be(2); // default value
    }

    [Fact]
    public void Parse_RuleFilterWithEquals_SetsRuleFilter()
    {
        // Arrange
        var parser = new CliArgumentParser(["--rule-filter=Rule1,Rule2,Rule3"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.RuleFilter.Should().HaveCount(3);
        options.RuleFilter.Should().Contain("Rule1");
        options.RuleFilter.Should().Contain("Rule2");
        options.RuleFilter.Should().Contain("Rule3");
    }

    [Fact]
    public void Parse_RuleFilterWithSpace_SetsRuleFilter()
    {
        // Arrange
        var parser = new CliArgumentParser(["--rule-filter", "RuleA,RuleB"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.RuleFilter.Should().HaveCount(2);
        options.RuleFilter.Should().Contain("RuleA");
        options.RuleFilter.Should().Contain("RuleB");
    }

    [Fact]
    public void Parse_RuleFilterWithSpaces_TrimsValues()
    {
        // Arrange
        var parser = new CliArgumentParser(["--rule-filter=  Rule1  ,  Rule2  "]);

        // Act
        var options = parser.Parse();

        // Assert
        options.RuleFilter.Should().HaveCount(2);
        options.RuleFilter.Should().Contain("Rule1");
        options.RuleFilter.Should().Contain("Rule2");
    }

    [Fact]
    public void Parse_NoFailOnViolations_SetsFailOnViolationsToFalse()
    {
        // Arrange
        var parser = new CliArgumentParser(["--no-fail-on-violations"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.FailOnViolations.Should().BeFalse();
    }

    [Fact]
    public void Parse_NoReport_SetsGenerateReportToFalse()
    {
        // Arrange
        var parser = new CliArgumentParser(["--no-report"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.GenerateReport.Should().BeFalse();
    }

    [Fact]
    public void Parse_ReportTypeWithEquals_SetsReportType()
    {
        // Arrange
        var parser = new CliArgumentParser(["--report-type=html"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.ReportType.Should().Be("html");
    }

    [Fact]
    public void Parse_ReportTypeWithSpace_SetsReportType()
    {
        // Arrange
        var parser = new CliArgumentParser(["--report-type", "xml"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.ReportType.Should().Be("xml");
    }

    [Fact]
    public void Parse_MixedFlags_AllFlagsParsedCorrectly()
    {
        // Arrange
        var parser = new CliArgumentParser([
            "--verbose",
            "--project=/path/to/project.csproj",
            "--format=json",
            "--timeout=120",
            "--threads=8",
            "--log-level=3",
            "--rule-filter=Rule1,Rule2"
        ]);

        // Act
        var options = parser.Parse();

        // Assert
        options.Verbose.Should().BeTrue();
        options.ProjectPath.Should().Be("/path/to/project.csproj");
        options.OutputFormat.Should().Be("json");
        options.AnalysisTimeoutSeconds.Should().Be(120);
        options.MaxParallelThreads.Should().Be(8);
        options.LogLevel.Should().Be(3);
        options.RuleFilter.Should().HaveCount(2);
        options.ShowHelp.Should().BeFalse();
        options.ShowVersion.Should().BeFalse();
    }

    [Fact]
    public void Parse_PositionalProjectPath_SetsProjectPath()
    {
        // Arrange
        var parser = new CliArgumentParser(["/path/to/project.csproj"]);

        // Act
        var options = parser.Parse();

        // Assert
        options.ProjectPath.Should().Be("/path/to/project.csproj");
    }

    [Fact]
    public void Parse_PositionalProjectPathWithOtherFlags_MixedCorrectly()
    {
        // Arrange
        var parser = new CliArgumentParser([
            "--verbose",
            "/path/to/project.csproj",
            "--format=json"
        ]);

        // Act
        var options = parser.Parse();

        // Assert
        options.Verbose.Should().BeTrue();
        options.ProjectPath.Should().Be("/path/to/project.csproj");
        options.OutputFormat.Should().Be("json");
    }

    [Fact]
    public void Parse_ShortFlags_AllParsedCorrectly()
    {
        // Arrange
        var parser = new CliArgumentParser([
            "-v",
            "--project=test.csproj",
            "--threads=4"
        ]);

        // Act
        var options = parser.Parse();

        // Assert
        options.ShowVersion.Should().BeTrue();
        options.ProjectPath.Should().Be("test.csproj");
        options.MaxParallelThreads.Should().Be(4);
    }

    [Fact]
    public void Parse_InvalidOption_Ignored()
    {
        // Arrange
        var parser = new CliArgumentParser([
            "--invalid-option",
            "--project=test.csproj"
        ]);

        // Act
        var options = parser.Parse();

        // Assert
        options.ProjectPath.Should().Be("test.csproj");
    }

    [Fact]
    public void Parse_MissingValueForProject_ThrowsArgumentException()
    {
        // Arrange
        var parser = new CliArgumentParser(["--project"]);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parser.Parse());
    }

    [Fact]
    public void Parse_MissingValueForFile_ThrowsArgumentException()
    {
        // Arrange
        var parser = new CliArgumentParser(["--file"]);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parser.Parse());
    }

    [Fact]
    public void Parse_MissingValueForOutput_ThrowsArgumentException()
    {
        // Arrange
        var parser = new CliArgumentParser(["--output"]);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parser.Parse());
    }

    [Fact]
    public void Parse_MissingValueForFormat_ThrowsArgumentException()
    {
        // Arrange
        var parser = new CliArgumentParser(["--format"]);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parser.Parse());
    }

    [Fact]
    public void Parse_MissingValueForConfig_ThrowsArgumentException()
    {
        // Arrange
        var parser = new CliArgumentParser(["--config"]);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parser.Parse());
    }

    [Fact]
    public void Parse_MissingValueForTimeout_ThrowsArgumentException()
    {
        // Arrange
        var parser = new CliArgumentParser(["--timeout"]);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parser.Parse());
    }

    [Fact]
    public void Parse_MissingValueForThreads_ThrowsArgumentException()
    {
        // Arrange
        var parser = new CliArgumentParser(["--threads"]);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parser.Parse());
    }

    [Fact]
    public void Parse_MissingValueForLogLevel_ThrowsArgumentException()
    {
        // Arrange
        var parser = new CliArgumentParser(["--log-level"]);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parser.Parse());
    }

    [Fact]
    public void Parse_MissingValueForRuleFilter_ThrowsArgumentException()
    {
        // Arrange
        var parser = new CliArgumentParser(["--rule-filter"]);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parser.Parse());
    }

    [Fact]
    public void Parse_MissingValueForReportType_ThrowsArgumentException()
    {
        // Arrange
        var parser = new CliArgumentParser(["--report-type"]);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parser.Parse());
    }

    [Fact]
    public void ParseSafe_WithValidArgs_ReturnsParsedOptions()
    {
        // Arrange
        var args = new[] { "--project=test.csproj", "--verbose" };

        // Act
        var options = CliArgumentParser.ParseSafe(args);

        // Assert
        options.Should().NotBeNull();
        options.ProjectPath.Should().Be("test.csproj");
        options.Verbose.Should().BeTrue();
    }

    [Fact]
    public void Parse_AllOutputFormats_Supported()
    {
        // Arrange
        var formats = new[] { "text", "json", "csv", "html", "xml", "sarif" };

        foreach (var format in formats)
        {
            // Act
            var parser = new CliArgumentParser([$"--format={format}"]);
            var options = parser.Parse();

            // Assert
            options.OutputFormat.Should().Be(format);
        }
    }

    [Fact]
    public void Parse_ComplexScenario_AllOptionsWorkTogether()
    {
        // Arrange
        var parser = new CliArgumentParser(new[]
        {
            "--project=/workspace/myproject.csproj",
            "--format=sarif",
            "--output=/reports/analysis.sarif",
            "--config=/config/guard-config.json",
            "--timeout=300",
            "--threads=16",
            "--log-level=2",
            "--rule-filter=LayerDependency,NamingConvention,AsyncPattern",
            "--verbose",
            "--no-fail-on-violations"
        });

        // Act
        var options = parser.Parse();

        // Assert
        options.ProjectPath.Should().Be("/workspace/myproject.csproj");
        options.OutputFormat.Should().Be("sarif");
        options.OutputFile.Should().Be("/reports/analysis.sarif");
        options.ConfigFile.Should().Be("/config/guard-config.json");
        options.AnalysisTimeoutSeconds.Should().Be(300);
        options.MaxParallelThreads.Should().Be(16);
        options.LogLevel.Should().Be(2);
        options.RuleFilter.Should().HaveCount(3);
        options.RuleFilter.Should().Contain("LayerDependency");
        options.RuleFilter.Should().Contain("NamingConvention");
        options.RuleFilter.Should().Contain("AsyncPattern");
        options.Verbose.Should().BeTrue();
        options.FailOnViolations.Should().BeFalse();
        options.GenerateReport.Should().BeTrue();
        options.ReportType.Should().Be("summary");
    }
}
