#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace RoslynGuardAnalyzer.Utilities;

/// <summary>
/// Provides validation methods for file system operations.
/// Validates parameters and state before file system operations.
/// </summary>
public static class FileSystemHelperValidation
{
    /// <summary>
    /// Validates a directory path for file system operations.
    /// Returns a list of human-readable validation problems, or an empty list if valid.
    /// </summary>
    /// <param name="directory">The directory path to validate.</param>
    /// <param name="additionalExclusions">Additional exclusion patterns.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateDirectory(string directory, string[]? additionalExclusions = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(directory);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(directory))
        {
            problems.Add("Directory path cannot be whitespace or empty.");
            return problems.AsReadOnly();
        }

        if (!FileSystemHelper.DirectoryExists(directory))
        {
            problems.Add($"Directory does not exist: '{directory}'.");
        }

        if (Path.GetPathRoot(directory) == directory)
        {
            problems.Add("Root directory paths are not supported for file operations.");
        }

        if (additionalExclusions != null)
        {
            foreach (var exclusion in additionalExclusions)
            {
                if (string.IsNullOrWhiteSpace(exclusion))
                {
                    problems.Add("Additional exclusion pattern cannot be null or whitespace.");
                    break;
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a file path for existence check.
    /// Returns a list of human-readable validation problems, or an empty list if valid.
    /// </summary>
    /// <param name="path">The file path to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateFileExists(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(path))
        {
            problems.Add("File path cannot be null or whitespace.");
            return problems.AsReadOnly();
        }

        if (Path.GetPathRoot(path) == path)
        {
            problems.Add("Root directory paths are not valid file paths.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a directory path for existence check.
    /// Returns a list of human-readable validation problems, or an empty list if valid.
    /// </summary>
    /// <param name="path">The directory path to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateDirectoryExists(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(path))
        {
            problems.Add("Directory path cannot be null or whitespace.");
            return problems.AsReadOnly();
        }

        if (Path.GetPathRoot(path) == path)
        {
            problems.Add("Root directory paths are not valid directory paths.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a file path for size retrieval.
    /// Returns a list of human-readable validation problems, or an empty list if valid.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGetFileSize(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            problems.Add("File path cannot be null or whitespace.");
            return problems.AsReadOnly();
        }

        if (Path.GetPathRoot(filePath) == filePath)
        {
            problems.Add("Root directory paths are not valid file paths.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a file path for last modified time retrieval.
    /// Returns a list of human-readable validation problems, or an empty list if valid.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGetLastModifiedTime(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            problems.Add("File path cannot be null or whitespace.");
            return problems.AsReadOnly();
        }

        if (Path.GetPathRoot(filePath) == filePath)
        {
            problems.Add("Root directory paths are not valid file paths.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a file path for reading.
    /// Returns a list of human-readable validation problems, or an empty list if valid.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateReadFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            problems.Add("File path cannot be null or whitespace.");
            return problems.AsReadOnly();
        }

        if (Path.GetPathRoot(filePath) == filePath)
        {
            problems.Add("Root directory paths are not valid file paths.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for file writing.
    /// Returns a list of human-readable validation problems, or an empty list if valid.
    /// </summary>
    /// <param name="filePath">The file path to write to.</param>
    /// <param name="content">The content to write.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateWriteFile(string filePath, string content)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentNullException.ThrowIfNull(content);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            problems.Add("File path cannot be null or whitespace.");
        }
        else
        {
            if (Path.GetPathRoot(filePath) == filePath)
            {
                problems.Add("Root directory paths are not valid file paths.");
            }
        }

        if (content == null)
        {
            problems.Add("Content cannot be null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for finding C# files.
    /// Returns a list of human-readable validation problems, or an empty list if valid.
    /// </summary>
    /// <param name="directory">The directory to search in.</param>
    /// <param name="additionalExclusions">Additional exclusion patterns.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateFindCSharpFiles(string directory, string[]? additionalExclusions = null)
        => ValidateDirectory(directory, additionalExclusions);

    /// <summary>
    /// Validates parameters for finding project files.
    /// Returns a list of human-readable validation problems, or an empty list if valid.
    /// </summary>
    /// <param name="directory">The directory to search in.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateFindProjectFiles(string directory)
        => ValidateDirectory(directory);

    /// <summary>
    /// Checks if the validation indicates any problems.
    /// </summary>
    /// <param name="problems">The list of validation problems.</param>
    /// <returns>True if valid (no problems); false otherwise.</returns>
    public static bool IsValid(this IReadOnlyList<string> problems) => problems.Count == 0;

    /// <summary>
    /// Checks if a directory path is valid for file operations.
    /// </summary>
    /// <param name="directory">The directory path to check.</param>
    /// <param name="additionalExclusions">Additional exclusion patterns.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValidDirectory(string directory, string[]? additionalExclusions = null)
        => ValidateDirectory(directory, additionalExclusions).IsValid();

    /// <summary>
    /// Checks if a file path is valid for existence check.
    /// </summary>
    /// <param name="path">The file path to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValidFileExists(string path)
        => ValidateFileExists(path).IsValid();

    /// <summary>
    /// Checks if a directory path is valid for existence check.
    /// </summary>
    /// <param name="path">The directory path to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValidDirectoryExists(string path)
        => ValidateDirectoryExists(path).IsValid();

    /// <summary>
    /// Checks if a file path is valid for size retrieval.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValidGetFileSize(string filePath)
        => ValidateGetFileSize(filePath).IsValid();

    /// <summary>
    /// Checks if a file path is valid for last modified time retrieval.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValidGetLastModifiedTime(string filePath)
        => ValidateGetLastModifiedTime(filePath).IsValid();

    /// <summary>
    /// Checks if a file path is valid for reading.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValidReadFile(string filePath)
        => ValidateReadFile(filePath).IsValid();

    /// <summary>
    /// Checks if write file parameters are valid.
    /// </summary>
    /// <param name="filePath">The file path to write to.</param>
    /// <param name="content">The content to write.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValidWriteFile(string filePath, string content)
        => ValidateWriteFile(filePath, content).IsValid();

    /// <summary>
    /// Checks if find C# files parameters are valid.
    /// </summary>
    /// <param name="directory">The directory to search in.</param>
    /// <param name="additionalExclusions">Additional exclusion patterns.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValidFindCSharpFiles(string directory, string[]? additionalExclusions = null)
        => ValidateFindCSharpFiles(directory, additionalExclusions).IsValid();

    /// <summary>
    /// Checks if find project files parameters are valid.
    /// </summary>
    /// <param name="directory">The directory to search in.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValidFindProjectFiles(string directory)
        => ValidateFindProjectFiles(directory).IsValid();

    /// <summary>
    /// Ensures that a directory path is valid for file operations, throwing an exception if not.
    /// </summary>
    /// <param name="directory">The directory path to validate.</param>
    /// <param name="additionalExclusions">Additional exclusion patterns.</param>
    /// <exception cref="ArgumentException">Thrown if the directory is not valid.</exception>
    public static void EnsureValidDirectory(string directory, string[]? additionalExclusions = null)
    {
        var problems = ValidateDirectory(directory, additionalExclusions);
        if (!problems.IsValid())
        {
            throw new ArgumentException("Directory validation failed: " + string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that a file path is valid for existence check, throwing an exception if not.
    /// </summary>
    /// <param name="path">The file path to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the path is not valid.</exception>
    public static void EnsureValidFileExists(string path)
    {
        var problems = ValidateFileExists(path);
        if (!problems.IsValid())
        {
            throw new ArgumentException("File existence validation failed: " + string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that a directory path is valid for existence check, throwing an exception if not.
    /// </summary>
    /// <param name="path">The directory path to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the path is not valid.</exception>
    public static void EnsureValidDirectoryExists(string path)
    {
        var problems = ValidateDirectoryExists(path);
        if (!problems.IsValid())
        {
            throw new ArgumentException("Directory existence validation failed: " + string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that a file path is valid for size retrieval, throwing an exception if not.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the path is not valid.</exception>
    public static void EnsureValidGetFileSize(string filePath)
    {
        var problems = ValidateGetFileSize(filePath);
        if (!problems.IsValid())
        {
            throw new ArgumentException("File size validation failed: " + string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that a file path is valid for last modified time retrieval, throwing an exception if not.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the path is not valid.</exception>
    public static void EnsureValidGetLastModifiedTime(string filePath)
    {
        var problems = ValidateGetLastModifiedTime(filePath);
        if (!problems.IsValid())
        {
            throw new ArgumentException("Last modified time validation failed: " + string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that a file path is valid for reading, throwing an exception if not.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the path is not valid.</exception>
    public static void EnsureValidReadFile(string filePath)
    {
        var problems = ValidateReadFile(filePath);
        if (!problems.IsValid())
        {
            throw new ArgumentException("File read validation failed: " + string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that write file parameters are valid, throwing an exception if not.
    /// </summary>
    /// <param name="filePath">The file path to write to.</param>
    /// <param name="content">The content to write.</param>
    /// <exception cref="ArgumentException">Thrown if the parameters are not valid.</exception>
    public static void EnsureValidWriteFile(string filePath, string content)
    {
        var problems = ValidateWriteFile(filePath, content);
        if (!problems.IsValid())
        {
            throw new ArgumentException("File write validation failed: " + string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that find C# files parameters are valid, throwing an exception if not.
    /// </summary>
    /// <param name="directory">The directory to search in.</param>
    /// <param name="additionalExclusions">Additional exclusion patterns.</param>
    /// <exception cref="ArgumentException">Thrown if the parameters are not valid.</exception>
    public static void EnsureValidFindCSharpFiles(string directory, string[]? additionalExclusions = null)
    {
        var problems = ValidateFindCSharpFiles(directory, additionalExclusions);
        if (!problems.IsValid())
        {
            throw new ArgumentException("Find C# files validation failed: " + string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that find project files parameters are valid, throwing an exception if not.
    /// </summary>
    /// <param name="directory">The directory to search in.</param>
    /// <exception cref="ArgumentException">Thrown if the parameters are not valid.</exception>
    public static void EnsureValidFindProjectFiles(string directory)
    {
        var problems = ValidateFindProjectFiles(directory);
        if (!problems.IsValid())
        {
            throw new ArgumentException("Find project files validation failed: " + string.Join(" ", problems));
        }
    }
}