#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace RoslynGuardAnalyzer.Services;

/// <summary>
/// Provides validation helpers for <see cref="OutputWriter"/> instances.
/// </summary>
public static class OutputWriterValidation
{
    /// <summary>
    /// Validates an <see cref="OutputWriter"/> instance.
    /// </summary>
    /// <param name="value">The <see cref="OutputWriter"/> to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this OutputWriter? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // OutputWriter has no public properties to validate beyond constructor parameter
        // The FormatterRegistry is validated during construction

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="OutputWriter"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="OutputWriter"/> to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this OutputWriter? value) => value is not null && value.Validate().Count == 0;

    /// <summary>
    /// Ensures that an <see cref="OutputWriter"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The <see cref="OutputWriter"/> to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
    public static void EnsureValid(this OutputWriter? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"OutputWriter is invalid. Problems: {string.Join("; ", problems)}",
                nameof(value));
        }
    }
}