using System;
using System.Collections.Generic;
using System.IO;
using RoslynGuardAnalyzer.Utilities;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class FileSystemHelperValidationTests
{
    [Fact]
    public void ValidateDirectory_HappyPath_ReturnsNoProblems()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var problems = FileSystemHelperValidation.ValidateDirectory(tempDir);

        Assert.Empty(problems);
    }

    [Fact]
    public void ValidateDirectory_NonExistent_ReturnsDirectoryDoesNotExistProblem()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        var problems = FileSystemHelperValidation.ValidateDirectory(tempDir);

        Assert.Contains($"Directory does not exist: '{tempDir}'.", problems);
    }

    [Fact]
    public void ValidateDirectory_RootPath_ReturnsRootPathProblem()
    {
        var root = Path.GetPathRoot(Path.GetTempPath());

        var problems = FileSystemHelperValidation.ValidateDirectory(root);

        Assert.Contains("Root directory paths are not supported for file operations.", problems);
    }

    [Fact]
    public void ValidateDirectory_AdditionalExclusionsWithWhitespace_AddsProblem()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var problems = FileSystemHelperValidation.ValidateDirectory(tempDir, new[] { "   " });

        Assert.Contains("Additional exclusion pattern cannot be null or whitespace.", problems);
    }

    [Fact]
    public void ValidateFileExists_HappyPath_ReturnsNoProblems()
    {
        var tempFile = Path.GetTempFileName();

        var problems = FileSystemHelperValidation.ValidateFileExists(tempFile);

        Assert.Empty(problems);
    }

    [Fact]
    public void ValidateFileExists_RootPath_ReturnsRootPathProblem()
    {
        var root = Path.GetPathRoot(Path.GetTempPath());

        var problems = FileSystemHelperValidation.ValidateFileExists(root);

        Assert.Contains("Root directory paths are not valid file paths.", problems);
    }

    [Fact]
    public void ValidateWriteFile_HappyPath_ReturnsNoProblems()
    {
        var tempFile = Path.GetTempFileName();
        var content = "test";

        var problems = FileSystemHelperValidation.ValidateWriteFile(tempFile, content);

        Assert.Empty(problems);
    }

    [Fact]
    public void ValidateWriteFile_NullContent_ThrowsArgumentNullException()
    {
        var tempFile = Path.GetTempFileName();

        Assert.Throws<ArgumentNullException>(() => FileSystemHelperValidation.ValidateWriteFile(tempFile, null!));
    }

    [Fact]
    public void ValidateWriteFile_RootPath_ReturnsRootPathProblem()
    {
        var root = Path.GetPathRoot(Path.GetTempPath());

        var problems = FileSystemHelperValidation.ValidateWriteFile(root, "content");

        Assert.Contains("Root directory paths are not valid file paths.", problems);
    }

    [Fact]
    public void IsValid_ReturnsTrueForEmptyListAndFalseForNonEmpty()
    {
        IReadOnlyList<string> empty = Array.Empty<string>();
        IReadOnlyList<string> nonEmpty = new List<string> { "problem" };

        Assert.True(empty.IsValid());
        Assert.False(nonEmpty.IsValid());
    }
}
