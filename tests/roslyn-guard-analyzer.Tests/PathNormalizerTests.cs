#nullable enable

using System;
using System.IO;

using FluentAssertions;
using RoslynGuardAnalyzer.Utilities;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="PathNormalizer"/> covering normalization, comparison, relative paths and path parts.
/// </summary>
public sealed class PathNormalizerTests
{
    [Fact]
    public void Normalize_ResolvesSegmentsAndUsesForwardSlashes()
    {
        // Arrange
        var input = Path.Combine("proj", ".", "src", "..", "lib", "A.cs");

        // Act
        var result = PathNormalizer.Normalize(input);

        // Assert
        result.Should().EndWith("/proj/lib/A.cs");
        result.Should().NotContain("/./");
        result.Should().NotContain("/../");
        result.Should().NotContain("\\");
        PathNormalizer.IsAbsolute(result).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_NullOrWhiteSpace_ThrowsArgumentException(string? path)
    {
        // Arrange
        var act = () => PathNormalizer.Normalize(path!);

        // Act & Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NormalizeMany_NormalizesEntriesInOrder_AndAcceptsNoEntries()
    {
        // Arrange
        var first = Path.Combine("a");
        var second = Path.Combine("b", "..", "c");

        // Act
        var result = PathNormalizer.NormalizeMany(first, second);
        var empty = PathNormalizer.NormalizeMany();

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().Be(PathNormalizer.Normalize(first));
        result[1].Should().Be(PathNormalizer.Normalize(second));
        empty.Should().BeEmpty();
    }

    [Fact]
    public void NormalizeMany_NullArrayOrNullEntry_ThrowsWithParameterName()
    {
        // Arrange
        var nullArray = () => PathNormalizer.NormalizeMany((string[])null!);
        var nullEntry = () => PathNormalizer.NormalizeMany("a", null!);

        // Act & Assert
        nullArray.Should().Throw<ArgumentNullException>().WithParameterName("paths");
        nullEntry.Should().Throw<ArgumentException>().WithParameterName("paths");
    }

    [Fact]
    public void ArePathsEqual_ComparesNormalizedPaths()
    {
        // Arrange
        var direct = Path.Combine("proj", "src", "A.cs");
        var indirect = Path.Combine("proj", "x", "..", "src", "A.cs");
        var other = Path.Combine("proj", "src", "B.cs");

        // Act & Assert
        PathNormalizer.ArePathsEqual(direct, indirect).Should().BeTrue();
        PathNormalizer.ArePathsEqual(direct, other).Should().BeFalse();
        var act = () => PathNormalizer.ArePathsEqual(direct, " ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetRelativePath_ReturnsPathRelativeToBase()
    {
        // Arrange
        var root = Path.GetFullPath("proj");
        var srcDir = Path.Combine(root, "src");

        // Act & Assert
        PathNormalizer.GetRelativePath(root, Path.Combine(root, "src", "A.cs"))
            .Should().Be(Path.Combine("src", "A.cs"));
        PathNormalizer.GetRelativePath(root, root).Should().Be(".");
        PathNormalizer.GetRelativePath(srcDir, Path.Combine(root, "lib", "B.cs"))
            .Should().Be(Path.Combine("..", "lib", "B.cs"));
    }

    [Fact]
    public void IsAbsolute_And_Combine_HandleRootedAndSegmentedPaths()
    {
        // Arrange
        var absolute = Path.GetFullPath("src");

        // Act & Assert
        PathNormalizer.IsAbsolute(absolute).Should().BeTrue();
        PathNormalizer.IsAbsolute(Path.Combine("src", "Program.cs")).Should().BeFalse();

        PathNormalizer.Combine().Should().BeEmpty();
        PathNormalizer.Combine("src", "..", "lib").Should().EndWith("/lib").And.NotContain("/src/");
        var act = () => PathNormalizer.Combine("src", "  ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetFileNameAndExtension_SplitPathParts_AndHasExtensionIsCaseInsensitive()
    {
        // Arrange
        var file = Path.Combine("dir", "Program.CS");

        // Act & Assert
        PathNormalizer.GetFileName(file).Should().Be("Program.CS");
        PathNormalizer.GetFileName("dir/").Should().BeEmpty();
        PathNormalizer.GetExtension("archive.tar.gz").Should().Be(".gz");
        PathNormalizer.GetExtension("Makefile").Should().BeEmpty();
        PathNormalizer.GetDirectoryName(file).Should().Be(PathNormalizer.Normalize("dir"));
        PathNormalizer.HasExtension(file, ".cs").Should().BeTrue();
        PathNormalizer.HasExtension(file, "cs").Should().BeFalse();
        PathNormalizer.HasExtension("Makefile", ".cs").Should().BeFalse();
    }
}
