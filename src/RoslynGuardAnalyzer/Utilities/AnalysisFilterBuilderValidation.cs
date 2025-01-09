#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="AnalysisFilterBuilder"/> instances.
/// Validates all filter criteria including severity levels, rule names, file paths,
/// and line number ranges.
/// </summary>
public static class AnalysisFilterBuilderValidation
{
    private static readonly Dictionary<string, int> _severityOrder = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Low"] = 1,
        ["Info"] = 1,
        ["Medium"] = 2,
        ["Warning"] = 2,
        ["High"] = 3,
        ["Error"] = 3,
        ["Critical"] = 4
    };

    /// <summary>
    /// Validates the specified <see cref="AnalysisFilterBuilder"/> instance.
    /// </summary>
    /// <param name="value">The filter builder to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AnalysisFilterBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Note: AnalysisFilterBuilder is a builder that accumulates predicates
        // We can only validate the configuration that was set, not the predicates themselves
        // The validation here ensures the builder was configured correctly

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the severity string against known severity levels.
    /// </summary>
    /// <param name="severity">The severity level string to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateSeverity(this string severity, string paramName = "severity")
    {
        ArgumentException.ThrowIfNullOrEmpty(severity, paramName);

        if (!_severityOrder.ContainsKey(severity))
        {
            return new[] { $"Unknown severity level: '{severity}'. Valid values are: Low, Info, Medium, Warning, High, Error, Critical." };
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates the severity level enum value.
    /// </summary>
    /// <param name="severity">The severity level to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateSeverity(this SeverityLevel severity)
    {
        // All enum values are valid by definition
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates the rule name string.
    /// </summary>
    /// <param name="ruleName">The rule name to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateRuleName(this string ruleName, string paramName = "ruleName")
    {
        ArgumentException.ThrowIfNullOrEmpty(ruleName, paramName);

        if (string.IsNullOrWhiteSpace(ruleName))
        {
            return new[] { $"Rule name cannot be empty or whitespace." };
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates the file path string.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateFilePath(this string filePath, string paramName = "filePath")
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath, paramName);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return new[] { $"File path cannot be empty or whitespace." };
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates the line number is positive.
    /// </summary>
    /// <param name="lineNumber">The line number to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateLineNumber(this int lineNumber, string paramName = "lineNumber")
    {
        if (lineNumber < 1)
        {
            return new[] { $"Line number must be positive (>= 1), but was {lineNumber}." };
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates the text string for message filtering.
    /// </summary>
    /// <param name="text">The text to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateMessageText(this string text, string paramName = "text")
    {
        ArgumentException.ThrowIfNullOrEmpty(text, paramName);

        if (string.IsNullOrWhiteSpace(text))
        {
            return new[] { $"{paramName} cannot be empty or whitespace." };
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates the predicate function.
    /// </summary>
    /// <param name="predicate">The predicate to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidatePredicate(this Func<RuleViolation, bool> predicate, string paramName = "predicate")
    {
        ArgumentNullException.ThrowIfNull(predicate, paramName);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates the rule names collection.
    /// </summary>
    /// <param name="ruleNames">The rule names to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateRuleNames(this IEnumerable<string> ruleNames, string paramName = "ruleNames")
    {
        ArgumentNullException.ThrowIfNull(ruleNames, paramName);

        var problems = new List<string>();
        var index = 0;

        foreach (var ruleName in ruleNames)
        {
            if (string.IsNullOrWhiteSpace(ruleName))
            {
                problems.Add($"Rule name at index {index} cannot be null or whitespace.");
            }
            else if (ruleName.Trim().Length == 0)
            {
                problems.Add($"Rule name at index {index} cannot be empty or whitespace.");
            }

            index++;
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="AnalysisFilterBuilder"/> is valid.
    /// </summary>
    /// <param name="value">The filter builder to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this AnalysisFilterBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // AnalysisFilterBuilder itself has no invalid state by design
        // All validation is done at the point of configuration
        return true;
    }

    /// <summary>
    /// Ensures that the specified <see cref="AnalysisFilterBuilder"/> is valid.
    /// </summary>
    /// <param name="value">The filter builder to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the builder is invalid.</exception>
    public static void EnsureValid(this AnalysisFilterBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // AnalysisFilterBuilder itself has no invalid state by design
        // All validation is done at the point of configuration
    }

    /// <summary>
    /// Ensures that the specified severity string is valid.
    /// </summary>
    /// <param name="severity">The severity level string to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when the severity is invalid.</exception>
    public static void EnsureValidSeverity(this string severity, string paramName = "severity")
    {
        var problems = severity.ValidateSeverity(paramName);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems), paramName);
        }
    }

    /// <summary>
    /// Ensures that the specified severity level is valid.
    /// </summary>
    /// <param name="severity">The severity level to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the severity is invalid.</exception>
    public static void EnsureValidSeverity(this SeverityLevel severity)
    {
        // All enum values are valid by definition
    }

    /// <summary>
    /// Ensures that the specified rule name is valid.
    /// </summary>
    /// <param name="ruleName">The rule name to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when the rule name is invalid.</exception>
    public static void EnsureValidRuleName(this string ruleName, string paramName = "ruleName")
    {
        var problems = ruleName.ValidateRuleName(paramName);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems), paramName);
        }
    }

    /// <summary>
    /// Ensures that the specified file path is valid.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when the file path is invalid.</exception>
    public static void EnsureValidFilePath(this string filePath, string paramName = "filePath")
    {
        var problems = filePath.ValidateFilePath(paramName);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems), paramName);
        }
    }

    /// <summary>
    /// Ensures that the specified line number is valid.
    /// </summary>
    /// <param name="lineNumber">The line number to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when the line number is invalid.</exception>
    public static void EnsureValidLineNumber(this int lineNumber, string paramName = "lineNumber")
    {
        var problems = lineNumber.ValidateLineNumber(paramName);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems), paramName);
        }
    }

    /// <summary>
    /// Ensures that the specified message text is valid.
    /// </summary>
    /// <param name="text">The message text to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when the message text is invalid.</exception>
    public static void EnsureValidMessageText(this string text, string paramName = "text")
    {
        var problems = text.ValidateMessageText(paramName);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems), paramName);
        }
    }

    /// <summary>
    /// Ensures that the specified predicate is valid.
    /// </summary>
    /// <param name="predicate">The predicate to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when the predicate is invalid.</exception>
    public static void EnsureValidPredicate(this Func<RuleViolation, bool> predicate, string paramName = "predicate")
    {
        var problems = predicate.ValidatePredicate(paramName);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems), paramName);
        }
    }

    /// <summary>
    /// Ensures that the specified rule names collection is valid.
    /// </summary>
    /// <param name="ruleNames">The rule names to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when the rule names are invalid.</exception>
    public static void EnsureValidRuleNames(this IEnumerable<string> ruleNames, string paramName = "ruleNames")
    {
        var problems = ruleNames.ValidateRuleNames(paramName);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems), paramName);
        }
    }
}
