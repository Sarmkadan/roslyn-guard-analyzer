#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Events;

/// <summary>
/// Fired when analysis starts for a project or file.
/// Allows subscribers to initialize resources or log analysis start.
/// </summary>
public sealed class AnalysisStartedEvent : Event
{
    public override string EventType => "AnalysisStarted";

    public required string ProjectPath { get; init; }
    public required string AnalysisId { get; init; }
    public string? ConfigFilePath { get; init; }

    public AnalysisStartedEvent()
    {
    }
}

/// <summary>
/// Fired when analysis completes successfully.
/// Contains final statistics and results.
/// </summary>
public sealed class AnalysisCompletedEvent : Event
{
    public override string EventType => "AnalysisCompleted";

    public required string ProjectPath { get; init; }
    public required string AnalysisId { get; init; }
    public required int ViolationCount { get; init; }
    public required int FilesAnalyzed { get; init; }
    public required long DurationMilliseconds { get; init; }

    public AnalysisCompletedEvent()
    {
    }
}

/// <summary>
/// Fired when analysis fails with an error.
/// Includes error details for logging and recovery.
/// </summary>
public sealed class AnalysisFailedEvent : Event
{
    public override string EventType => "AnalysisFailed";

    public required string ProjectPath { get; init; }
    public required string AnalysisId { get; init; }
    public required string ErrorMessage { get; init; }
    public string? ErrorStackTrace { get; init; }

    public AnalysisFailedEvent()
    {
    }
}

/// <summary>
/// Fired when a rule violation is detected.
/// Allows real-time processing of violations as they're found.
/// </summary>
public sealed class ViolationDetectedEvent : Event
{
    public override string EventType => "ViolationDetected";

    public required RuleViolation Violation { get; init; }
    public required string RuleName { get; init; }
    public required string Severity { get; init; }

    public ViolationDetectedEvent()
    {
    }
}

/// <summary>
/// Fired when a file is about to be analyzed.
/// Allows subscribers to track analysis progress file-by-file.
/// </summary>
public sealed class FileAnalysisStartedEvent : Event
{
    public override string EventType => "FileAnalysisStarted";

    public required string FilePath { get; init; }
    public required string AnalysisId { get; init; }

    public FileAnalysisStartedEvent()
    {
    }
}

/// <summary>
/// Fired when a file analysis completes.
/// Includes count of violations found in that file.
/// </summary>
public sealed class FileAnalysisCompletedEvent : Event
{
    public override string EventType => "FileAnalysisCompleted";

    public required string FilePath { get; init; }
    public required string AnalysisId { get; init; }
    public required int ViolationCount { get; init; }
    public required long DurationMilliseconds { get; init; }

    public FileAnalysisCompletedEvent()
    {
    }
}

/// <summary>
/// Fired when a rule executes successfully.
/// Useful for diagnostics and performance monitoring.
/// </summary>
public sealed class RuleExecutedEvent : Event
{
    public override string EventType => "RuleExecuted";

    public required string RuleName { get; init; }
    public required int ViolationsFound { get; init; }
    public required long ExecutionTimeMilliseconds { get; init; }

    public RuleExecutedEvent()
    {
    }
}

/// <summary>
/// Fired when caching operations occur.
/// Useful for diagnostics and cache hit/miss monitoring.
/// </summary>
public sealed class CacheOperationEvent : Event
{
    public override string EventType => "CacheOperation";

    public required string OperationType { get; init; } // Get, Set, Hit, Miss, Invalidate
    public required string CacheKey { get; init; }
    public bool Success { get; init; }

    public CacheOperationEvent()
    {
    }
}
