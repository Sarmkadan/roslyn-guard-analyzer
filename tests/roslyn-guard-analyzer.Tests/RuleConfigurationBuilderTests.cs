#nullable enable
using System;
using System.Collections.Generic;
using RoslynGuardAnalyzer.Configuration;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public sealed class RuleConfigurationBuilderTests
{
    [Fact]
    public void Constructor_WithValidName_SetsName()
    {
        // Arrange
        const string ruleName = "MyRule";

        // Act
        var builder = new RuleConfigurationBuilder(ruleName);
        var config = builder.Build();

        // Assert
        Assert.Equal(ruleName, config.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsArgumentException(string? name)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RuleConfigurationBuilder(name!));
    }

    [Fact]
    public void WithSeverity_ValidValue_ReturnsSameBuilder()
    {
        // Arrange
        var builder = new RuleConfigurationBuilder("TestRule");

        // Act
        var returned = builder.WithSeverity("High");

        // Assert
        Assert.Same(builder, returned);
    }

    [Theory]
    [InlineData("VeryLow")]
    [InlineData("criticality")]
    [InlineData("")]
    public void WithSeverity_InvalidValue_ThrowsArgumentException(string severity)
    {
        // Arrange
        var builder = new RuleConfigurationBuilder("TestRule");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithSeverity(severity));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithParameter_NullOrEmptyKey_ThrowsArgumentException(string? key)
    {
        // Arrange
        var builder = new RuleConfigurationBuilder("TestRule");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithParameter(key!, 123));
    }

    [Fact]
    public void WithParameters_Null_DoesNotThrowAndKeepsBuilder()
    {
        // Arrange
        var builder = new RuleConfigurationBuilder("TestRule");

        // Act
        var returned = builder.WithParameters(null!);

        // Assert
        Assert.Same(builder, returned);
    }

    [Fact]
    public void Build_WithAllOptions_PopulatesConfiguration()
    {
        // Arrange
        var builder = new RuleConfigurationBuilder("ComplexRule")
            .WithEnabled(false)
            .WithSeverity("Critical")
            .WithDescription("A complex rule")
            .WithParameter("ParamA", 42)
            .WithParameter("ParamB", "value");

        // Act
        RuleConfiguration config = builder.Build();

        // Assert
        Assert.Equal("ComplexRule", config.Name);
        Assert.Equal("A complex rule", config.Description);

        // The custom settings are stored as strings; we verify via the public API.
        // RuleConfiguration exposes SetCustomSetting, which stores values as strings.
        // We can retrieve them using reflection if no getter exists.
        var settingsField = typeof(RuleConfiguration).GetField("_customSettings",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(settingsField);

        var dict = settingsField!.GetValue(config) as IDictionary<string, string>;
        Assert.NotNull(dict);
        Assert.Equal("False", dict!["Enabled"]);
        Assert.Equal("Critical", dict!["Severity"]);
        Assert.Equal("42", dict!["ParamA"]);
        Assert.Equal("value", dict!["ParamB"]);
    }

    [Fact]
    public void CreateNamingConvention_ReturnsBuilderWithExpectedName()
    {
        // Act
        var builder = RuleConfigurationBuilder.CreateNamingConvention();
        var config = builder.Build();

        // Assert
        Assert.Equal("NamingConvention", config.Name);
        Assert.Equal("Enforces C# naming conventions", config.Description);
    }

    [Fact]
    public void CreateLayerDependency_ReturnsBuilderWithExpectedName()
    {
        // Act
        var builder = RuleConfigurationBuilder.CreateLayerDependency();
        var config = builder.Build();

        // Assert
        Assert.Equal("LayerDependency", config.Name);
        Assert.Equal("Enforces architectural layer dependencies", config.Description);
    }

    [Fact]
    public void CreateAsyncPatterns_ReturnsBuilderWithExpectedName()
    {
        // Act
        var builder = RuleConfigurationBuilder.CreateAsyncPatterns();
        var config = builder.Build();

        // Assert
        Assert.Equal("AsyncPatterns", config.Name);
        Assert.Equal("Validates async/await usage patterns", config.Description);
    }
}
