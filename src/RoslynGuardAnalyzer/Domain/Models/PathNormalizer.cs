#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Diagnostics.CodeAnalysis;

namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Provides consistent path normalization across different operating systems and components.
/// Ensures that file paths are normalized to a standard format for reliable comparison and matching.
/// </summary>
public static class PathNormalizer
{
    /// <summary>
    /// Normalization mode for file paths.
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// Case-sensitive comparison with forward slashes (Linux/Unix style)
        /// </summary>
        CaseSensitive,

        /// <summary>
        /// Case-insensitive comparison with forward slashes (Windows/Linux compatible)
        /// </summary>
        CaseInsensitive
    }

    /// <summary>
    /// Normalizes a file path for consistent comparison across different operating systems.
    ///
    /// Converts path separators to forward slashes, removes redundant separators,
    /// handles relative paths, and optionally normalizes case.
    /// </summary>
    /// <param name="filePath">The file path to normalize</param>
    /// <param name="mode">The normalization mode to use</param>
    /// <returns>Normalized file path with consistent format</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null</exception>
    public static string Normalize(string filePath, Mode mode = Mode.CaseInsensitive)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return filePath;
        }

        // Convert to forward slashes for consistency across platforms
        var normalized = filePath.Replace('\\', '/');

        // Remove redundant separators (e.g., "/./" -> "/")
        normalized = normalized.Replace("/./", "/");
        normalized = normalized.Replace("/../", "/");

        // Handle "/./" at the start
        if (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized.Substring(2);
        }

        // Handle "/./" at the end
        if (normalized.EndsWith("/.", StringComparison.Ordinal))
        {
            normalized = normalized.Substring(0, normalized.Length - 2);
        }

        // Remove trailing slashes
        normalized = normalized.TrimEnd('/');

        // Normalize case if requested
        if (mode == Mode.CaseInsensitive)
        {
            normalized = normalized.ToLowerInvariant();
        }

        return normalized;
    }

    /// <summary>
    /// Compares two file paths using the specified normalization mode.
    /// </summary>
    /// <param name="path1">First file path to compare</param>
    /// <param name="path2">Second file path to compare</param>
    /// <param name="mode">Normalization mode to use for comparison</param>
    /// <returns>True if the paths represent the same file after normalization</returns>
    /// <exception cref="ArgumentNullException">Thrown when either path is null</exception>
    public static bool Equals(string path1, string path2, Mode mode = Mode.CaseInsensitive)
    {
        ArgumentNullException.ThrowIfNull(path1);
        ArgumentNullException.ThrowIfNull(path2);

        var normalized1 = Normalize(path1, mode);
        var normalized2 = Normalize(path2, mode);

        return mode == Mode.CaseInsensitive
            ? string.Equals(normalized1, normalized2, StringComparison.OrdinalIgnoreCase)
            : string.Equals(normalized1, normalized2, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the normalized file path for display purposes.
    /// This preserves the original path format but ensures consistent separators.
    /// </summary>
    /// <param name="filePath">The file path to normalize for display</param>
    /// <returns>Normalized file path with consistent separators</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null</exception>
    public static string NormalizeForDisplay(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return filePath;
        }

        // Convert to forward slashes for consistent display
        return filePath.Replace('\\', '/');
    }

    /// <summary>
    /// Gets the file name from a path using consistent normalization.
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <returns>The file name portion of the path</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null</exception>
    public static string GetFileName(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        // Use Path.GetFileName which handles both forward and backward slashes
        return System.IO.Path.GetFileName(filePath);
    }

    /// <summary>
    /// Determines whether two paths are equivalent after normalization.
    /// This is a convenience method that uses CaseInsensitive mode by default.
    /// </summary>
    /// <param name="path1">First file path to compare</param>
    /// <param name="path2">Second file path to compare</param>
    /// <returns>True if the paths represent the same file after normalization</returns>
    /// <exception cref="ArgumentNullException">Thrown when either path is null</exception>
    public static bool AreEquivalent(string path1, string path2)
    {
        return Equals(path1, path2, Mode.CaseInsensitive);
    }

    /// <summary>
    /// Gets a hash code for a file path using the same normalization as Equals.
    /// </summary>
    /// <param name="filePath">The file path to get hash code for</param>
    /// <returns>Hash code for the normalized file path</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null</exception>
    public static int GetHashCode(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        // Normalize the path using the same mode as Equals (CaseInsensitive)
        var normalized = Normalize(filePath, Mode.CaseInsensitive);
        return StringComparer.Ordinal.GetHashCode(normalized);
    }
}