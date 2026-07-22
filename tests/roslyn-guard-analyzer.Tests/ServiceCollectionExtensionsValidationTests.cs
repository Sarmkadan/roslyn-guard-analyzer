#nullable enable

using System;
using System.IO;
using FluentAssertions;
using RoslynGuardAnalyzer.Infrastructure;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="ServiceCollectionExtensionsValidation"/>.
/// </summary>
public class ServiceCollectionExtensionsValidationTests
{
    #region Validate (AnalyzerConfiguration extension method)

    [Fact]
    public void Validate_WithValidConfiguration_ReturnsEmptyList()
    {
        // Arrange
        var config = new AnalyzerConfiguration
        {
            DataDirectory = "/valid/absolute/path",
            MaxViolationsToReport = 100,
            AnalysisTimeoutSeconds = 600,
            LogLevel = 2,
            MaxParallelThreads = 4,
            DefaultReportFormat = "sarif"
        };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        AnalyzerConfiguration? config = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => config!.Validate());
    }

    [Fact]
    public void Validate_WithNullDataDirectory_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { DataDirectory = null! };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("DataDirectory cannot be null or whitespace.");
    }

    [Fact]
    public void Validate_WithEmptyDataDirectory_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { DataDirectory = "" };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("DataDirectory cannot be null or whitespace.");
    }

    [Fact]
    public void Validate_WithWhitespaceDataDirectory_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { DataDirectory = "   " };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("DataDirectory cannot be null or whitespace.");
    }

    [Fact]
    public void Validate_WithRelativeDataDirectory_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { DataDirectory = "relative/path" };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("DataDirectory 'relative/path' must be an absolute path.");
    }

    [Fact]
    public void Validate_WithPathContainingParentDirectory_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { DataDirectory = "/path/with/../invalid" };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("DataDirectory '/path/with/../invalid' contains invalid path segments.");
    }

    [Fact]
    public void Validate_WithZeroMaxViolationsToReport_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { MaxViolationsToReport = 0 };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("MaxViolationsToReport must be greater than 0, but was 0.");
    }

    [Fact]
    public void Validate_WithNegativeMaxViolationsToReport_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { MaxViolationsToReport = -1 };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("MaxViolationsToReport must be greater than 0, but was -1.");
    }

    [Fact]
    public void Validate_WithZeroAnalysisTimeoutSeconds_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { AnalysisTimeoutSeconds = 0 };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("AnalysisTimeoutSeconds must be greater than 0, but was 0.");
    }

    [Fact]
    public void Validate_WithNegativeAnalysisTimeoutSeconds_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { AnalysisTimeoutSeconds = -1 };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("AnalysisTimeoutSeconds must be greater than 0, but was -1.");
    }

    [Fact]
    public void Validate_WithAnalysisTimeoutExceeding24Hours_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { AnalysisTimeoutSeconds = 86401 };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("AnalysisTimeoutSeconds cannot exceed 86400 seconds (24 hours), but was 86401.");
    }

    [Fact]
    public void Validate_WithLogLevelBelowMinimum_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { LogLevel = -1 };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("LogLevel must be between 0 and 4 inclusive, but was -1.");
    }

    [Fact]
    public void Validate_WithLogLevelAboveMaximum_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { LogLevel = 5 };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("LogLevel must be between 0 and 4 inclusive, but was 5.");
    }

    [Fact]
    public void Validate_WithZeroMaxParallelThreads_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { MaxParallelThreads = 0 };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("MaxParallelThreads must be greater than 0, but was 0.");
    }

    [Fact]
    public void Validate_WithNegativeMaxParallelThreads_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { MaxParallelThreads = -1 };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("MaxParallelThreads must be greater than 0, but was -1.");
    }

    [Fact]
    public void Validate_WithMaxParallelThreadsExceedingLimit_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { MaxParallelThreads = 1025 };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("MaxParallelThreads cannot exceed 1024, but was 1025.");
    }

    [Fact]
    public void Validate_WithNullDefaultReportFormat_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { DefaultReportFormat = null! };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("DefaultReportFormat cannot be null or whitespace.");
    }

    [Fact]
    public void Validate_WithEmptyDefaultReportFormat_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { DefaultReportFormat = "" };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("DefaultReportFormat cannot be null or whitespace.");
    }

    [Fact]
    public void Validate_WithDefaultReportFormatExceedingLength_ReturnsError()
    {
        // Arrange
        var config = new AnalyzerConfiguration { DefaultReportFormat = new string('a', 65) };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("DefaultReportFormat cannot exceed 64 characters, but was 65.");
    }

    #endregion

    #region IsValid (AnalyzerConfiguration extension method)

    [Fact]
    public void IsValid_WithValidConfiguration_ReturnsTrue()
    {
        // Arrange
        var config = new AnalyzerConfiguration
        {
            DataDirectory = "/valid/path",
            MaxViolationsToReport = 100,
            AnalysisTimeoutSeconds = 600,
            LogLevel = 2,
            MaxParallelThreads = 4,
            DefaultReportFormat = "sarif"
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithInvalidConfiguration_ReturnsFalse()
    {
        // Arrange
        var config = new AnalyzerConfiguration
        {
            DataDirectory = "",
            MaxViolationsToReport = 0,
            AnalysisTimeoutSeconds = 0,
            LogLevel = 5,
            MaxParallelThreads = 0,
            DefaultReportFormat = ""
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        AnalyzerConfiguration? config = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensionsValidation.IsValid(config!));
    }

    #endregion

    #region EnsureValid (AnalyzerConfiguration extension method)

    [Fact]
    public void EnsureValid_WithValidConfiguration_DoesNotThrow()
    {
        // Arrange
        var config = new AnalyzerConfiguration
        {
            DataDirectory = "/valid/path",
            MaxViolationsToReport = 100,
            AnalysisTimeoutSeconds = 600,
            LogLevel = 2,
            MaxParallelThreads = 4,
            DefaultReportFormat = "sarif"
        };

        // Act
        Action act = () => config.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_WithInvalidConfiguration_ThrowsArgumentException()
    {
        // Arrange
        var config = new AnalyzerConfiguration
        {
            DataDirectory = "",
            MaxViolationsToReport = 0,
            AnalysisTimeoutSeconds = 0,
            LogLevel = 5,
            MaxParallelThreads = 0,
            DefaultReportFormat = ""
        };

        // Act
        Action act = () => config.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Analyzer configuration is invalid*");
    }

    [Fact]
    public void EnsureValid_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        AnalyzerConfiguration? config = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => config!.EnsureValid());
    }

    #endregion

    #region ValidateDataDirectory

    [Fact]
    public void ValidateDataDirectory_WithValidAbsolutePath_ReturnsEmptyList()
    {
        // Arrange
        var path = "/valid/absolute/path";

        // Act
        var result = ServiceCollectionExtensionsValidation.ValidateDataDirectory(path);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateDataDirectory_WithNullPath_ReturnsError()
    {
        // Arrange
        string? path = null;

        // Act
        var result = ServiceCollectionExtensionsValidation.ValidateDataDirectory(path!);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("DataDirectory cannot be null or whitespace.");
    }

    [Fact]
    public void ValidateDataDirectory_WithEmptyPath_ReturnsError()
    {
        // Arrange
        var path = "";

        // Act
        var result = ServiceCollectionExtensionsValidation.ValidateDataDirectory(path);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("DataDirectory cannot be null or whitespace.");
    }

    [Fact]
    public void ValidateDataDirectory_WithWhitespacePath_ReturnsError()
    {
        // Arrange
        var path = "   ";

        // Act
        var result = ServiceCollectionExtensionsValidation.ValidateDataDirectory(path);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("DataDirectory cannot be null or whitespace.");
    }

    [Fact]
    public void ValidateDataDirectory_WithRelativePath_ReturnsError()
    {
        // Arrange
        var path = "relative/path";

        // Act
        var result = ServiceCollectionExtensionsValidation.ValidateDataDirectory(path);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("DataDirectory 'relative/path' must be an absolute path.");
    }

    [Fact]
    public void ValidateDataDirectory_WithPathContainingParentDirectory_ReturnsError()
    {
        // Arrange
        var path = "/path/with/../invalid";

        // Act
        var result = ServiceCollectionExtensionsValidation.ValidateDataDirectory(path);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("DataDirectory '/path/with/../invalid' contains invalid path segments.");
    }


    #endregion

    #region ValidatePositiveInt

    [Fact]
    public void ValidatePositiveInt_WithPositiveValueAndNoMax_ReturnsEmptyList()
    {
        // Arrange
        var value = 42;
        var paramName = "testParam";

        // Act
        var result = ServiceCollectionExtensionsValidation.ValidatePositiveInt(value, paramName);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidatePositiveInt_WithPositiveValueAndWithinMax_ReturnsEmptyList()
    {
        // Arrange
        var value = 50;
        var paramName = "testParam";

        // Act
        var result = ServiceCollectionExtensionsValidation.ValidatePositiveInt(value, paramName, maxValue: 100);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidatePositiveInt_WithZeroValue_ReturnsError()
    {
        // Arrange
        var value = 0;
        var paramName = "testParam";

        // Act
        var result = ServiceCollectionExtensionsValidation.ValidatePositiveInt(value, paramName);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("testParam must be greater than 0, but was 0.");
    }

    [Fact]
    public void ValidatePositiveInt_WithNegativeValue_ReturnsError()
    {
        // Arrange
        var value = -5;
        var paramName = "testParam";

        // Act
        var result = ServiceCollectionExtensionsValidation.ValidatePositiveInt(value, paramName);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("testParam must be greater than 0, but was -5.");
    }

    [Fact]
    public void ValidatePositiveInt_WithValueExceedingMax_ReturnsError()
    {
        // Arrange
        var value = 150;
        var paramName = "testParam";

        // Act
        var result = ServiceCollectionExtensionsValidation.ValidatePositiveInt(value, paramName, maxValue: 100);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("testParam cannot exceed 100, but was 150.");
    }

    [Fact]
    public void ValidatePositiveInt_WithNullParamName_ThrowsArgumentNullException()
    {
        // Arrange
        var value = 42;
        string? paramName = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensionsValidation.ValidatePositiveInt(value, paramName!));
    }

    [Fact]
    public void ValidatePositiveInt_WithEmptyParamName_ThrowsArgumentException()
    {
        // Arrange
        var value = 42;
        var paramName = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ServiceCollectionExtensionsValidation.ValidatePositiveInt(value, paramName));
    }

    #endregion

    #region ValidateLogLevel

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void ValidateLogLevel_WithValidLogLevel_ReturnsEmptyList(int logLevel)
    {
        // Act
        var result = ServiceCollectionExtensionsValidation.ValidateLogLevel(logLevel);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(5)]
    [InlineData(100)]
    public void ValidateLogLevel_WithInvalidLogLevel_ReturnsError(int logLevel)
    {
        // Act
        var result = ServiceCollectionExtensionsValidation.ValidateLogLevel(logLevel);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be($"LogLevel must be between 0 and 4 inclusive, but was {logLevel}.");
    }

    #endregion

    #region ValidateReportFormat

    [Fact]
    public void ValidateReportFormat_WithValidFormat_ReturnsEmptyList()
    {
        // Arrange
        var format = "sarif";

        // Act
        var result = ServiceCollectionExtensionsValidation.ValidateReportFormat(format);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateReportFormat_WithNullFormat_ReturnsError()
    {
        // Arrange
        string? format = null;

        // Act
        var result = ServiceCollectionExtensionsValidation.ValidateReportFormat(format!);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Report format cannot be null or whitespace.");
    }

    [Fact]
    public void ValidateReportFormat_WithEmptyFormat_ReturnsError()
    {
        // Arrange
        var format = "";

        // Act
        var result = ServiceCollectionExtensionsValidation.ValidateReportFormat(format);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Report format cannot be null or whitespace.");
    }

    [Fact]
    public void ValidateReportFormat_WithWhitespaceFormat_ReturnsError()
    {
        // Arrange
        var format = "   ";

        // Act
        var result = ServiceCollectionExtensionsValidation.ValidateReportFormat(format);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Report format cannot be null or whitespace.");
    }

    [Fact]
    public void ValidateReportFormat_WithFormatExceedingLength_ReturnsError()
    {
        // Arrange
        var format = new string('a', 65);

        // Act
        var result = ServiceCollectionExtensionsValidation.ValidateReportFormat(format);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Report format cannot exceed 64 characters, but was 65.");
    }

    [Theory]
    [InlineData("sarif_2024")]
    [InlineData("custom-report")]
    [InlineData("Text.Report")]
    [InlineData("report_v1.0")]
    public void ValidateReportFormat_WithValidSpecialCharacters_ReturnsEmptyList(string format)
    {
        // Act
        var result = ServiceCollectionExtensionsValidation.ValidateReportFormat(format);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("sarif@2024")]
    [InlineData("custom|report")]
    [InlineData("report with spaces")]
    [InlineData("report#invalid")]
    public void ValidateReportFormat_WithInvalidCharacters_ReturnsError(string format)
    {
        // Act
        var result = ServiceCollectionExtensionsValidation.ValidateReportFormat(format);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("contains invalid characters");
    }

    #endregion
}