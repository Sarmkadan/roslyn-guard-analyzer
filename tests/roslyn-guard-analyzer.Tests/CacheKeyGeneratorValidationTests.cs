using System;
using System.Collections.Generic;
using RoslynGuardAnalyzer.Caching;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class CacheKeyGeneratorValidationTests
{
    // ---------- ValidateGenerateProjectAnalysisKey ----------
    [Fact]
    public void ValidateGenerateProjectAnalysisKey_HappyPath_ReturnsEmpty()
    {
        var result = CacheKeyGeneratorValidation.ValidateGenerateProjectAnalysisKey("C:\\proj.csproj");
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateGenerateProjectAnalysisKey_WhitespaceConfigHash_ReturnsProblem()
    {
        var result = CacheKeyGeneratorValidation.ValidateGenerateProjectAnalysisKey(
            "C:\\proj.csproj",
            "   ");
        Assert.Single(result);
        Assert.Contains("Configuration hash cannot be whitespace", result);
    }

    // ---------- ValidateGenerateFileAnalysisKey ----------
    [Fact]
    public void ValidateGenerateFileAnalysisKey_HappyPath_ReturnsEmpty()
    {
        var result = CacheKeyGeneratorValidation.ValidateGenerateFileAnalysisKey("C:\\file.cs");
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateGenerateFileAnalysisKey_WhitespaceFileContentHash_ReturnsProblem()
    {
        var result = CacheKeyGeneratorValidation.ValidateGenerateFileAnalysisKey(
            "C:\\file.cs",
            "  ");
        Assert.Single(result);
        Assert.Contains("File content hash cannot be whitespace", result);
    }

    // ---------- ValidateGenerateResultKey ----------
    [Fact]
    public void ValidateGenerateResultKey_HappyPath_ReturnsEmpty()
    {
        var result = CacheKeyGeneratorValidation.ValidateGenerateResultKey("analysis-123");
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateGenerateResultKey_NullOrEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CacheKeyGeneratorValidation.ValidateGenerateResultKey(null!));
        Assert.Throws<ArgumentException>(() =>
            CacheKeyGeneratorValidation.ValidateGenerateResultKey(string.Empty));
    }

    // ---------- ValidateGenerateRuleExecutionKey ----------
    [Fact]
    public void ValidateGenerateRuleExecutionKey_HappyPath_ReturnsEmpty()
    {
        var result = CacheKeyGeneratorValidation.ValidateGenerateRuleExecutionKey("RuleA", "TargetB");
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateGenerateRuleExecutionKey_NullOrEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CacheKeyGeneratorValidation.ValidateGenerateRuleExecutionKey(null!, "Target"));
        Assert.Throws<ArgumentException>(() =>
            CacheKeyGeneratorValidation.ValidateGenerateRuleExecutionKey(string.Empty, "Target"));
        Assert.Throws<ArgumentException>(() =>
            CacheKeyGeneratorValidation.ValidateGenerateRuleExecutionKey("Rule", null!));
        Assert.Throws<ArgumentException>(() =>
            CacheKeyGeneratorValidation.ValidateGenerateRuleExecutionKey("Rule", string.Empty));
    }

    // ---------- ValidateGenerateCodeElementKey ----------
    [Fact]
    public void ValidateGenerateCodeElementKey_HappyPath_ReturnsEmpty()
    {
        var result = CacheKeyGeneratorValidation.ValidateGenerateCodeElementKey("MyNamespace.MyClass");
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateGenerateCodeElementKey_WhitespaceMemberName_ReturnsProblem()
    {
        var result = CacheKeyGeneratorValidation.ValidateGenerateCodeElementKey(
            "MyNamespace.MyClass",
            "   ");
        Assert.Single(result);
        Assert.Contains("Member name cannot be whitespace", result);
    }

    // ---------- ValidateComputeHash ----------
    [Fact]
    public void ValidateComputeHash_HappyPath_ReturnsEmpty()
    {
        var result = CacheKeyGeneratorValidation.ValidateComputeHash("some input");
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateComputeHash_WhitespaceInput_ReturnsProblem()
    {
        var result = CacheKeyGeneratorValidation.ValidateComputeHash("   ");
        Assert.Single(result);
        Assert.Contains("Input cannot be whitespace", result);
    }

    // ---------- ValidateComputeFileHash ----------
    [Fact]
    public void ValidateComputeFileHash_HappyPath_ReturnsEmpty()
    {
        var result = CacheKeyGeneratorValidation.ValidateComputeFileHash("C:\\file.cs");
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateComputeFileHash_NullOrEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CacheKeyGeneratorValidation.ValidateComputeFileHash(null!));
        Assert.Throws<ArgumentException>(() =>
            CacheKeyGeneratorValidation.ValidateComputeFileHash(string.Empty));
    }

    // ---------- ValidateCreateCompositeKey ----------
    [Fact]
    public void ValidateCreateCompositeKey_HappyPath_ReturnsEmpty()
    {
        var result = CacheKeyGeneratorValidation.ValidateCreateCompositeKey("a", "b", "c");
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateCreateCompositeKey_NullOrEmptyComponents_ReturnsProblem()
    {
        var result = CacheKeyGeneratorValidation.ValidateCreateCompositeKey();
        Assert.Single(result);
        Assert.Contains("At least one component is required", result);
    }

    [Fact]
    public void ValidateCreateCompositeKey_WhitespaceComponent_ReturnsProblem()
    {
        var result = CacheKeyGeneratorValidation.ValidateCreateCompositeKey("valid", "   ", "also");
        Assert.Single(result);
        Assert.Contains("Component at index 1 cannot be null, empty, or whitespace", result);
    }

    // ---------- ValidateGeneratePatternKey ----------
    [Fact]
    public void ValidateGeneratePatternKey_HappyPath_ReturnsEmpty()
    {
        var result = CacheKeyGeneratorValidation.ValidateGeneratePatternKey("prefix");
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateGeneratePatternKey_NullOrEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CacheKeyGeneratorValidation.ValidateGeneratePatternKey(null!));
        Assert.Throws<ArgumentException>(() =>
            CacheKeyGeneratorValidation.ValidateGeneratePatternKey(string.Empty));
    }
}
