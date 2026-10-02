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
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(root);
        try
        {
            var csFile = Path.Combine(root, "Program.cs");
            File.WriteAllText(csFile, "class Program {}");

            var binFolder = Path.Combine(root, "bin");
            Directory.CreateDirectory(binFolder);
            var csInBin = Path.Combine(binFolder, "Ignored.cs");
            File.WriteAllText(csInBin, "class Ignored {}");

            var txtFile = Path.Combine(root, "readme.txt");
            File.WriteAllText(txtFile, "hello");

            var result = FileSystemHelper.FindCSharpFiles(root);

            Assert.Single(result);
            Assert.Equal(csFile, result.First());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task ReadFileAsync_ExistingFile_ReturnsContent()
    {
        var tempFile = Path.GetTempFileName();
        var expected = "hello world\nline2";
        await File.WriteAllTextAsync(tempFile, expected);
        try
        {
            var content = await FileSystemHelper.ReadFileAsync(tempFile);
            Assert.Equal(expected, content);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadFileAsync_NonExistingFile_ThrowsDirectoryNotFoundException()
    {
        // Parent directory doesn't exist, so DirectoryNotFoundException is thrown (not caught by FileNotFoundException handler)
        var nonExisting = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "nope.txt");
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => FileSystemHelper.ReadFileAsync(nonExisting));
    }

    [Fact]
    public async Task WriteFileAsync_CreatesFileAndReturnsTrue()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var filePath = Path.Combine(dir, "output.txt");
        var content = "some content";

        try
        {
            var success = await FileSystemHelper.WriteFileAsync(filePath, content);
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
    public async Task WriteFileAsync_InvalidPath_ThrowsArgumentException()
    {
        // Path with null character causes ArgumentException from Path.GetFullPath before try/catch
        var invalidPath = "\0invalid.txt";
        var content = "data";
        await Assert.ThrowsAsync<ArgumentException>(() => FileSystemHelper.WriteFileAsync(invalidPath, content));
    }

    [Fact]
    public void FileExists_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => FileSystemHelper.FileExists(null!));
    }

    [Fact]
    public void FileExists_NonExistingPath_ReturnsFalse()
    {
        var nonExisting = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "none.txt");
        Assert.False(FileSystemHelper.FileExists(nonExisting));
    }

    [Fact]
    public void DirectoryExists_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => FileSystemHelper.DirectoryExists(null!));
    }

    [Fact]
    public void DirectoryExists_NonExistingPath_ReturnsFalse()
    {
        var nonExisting = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Assert.False(FileSystemHelper.DirectoryExists(nonExisting));
    }

    [Fact]
    public void GetFileSize_ExistingAndMissingFile_BehavesAsExpected()
    {
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

        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "missing.txt");
        Assert.Equal(-1, FileSystemHelper.GetFileSize(missing));
    }

    [Fact]
    public void GetLastModifiedTime_ExistingFile_ReturnsRecentTime()
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "x");
        try
        {
            var modTime = FileSystemHelper.GetLastModifiedTime(tempFile);
            Assert.NotNull(modTime);
            Assert.InRange(modTime!.Value, DateTime.Now.AddMinutes(-1), DateTime.Now.AddMinutes(1));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void GetLastModifiedTime_MissingFile_ReturnsNonNull()
    {
        // File.GetLastWriteTime for non-existent file returns a default date, doesn't throw
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
        var result = FileSystemHelper.GetLastModifiedTime(missing);
        // Returns a DateTime (epoch-like), not null
        Assert.NotNull(result);
    }
}
