using System;
using System.Text.Json;
using RoslynGuardAnalyzer.Data;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class RuleRepositoryJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        var repository = new RuleRepository();
        var json = repository.ToJson();

        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.Contains("{", json);
        Assert.Contains("}", json);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        RuleRepository? repository = null;

        Assert.Throws<ArgumentNullException>(() => repository!.ToJson());
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsInstance()
    {
        var repository = new RuleRepository();
        var json = repository.ToJson();
        var result = RuleRepositoryJsonExtensions.FromJson(json);

        Assert.NotNull(result);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => RuleRepositoryJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void FromJson_EmptyOrWhiteSpace_ReturnsNull()
    {
        var empty = RuleRepositoryJsonExtensions.FromJson("");
        var whitespace = RuleRepositoryJsonExtensions.FromJson("   ");

        Assert.Null(empty);
        Assert.Null(whitespace);
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndInstance()
    {
        var repository = new RuleRepository();
        var json = repository.ToJson();
        var success = RuleRepositoryJsonExtensions.TryFromJson(json, out var result);

        Assert.True(success);
        Assert.NotNull(result);
    }

    [Fact]
    public void TryFromJson_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => RuleRepositoryJsonExtensions.TryFromJson(null!, out _));
    }

    [Fact]
    public void TryFromJson_EmptyOrWhiteSpace_ReturnsFalse()
    {
        var successEmpty = RuleRepositoryJsonExtensions.TryFromJson("", out var resultEmpty);
        var successWhitespace = RuleRepositoryJsonExtensions.TryFromJson("   ", out var resultWhitespace);

        Assert.False(successEmpty);
        Assert.False(successWhitespace);
        Assert.Null(resultEmpty);
        Assert.Null(resultWhitespace);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        var success = RuleRepositoryJsonExtensions.TryFromJson("{invalid json}", out var result);

        Assert.False(success);
        Assert.Null(result);
    }
}
