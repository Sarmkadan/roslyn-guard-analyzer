#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for CliArgumentParser bounds checking
// =============================================================================

using System;
using System.IO;
using FluentAssertions;
using RoslynGuardAnalyzer.Cli;
using Xunit;

namespace RoslynGuardAnalyzer.Tests.Cli;

public class CliArgumentParserBoundsTests
{
    [Fact]
    public void Parse_ResponseFile_ExpandsCorrectly()
    {
        // Arrange
        var responseFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(responseFile, "--verbose\n--project=test.csproj\n");

            var args = new[] { "@" + responseFile };
            var parser = new CliArgumentParser(args);

            // Act
            var options = parser.Parse();

            // Assert
            options.Should().NotBeNull();
            options.Verbose.Should().BeTrue();
            options.ProjectPath.Should().Be("test.csproj");
        }
        finally
        {
            File.Delete(responseFile);
        }
    }

    [Fact]
    public void Parse_ResponseFileWithComments_IgnoresComments()
    {
        // Arrange
        var responseFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(responseFile, "# This is a comment\n--verbose\n// Another comment\n--project=test.csproj\n");

            var args = new[] { "@" + responseFile };
            var parser = new CliArgumentParser(args);

            // Act
            var options = parser.Parse();

            // Assert
            options.Should().NotBeNull();
            options.Verbose.Should().BeTrue();
            options.ProjectPath.Should().Be("test.csproj");
        }
        finally
        {
            File.Delete(responseFile);
        }
    }

    [Fact]
    public void Parse_ResponseFileWithEmptyLines_IgnoresEmptyLines()
    {
        // Arrange
        var responseFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(responseFile, "--verbose\n\n\n--project=test.csproj\n");

            var args = new[] { "@" + responseFile };
            var parser = new CliArgumentParser(args);

            // Act
            var options = parser.Parse();

            // Assert
            options.Should().NotBeNull();
            options.Verbose.Should().BeTrue();
            options.ProjectPath.Should().Be("test.csproj");
        }
        finally
        {
            File.Delete(responseFile);
        }
    }

    [Fact]
    public void Parse_ResponseFileNotFound_ThrowsArgumentException()
    {
        // Arrange
        var args = new[] { "@nonexistent.txt" };
        var parser = new CliArgumentParser(args);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parser.Parse());
    }

    [Fact]
    public void Parse_ResponseFileTooLarge_ThrowsArgumentException()
    {
        // Arrange
        var responseFile = Path.GetTempFileName();
        try
        {
            // Create a file larger than MaxResponseFileSizeBytes (1MB)
            var largeContent = new string('x', 2_000_000);
            File.WriteAllText(responseFile, largeContent);

            var args = new[] { "@" + responseFile };
            var parser = new CliArgumentParser(args);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => parser.Parse());
        }
        finally
        {
            File.Delete(responseFile);
        }
    }

    [Fact]
    public void Parse_ResponseFileEmptyPath_ThrowsArgumentException()
    {
        // Arrange
        var args = new[] { "@" };
        var parser = new CliArgumentParser(args);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parser.Parse());
    }

    [Fact]
    public void Parse_CircularResponseFile_ThrowsArgumentException()
    {
        // Arrange
        var responseFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(responseFile, "@" + responseFile);

            var args = new[] { "@" + responseFile };
            var parser = new CliArgumentParser(args);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => parser.Parse());
        }
        finally
        {
            File.Delete(responseFile);
        }
    }

    [Fact]
    public void Parse_DuplicateResponseFile_HandledGracefully()
    {
        // Arrange
        var responseFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(responseFile, "--verbose\n--project=test.csproj");

            // File referenced twice - should only process once
            var args = new[] { "@" + responseFile, "@" + responseFile };
            var parser = new CliArgumentParser(args);

            // Act
            var options = parser.Parse();

            // Assert
            options.Should().NotBeNull();
            options.Verbose.Should().BeTrue();
            options.ProjectPath.Should().Be("test.csproj");
        }
        finally
        {
            File.Delete(responseFile);
        }
    }

    [Fact]
    public void Parse_ResponseFileNestedExpansion_WorksCorrectly()
    {
        // Arrange
        var responseFile1 = Path.GetTempFileName();
        var responseFile2 = Path.GetTempFileName();
        try
        {
            File.WriteAllText(responseFile1, "--verbose\n@" + responseFile2);
            File.WriteAllText(responseFile2, "--project=nested.csproj\n--format=json");

            var args = new[] { "@" + responseFile1 };
            var parser = new CliArgumentParser(args);

            // Act
            var options = parser.Parse();

            // Assert
            options.Should().NotBeNull();
            options.Verbose.Should().BeTrue();
            options.ProjectPath.Should().Be("nested.csproj");
            options.OutputFormat.Should().Be("json");
        }
        finally
        {
            File.Delete(responseFile1);
            File.Delete(responseFile2);
        }
    }

    [Fact]
    public void Parse_ResponseFileDeepRecursion_ThrowsArgumentException()
    {
        // Arrange - create a chain of response files that exceeds MaxResponseFileRecursionDepth (50)
        var responseFiles = new string[60];
        for (int i = 0; i < responseFiles.Length; i++)
        {
            responseFiles[i] = Path.GetTempFileName();
        }

        try
        {
            // Create a chain: file0 -> file1 -> file2 -> ... -> file59 -> "--project=test.csproj"
            for (int i = 0; i < responseFiles.Length - 1; i++)
            {
                File.WriteAllText(responseFiles[i], "@" + responseFiles[i + 1]);
            }
            File.WriteAllText(responseFiles[responseFiles.Length - 1], "--project=test.csproj");

            var args = new[] { "@" + responseFiles[0] };
            var parser = new CliArgumentParser(args);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => parser.Parse());
        }
        finally
        {
            foreach (var file in responseFiles)
            {
                File.Delete(file);
            }
        }
    }

    [Fact]
    public void Parse_TooManyArguments_ThrowsArgumentException()
    {
        // Arrange
        var args = new string[15_000];
        for (int i = 0; i < args.Length; i++)
        {
            args[i] = "--verbose";
        }

        var parser = new CliArgumentParser(args);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parser.Parse());
    }

    [Fact]
    public void Parse_LargeTotalArgumentLength_ThrowsArgumentException()
    {
        // Arrange
        var largeArg = new string('x', 2_000_000); // 2MB argument
        var args = new[] { largeArg };
        var parser = new CliArgumentParser(args);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parser.Parse());
    }

    [Fact]
    public void ParseSafe_ResponseFileError_ReturnsHelpOptions()
    {
        // Arrange
        var args = new[] { "@nonexistent.txt" };

        // Act
        var options = CliArgumentParser.ParseSafe(args);

        // Assert
        options.Should().NotBeNull();
        options.ShowHelp.Should().BeTrue();
    }

    [Fact]
    public void Parse_ResponseFileWithMixedArgs_ExpandsCorrectly()
    {
        // Arrange
        var responseFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(responseFile, "--format=json\n--timeout=300");

            var args = new[] { "--verbose", "@" + responseFile, "--project=test.csproj" };
            var parser = new CliArgumentParser(args);

            // Act
            var options = parser.Parse();

            // Assert
            options.Should().NotBeNull();
            options.Verbose.Should().BeTrue();
            options.ProjectPath.Should().Be("test.csproj");
            options.OutputFormat.Should().Be("json");
            options.AnalysisTimeoutSeconds.Should().Be(300);
        }
        finally
        {
            File.Delete(responseFile);
        }
    }

    [Fact]
    public void Parse_ResponseFileWithWindowsLineEndings_ExpandsCorrectly()
    {
        // Arrange
        var responseFile = Path.GetTempFileName();
        try
        {
            // Windows line endings (CRLF)
            File.WriteAllText(responseFile, "--verbose\r\n--project=test.csproj\r\n");

            var args = new[] { "@" + responseFile };
            var parser = new CliArgumentParser(args);

            // Act
            var options = parser.Parse();

            // Assert
            options.Should().NotBeNull();
            options.Verbose.Should().BeTrue();
            options.ProjectPath.Should().Be("test.csproj");
        }
        finally
        {
            File.Delete(responseFile);
        }
    }

    [Fact]
    public void Parse_ResponseFileWithUnixLineEndings_ExpandsCorrectly()
    {
        // Arrange
        var responseFile = Path.GetTempFileName();
        try
        {
            // Unix line endings (LF)
            File.WriteAllText(responseFile, "--verbose\n--project=test.csproj\n");

            var args = new[] { "@" + responseFile };
            var parser = new CliArgumentParser(args);

            // Act
            var options = parser.Parse();

            // Assert
            options.Should().NotBeNull();
            options.Verbose.Should().BeTrue();
            options.ProjectPath.Should().Be("test.csproj");
        }
        finally
        {
            File.Delete(responseFile);
        }
    }
}