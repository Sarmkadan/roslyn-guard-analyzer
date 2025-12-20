// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;
using RoslynGuardAnalyzer.Exceptions;

namespace RoslynGuardAnalyzer.Middleware;

/// <summary>
/// Handles exceptions during analysis and decides whether to fail fast or continue.
/// Converts specific exception types to meaningful error messages.
/// </summary>
public sealed class ErrorHandlingMiddleware : IMiddleware
{
    private readonly bool _failOnError;

    public string Name => "ErrorHandling";

    public ErrorHandlingMiddleware(bool failOnError = true)
    {
        _failOnError = failOnError;
    }

    public async Task InvokeAsync(PipelineContext context, MiddlewareDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (RoslynGuardException ex) when (!_failOnError)
        {
            // Log the error but don't throw, allowing analysis to continue
            context.ErrorMessage = $"Handled error: {ex.Message}";
            Console.Error.WriteLine($"Warning: {ex.Message}");
        }
        catch (FileAccessException ex) when (!_failOnError)
        {
            context.ErrorMessage = $"File access error: {ex.FilePath} - {ex.Message}";
            Console.Error.WriteLine($"Warning: Could not access {ex.FilePath}");
        }
        catch (ConfigurationException ex) when (!_failOnError)
        {
            context.ErrorMessage = $"Configuration error: {ex.Message}";
            Console.Error.WriteLine($"Warning: {ex.Message}");
        }
        catch (TimeoutException ex) when (!_failOnError)
        {
            context.ErrorMessage = $"Analysis timeout: {ex.Message}";
            Console.Error.WriteLine($"Warning: Analysis exceeded timeout");
        }
        catch (OperationCanceledException ex) when (!_failOnError)
        {
            context.IsCancelled = true;
            context.ErrorMessage = "Analysis was cancelled";
        }
    }
}
