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

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new RuleConfigurationBuilder(null!));
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new RuleConfigurationBuilder(""));
    }

    [Fact]
    public void Constructor_WithWhitespaceName_DoesNotThrow()
    {
        // ThrowIfNullOrEmpty does not check whitespace
        var builder = new RuleConfigurationBuilder("   ");
        var config = builder.Build();
        Assert.Equal("   ", config.Name);
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

    [Fact]
    public void WithParameter_NullKey_ThrowsArgumentNullException()
    {
        var builder = new RuleConfigurationBuilder("TestRule");
        Assert.Throws<ArgumentNullException>(() => builder.WithParameter(null!, 123));
    }

    [Fact]
    public void WithParameter_EmptyKey_ThrowsArgumentException()
    {
        var builder = new RuleConfigurationBuilder("TestRule");
        Assert.Throws<ArgumentException>(() => builder.WithParameter("", 123));
    }

    [Fact]
    public void WithParameter_WhitespaceKey_DoesNotThrow()
    {
        // ThrowIfNullOrEmpty does not check whitespace
        var builder = new RuleConfigurationBuilder("TestRule");
        var returned = builder.WithParameter("   ", 123);
        Assert.Same(builder, returned);
    }

    [Fact]
    public void WithParameters_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new RuleConfigurationBuilder("TestRule");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithParameters(null!));
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

        // CustomSettings is a public property
        Assert.Equal("False", config.CustomSettings["Enabled"]);
        Assert.Equal("Critical", config.CustomSettings["Severity"]);
        Assert.Equal("42", config.CustomSettings["ParamA"]);
        Assert.Equal("value", config.CustomSettings["ParamB"]);
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
