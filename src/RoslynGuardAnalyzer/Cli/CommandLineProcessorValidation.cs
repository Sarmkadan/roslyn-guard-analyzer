#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace RoslynGuardAnalyzer.Cli;

/// <summary>
/// Provides validation helpers for <see cref="CommandLineProcessor"/> instances.
/// Validates the state of a processed command-line processor including options and paths.
/// </summary>
public static class CommandLineProcessorValidation
{
    /// <summary>
    /// Validates the command-line processor and returns any problems found.
    /// </summary>
    /// <param name="value">The command-line processor to validate.</param>
/// <returns>An empty read-only list if the processor is valid; otherwise, a read-only list of human-readable problem descriptions.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CommandLineProcessor value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate that Process was called (options are parsed)
        var options = value.GetOptions();
        if (options is null)
        {
            errors.Add("Command-line processor has not processed arguments yet (GetOptions returned null)");
            return errors.AsReadOnly();
        }

        // Validate options structure
        if (!options.Validate(out var optionErrors))
        {
            errors.AddRange(optionErrors);
        }

        // Validate paths if in analysis mode
        if (options.IsAnalysisMode)
        {
            var (valid, pathErrors) = value.ValidatePaths();
            if (!valid)
            {
                errors.AddRange(pathErrors);
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the command-line processor is in a valid state.
    /// </summary>
    /// <param name="value">The command-line processor to check.</param>
/// <returns><see langword="true"/> if the processor is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this CommandLineProcessor value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        return errors.Count == 0;
    }

    /// <summary>
    /// Ensures that the command-line processor is in a valid state.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all problems if validation fails.
    /// </summary>
    /// <param name="value">The command-line processor to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the processor is not valid. The exception message contains a list of all validation problems.</exception>
    public static void EnsureValid(this CommandLineProcessor value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"Command-line processor is not valid. Problems: {string.Join("; ", errors)}");
    }
}