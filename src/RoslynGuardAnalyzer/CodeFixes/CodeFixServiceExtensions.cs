// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

using Microsoft.Extensions.DependencyInjection;

namespace RoslynGuardAnalyzer.CodeFixes;

/// <summary>
/// Extension methods for registering code-fix services with the dependency-injection container.
/// </summary>
public static class CodeFixServiceExtensions
{
    /// <summary>
    /// Registers <see cref="ICodeFixService"/> and its default implementation
    /// <see cref="CodeFixService"/> as singletons in the service collection.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance to allow method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddCodeFixServices(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddSingleton<ICodeFixService, CodeFixService>();

        return services;
    }
}
