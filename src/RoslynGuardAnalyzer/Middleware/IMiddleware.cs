#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoslynGuardAnalyzer.Middleware;

/// <summary>
/// Defines the contract for middleware components in the analysis pipeline.
/// Middleware can inspect, transform, or short-circuit the analysis flow.
/// </summary>
public interface IMiddleware
{
    /// <summary>
    /// Gets the middleware name for logging and diagnostics.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the middleware logic on the given context.
    /// Calls next() to invoke the next middleware in the pipeline.
    /// </summary>
    Task InvokeAsync(PipelineContext context, MiddlewareDelegate next);
}

/// <summary>
/// Represents the next middleware in the pipeline to be invoked.
/// </summary>
public delegate Task MiddlewareDelegate(PipelineContext context);

/// <summary>
/// Contains contextual information passed through the middleware pipeline.
/// Allows middleware to share state and influence the analysis process.
/// </summary>
public sealed class PipelineContext
{
    public required string ProjectPath { get; init; }
    public required string AnalysisId { get; init; }
    public Dictionary<string, object> Items { get; } = [];
    public long StartTimeMilliseconds { get; set; }
    public long EndTimeMilliseconds { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsCancelled { get; set; }

    /// <summary>
    /// Returns the elapsed time for this analysis in milliseconds.
    /// </summary>
    public long GetElapsedMilliseconds() => EndTimeMilliseconds - StartTimeMilliseconds;

    /// <summary>
    /// Safely retrieves a value from the context items dictionary.
    /// </summary>
    public T? GetItem<T>(string key) where T : class
    {
        if (Items.TryGetValue(key, out var value))
            return value as T;
        return null;
    }

    /// <summary>
    /// Sets or updates a value in the context items dictionary.
    /// </summary>
    public void SetItem<T>(string key, T value) where T : class
    {
        Items[key] = value;
    }
}
