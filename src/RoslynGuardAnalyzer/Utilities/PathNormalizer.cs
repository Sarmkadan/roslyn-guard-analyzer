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
    public static string Normalize(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        // Convert to full path to resolve relative paths
        var fullPath = Path.GetFullPath(path);

        // Use forward slashes for consistency
        return fullPath.Replace(Path.DirectorySeparatorChar, '/');
    }

    /// <summary>
    /// Normalizes multiple paths and returns them as a collection.
    /// </summary>
    public static string[] NormalizeMany(params string[] paths)
    {
        return paths.Select(Normalize).ToArray();
    }

    /// <summary>
    /// Checks if two paths point to the same file or directory (after normalization).
    /// </summary>
    public static bool ArePathsEqual(string path1, string path2)
    {
        var normalized1 = Normalize(path1);
        var normalized2 = Normalize(path2);

        // Case-insensitive on Windows, case-sensitive on Unix
        var comparison = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        return string.Equals(normalized1, normalized2, comparison);
    }

    /// <summary>
    /// Makes a path relative to a base directory (after normalization).
    /// </summary>
    public static string GetRelativePath(string basePath, string targetPath)
    {
        var normalizedBase = Normalize(basePath);
        var normalizedTarget = Normalize(targetPath);

        try
        {
            return Path.GetRelativePath(normalizedBase, normalizedTarget);
        }
        catch
        {
            // If path calculation fails, return the target path
            return normalizedTarget;
        }
    }

    /// <summary>
    /// Checks if a path is absolute (not relative).
    /// </summary>
    public static bool IsAbsolute(string path)
    {
        return Path.IsPathRooted(path);
    }

    /// <summary>
    /// Combines multiple path segments, normalizing the result.
    /// </summary>
    public static string Combine(params string[] segments)
    {
        if (segments is null || segments.Length == 0)
            return string.Empty;

        var combined = Path.Combine(segments);
        return Normalize(combined);
    }

    /// <summary>
    /// Gets the directory name from a path, normalized.
    /// </summary>
    public static string GetDirectoryName(string path)
    {
        return Normalize(Path.GetDirectoryName(path) ?? string.Empty);
    }

    /// <summary>
    /// Gets the file name from a path.
    /// </summary>
    public static string GetFileName(string path)
    {
        return Path.GetFileName(path);
    }

    /// <summary>
    /// Gets the file extension from a path.
    /// </summary>
    public static string GetExtension(string path)
    {
        return Path.GetExtension(path);
    }

    /// <summary>
    /// Checks if a path has a specific extension (case-insensitive).
    /// </summary>
    public static bool HasExtension(string path, string extension)
    {
        var ext = GetExtension(path);
        return ext.Equals(extension, StringComparison.OrdinalIgnoreCase);
    }
}

internal class RuntimeInformation
{
    public static bool IsOSPlatform(OSPlatform platform)
    {
        return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(platform);
    }
}

public class OSPlatform
{
    public static readonly OSPlatform Windows = new();
    public static readonly OSPlatform Linux = new();
    public static readonly OSPlatform OSX = new();
}
