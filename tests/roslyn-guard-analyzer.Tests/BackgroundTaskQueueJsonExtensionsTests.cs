using System;
using System.Text.Json;
using RoslynGuardAnalyzer.Services;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class BackgroundTaskQueueJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        var queue = new BackgroundTaskQueue();
        var json = queue.ToJson();

        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.Contains("{", json);
        Assert.Contains("}", json);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        BackgroundTaskQueue? queue = null;

        Assert.Throws<ArgumentNullException>(() => queue!.ToJson());
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsInstance()
    {
        var queue = new BackgroundTaskQueue();
        var json = queue.ToJson();
        var result = BackgroundTaskQueueJsonExtensions.FromJson(json);

        Assert.NotNull(result);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => BackgroundTaskQueueJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void FromJson_EmptyOrWhiteSpace_ReturnsNull()
    {
        var empty = BackgroundTaskQueueJsonExtensions.FromJson("");
        var whitespace = BackgroundTaskQueueJsonExtensions.FromJson("   ");

        Assert.Null(empty);
        Assert.Null(whitespace);
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndInstance()
    {
        var queue = new BackgroundTaskQueue();
        var json = queue.ToJson();
        var success = BackgroundTaskQueueJsonExtensions.TryFromJson(json, out var result);

        Assert.True(success);
        Assert.NotNull(result);
    }

    [Fact]
    public void TryFromJson_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => BackgroundTaskQueueJsonExtensions.TryFromJson(null!, out _));
    }

    [Fact]
    public void TryFromJson_EmptyOrWhiteSpace_ReturnsFalse()
    {
        var successEmpty = BackgroundTaskQueueJsonExtensions.TryFromJson("", out var resultEmpty);
        var successWhitespace = BackgroundTaskQueueJsonExtensions.TryFromJson("   ", out var resultWhitespace);

        Assert.False(successEmpty);
        Assert.False(successWhitespace);
        Assert.Null(resultEmpty);
        Assert.Null(resultWhitespace);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        var success = BackgroundTaskQueueJsonExtensions.TryFromJson("{invalid json}", out var result);

        Assert.False(success);
        Assert.Null(result);
    }
}
