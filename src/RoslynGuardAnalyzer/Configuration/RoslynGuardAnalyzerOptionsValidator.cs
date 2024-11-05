#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;

namespace RoslynGuardAnalyzer.Configuration;

/// <summary>
/// Validates RoslynGuardAnalyzerOptions using the IValidateOptions pattern.
/// Provides validation that runs during DI container initialization.
/// </summary>
public sealed class RoslynGuardAnalyzerOptionsValidator : IValidateOptions<RoslynGuardAnalyzerOptions>
{
    /// <summary>
    /// Validates the options.
    /// </summary>
    /// <param name="name">The name of the options (null for unnamed options)</param>
    /// <param name="options">The options instance to validate</param>
    /// <returns>Validation result with errors if any</returns>
    public ValidateOptionsResult Validate(string? name, RoslynGuardAnalyzerOptions options)
    {
        var errors = options.Validate();

        if (errors.Count == 0)
        {
            return ValidateOptionsResult.Success;
        }

        var errorMessage = string.Join("\n", errors);
        return ValidateOptionsResult.Fail(errorMessage);
    }
}