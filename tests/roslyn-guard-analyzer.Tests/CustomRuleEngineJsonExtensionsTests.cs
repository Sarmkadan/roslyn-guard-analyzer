using System;
using System.Runtime.Serialization;
using Xunit;
using RoslynGuardAnalyzer.Rules;

namespace RoslynGuardAnalyzer.Tests;

public class CustomRuleEngineJsonExtensionsTests
{
    [Fact]
    public void ToJson_NullValue_ThrowsArgumentNullException()
    {
        CustomRuleEngine? engine = null;
        Assert.Throws<ArgumentNullException>(() => engine!.ToJson());
    }

    [Fact]
    public void ToJson_ValidEngine_ReturnsJson()
    {
        // Create an instance without invoking the constructor (which requires a registry)
        var engine = (CustomRuleEngine)FormatterServices.GetUninitializedObject(typeof(CustomRuleEngine));

        var json = engine.ToJson();

        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.StartsWith("{", json.Trim());
    }

    [Fact]
    public void FromJson_NullOrEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CustomRuleEngineJsonExtensions.FromJson(null!));
        Assert.Throws<ArgumentException>(() => CustomRuleEngineJsonExtensions.FromJson(string.Empty));
        Assert.Throws<ArgumentException>(() => CustomRuleEngineJsonExtensions.FromJson("   "));
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsEngine()
    {
        var json = "{}";
        var engine = CustomRuleEngineJsonExtensions.FromJson(json);
        Assert.NotNull(engine);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        var invalidJson = "{ invalid json }";
        var result = CustomRuleEngineJsonExtensions.TryFromJson(invalidJson, out var engine);
        Assert.False(result);
        Assert.Null(engine);
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrue()
    {
        var json = "{}";
        var result = CustomRuleEngineJsonExtensions.TryFromJson(json, out var engine);
        Assert.True(result);
        Assert.NotNull(engine);
    }
}
