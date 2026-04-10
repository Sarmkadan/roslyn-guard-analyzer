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
/// Manages and executes a chain of middleware components in order.
/// Implements the pipeline/filter pattern for composable request handling.
/// </summary>
public sealed class AnalysisPipeline
{
    private readonly List<IMiddleware> _middlewares = [];
    private MiddlewareDelegate? _finalHandler;

    public IReadOnlyList<IMiddleware> Middlewares => _middlewares.AsReadOnly();

    /// <summary>
    /// Adds a middleware component to the pipeline.
    /// Middleware is executed in the order it was added.
    /// </summary>
    public AnalysisPipeline Use(IMiddleware middleware)
    {
        if (middleware is null)
            throw new ArgumentNullException(nameof(middleware));

        _middlewares.Add(middleware);
        return this;
    }

    /// <summary>
    /// Sets the final handler to be called after all middleware.
    /// </summary>
    public AnalysisPipeline UseHandler(MiddlewareDelegate handler)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        _finalHandler = handler;
        return this;
    }

    /// <summary>
    /// Executes the pipeline with the given context.
    /// Builds the middleware chain and invokes it with proper ordering.
    /// </summary>
    public async Task ExecuteAsync(PipelineContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        var handler = BuildMiddlewareChain();
        await handler(context);
    }

    /// <summary>
    /// Builds the complete middleware chain by composing all registered middleware.
    /// The final handler is called after all middleware in the chain.
    /// </summary>
    private MiddlewareDelegate BuildMiddlewareChain()
    {
        // Start with the final handler, or a no-op if none was provided
        MiddlewareDelegate next = _finalHandler ?? (ctx => Task.CompletedTask);

        // Build the chain in reverse order so the first middleware added runs first
        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var currentNext = next;

            // Capture the middleware in closure to ensure correct reference
            next = async context =>
            {
                await middleware.InvokeAsync(context, currentNext);
            };
        }

        return next;
    }

    /// <summary>
    /// Gets a string representation of the middleware chain for diagnostics.
    /// </summary>
    public string GetChainDescription()
    {
        var names = new List<string>();
        foreach (var middleware in _middlewares)
        {
            names.Add(middleware.Name);
        }

        return "[" + string.Join(" -> ", names) + (names.Count > 0 ? " -> Handler" : " -> [No middleware]") + "]";
    }
}
