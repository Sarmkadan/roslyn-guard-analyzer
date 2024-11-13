// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RoslynGuardAnalyzer.Middleware;

/// <summary>
/// Logs analysis lifecycle events (start, end, errors) with timing information.
/// Captures performance metrics for diagnostics and performance monitoring.
/// </summary>
public sealed class LoggingMiddleware : IMiddleware
{
    private readonly int _logLevel;

    public string Name => "Logging";

    public LoggingMiddleware(int logLevel = 3)
    {
        _logLevel = logLevel; // 0=silent, 1=error, 2=warn, 3=info, 4=debug
    }

    public async Task InvokeAsync(PipelineContext context, MiddlewareDelegate next)
    {
        context.StartTimeMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (_logLevel >= 3)
            Log($"[{context.AnalysisId}] Starting analysis for: {context.ProjectPath}");

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);

            stopwatch.Stop();
            context.EndTimeMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (_logLevel >= 3)
                Log($"[{context.AnalysisId}] Analysis completed successfully in {stopwatch.ElapsedMilliseconds}ms");

            if (_logLevel >= 4)
                Log($"[{context.AnalysisId}] Context items: {context.Items.Count}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            context.EndTimeMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            context.ErrorMessage = ex.Message;

            if (_logLevel >= 1)
                LogError($"[{context.AnalysisId}] Analysis failed after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");

            if (_logLevel >= 4)
                LogError($"[{context.AnalysisId}] Stack trace: {ex.StackTrace}");

            throw;
        }
    }

    private void Log(string message)
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}");
    }

    private void LogError(string message)
    {
        Console.Error.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] ERROR: {message}");
    }
}
