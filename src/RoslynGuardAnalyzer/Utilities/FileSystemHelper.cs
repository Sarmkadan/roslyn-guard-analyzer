#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoslynGuardAnalyzer.Utilities;

/// <summary>
/// Provides file system operations with error handling and filtering.
/// Handles exclusion patterns for common build artifacts (bin, obj, .git).
/// </summary>
public static class FileSystemHelper
{
    private static readonly string[] DefaultExclusionPatterns =
    [
        "bin", "obj", ".git", ".vs", ".vscode", "node_modules",
        ".nuget", "packages", ".build", "dist", "coverage"
    ];

    /// <summary>
    /// Finds all C# files in a directory, respecting exclusion patterns.
    /// </summary>
    public static string[] FindCSharpFiles(string directory, string[]? additionalExclusions = null)
    {
        var exclusions = DefaultExclusionPatterns.Concat(additionalExclusions ?? []).ToHashSet();

        try
        {
            var files = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);
            return files.Where(f => !ShouldExclude(f, exclusions)).ToArray();
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }

    /// <summary>
    /// Finds all project files (.csproj, .fsproj) in a directory.
    /// </summary>
    public static string[] FindProjectFiles(string directory)
    {
        var exclusions = DefaultExclusionPatterns.ToHashSet();

        try
        {
            var csprojFiles = Directory.GetFiles(directory, "*.csproj", SearchOption.AllDirectories);
            var fsprojFiles = Directory.GetFiles(directory, "*.fsproj", SearchOption.AllDirectories);

            var allFiles = csprojFiles.Concat(fsprojFiles).ToArray();
            return allFiles.Where(f => !ShouldExclude(f, exclusions)).ToArray();
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }

    /// <summary>
    /// Safely reads a file's contents with error handling.
    /// </summary>
    public static async Task<string?> ReadFileAsync(string filePath)
    {
        try
        {
            return await File.ReadAllTextAsync(filePath);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    /// <summary>
    /// Safely writes content to a file, creating directories as needed.
    /// </summary>
    public static async Task<bool> WriteFileAsync(string filePath, string content)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(filePath, content);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a path exists and is a file (not a directory).
    /// </summary>
    public static bool FileExists(string path)
    {
        try
        {
            return File.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a path exists and is a directory.
    /// </summary>
    public static bool DirectoryExists(string path)
    {
        try
        {
            return Directory.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the size of a file in bytes, or -1 if file doesn't exist.
    /// </summary>
    public static long GetFileSize(string filePath)
    {
        try
        {
            var info = new FileInfo(filePath);
            return info.Exists ? info.Length : -1;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// Gets the last modified time of a file.
    /// </summary>
    public static DateTime? GetLastModifiedTime(string filePath)
    {
        try
        {
            return File.GetLastWriteTime(filePath);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Determines if a path should be excluded based on exclusion patterns.
    /// </summary>
    private static bool ShouldExclude(string path, ISet<string> exclusions)
    {
        var pathParts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        foreach (var exclusion in exclusions)
        {
            if (pathParts.Any(part => part.Equals(exclusion, StringComparison.OrdinalIgnoreCase)))
                return true;
        }

        return false;
    }
}
