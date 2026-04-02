// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace RoslynGuardAnalyzer.Exceptions;

/// <summary>
/// Base exception for all Roslyn Guard Analyzer errors.
/// </summary>
public abstract class RoslynGuardException : Exception
{
    public string ErrorCode { get; set; }
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
    public string RuleId { get; set; }

    public RuleNotFoundException(string ruleId)
        : base($"Rule with ID '{ruleId}' was not found.", ErrorCodes.RuleNotFound)
    {
        RuleId = ruleId;
    }

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
    public string? ProjectPath { get; set; }
    public List<string> Details { get; set; }

    public AnalysisException(string message)
        : base(message, ErrorCodes.AnalysisFailed)
    {
        Details = new List<string>();
    }

    public AnalysisException(string message, Exception innerException)
        : base(message, innerException, ErrorCodes.AnalysisFailed)
    {
        Details = new List<string>();
    }

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
    public string? ConfigKey { get; set; }

    public ConfigurationException(string message)
        : base(message, ErrorCodes.InvalidConfiguration)
    {
    }

    public ConfigurationException(string message, string configKey)
        : base(message, ErrorCodes.InvalidConfiguration)
    {
        ConfigKey = configKey;
    }

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
    public string FilePath { get; set; }

    public FileAccessException(string filePath, string message)
        : base(message, ErrorCodes.IoException)
    {
        FilePath = filePath;
    }

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
    public string FilePath { get; set; }

    public ParseException(string filePath, string message)
        : base(message, ErrorCodes.ParseException)
    {
        FilePath = filePath;
    }

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
    public int TimeoutSeconds { get; set; }

    public AnalysisTimeoutException(int timeoutSeconds)
        : base($"Analysis timed out after {timeoutSeconds} seconds.", ErrorCodes.TimeoutException)
    {
        TimeoutSeconds = timeoutSeconds;
    }

    public AnalysisTimeoutException(int timeoutSeconds, string message)
        : base(message, ErrorCodes.TimeoutException)
    {
        TimeoutSeconds = timeoutSeconds;
    }
}
