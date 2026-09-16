#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

using RoslynGuardAnalyzer.Core;

namespace RoslynGuardAnalyzer.Exceptions;

/// <summary>
/// Base exception for all Roslyn Guard Analyzer errors.
/// </summary>
public abstract class RoslynGuardException : Exception
{
    /// <summary>
    /// Gets or sets the error code associated with the exception.
    /// </summary>
    public string ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the exception occurred.
    /// </summary>
    public DateTime OccurredAt { get; set; }

    protected RoslynGuardException(string message, string errorCode = "ERR000")
        : base(message)
    {
        ErrorCode = errorCode;
        OccurredAt = DateTime.UtcNow;
    }

    protected RoslynGuardException(string message, Exception innerException, string errorCode = "ERR000")
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        OccurredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns a string that represents the exception, including its error code and occurrence time.
    /// </summary>
    public override string ToString()
    {
        return $"[{ErrorCode}] {Message} (occurred at {OccurredAt:yyyy-MM-dd HH:mm:ss})";
    }
}

/// <summary>
/// Thrown when a requested rule is not found.
/// </summary>
public sealed class RuleNotFoundException : RoslynGuardException
{
    /// <summary>
    /// Gets or sets the identifier of the rule that was not found.
    /// </summary>
    public string RuleId { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuleNotFoundException"/> class for the specified rule.
    /// </summary>
    public RuleNotFoundException(string ruleId)
        : base($"Rule with ID '{ruleId}' was not found.", ErrorCodes.RuleNotFound)
    {
        RuleId = ruleId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuleNotFoundException"/> class with a custom message.
    /// </summary>
    public RuleNotFoundException(string ruleId, string message)
        : base(message, ErrorCodes.RuleNotFound)
    {
        RuleId = ruleId;
    }
}

/// <summary>
/// Thrown when analysis encounters a critical error.
/// </summary>
public sealed class AnalysisException : RoslynGuardException
{
    /// <summary>
    /// Gets or sets the path of the project being analyzed when the error occurred.
    /// </summary>
    public string? ProjectPath { get; set; }

    /// <summary>
    /// Gets or sets the collection of details associated with the analysis error.
    /// </summary>
    public List<string> Details { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisException"/> class with the specified message.
    /// </summary>
    public AnalysisException(string message)
        : base(message, ErrorCodes.AnalysisFailed)
    {
        Details = new List<string>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisException"/> class with an inner exception.
    /// </summary>
    public AnalysisException(string message, Exception innerException)
        : base(message, innerException, ErrorCodes.AnalysisFailed)
    {
        Details = new List<string>();
    }

    /// <summary>
    /// Adds a detail to the analysis error when the supplied value is not empty or whitespace.
    /// </summary>
    public void AddDetail(string detail)
    {
        if (!string.IsNullOrWhiteSpace(detail))
            Details.Add(detail);
    }
}

/// <summary>
/// Thrown when configuration is invalid or incomplete.
/// </summary>
public sealed class ConfigurationException : RoslynGuardException
{
    /// <summary>
    /// Gets or sets the configuration key associated with the error.
    /// </summary>
    public string? ConfigKey { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class with the specified message.
    /// </summary>
    public ConfigurationException(string message)
        : base(message, ErrorCodes.InvalidConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class for the specified configuration key.
    /// </summary>
    public ConfigurationException(string message, string configKey)
        : base(message, ErrorCodes.InvalidConfiguration)
    {
        ConfigKey = configKey;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class with an inner exception.
    /// </summary>
    public ConfigurationException(string message, Exception innerException)
        : base(message, innerException, ErrorCodes.InvalidConfiguration)
    {
    }
}

/// <summary>
/// Thrown when file I/O operations fail.
/// </summary>
public sealed class FileAccessException : RoslynGuardException
{
    /// <summary>
    /// Gets or sets the path of the file that could not be accessed.
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAccessException"/> class for the specified file.
    /// </summary>
    public FileAccessException(string filePath, string message)
        : base(message, ErrorCodes.IoException)
    {
        FilePath = filePath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAccessException"/> class with an inner exception.
    /// </summary>
    public FileAccessException(string filePath, string message, Exception innerException)
        : base(message, innerException, ErrorCodes.IoException)
    {
        FilePath = filePath;
    }
}

/// <summary>
/// Thrown when code parsing fails.
/// </summary>
public sealed class ParseException : RoslynGuardException
{
    /// <summary>
    /// Gets or sets the path of the file that could not be parsed.
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseException"/> class for the specified file.
    /// </summary>
    public ParseException(string filePath, string message)
        : base(message, ErrorCodes.ParseException)
    {
        FilePath = filePath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseException"/> class with an inner exception.
    /// </summary>
    public ParseException(string filePath, string message, Exception innerException)
        : base(message, innerException, ErrorCodes.ParseException)
    {
        FilePath = filePath;
    }
}

/// <summary>
/// Thrown when analysis times out.
/// </summary>
public sealed class AnalysisTimeoutException : RoslynGuardException
{
    /// <summary>
    /// Gets or sets the analysis timeout duration, in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisTimeoutException"/> class for the specified timeout duration.
    /// </summary>
    public AnalysisTimeoutException(int timeoutSeconds)
        : base($"Analysis timed out after {timeoutSeconds} seconds.", ErrorCodes.TimeoutException)
    {
        TimeoutSeconds = timeoutSeconds;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisTimeoutException"/> class with a custom message.
    /// </summary>
    public AnalysisTimeoutException(int timeoutSeconds, string message)
        : base(message, ErrorCodes.TimeoutException)
    {
        TimeoutSeconds = timeoutSeconds;
    }
}
