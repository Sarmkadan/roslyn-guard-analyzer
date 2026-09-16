#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Events
{
    /// <summary>
    /// Fired when analysis starts for a project or file.
    /// Allows subscribers to initialize resources or log analysis start.
    /// </summary>
    public sealed class AnalysisStartedEvent : Event
    {
        public override string EventType => "AnalysisStarted";

        /// <summary>
        /// Gets or sets the project path.
        /// </summary>
        public required string ProjectPath { get; init; }

        /// <summary>
        /// Gets or sets the analysis identifier.
        /// </summary>
        public required string AnalysisId { get; init; }

        /// <summary>
        /// Gets or sets the configuration file path.
        /// </summary>
        public string? ConfigFilePath { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisStartedEvent"/> class.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the project path.
        /// </summary>
        public required string ProjectPath { get; init; }

        /// <summary>
        /// Gets or sets the analysis identifier.
        /// </summary>
        public required string AnalysisId { get; init; }

        /// <summary>
        /// Gets or sets the number of violations found.
        /// </summary>
        public required int ViolationCount { get; init; }

        /// <summary>
        /// Gets or sets the number of files analyzed.
        /// </summary>
        public required int FilesAnalyzed { get; init; }

        /// <summary>
        /// Gets or sets the duration of the analysis in milliseconds.
        /// </summary>
        public required long DurationMilliseconds { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisCompletedEvent"/> class.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the project path.
        /// </summary>
        public required string ProjectPath { get; init; }

        /// <summary>
        /// Gets or sets the analysis identifier.
        /// </summary>
        public required string AnalysisId { get; init; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public required string ErrorMessage { get; init; }

        /// <summary>
        /// Gets or sets the error stack trace.
        /// </summary>
        public string? ErrorStackTrace { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisFailedEvent"/> class.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the rule violation.
        /// </summary>
        public required RuleViolation Violation { get; init; }

        /// <summary>
        /// Gets or sets the name of the rule.
        /// </summary>
        public required string RuleName { get; init; }

        /// <summary>
        /// Gets or sets the severity of the violation.
        /// </summary>
        public required string Severity { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViolationDetectedEvent"/> class.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        public required string FilePath { get; init; }

        /// <summary>
        /// Gets or sets the analysis identifier.
        /// </summary>
        public required string AnalysisId { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAnalysisStartedEvent"/> class.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        public required string FilePath { get; init; }

        /// <summary>
        /// Gets or sets the analysis identifier.
        /// </summary>
        public required string AnalysisId { get; init; }

        /// <summary>
        /// Gets or sets the number of violations found in the file.
        /// </summary>
        public required int ViolationCount { get; init; }

        /// <summary>
        /// Gets or sets the duration of the file analysis in milliseconds.
        /// </summary>
        public required long DurationMilliseconds { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAnalysisCompletedEvent"/> class.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the name of the rule.
        /// </summary>
        public required string RuleName { get; init; }

        /// <summary>
        /// Gets or sets the number of violations found by the rule.
        /// </summary>
        public required int ViolationsFound { get; init; }

        /// <summary>
        /// Gets or sets the execution time of the rule in milliseconds.
        /// </summary>
        public required long ExecutionTimeMilliseconds { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleExecutedEvent"/> class.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the type of cache operation (Get, Set, Hit, Miss, Invalidate).
        /// </summary>
        public required string OperationType { get; init; } // Get, Set, Hit, Miss, Invalidate

        /// <summary>
        /// Gets or sets the cache key.
        /// </summary>
        public required string CacheKey { get; init; }

        /// <summary>
        /// Gets or sets whether the cache operation was successful.
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheOperationEvent"/> class.
        /// </summary>
        public CacheOperationEvent()
        {
        }
    }
}