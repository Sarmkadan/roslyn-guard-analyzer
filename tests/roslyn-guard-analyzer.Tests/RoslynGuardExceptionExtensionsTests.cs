#nullable enable

using System;
using System.Collections.Generic;
using FluentAssertions;
using RoslynGuardAnalyzer.Exceptions;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="RoslynGuardExceptionExtensions"/>.
/// </summary>
public class RoslynGuardExceptionExtensionsTests
{
    #region FormatErrorReport tests

    [Fact]
    public void FormatErrorReport_RuleNotFoundException_ReturnsFormattedReport()
    {
        // Arrange
        var exception = new RuleNotFoundException("RULE001", "Custom message");

        // Act
        var report = exception.FormatErrorReport();

        // Assert
        report.Should().NotBeNullOrEmpty();
        report.Should().Contain("=== ROSLYN GUARD ANALYZER ERROR REPORT ===");
        report.Should().Contain("Error Code: ERR003");
        report.Should().Contain("Exception Type: RuleNotFoundException");
        report.Should().Contain("Rule ID: RULE001");
        report.Should().Contain("Message: Custom message");
        report.Should().Contain("END OF ERROR REPORT");
    }

    [Fact]
    public void FormatErrorReport_AnalysisException_ReturnsFormattedReport()
    {
        // Arrange
        var exception = new AnalysisException("Analysis failed");
        exception.ProjectPath = "/path/to/project.csproj";
        exception.AddDetail("Detail 1");
        exception.AddDetail("Detail 2");

        // Act
        var report = exception.FormatErrorReport();

        // Assert
        report.Should().NotBeNullOrEmpty();
        report.Should().Contain("Exception Type: AnalysisException");
        report.Should().Contain("Project Path: /path/to/project.csproj");
        report.Should().Contain("Details (2 items):");
        report.Should().Contain(" - Detail 1");
        report.Should().Contain(" - Detail 2");
    }

    [Fact]
    public void FormatErrorReport_ConfigurationException_ReturnsFormattedReport()
    {
        // Arrange
        var exception = new ConfigurationException("Invalid config", "MaxThreads");

        // Act
        var report = exception.FormatErrorReport();

        // Assert
        report.Should().NotBeNullOrEmpty();
        report.Should().Contain("Exception Type: ConfigurationException");
        report.Should().Contain("Config Key: MaxThreads");
    }

    [Fact]
    public void FormatErrorReport_FileAccessException_ReturnsFormattedReport()
    {
        // Arrange
        var exception = new FileAccessException("/path/to/file.cs", "Cannot read file");

        // Act
        var report = exception.FormatErrorReport();

        // Assert
        report.Should().NotBeNullOrEmpty();
        report.Should().Contain("Exception Type: FileAccessException");
        report.Should().Contain("File Path: /path/to/file.cs");
    }

    [Fact]
    public void FormatErrorReport_ParseException_ReturnsFormattedReport()
    {
        // Arrange
        var exception = new ParseException("/path/to/file.cs", "Syntax error");

        // Act
        var report = exception.FormatErrorReport();

        // Assert
        report.Should().NotBeNullOrEmpty();
        report.Should().Contain("Exception Type: ParseException");
        report.Should().Contain("File Path: /path/to/file.cs");
    }

    [Fact]
    public void FormatErrorReport_AnalysisTimeoutException_ReturnsFormattedReport()
    {
        // Arrange
        var exception = new AnalysisTimeoutException(30);

        // Act
        var report = exception.FormatErrorReport();

        // Assert
        report.Should().NotBeNullOrEmpty();
        report.Should().Contain("Exception Type: AnalysisTimeoutException");
        report.Should().Contain("Timeout: 30 seconds");
    }

    [Fact]
    public void FormatErrorReport_BaseRoslynGuardException_ReturnsFormattedReport()
    {
        // Arrange
        var exception = new AnalysisException("Base exception");
        exception.ErrorCode = "ERR999";

        // Act
        var report = exception.FormatErrorReport();

        // Assert
        report.Should().NotBeNullOrEmpty();
        report.Should().Contain("Exception Type: AnalysisException");
        report.Should().Contain("Error Code: ERR999");
    }

    [Fact]
    public void FormatErrorReport_WithInnerException_IncludesInnerExceptionInReport()
    {
        // Arrange
        var inner = new InvalidOperationException("Inner error");
        var exception = new AnalysisException("Outer error", inner);

        // Act
        var report = exception.FormatErrorReport();

        // Assert
        report.Should().NotBeNullOrEmpty();
        report.Should().Contain("=== INNER EXCEPTION ===");
        report.Should().Contain("Inner error");
    }

    [Fact]
    public void FormatErrorReport_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        RoslynGuardException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.FormatErrorReport());
    }

    #endregion

    #region ToErrorSummary tests

    [Fact]
    public void ToErrorSummary_RuleNotFoundException_ReturnsSummaryWithRuleId()
    {
        // Arrange
        var exception = new RuleNotFoundException("RULE001");

        // Act
        var summary = exception.ToErrorSummary();

        // Assert
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("[ERR003]");
        summary.Should().Contain("Rule with ID 'RULE001' was not found.");
        summary.Should().Contain("| Rule: RULE001");
        summary.Should().Contain("Occurred:");
    }

    [Fact]
    public void ToErrorSummary_AnalysisException_ReturnsSummaryWithProjectPath()
    {
        // Arrange
        var exception = new AnalysisException("Analysis failed");
        exception.ProjectPath = "/path/to/project.csproj";

        // Act
        var summary = exception.ToErrorSummary();

        // Assert
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("[ERR002]");
        summary.Should().Contain("| Project: /path/to/project.csproj");
    }

    [Fact]
    public void ToErrorSummary_AnalysisExceptionWithDetails_WhenIncludeDetailsTrue_IncludesFirstDetail()
    {
        // Arrange
        var exception = new AnalysisException("Analysis failed");
        exception.ProjectPath = "/path/to/project.csproj";
        exception.AddDetail("First detail");
        exception.AddDetail("Second detail");

        // Act
        var summary = exception.ToErrorSummary(includeDetails: true);

        // Assert
        summary.Should().Contain("| Details: First detail");
    }

    [Fact]
    public void ToErrorSummary_AnalysisExceptionWithDetails_WhenIncludeDetailsFalse_ExcludesDetails()
    {
        // Arrange
        var exception = new AnalysisException("Analysis failed");
        exception.ProjectPath = "/path/to/project.csproj";
        exception.AddDetail("First detail");
        exception.AddDetail("Second detail");

        // Act
        var summary = exception.ToErrorSummary(includeDetails: false);

        // Assert
        summary.Should().NotContain("Details:");
    }

    [Fact]
    public void ToErrorSummary_ConfigurationException_ReturnsSummaryWithConfigKey()
    {
        // Arrange
        var exception = new ConfigurationException("Invalid config", "MaxThreads");

        // Act
        var summary = exception.ToErrorSummary();

        // Assert
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("| Config: MaxThreads");
    }

    [Fact]
    public void ToErrorSummary_FileAccessException_ReturnsSummaryWithFilePath()
    {
        // Arrange
        var exception = new FileAccessException("/path/to/file.cs", "Cannot read file");

        // Act
        var summary = exception.ToErrorSummary();

        // Assert
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("| File: /path/to/file.cs");
    }

    [Fact]
    public void ToErrorSummary_ParseException_ReturnsSummaryWithFilePath()
    {
        // Arrange
        var exception = new ParseException("/path/to/file.cs", "Syntax error");

        // Act
        var summary = exception.ToErrorSummary();

        // Assert
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("| File: /path/to/file.cs");
    }

    [Fact]
    public void ToErrorSummary_AnalysisTimeoutException_ReturnsSummaryWithTimeout()
    {
        // Arrange
        var exception = new AnalysisTimeoutException(45);

        // Act
        var summary = exception.ToErrorSummary();

        // Assert
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("| Timeout: 45s");
    }

    [Fact]
    public void ToErrorSummary_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        RoslynGuardException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.ToErrorSummary());
    }

    #endregion

    #region IsCritical tests

    [Fact]
    public void IsCritical_AnalysisException_ReturnsTrue()
    {
        // Arrange
        var exception = new AnalysisException("Analysis failed");

        // Act
        var isCritical = exception.IsCritical();

        // Assert
        isCritical.Should().BeTrue();
    }

    [Fact]
    public void IsCritical_ConfigurationException_ReturnsTrue()
    {
        // Arrange
        var exception = new ConfigurationException("Invalid config");

        // Act
        var isCritical = exception.IsCritical();

        // Assert
        isCritical.Should().BeTrue();
    }

    [Fact]
    public void IsCritical_FileAccessException_ReturnsTrue()
    {
        // Arrange
        var exception = new FileAccessException("/path/to/file.cs", "Cannot read file");

        // Act
        var isCritical = exception.IsCritical();

        // Assert
        isCritical.Should().BeTrue();
    }

    [Fact]
    public void IsCritical_ParseException_ReturnsTrue()
    {
        // Arrange
        var exception = new ParseException("/path/to/file.cs", "Syntax error");

        // Act
        var isCritical = exception.IsCritical();

        // Assert
        isCritical.Should().BeTrue();
    }

    [Fact]
    public void IsCritical_AnalysisTimeoutException_ReturnsTrue()
    {
        // Arrange
        var exception = new AnalysisTimeoutException(30);

        // Act
        var isCritical = exception.IsCritical();

        // Assert
        isCritical.Should().BeTrue();
    }

    [Fact]
    public void IsCritical_RuleNotFoundException_ReturnsFalse()
    {
        // Arrange
        var exception = new RuleNotFoundException("RULE001");

        // Act
        var isCritical = exception.IsCritical();

        // Assert
        isCritical.Should().BeFalse();
    }

    [Fact]
    public void IsCritical_BaseRoslynGuardException_ReturnsFalse()
    {
        // Arrange
        var exception = new RuleNotFoundException("RULE001");

        // Act
        var isCritical = exception.IsCritical();

        // Assert
        isCritical.Should().BeFalse();
    }

    [Fact]
    public void IsCritical_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        RoslynGuardException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.IsCritical());
    }

    #endregion

    #region ToPropertyDictionary tests

    [Fact]
    public void ToPropertyDictionary_RuleNotFoundException_ReturnsDictionaryWithRuleId()
    {
        // Arrange
        var exception = new RuleNotFoundException("RULE001");

        // Act
        var dict = exception.ToPropertyDictionary();

        // Assert
        dict.Should().NotBeNull();
        dict.Should().ContainKey("ErrorCode").WhoseValue.Should().Be("ERR003");
        dict.Should().ContainKey("OccurredAt").WhoseValue.Should().BeOfType<DateTime>();
        dict.Should().ContainKey("Message").WhoseValue.Should().Be("Rule with ID 'RULE001' was not found.");
        dict.Should().ContainKey("ExceptionType").WhoseValue.Should().Be("RuleNotFoundException");
        dict.Should().ContainKey("IsCritical").WhoseValue.Should().Be(false);
        dict.Should().ContainKey("RuleId").WhoseValue.Should().Be("RULE001");
    }

    [Fact]
    public void ToPropertyDictionary_AnalysisException_ReturnsDictionaryWithProjectPathAndDetails()
    {
        // Arrange
        var exception = new AnalysisException("Analysis failed");
        exception.ProjectPath = "/path/to/project.csproj";
        exception.AddDetail("Detail 1");
        exception.AddDetail("Detail 2");

        // Act
        var dict = exception.ToPropertyDictionary();

        // Assert
        dict.Should().NotBeNull();
        dict.Should().ContainKey("ProjectPath").WhoseValue.Should().Be("/path/to/project.csproj");
        dict.Should().ContainKey("Details").WhoseValue.As<IEnumerable<string>>()
            .Should().BeEquivalentTo(new[] { "Detail 1", "Detail 2" });
    }

    [Fact]
    public void ToPropertyDictionary_AnalysisExceptionWithEmptyDetails_ReturnsDictionaryWithEmptyArray()
    {
        // Arrange
        var exception = new AnalysisException("Analysis failed");

        // Act
        var dict = exception.ToPropertyDictionary();

        // Assert
        dict.Should().ContainKey("Details").WhoseValue.As<IEnumerable<string>>()
            .Should().BeEmpty();
    }

    [Fact]
    public void ToPropertyDictionary_ConfigurationException_ReturnsDictionaryWithConfigKey()
    {
        // Arrange
        var exception = new ConfigurationException("Invalid config", "MaxThreads");

        // Act
        var dict = exception.ToPropertyDictionary();

        // Assert
        dict.Should().NotBeNull();
        dict.Should().ContainKey("ConfigKey").WhoseValue.Should().Be("MaxThreads");
    }

    [Fact]
    public void ToPropertyDictionary_FileAccessException_ReturnsDictionaryWithFilePath()
    {
        // Arrange
        var exception = new FileAccessException("/path/to/file.cs", "Cannot read file");

        // Act
        var dict = exception.ToPropertyDictionary();

        // Assert
        dict.Should().NotBeNull();
        dict.Should().ContainKey("FilePath").WhoseValue.Should().Be("/path/to/file.cs");
    }

    [Fact]
    public void ToPropertyDictionary_ParseException_ReturnsDictionaryWithFilePath()
    {
        // Arrange
        var exception = new ParseException("/path/to/file.cs", "Syntax error");

        // Act
        var dict = exception.ToPropertyDictionary();

        // Assert
        dict.Should().NotBeNull();
        dict.Should().ContainKey("FilePath").WhoseValue.Should().Be("/path/to/file.cs");
    }

    [Fact]
    public void ToPropertyDictionary_AnalysisTimeoutException_ReturnsDictionaryWithTimeoutSeconds()
    {
        // Arrange
        var exception = new AnalysisTimeoutException(60);

        // Act
        var dict = exception.ToPropertyDictionary();

        // Assert
        dict.Should().NotBeNull();
        dict.Should().ContainKey("TimeoutSeconds").WhoseValue.Should().Be(60);
    }

    [Fact]
    public void ToPropertyDictionary_BaseRoslynGuardException_ReturnsDictionaryWithBaseProperties()
    {
        // Arrange
        var exception = new AnalysisException("Base exception");
        exception.ErrorCode = "ERR999";

        // Act
        var dict = exception.ToPropertyDictionary();

        // Assert
        dict.Should().NotBeNull();
        dict.Should().ContainKey("ErrorCode").WhoseValue.Should().Be("ERR999");
        dict.Should().ContainKey("Message").WhoseValue.Should().Be("Base exception");
        dict.Should().ContainKey("ExceptionType").WhoseValue.Should().Be("AnalysisException");
        dict.Should().ContainKey("IsCritical").WhoseValue.Should().Be(true);
    }

    [Fact]
    public void ToPropertyDictionary_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        RoslynGuardException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.ToPropertyDictionary());
    }

    [Fact]
    public void ToPropertyDictionary_DictionaryIsCaseInsensitive()
    {
        // Arrange
        var exception = new AnalysisException("Test");

        // Act
        var dict = exception.ToPropertyDictionary();

        // Assert
        dict.Should().NotBeNull();
        dict.Should().ContainKey("errorcode"); // lowercase
        dict.Should().ContainKey("ERRORCODE"); // uppercase
        dict.Should().ContainKey("ErrorCode"); // mixed case
    }

    #endregion
}