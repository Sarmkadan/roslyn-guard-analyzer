#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;

namespace RoslynGuardAnalyzer.Utilities;

/// <summary>
/// Extension methods for common validation operations.
/// Provides fluent validation with detailed error messages.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Extension methods require static class")]
public static class ValidationExtensions
{
    /// <summary>
    /// Validates that a string is not null or empty.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="error">Output error message if validation fails; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the string is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidString(this string? value, [NotNullWhen(false)] out string? error)
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
    /// <typeparam name="T">The type of values to compare, must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <param name="error">Output error message if validation fails; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the value is within range; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/></exception>
    public static bool IsInRange<T>(this T value, T min, T max, [NotNullWhen(false)] out string? error) where T : IComparable<T>
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
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="error">Output error message if validation fails; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the collection has items; otherwise, <see langword="false"/>.</returns>
    public static bool HasItems<T>(this IEnumerable<T>? collection, [NotNullWhen(false)] out string? error)
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
    /// <param name="filePath">The file path to validate.</param>
    /// <param name="error">Output error message if validation fails; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the file exists; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/></exception>
    public static bool FilePathExists(this string filePath, [NotNullWhen(false)] out string? error)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        error = null;
        if (!File.Exists(filePath))
        {
            error = $"File not found: {filePath}";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that a directory path exists.
    /// </summary>
    /// <param name="dirPath">The directory path to validate.</param>
    /// <param name="error">Output error message if validation fails; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the directory exists; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dirPath"/> is <see langword="null"/></exception>
    public static bool DirectoryPathExists(this string dirPath, [NotNullWhen(false)] out string? error)
    {
        ArgumentNullException.ThrowIfNull(dirPath);

        error = null;
        if (!Directory.Exists(dirPath))
        {
            error = $"Directory not found: {dirPath}";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that a value is one of the allowed values.
    /// </summary>
    /// <typeparam name="T">The type of value to validate.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="allowedValues">The collection of allowed values.</param>
    /// <param name="error">Output error message if validation fails; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the value is in the allowed set; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="allowedValues"/> is <see langword="null"/></exception>
    public static bool IsOneOf<T>(this T value, IEnumerable<T> allowedValues, [NotNullWhen(false)] out string? error) where T : IEquatable<T>
    {
        ArgumentNullException.ThrowIfNull(allowedValues);

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
    /// <param name="value">The value to validate.</param>
    /// <param name="error">Output error message if validation fails; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the value is positive; otherwise, <see langword="false"/>.</returns>
    public static bool IsPositive(this int value, [NotNullWhen(false)] out string? error)
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
    /// <param name="value">The value to validate.</param>
    /// <param name="error">Output error message if validation fails; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the value is non-negative; otherwise, <see langword="false"/>.</returns>
    public static bool IsNonNegative(this int value, [NotNullWhen(false)] out string? error)
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
    /// <param name="value">The string value to validate.</param>
    /// <param name="pattern">The regex pattern to match against.</param>
    /// <param name="error">Output error message if validation fails; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the string matches the pattern; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> or <paramref name="pattern"/> is <see langword="null"/></exception>
    public static bool MatchesPattern(this string value, string pattern, [NotNullWhen(false)] out string? error)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(pattern);

        error = null;
        try
        {
            if (!Regex.IsMatch(value, pattern))
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
    /// <param name="targetType">The target type to check assignability to.</param>
    /// <param name="sourceType">The source type to check.</param>
    /// <param name="error">Output error message if validation fails; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the source type is assignable to the target type; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="targetType"/> or <paramref name="sourceType"/> is <see langword="null"/></exception>
    public static bool IsAssignableFrom(this Type targetType, Type sourceType, [NotNullWhen(false)] out string? error)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        ArgumentNullException.ThrowIfNull(sourceType);

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
    /// <param name="validations">Array of validation conditions and their corresponding error messages.</param>
    /// <returns>Tuple containing whether all validations passed and a list of error messages.</returns>
    public static (bool IsValid, List<string> Errors) ValidateAll(params (bool Condition, string Error)[] validations)
    {
        ArgumentNullException.ThrowIfNull(validations);

        var errors = validations
            .Where(v => !v.Condition)
            .Select(v => v.Error)
            .ToList();

        return (errors.Count == 0, errors);
    }
}
