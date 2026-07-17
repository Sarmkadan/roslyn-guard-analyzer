#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="CodeElement"/> instances.
/// </summary>
public static class CodeElementValidation
{
    /// <summary>
    /// Validates a <see cref="CodeElement"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The code element to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the element is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CodeElement value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate required string properties
        ValidateRequiredString(value.Id, nameof(value.Id), problems);
        ValidateRequiredString(value.Name, nameof(value.Name), problems);
        ValidateRequiredString(value.FilePath, nameof(value.FilePath), problems);
        ValidateRequiredString(value.Namespace, nameof(value.Namespace), problems);

        // Validate line numbers using pattern matching for consistency
        if (value.StartLineNumber <= 0)
        {
            problems.Add($"StartLineNumber must be greater than 0, but was {value.StartLineNumber}.");
        }

        if (value.EndLineNumber < value.StartLineNumber)
        {
            problems.Add($"EndLineNumber ({value.EndLineNumber}) must be greater than or equal to StartLineNumber ({value.StartLineNumber}).");
        }

        // Validate complexity
        if (value.Complexity < 1)
        {
            problems.Add($"Complexity must be at least 1, but was {value.Complexity}.");
        }

        // Validate analyzed timestamp with tighter future check
        if (value.AnalyzedAt == default)
        {
            problems.Add("AnalyzedAt must be set to a valid DateTime.");
        }
        else if (value.AnalyzedAt > DateTime.UtcNow.AddMinutes(1))
        {
            problems.Add($"AnalyzedAt cannot be in the future. Found: {value.AnalyzedAt:O}.");
        }

        // Validate optional string properties using pattern matching
        if (value.ParentName is not null && string.IsNullOrWhiteSpace(value.ParentName.Trim()))
        {
            problems.Add("ParentName contains only whitespace.");
        }

        if (value.FullyQualifiedName is not null && string.IsNullOrWhiteSpace(value.FullyQualifiedName.Trim()))
        {
            problems.Add("FullyQualifiedName contains only whitespace.");
        }

        if (value.ReturnType is not null && string.IsNullOrWhiteSpace(value.ReturnType.Trim()))
        {
            problems.Add("ReturnType contains only whitespace.");
        }

        // Validate collections using pattern matching
        if (value.Attributes is null)
        {
            problems.Add("Attributes collection must not be null.");
        }

        if (value.Dependencies is null)
        {
            problems.Add("Dependencies collection must not be null.");
        }

        if (value.SuppressDirectives is null)
        {
            problems.Add("SuppressDirectives collection must not be null.");
        }

        if (value.Parameters is null)
        {
            problems.Add("Parameters collection must not be null.");
        }

        // Validate collection contents using foreach for better readability
        if (value.Attributes is not null)
        {
            var index = 0;
            foreach (var attribute in value.Attributes)
            {
                if (string.IsNullOrWhiteSpace(attribute))
                {
                    problems.Add($"Attributes[{index}] is null or whitespace.");
                }
                index++;
            }
        }

        if (value.Dependencies is not null)
        {
            var index = 0;
            foreach (var dependency in value.Dependencies)
            {
                if (string.IsNullOrWhiteSpace(dependency))
                {
                    problems.Add($"Dependencies[{index}] is null or whitespace.");
                }
                index++;
            }
        }

        if (value.SuppressDirectives is not null)
        {
            var index = 0;
            foreach (var directive in value.SuppressDirectives)
            {
                if (string.IsNullOrWhiteSpace(directive))
                {
                    problems.Add($"SuppressDirectives[{index}] is null or whitespace.");
                }
                index++;
            }
        }

        if (value.Parameters is not null)
        {
            var index = 0;
            foreach (var parameter in value.Parameters)
            {
                if (string.IsNullOrWhiteSpace(parameter))
                {
                    problems.Add($"Parameters[{index}] is null or whitespace.");
                }
                index++;
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="CodeElement"/> instance is valid.
    /// </summary>
    /// <param name="value">The code element to check.</param>
    /// <returns>True if the element is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this CodeElement value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="CodeElement"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The code element to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the element is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this CodeElement value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"CodeElement is invalid. Problems: {string.Join(" ", problems)}",
            nameof(value));
    }

    private static void ValidateRequiredString(string? value, string paramName, List<string> problems)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            problems.Add($"{paramName} must not be null or whitespace.");
        }
    }
}