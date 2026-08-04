using System;
using System.Threading.Tasks;
using Xunit;
using RoslynGuardAnalyzer.Middleware;

namespace RoslynGuardAnalyzer.Tests;

public class AnalysisPipelineTests
{
    private class TestMiddleware : IMiddleware
    {
        public string Name { get; }
        public bool Invoked { get; private set; }

        public TestMiddleware(string name)
        {
            Name = name;
        }

        public Task InvokeAsync(PipelineContext context, MiddlewareDelegate next)
        {
            Invoked = true;
            return next(context);
        }
    }

    [Fact]
    public void Use_ShouldAddMiddleware_ToPipeline()
    {
        // Arrange
        var pipeline = new AnalysisPipeline();
        var middleware = new TestMiddleware("TestMiddleware");

        // Act
        pipeline.Use(middleware);

        // Assert
        Assert.Single(pipeline.Middlewares);
        Assert.Same(middleware, pipeline.Middlewares[0]);
    }

    [Fact]
    public void Use_ShouldThrowArgumentNullException_WhenMiddlewareIsNull()
    {
        // Arrange
        var pipeline = new AnalysisPipeline();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => pipeline.Use(null!));
    }

    [Fact]
    public void UseHandler_ShouldThrowArgumentNullException_WhenHandlerIsNull()
    {
        // Arrange
        var pipeline = new AnalysisPipeline();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => pipeline.UseHandler(null!));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldInvokeMiddlewareChain_AndHandler()
    {
        // Arrange
        var pipeline = new AnalysisPipeline();
        var m1 = new TestMiddleware("M1");
        var m2 = new TestMiddleware("M2");
        bool handlerCalled = false;

        pipeline.Use(m1);
        pipeline.Use(m2);
        pipeline.UseHandler(ctx => 
        { 
            handlerCalled = true; 
            return Task.CompletedTask; 
        });

        // Initialize required properties to satisfy CS9035
        var context = new PipelineContext { ProjectPath = "test.csproj", AnalysisId = "test-id" };

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        Assert.True(m1.Invoked);
        Assert.True(m2.Invoked);
        Assert.True(handlerCalled);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange
        var pipeline = new AnalysisPipeline();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => pipeline.ExecuteAsync(null!));
    }

    [Fact]
    public void GetChainDescription_ShouldReturnFormattedString()
    {
        // Arrange
        var pipeline = new AnalysisPipeline();
        pipeline.Use(new TestMiddleware("First"));
        pipeline.Use(new TestMiddleware("Second"));

        // Act
        var description = pipeline.GetChainDescription();

        // Assert
        Assert.Contains("First -> Second -> Handler", description);
    }
}
