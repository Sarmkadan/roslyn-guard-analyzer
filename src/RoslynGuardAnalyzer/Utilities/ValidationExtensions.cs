#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace RoslynGuardAnalyzer.Utilities;

/// <summary>
/// Extension methods for common validation operations.
/// Provides fluent validation with detailed error messages.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validates that a string is not null or empty.
    /// </summary>
    public static bool IsValidString(this string? value, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            error = "String cannot be null or empty";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that a value is within a range.
    /// </summary>
    public static bool IsInRange<T>(this T value, T min, T max, out string? error) where T : IComparable<T>
    {
        error = null;
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            error = $"Value {value} is not between {min} and {max}";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that a collection is not null or empty.
    /// </summary>
    public static bool HasItems<T>(this IEnumerable<T>? collection, out string? error)
    {
        error = null;
        if (collection is null || !collection.Any())
        {
            error = "Collection cannot be null or empty";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that a file path exists.
    /// </summary>
    public static bool FilePathExists(this string filePath, out string? error)
    {
        error = null;
        if (!System.IO.File.Exists(filePath))
        {
            error = $"File not found: {filePath}";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that a directory path exists.
    /// </summary>
    public static bool DirectoryPathExists(this string dirPath, out string? error)
    {
        error = null;
        if (!System.IO.Directory.Exists(dirPath))
        {
            error = $"Directory not found: {dirPath}";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that a value is one of the allowed values.
    /// </summary>
    public static bool IsOneOf<T>(this T value, IEnumerable<T> allowedValues, out string? error) where T : IEquatable<T>
    {
        error = null;
        if (!allowedValues.Any(v => v.Equals(value)))
        {
            error = $"Value {value} is not in the list of allowed values";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that a numeric value is positive.
    /// </summary>
    public static bool IsPositive(this int value, out string? error)
    {
        error = null;
        if (value <= 0)
        {
            error = $"Value {value} must be greater than 0";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that a numeric value is non-negative.
    /// </summary>
    public static bool IsNonNegative(this int value, out string? error)
    {
        error = null;
        if (value < 0)
        {
            error = $"Value {value} cannot be negative";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that a string matches a regex pattern.
    /// </summary>
    public static bool MatchesPattern(this string value, string pattern, out string? error)
    {
        error = null;
        try
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, pattern))
            {
                error = $"Value '{value}' does not match pattern '{pattern}'";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            error = $"Pattern validation error: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// Validates that a type is assignable from another type.
    /// </summary>
    public static bool IsAssignableFrom(this Type targetType, Type sourceType, out string? error)
    {
        error = null;
        if (!targetType.IsAssignableFrom(sourceType))
        {
            error = $"Type {sourceType.Name} is not assignable to {targetType.Name}";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Performs multiple validations and returns all errors.
    /// </summary>
    public static (bool IsValid, List<string> Errors) ValidateAll(params (bool Condition, string Error)[] validations)
    {
        var errors = validations
            .Where(v => !v.Condition)
            .Select(v => v.Error)
            .ToList();

        return (errors.Count == 0, errors);
    }
}
