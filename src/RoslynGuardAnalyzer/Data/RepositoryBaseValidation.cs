#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace RoslynGuardAnalyzer.Data;

/// <summary>
/// Provides validation helpers for <see cref="RepositoryBase{T}"/> instances.
/// </summary>
public static class RepositoryBaseValidation
{
    /// <summary>
    /// Validates a repository instance and returns a list of human-readable problems.
    /// </summary>
    /// <typeparam name="T">The entity type managed by the repository.</typeparam>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns>A list of validation problems; empty if the repository is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate<T>(this RepositoryBase<T> value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if a repository instance is valid.
    /// </summary>
    /// <typeparam name="T">The entity type managed by the repository.</typeparam>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns>True if the repository is valid; otherwise, false.</returns>
    public static bool IsValid<T>(this RepositoryBase<T>? value) where T : class
        => value is not null;

    /// <summary>
    /// Ensures that a repository instance is valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The entity type managed by the repository.</typeparam>
    /// <param name="value">The repository instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void EnsureValid<T>(this RepositoryBase<T> value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);
    }

    /// <summary>
    /// Validates repository operations parameters.
    /// </summary>
    /// <param name="id">The entity ID to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is null.</exception>
    public static IReadOnlyList<string> ValidateId(string? id)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(id))
        {
            problems.Add("ID cannot be null, empty, or whitespace.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates repository operations parameters.
    /// </summary>
    /// <typeparam name="T">The entity type managed by the repository.</typeparam>
    /// <param name="id">The entity ID to validate.</param>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> or <paramref name="entity"/> is null.</exception>
    public static IReadOnlyList<string> ValidateEntity<T>(string? id, T? entity) where T : class
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(id))
        {
            problems.Add("ID cannot be null, empty, or whitespace.");
        }

        if (entity is null)
        {
            problems.Add("Entity cannot be null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates repository operations parameters.
    /// </summary>
    /// <typeparam name="T">The entity type managed by the repository.</typeparam>
    /// <param name="entities">The entities dictionary to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entities"/> is null.</exception>
    public static IReadOnlyList<string> ValidateEntities<T>(Dictionary<string, T>? entities) where T : class
    {
        var problems = new List<string>();

        if (entities is null)
        {
            problems.Add("Entities dictionary cannot be null.");
            return problems.AsReadOnly();
        }

        if (entities.Count == 0)
        {
            problems.Add("Entities dictionary cannot be empty.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates repository operations parameters.
    /// </summary>
    /// <typeparam name="T">The entity type managed by the repository.</typeparam>
    /// <param name="predicate">The predicate function to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is null.</exception>
    public static IReadOnlyList<string> ValidatePredicate<T>(Func<T, bool>? predicate) where T : class
    {
        var problems = new List<string>();

        if (predicate is null)
        {
            problems.Add("Predicate cannot be null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Ensures that repository operation parameters are valid, throwing an exception if not.
    /// </summary>
    /// <param name="id">The entity ID to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is empty or whitespace.</exception>
    public static void EnsureValidId(string? id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("ID cannot be empty or whitespace.", nameof(id));
        }
    }

    /// <summary>
    /// Ensures that repository operation parameters are valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The entity type managed by the repository.</typeparam>
    /// <param name="id">The entity ID to validate.</param>
    /// <param name="entity">The entity to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> or <paramref name="entity"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is empty or whitespace.</exception>
    public static void EnsureValidEntity<T>(string? id, T? entity) where T : class
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("ID cannot be empty or whitespace.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(entity);
    }

    /// <summary>
    /// Ensures that repository operation parameters are valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The entity type managed by the repository.</typeparam>
    /// <param name="entities">The entities dictionary to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entities"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="entities"/> is empty.</exception>
    public static void EnsureValidEntities<T>(Dictionary<string, T>? entities) where T : class
    {
        ArgumentNullException.ThrowIfNull(entities);

        if (entities.Count == 0)
        {
            throw new ArgumentException("Entities dictionary cannot be empty.", nameof(entities));
        }
    }

    /// <summary>
    /// Ensures that repository operation parameters are valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The entity type managed by the repository.</typeparam>
    /// <param name="predicate">The predicate function to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is null.</exception>
    public static void EnsureValidPredicate<T>(Func<T, bool>? predicate) where T : class
    {
        ArgumentNullException.ThrowIfNull(predicate);
    }
}