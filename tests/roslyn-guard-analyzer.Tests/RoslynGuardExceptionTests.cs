#nullable enable

using System;
using FluentAssertions;
using RoslynGuardAnalyzer.Exceptions;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="RoslynGuardException"/> and derived exception classes.
/// </summary>
public class RoslynGuardExceptionTests
{
    #region RoslynGuardException base class tests

    [Fact]
    public void RoslynGuardException_DefaultConstructor_SetsDefaultErrorCodeAndOccurredAt()
    {
        // Arrange & Act
        var exception = new TestRoslynGuardException("Test message");

        // Assert
        exception.ErrorCode.Should().Be("ERR000");
        exception.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        exception.Message.Should().Be("Test message");
    }

    [Fact]
    public void RoslynGuardException_CustomErrorCode_SetsErrorCode()
    {
        // Arrange & Act
        var exception = new TestRoslynGuardException("Test message", "CUSTOM001");

        // Assert
        exception.ErrorCode.Should().Be("CUSTOM001");
    }

    [Fact]
    public void RoslynGuardException_WithInnerException_SetsInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new TestRoslynGuardException("Outer message", innerException);

        // Assert
        exception.InnerException.Should().BeSameAs(innerException);
        exception.Message.Should().Be("Outer message");
    }

    [Fact]
    public void RoslynGuardException_CustomErrorCodeAndInnerException_SetsBoth()
    {
        // Arrange
        var innerException = new ArgumentException("Inner argument error");

        // Act
        var exception = new TestRoslynGuardException("Outer message", innerException, "CUSTOM002");

        // Assert
        exception.ErrorCode.Should().Be("CUSTOM002");
        exception.InnerException.Should().BeSameAs(innerException);
        exception.Message.Should().Be("Outer message");
    }

    [Fact]
    public void RoslynGuardException_ToString_ReturnsFormattedString()
    {
        // Arrange
        var exception = new TestRoslynGuardException("Test message", "TEST001");
        exception.OccurredAt = new DateTime(2024, 1, 1, 12, 0, 0);

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Be("[TEST001] Test message (occurred at 2024-01-01 12:00:00)");
    }

    [Fact]
    public void RoslynGuardException_OccurredAt_IsUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;
        var exception = new TestRoslynGuardException("Test message");
        var after = DateTime.UtcNow;

        // Assert
        exception.OccurredAt.Should().BeOnOrAfter(before);
        exception.OccurredAt.Should().BeOnOrBefore(after);
    }

    #endregion

    #region RuleNotFoundException tests

    [Fact]
    public void RuleNotFoundException_ConstructorWithRuleId_SetsRuleIdAndDefaultMessage()
    {
        // Arrange & Act
        var exception = new RuleNotFoundException("RULE001");

        // Assert
        exception.RuleId.Should().Be("RULE001");
        exception.ErrorCode.Should().Be("ERR003");
        exception.Message.Should().Be("Rule with ID 'RULE001' was not found.");
    }

    [Fact]
    public void RuleNotFoundException_ConstructorWithRuleIdAndMessage_SetsRuleIdAndCustomMessage()
    {
        // Arrange & Act
        var exception = new RuleNotFoundException("RULE002", "Custom rule not found message");

        // Assert
        exception.RuleId.Should().Be("RULE002");
        exception.Message.Should().Be("Custom rule not found message");
        exception.ErrorCode.Should().Be("ERR003");
    }

    [Fact]
    public void RuleNotFoundException_EmptyRuleId_StillCreatesException()
    {
        // Arrange & Act
        var exception = new RuleNotFoundException("");

        // Assert
        exception.RuleId.Should().BeEmpty();
        exception.Message.Should().Be("Rule with ID '' was not found.");
    }

    [Fact]
    public void RuleNotFoundException_NullRuleId_StillCreatesException()
    {
        // Arrange & Act
        var exception = new RuleNotFoundException(null!);

        // Assert
        exception.RuleId.Should().BeNull();
        exception.Message.Should().Be("Rule with ID '' was not found.");
    }

    [Fact]
    public void RuleNotFoundException_ToString_IncludesRuleId()
    {
        // Arrange
        var exception = new RuleNotFoundException("RULE003");
        exception.OccurredAt = new DateTime(2024, 2, 15, 10, 30, 0);

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Be("[ERR003] Rule with ID 'RULE003' was not found. (occurred at 2024-02-15 10:30:00)");
    }

    #endregion

    #region AnalysisException tests

    [Fact]
    public void AnalysisException_DefaultConstructor_InitializesEmptyDetailsList()
    {
        // Arrange & Act
        var exception = new AnalysisException("Analysis failed");

        // Assert
        exception.ErrorCode.Should().Be("ERR002");
        exception.ProjectPath.Should().BeNull();
        exception.Details.Should().NotBeNull();
        exception.Details.Should().BeEmpty();
        exception.Message.Should().Be("Analysis failed");
    }

    [Fact]
    public void AnalysisException_ConstructorWithInnerException_InitializesEmptyDetailsList()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new AnalysisException("Analysis failed", innerException);

        // Assert
        exception.ErrorCode.Should().Be("ERR002");
        exception.InnerException.Should().BeSameAs(innerException);
        exception.Details.Should().NotBeNull();
        exception.Details.Should().BeEmpty();
    }

    [Fact]
    public void AnalysisException_AddDetail_AddsDetailToList()
    {
        // Arrange
        var exception = new AnalysisException("Analysis failed");

        // Act
        exception.AddDetail("First detail");
        exception.AddDetail("Second detail");

        // Assert
        exception.Details.Should().HaveCount(2);
        exception.Details[0].Should().Be("First detail");
        exception.Details[1].Should().Be("Second detail");
    }

    [Fact]
    public void AnalysisException_AddDetail_WithNullDetail_DoesNotAdd()
    {
        // Arrange
        var exception = new AnalysisException("Analysis failed");

        // Act
        exception.AddDetail(null);
        exception.AddDetail("");
        exception.AddDetail("   ");

        // Assert
        exception.Details.Should().BeEmpty();
    }

    [Fact]
    public void AnalysisException_AddDetail_WithWhitespaceDetail_DoesNotAdd()
    {
        // Arrange
        var exception = new AnalysisException("Analysis failed");

        // Act
        exception.AddDetail("   ");

        // Assert
        exception.Details.Should().BeEmpty();
    }

    [Fact]
    public void AnalysisException_SetProjectPath_SetsProjectPath()
    {
        // Arrange
        var exception = new AnalysisException("Analysis failed");

        // Act
        exception.ProjectPath = "/path/to/project.csproj";

        // Assert
        exception.ProjectPath.Should().Be("/path/to/project.csproj");
    }

    [Fact]
    public void AnalysisException_ToString_IncludesProjectPathAndDetailsCount()
    {
        // Arrange
        var exception = new AnalysisException("Analysis failed");
        exception.ProjectPath = "/path/to/project.csproj";
        exception.AddDetail("Detail 1");
        exception.AddDetail("Detail 2");
        exception.OccurredAt = new DateTime(2024, 3, 20, 14, 45, 30);

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("[ERR002] Analysis failed (occurred at 2024-03-20 14:45:30)");
    }

    [Fact]
    public void AnalysisException_DetailsList_IsMutable()
    {
        // Arrange
        var exception = new AnalysisException("Analysis failed");
        var details = exception.Details;

        // Act
        details.Add("Added after getting list");

        // Assert
        exception.Details.Should().HaveCount(1);
        exception.Details[0].Should().Be("Added after getting list");
    }

    #endregion

    #region ConfigurationException tests

    [Fact]
    public void ConfigurationException_ConstructorWithMessage_SetsMessageAndDefaultErrorCode()
    {
        // Arrange & Act
        var exception = new ConfigurationException("Invalid configuration");

        // Assert
        exception.ErrorCode.Should().Be("ERR001");
        exception.Message.Should().Be("Invalid configuration");
        exception.ConfigKey.Should().BeNull();
    }

    [Fact]
    public void ConfigurationException_ConstructorWithMessageAndConfigKey_SetsBoth()
    {
        // Arrange & Act
        var exception = new ConfigurationException("Invalid MaxThreads config", configKey: "MaxThreads");

        // Assert
        exception.ErrorCode.Should().Be("ERR001");
        exception.Message.Should().Be("Invalid MaxThreads config");
        exception.ConfigKey.Should().Be("MaxThreads");
    }

    [Fact]
    public void ConfigurationException_ConstructorWithMessageAndInnerException_SetsMessageAndInnerException()
    {
        // Arrange
        var innerException = new ArgumentException("Invalid value");

        // Act
        var exception = new ConfigurationException("Invalid config", innerException);

        // Assert
        exception.ErrorCode.Should().Be("ERR001");
        exception.Message.Should().Be("Invalid config");
        exception.InnerException.Should().BeSameAs(innerException);
        exception.ConfigKey.Should().BeNull();
    }

    [Fact]
    public void ConfigurationException_EmptyConfigKey_StillCreatesException()
    {
        // Arrange & Act
        var exception = new ConfigurationException("Invalid config", configKey: "");

        // Assert
        exception.ConfigKey.Should().BeEmpty();
    }

    [Fact]
    public void ConfigurationException_NullConfigKey_StillCreatesException()
    {
        // Arrange & Act
        var exception = new ConfigurationException("Invalid config", configKey: null!);

        // Assert
        exception.ConfigKey.Should().BeNull();
    }

    [Fact]
    public void ConfigurationException_ToString_IncludesConfigKey()
    {
        // Arrange
        var exception = new ConfigurationException("Invalid MaxThreads", "MaxThreads");
        exception.OccurredAt = new DateTime(2024, 4, 10, 9, 15, 0);

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Be("[ERR001] Invalid MaxThreads (occurred at 2024-04-10 09:15:00)");
    }

    #endregion

    #region FileAccessException tests

    [Fact]
    public void FileAccessException_Constructor_SetsFilePathAndMessage()
    {
        // Arrange & Act
        var exception = new FileAccessException("/path/to/file.cs", "Cannot read file");

        // Assert
        exception.ErrorCode.Should().Be("ERR004");
        exception.FilePath.Should().Be("/path/to/file.cs");
        exception.Message.Should().Be("Cannot read file");
    }

    [Fact]
    public void FileAccessException_ConstructorWithInnerException_SetsFilePathMessageAndInnerException()
    {
        // Arrange
        var innerException = new System.IO.IOException("IO error");

        // Act
        var exception = new FileAccessException("/path/to/file.cs", "Cannot read file", innerException);

        // Assert
        exception.ErrorCode.Should().Be("ERR004");
        exception.FilePath.Should().Be("/path/to/file.cs");
        exception.Message.Should().Be("Cannot read file");
        exception.InnerException.Should().BeSameAs(innerException);
    }

    [Fact]
    public void FileAccessException_EmptyFilePath_StillCreatesException()
    {
        // Arrange & Act
        var exception = new FileAccessException("", "Cannot read file");

        // Assert
        exception.FilePath.Should().BeEmpty();
    }

    [Fact]
    public void FileAccessException_NullFilePath_StillCreatesException()
    {
        // Arrange & Act
        var exception = new FileAccessException(null!, "Cannot read file");

        // Assert
        exception.FilePath.Should().BeNull();
    }

    [Fact]
    public void FileAccessException_ToString_IncludesFilePath()
    {
        // Arrange
        var exception = new FileAccessException("/path/to/file.cs", "Cannot read file");
        exception.OccurredAt = new DateTime(2024, 5, 5, 16, 30, 0);

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Be("[ERR004] Cannot read file (occurred at 2024-05-05 16:30:00)");
    }

    #endregion

    #region ParseException tests

    [Fact]
    public void ParseException_Constructor_SetsFilePathAndMessage()
    {
        // Arrange & Act
        var exception = new ParseException("/path/to/file.cs", "Syntax error");

        // Assert
        exception.ErrorCode.Should().Be("ERR005");
        exception.FilePath.Should().Be("/path/to/file.cs");
        exception.Message.Should().Be("Syntax error");
    }

    [Fact]
    public void ParseException_ConstructorWithInnerException_SetsFilePathMessageAndInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Parse error");

        // Act
        var exception = new ParseException("/path/to/file.cs", "Syntax error", innerException);

        // Assert
        exception.ErrorCode.Should().Be("ERR005");
        exception.FilePath.Should().Be("/path/to/file.cs");
        exception.Message.Should().Be("Syntax error");
        exception.InnerException.Should().BeSameAs(innerException);
    }

    [Fact]
    public void ParseException_EmptyFilePath_StillCreatesException()
    {
        // Arrange & Act
        var exception = new ParseException("", "Syntax error");

        // Assert
        exception.FilePath.Should().BeEmpty();
    }

    [Fact]
    public void ParseException_NullFilePath_StillCreatesException()
    {
        // Arrange & Act
        var exception = new ParseException(null!, "Syntax error");

        // Assert
        exception.FilePath.Should().BeNull();
    }

    [Fact]
    public void ParseException_ToString_IncludesFilePath()
    {
        // Arrange
        var exception = new ParseException("/path/to/file.cs", "Syntax error");
        exception.OccurredAt = new DateTime(2024, 6, 15, 11, 20, 0);

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Be("[ERR005] Syntax error (occurred at 2024-06-15 11:20:00)");
    }

    #endregion

    #region AnalysisTimeoutException tests

    [Fact]
    public void AnalysisTimeoutException_ConstructorWithTimeoutSeconds_SetsTimeoutAndDefaultMessage()
    {
        // Arrange & Act
        var exception = new AnalysisTimeoutException(30);

        // Assert
        exception.ErrorCode.Should().Be("ERR006");
        exception.TimeoutSeconds.Should().Be(30);
        exception.Message.Should().Be("Analysis timed out after 30 seconds.");
    }

    [Fact]
    public void AnalysisTimeoutException_ConstructorWithTimeoutSecondsAndMessage_SetsTimeoutAndCustomMessage()
    {
        // Arrange & Act
        var exception = new AnalysisTimeoutException(45, "Custom timeout message");

        // Assert
        exception.ErrorCode.Should().Be("ERR006");
        exception.TimeoutSeconds.Should().Be(45);
        exception.Message.Should().Be("Custom timeout message");
    }

    [Fact]
    public void AnalysisTimeoutException_ZeroTimeout_StillCreatesException()
    {
        // Arrange & Act
        var exception = new AnalysisTimeoutException(0);

        // Assert
        exception.TimeoutSeconds.Should().Be(0);
        exception.Message.Should().Be("Analysis timed out after 0 seconds.");
    }

    [Fact]
    public void AnalysisTimeoutException_NegativeTimeout_StillCreatesException()
    {
        // Arrange & Act
        var exception = new AnalysisTimeoutException(-1);

        // Assert
        exception.TimeoutSeconds.Should().Be(-1);
        exception.Message.Should().Be("Analysis timed out after -1 seconds.");
    }

    [Fact]
    public void AnalysisTimeoutException_LargeTimeout_StillCreatesException()
    {
        // Arrange & Act
        var exception = new AnalysisTimeoutException(9999);

        // Assert
        exception.TimeoutSeconds.Should().Be(9999);
    }

    [Fact]
    public void AnalysisTimeoutException_ToString_IncludesTimeout()
    {
        // Arrange
        var exception = new AnalysisTimeoutException(60);
        exception.OccurredAt = new DateTime(2024, 7, 22, 18, 0, 0);

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Be("[ERR006] Analysis timed out after 60 seconds. (occurred at 2024-07-22 18:00:00)");
    }

    #endregion

    #region ErrorCode constants tests

    [Fact]
    public void AllExceptionTypes_UseCorrectErrorCodes()
    {
        // Arrange & Act
        var baseException = new TestRoslynGuardException("Test");
        var ruleNotFound = new RuleNotFoundException("RULE001");
        var analysis = new AnalysisException("Analysis failed");
        var config = new ConfigurationException("Config error");
        var fileAccess = new FileAccessException("/path/file", "Error");
        var parse = new ParseException("/path/file", "Error");
        var timeout = new AnalysisTimeoutException(30);

        // Assert
        baseException.ErrorCode.Should().Be("ERR000");
        ruleNotFound.ErrorCode.Should().Be("ERR003");
        analysis.ErrorCode.Should().Be("ERR002");
        config.ErrorCode.Should().Be("ERR001");
        fileAccess.ErrorCode.Should().Be("ERR004");
        parse.ErrorCode.Should().Be("ERR005");
        timeout.ErrorCode.Should().Be("ERR006");
    }

    #endregion

    #region Helper class for testing base RoslynGuardException

    private sealed class TestRoslynGuardException : RoslynGuardException
    {
        public TestRoslynGuardException(string message, string errorCode = "ERR000")
            : base(message, errorCode)
        {
        }

        public TestRoslynGuardException(string message, Exception innerException, string errorCode = "ERR000")
            : base(message, innerException, errorCode)
        {
        }
    }

    #endregion
}