#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;
using RoslynGuardAnalyzer.Services;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="OutputWriterExtensions"/> covering format lookup, format validation and guarded async writes.
/// </summary>
public sealed class OutputWriterExtensionsTests
{
    [Fact]
    public void GetSupportedFormatsList_DefaultWriter_ReturnsDefaultFormatsAsReadOnlyList()
    {
        // Arrange
        var writer = new OutputWriter();

        // Act
        var formats = writer.GetSupportedFormatsList();

        // Assert
        formats.Should().BeAssignableTo<IReadOnlyList<string>>();
        formats.Select(f => f.ToLowerInvariant()).Should().BeEquivalentTo("json", "csv", "html", "sarif");
        Action add = () => ((IList<string>)formats).Add("xml");
        add.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void GetSupportedFormatsList_NullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        OutputWriter writer = null!;

        // Act
        Action act = () => writer.GetSupportedFormatsList();

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("writer");
    }

    [Theory]
    [InlineData("json")]
    [InlineData("JSON")]
    [InlineData("Sarif")]
    public void EnsureFormatSupported_SupportedFormat_DoesNotThrow(string format)
    {
        // Arrange
        var writer = new OutputWriter();

        // Act
        Action act = () => writer.EnsureFormatSupported(format);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureFormatSupported_UnsupportedFormat_ThrowsNotSupportedExceptionWithFormatInMessage()
    {
        // Arrange
        var writer = new OutputWriter();

        // Act
        Action act = () => writer.EnsureFormatSupported("xml");

        // Assert
        act.Should().ThrowExactly<NotSupportedException>()
            .WithMessage("*'xml'*");
    }

    [Fact]
    public void EnsureFormatSupported_NullFormat_ThrowsArgumentNullException()
    {
        // Arrange
        var writer = new OutputWriter();

        // Act
        Action act = () => writer.EnsureFormatSupported(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("format");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EnsureFormatSupported_BlankFormat_ThrowsArgumentException(string format)
    {
        // Arrange
        var writer = new OutputWriter();

        // Act
        Action act = () => writer.EnsureFormatSupported(format);

        // Assert
        act.Should().ThrowExactly<ArgumentException>().WithParameterName("format");
    }

    [Fact]
    public async Task WriteIfSupportedAsync_SupportedFormat_InvokesWriteDelegateOnce()
    {
        // Arrange
        var writer = new OutputWriter();
        var invocations = 0;
        Func<Task> write = () =>
        {
            invocations++;
            return Task.CompletedTask;
        };

        // Act
        await writer.WriteIfSupportedAsync("csv", write);

        // Assert
        invocations.Should().Be(1);
    }

    [Fact]
    public async Task WriteIfSupportedAsync_UnsupportedFormat_ThrowsWithoutInvokingDelegate()
    {
        // Arrange
        var writer = new OutputWriter();
        var invocations = 0;
        Func<Task> write = () =>
        {
            invocations++;
            return Task.CompletedTask;
        };

        // Act
        Func<Task> act = () => writer.WriteIfSupportedAsync("xml", write);

        // Assert
        await act.Should().ThrowAsync<NotSupportedException>();
        invocations.Should().Be(0);
    }

    [Fact]
    public async Task WriteIfSupportedAsync_NullWriteDelegate_ThrowsArgumentNullException()
    {
        // Arrange
        var writer = new OutputWriter();

        // Act
        Func<Task> act = () => writer.WriteIfSupportedAsync("json", null!);

        // Assert
        await act.Should().ThrowExactlyAsync<ArgumentNullException>().WithParameterName("writeAsync");
    }
}
