#nullable enable

using System;
using System.Threading.Tasks;
using FluentAssertions;
using RoslynGuardAnalyzer.Middleware;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="ErrorHandlingMiddleware"/>.
/// </summary>
public sealed class ErrorHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware();
        MiddlewareDelegate next = _ => Task.CompletedTask;

        // Act
        var act = () => middleware.InvokeAsync(null!, next);

        // Assert
        var exception = await act.Should().ThrowAsync<ArgumentNullException>();
        exception.Which.ParamName.Should().Be("context");
    }

    [Fact]
    public async Task InvokeAsync_WithNullNext_ThrowsArgumentNullException()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware();
        var context = new PipelineContext { ProjectPath = "test.csproj", AnalysisId = "test-id" };

        // Act
        var act = () => middleware.InvokeAsync(context, null!);

        // Assert
        var exception = await act.Should().ThrowAsync<ArgumentNullException>();
        exception.Which.ParamName.Should().Be("next");
    }
}
