#nullable enable

using System;
using Xunit;
using FluentAssertions;
using RoslynGuardAnalyzer.Caching;

namespace RoslynGuardAnalyzer.Tests;

public class CacheKeyGeneratorTests
{
    [Theory]
    [InlineData("test", "9f86d081884c7d65")]
    [InlineData("", "empty")]
    [InlineData(null!, "empty")]
    public void ComputeHash_ShouldReturnExpectedFormat(string input, string expected)
    {
        // Act
        var result = CacheKeyGenerator.ComputeHash(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GenerateProjectAnalysisKey_ShouldReturnKeyWithCorrectPrefix()
    {
        // Act
        var result = CacheKeyGenerator.GenerateProjectAnalysisKey("/path/to/project", "configHash");

        // Assert
        result.Should().StartWith("analysis_project_");
        result.Should().Contain("_configHash");
    }

    [Fact]
    public void GenerateFileAnalysisKey_WithNullContentHash_ShouldReturnKeyWithEmptyContent()
    {
        // Act
        var result = CacheKeyGenerator.GenerateFileAnalysisKey("/path/to/file.cs", null);

        // Assert
        result.Should().StartWith("analysis_file_");
        result.Should().EndWith("_");
    }

    [Fact]
    public void GenerateRuleExecutionKey_ShouldReturnKeyWithCorrectFormat()
    {
        // Act
        var result = CacheKeyGenerator.GenerateRuleExecutionKey("RuleA", "TargetB");

        // Assert
        result.Should().StartWith("rule_exec_");
        result.Split('_').Should().HaveCount(4); // rule_exec_hash_hash
    }

    [Fact]
    public void GenerateCodeElementKey_WithoutMember_ShouldReturnCorrectFormat()
    {
        // Act
        var result = CacheKeyGenerator.GenerateCodeElementKey("MyType");

        // Assert
        result.Should().StartWith("element_");
        // Ensure there is only one underscore
        result.Should().NotContain("__");
    }

    [Fact]
    public void CreateCompositeKey_WithMultipleComponents_ShouldReturnHash()
    {
        // Act
        var result = CacheKeyGenerator.CreateCompositeKey("comp1", "comp2");

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CreateCompositeKey_WithNoComponents_ThrowsArgumentException()
    {
        // Act
        Action act = () => CacheKeyGenerator.CreateCompositeKey(Array.Empty<string>());

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GeneratePatternKey_ShouldReturnPattern()
    {
        // Act
        var result = CacheKeyGenerator.GeneratePatternKey("test");

        // Assert
        result.Should().Be("test_*");
    }
}
