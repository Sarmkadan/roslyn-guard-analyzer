#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RoslynGuardAnalyzer.Utilities;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public sealed class FileSystemHelperTests
{
    [Fact]
    public void FindCSharpFiles_ReturnsOnlyCsFilesAndRespectsExclusions()
    {
        // Arrange
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(root);
        try
        {
            // regular cs file
            var csFile = Path.Combine(root, "Program.cs");
            File.WriteAllText(csFile, "class Program {}");

            // file in excluded folder (bin)
            var binFolder = Path.Combine(root, "bin");
            Directory.CreateDirectory(binFolder);
            var csInBin = Path.Combine(binFolder, "Ignored.cs");
            File.WriteAllText(csInBin, "class Ignored {}");

            // non‑cs file
            var txtFile = Path.Combine(root, "readme.txt");
            File.WriteAllText(txtFile, "hello");

            // Act
            var result = FileSystemHelper.FindCSharpFiles(root);

            // Assert
            Assert.Single(result);
            Assert.Equal(csFile, result[0]);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void FindProjectFiles_ReturnsCsprojAndFsprojAndRespectsExclusions()
    {
        // Arrange
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(root);
        try
        {
            var csproj = Path.Combine(root, "MyApp.csproj");
            File.WriteAllText(csproj, "<Project></Project>");

            var fsproj = Path.Combine(root, "MyLib.fsproj");
            File.WriteAllText(fsproj, "<Project></Project>");

            var binFolder = Path.Combine(root, "bin");
            Directory.CreateDirectory(binFolder);
            var csprojInBin = Path.Combine(binFolder, "Ignored.csproj");
            File.WriteAllText(csprojInBin, "<Project></Project>");

            // Act
            var result = FileSystemHelper.FindProjectFiles(root);

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Contains(csproj, result);
            Assert.Contains(fsproj, result);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task ReadFileAsync_ExistingFile_ReturnsContent()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var expected = "Hello, world!";
        await File.WriteAllTextAsync(tempFile, expected);

        try
        {
            // Act
            var actual = await FileSystemHelper.ReadFileAsync(tempFile);

            // Assert
            Assert.Equal(expected, actual);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadFileAsync_NonExistingFile_ReturnsNull()
    {
        // Arrange
        var nonExisting = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "nope.txt");

        // Act
        var result = await FileSystemHelper.ReadFileAsync(nonExisting);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task WriteFileAsync_CreatesFileAndReturnsTrue()
    {
        // Arrange
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var filePath = Path.Combine(dir, "output.txt");
        var content = "some content";

        try
        {
            // Act
            var success = await FileSystemHelper.WriteFileAsync(filePath, content);

            // Assert
            Assert.True(success);
            Assert.True(File.Exists(filePath));
            var readBack = await File.ReadAllTextAsync(filePath);
            Assert.Equal(content, readBack);
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }

    [Fact]
    public async Task WriteFileAsync_InvalidPath_ReturnsFalse()
    {
        // Arrange
        // Using a path with an illegal character (e.g., '\0') forces an IOException.
        var invalidPath = "\0invalid.txt";
        var content = "data";

        // Act
        var result = await FileSystemHelper.WriteFileAsync(invalidPath, content);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void FileExists_NullOrNonExisting_ReturnsFalse()
    {
        // Null input
        Assert.False(FileSystemHelper.FileExists(null!));

        // Non‑existing path
        var nonExisting = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "none.txt");
        Assert.False(FileSystemHelper.FileExists(nonExisting));
    }

    [Fact]
    public void DirectoryExists_NullOrNonExisting_ReturnsFalse()
    {
        // Null input
        Assert.False(FileSystemHelper.DirectoryExists(null!));

        // Non‑existing directory
        var nonExisting = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Assert.False(FileSystemHelper.DirectoryExists(nonExisting));
    }

    [Fact]
    public void GetFileSize_ExistingAndMissingFile_BehavesAsExpected()
    {
        // Existing file
        var tempFile = Path.GetTempFileName();
        var data = new byte[123];
        new Random().NextBytes(data);
        File.WriteAllBytes(tempFile, data);

        try
        {
            var size = FileSystemHelper.GetFileSize(tempFile);
            Assert.Equal(data.Length, size);
        }
        finally
        {
            File.Delete(tempFile);
        }

        // Missing file
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "missing.txt");
        Assert.Equal(-1, FileSystemHelper.GetFileSize(missing));
    }

    [Fact]
    public void GetLastModifiedTime_ExistingAndMissingFile_BehavesAsExpected()
    {
        // Existing file
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "x");
        try
        {
            var modTime = FileSystemHelper.GetLastModifiedTime(tempFile);
            Assert.NotNull(modTime);
            // The returned time should be close to now (within a minute)
            Assert.InRange(modTime!.Value, DateTime.Now.AddMinutes(-1), DateTime.Now.AddMinutes(1));
        }
        finally
        {
            File.Delete(tempFile);
        }

        // Missing file
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "missing.txt");
        Assert.Null(FileSystemHelper.GetLastModifiedTime(missing));
    }
}
