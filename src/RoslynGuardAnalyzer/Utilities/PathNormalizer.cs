#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;
using System.Linq;

namespace RoslynGuardAnalyzer.Utilities;

/// <summary>
/// Normalizes file system paths for consistent comparison and processing.
/// Handles Windows/Unix path separators, relative paths, and redundant segments.
/// </summary>
public static class PathNormalizer
{
    /// <summary>
    /// Normalizes a path by resolving . and .. segments and using forward slashes.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="path"/> is null, empty, or consists only of white-space characters.</exception>
    public static string Normalize(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath = Path.GetFullPath(path);
        return fullPath.Replace(Path.DirectorySeparatorChar, '/');
    }

    /// <summary>
    /// Normalizes multiple paths and returns them as a collection.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="paths"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="paths"/> contains a null, empty, or white-space entry.</exception>
    public static string[] NormalizeMany(params string[] paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        for (var index = 0; index < paths.Length; index++)
        {
            if (paths[index] is null)
                throw new ArgumentException($"Path at index {index} cannot be null.", nameof(paths));
        }

        return paths.Select(Normalize).ToArray();
    }

    /// <summary>
    /// Checks if two paths point to the same file or directory (after normalization).
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="path1"/> or <paramref name="path2"/> is null, empty, or consists only of white-space characters.</exception>
    public static bool ArePathsEqual(string path1, string path2)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path1);
        ArgumentException.ThrowIfNullOrWhiteSpace(path2);

        var normalized1 = Normalize(path1);
        var normalized2 = Normalize(path2);

        var comparison = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        return string.Equals(normalized1, normalized2, comparison);
    }

    /// <summary>
    /// Makes a path relative to a base directory (after normalization).
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="basePath"/> or <paramref name="targetPath"/> is null, empty, or consists only of white-space characters.</exception>
    public static string GetRelativePath(string basePath, string targetPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(basePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);

        var normalizedBase = Normalize(basePath);
        var normalizedTarget = Normalize(targetPath);

        try
        {
            return Path.GetRelativePath(normalizedBase, normalizedTarget);
        }
        catch
        {
            return normalizedTarget;
        }
    }

    /// <summary>
    /// Checks if a path is absolute (not relative).
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="path"/> is null, empty, or consists only of white-space characters.</exception>
    public static bool IsAbsolute(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Path.IsPathRooted(path);
    }

    /// <summary>
    /// Combines multiple path segments, normalizing the result.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="segments"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="segments"/> contains a null, empty, or white-space entry.</exception>
    public static string Combine(params string[] segments)
    {
        ArgumentNullException.ThrowIfNull(segments);

        if (segments.Length == 0)
            return string.Empty;

        foreach (var segment in segments)
            ArgumentException.ThrowIfNullOrWhiteSpace(segment);

        var combined = Path.Combine(segments);
        return Normalize(combined);
    }

    /// <summary>
    /// Gets the directory name from a path, normalized.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="path"/> is null, empty, or consists only of white-space characters.</exception>
    public static string GetDirectoryName(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Normalize(Path.GetDirectoryName(path) ?? string.Empty);
    }

    /// <summary>
    /// Gets the file name from a path.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="path"/> is null, empty, or consists only of white-space characters.</exception>
    public static string GetFileName(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Path.GetFileName(path);
    }

    /// <summary>
    /// Gets the file extension from a path.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="path"/> is null, empty, or consists only of white-space characters.</exception>
    public static string GetExtension(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Path.GetExtension(path);
    }

    /// <summary>
    /// Checks if a path has a specific extension (case-insensitive).
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="path"/> or <paramref name="extension"/> is null, empty, or consists only of white-space characters.</exception>
    public static bool HasExtension(string path, string extension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(extension);

        var ext = GetExtension(path);
        return ext.Equals(extension, StringComparison.OrdinalIgnoreCase);
    }
}
