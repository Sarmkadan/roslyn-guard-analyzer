#nullable enable
using System;
using System.Threading.Tasks;
using Xunit;
using RoslynGuardAnalyzer.Middleware;

namespace RoslynGuardAnalyzer.Tests.Middleware;

public class LoggingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithNullContext_ThrowsArgumentNullException()
    {
        var middleware = new LoggingMiddleware();
        MiddlewareDelegate next = _ => Task.CompletedTask;

        await Assert.ThrowsAsync<ArgumentNullException>(() => middleware.InvokeAsync(null!, next));
    }

    [Fact]
    public async Task InvokeAsync_WithNullNext_ThrowsArgumentNullException()
    {
        var middleware = new LoggingMiddleware();
        var context = new PipelineContext
        {
            AnalysisId = Guid.NewGuid().ToString(),
            ProjectPath = "dummy.csproj"
        };

        await Assert.ThrowsAsync<ArgumentNullException>(() => middleware.InvokeAsync(context, null!));
    }

    [Fact]
    public async Task InvokeAsync_WithValidParameters_CompletesSuccessfully()
    {
        var middleware = new LoggingMiddleware();
        var context = new PipelineContext
        {
            AnalysisId = Guid.NewGuid().ToString(),
            ProjectPath = "dummy.csproj"
        };

        MiddlewareDelegate next = ctx =>
        {
            // Simulate some work
            ctx.Items["key"] = "value";
            return Task.CompletedTask;
        };

        await middleware.InvokeAsync(context, next);

        Assert.NotNull(context.EndTimeMilliseconds);
        Assert.True(context.EndTimeMilliseconds >= context.StartTimeMilliseconds);
        Assert.Null(context.ErrorMessage);
    }
}
