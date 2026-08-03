using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RoslynGuardAnalyzer.Configuration;
using RoslynGuardAnalyzer.Cli;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class ConfigurationValidatorTests
{
    #region ValidationResult tests

    [Fact]
    public void ValidationResult_AddError_SetsIsValidFalseAndStoresMessage()
    {
        var result = new ConfigurationValidator.ValidationResult { IsValid = true };
        result.AddError("sample error");

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("sample error", result.Errors[0]);
    }

    [Fact]
    public void ValidationResult_AddWarning_DoesNotChangeIsValid()
    {
        var result = new ConfigurationValidator.ValidationResult { IsValid = true };
        result.AddWarning("sample warning");

        Assert.True(result.IsValid);
        Assert.Single(result.Warnings);
        Assert.Equal("sample warning", result.Warnings[0]);
    }

    [Fact]
    public void ValidationResult_ToString_FormatsValidAndInvalidOutputs()
    {
        var valid = new ConfigurationValidator.ValidationResult { IsValid = true };
        var invalid = new ConfigurationValidator.ValidationResult { IsValid = false };
        invalid.AddError("err1");
        invalid.AddWarning("warn1");

        var validStr = valid.ToString();
        var invalidStr = invalid.ToString();

        Assert.Contains("✓ Configuration is valid", validStr);
        Assert.Contains("✗ Configuration has errors", invalidStr);
        Assert.Contains("Errors (1):", invalidStr);
        Assert.Contains("- err1", invalidStr);
        Assert.Contains("Warnings (1):", invalidStr);
        Assert.Contains("! warn1", invalidStr);
    }

    #endregion

    #region ValidateAnalysisConfig tests

    private AnalysisConfig CreateValidAnalysisConfig()
    {
        return new AnalysisConfig
        {
            MinimumSeverity = "Medium",
            MaxViolationsToReport = 20,
            OutputFormat = "json",
            EnabledRules = new List<string> { "RuleA", "RuleB" },
            ExcludePatterns = new List<string>()
        };
    }

    [Fact]
    public void ValidateAnalysisConfig_HappyPath_ReturnsValidResult()
    {
        var config = CreateValidAnalysisConfig();

        var result = ConfigurationValidator.ValidateAnalysisConfig(config);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateAnalysisConfig_NullConfig_ReturnsError()
    {
        var result = ConfigurationValidator.ValidateAnalysisConfig(null!);

        Assert.False(result.IsValid);
        Assert.Contains("Configuration cannot be null", result.Errors);
    }

    [Fact]
    public void ValidateAnalysisConfig_InvalidSeverity_AddsError()
    {
        var config = CreateValidAnalysisConfig();
        config.MinimumSeverity = "Unknown";

        var result = ConfigurationValidator.ValidateAnalysisConfig(config);

        Assert.False(result.IsValid);
        Assert.Contains("Invalid minimum severity", result.Errors.First());
    }

    #endregion

    #region ValidateCliOptions tests

    [Fact]
    public void ValidateCliOptions_HappyPath_ReturnsValidResult()
    {
        var options = new CliOptions
        {
            ProjectPath = null,
            FilePath = null,
            ConfigFile = null,
            AnalysisTimeoutSeconds = 30,
            MaxParallelThreads = 2
        };

        var result = ConfigurationValidator.ValidateCliOptions(options);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateCliOptions_NonExistingProjectPath_AddsError()
    {
        var options = new CliOptions
        {
            ProjectPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()),
            FilePath = null,
            ConfigFile = null,
            AnalysisTimeoutSeconds = 30,
            MaxParallelThreads = 2
        };

        var result = ConfigurationValidator.ValidateCliOptions(options);

        Assert.False(result.IsValid);
        Assert.Contains($"Project path not found: {options.ProjectPath}", result.Errors);
    }

    [Fact]
    public void ValidateCliOptions_TemporaryFile_ValidatesSuccessfully()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var options = new CliOptions
            {
                ProjectPath = null,
                FilePath = tempFile,
                ConfigFile = null,
                AnalysisTimeoutSeconds = 30,
                MaxParallelThreads = 2
            };

            var result = ConfigurationValidator.ValidateCliOptions(options);
            Assert.True(result.IsValid);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    #endregion

    #region ValidateRuleNames tests

    [Fact]
    public void ValidateRuleNames_AllKnownRules_ReturnsValid()
    {
        var ruleNames = new[] { "RuleA", "RuleB" };
        var supported = new[] { "RuleA", "RuleB", "RuleC" };

        var result = ConfigurationValidator.ValidateRuleNames(ruleNames, supported);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateRuleNames_UnknownRule_AddsError()
    {
        var ruleNames = new[] { "RuleX" };
        var supported = new[] { "RuleA", "RuleB" };

        var result = ConfigurationValidator.ValidateRuleNames(ruleNames, supported);

        Assert.False(result.IsValid);
        Assert.Contains("Unknown rule: RuleX", result.Errors);
    }

    #endregion

    #region ValidateComprehensive tests

    [Fact]
    public void ValidateComprehensive_BothValid_ReturnsCombinedValidResult()
    {
        var analysisConfig = CreateValidAnalysisConfig();
        var cliOptions = new CliOptions
        {
            ProjectPath = null,
            FilePath = null,
            ConfigFile = null,
            AnalysisTimeoutSeconds = 30,
            MaxParallelThreads = 2
        };

        var result = ConfigurationValidator.ValidateComprehensive(analysisConfig, cliOptions);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void ValidateComprehensive_WithErrors_CombinesAllErrors()
    {
        var analysisConfig = new AnalysisConfig
        {
            MinimumSeverity = "Bad",
            MaxViolationsToReport = -1,
            OutputFormat = "unknown",
            EnabledRules = new List<string>(),
            ExcludePatterns = new List<string> { "" }
        };
        var cliOptions = new CliOptions
        {
            ProjectPath = "nonexistent",
            FilePath = "nonexistent",
            ConfigFile = "nonexistent",
            AnalysisTimeoutSeconds = 0,
            MaxParallelThreads = -5
        };

        var result = ConfigurationValidator.ValidateComprehensive(analysisConfig, cliOptions);

        Assert.False(result.IsValid);
        // At least one error from each validator should be present
        Assert.True(result.Errors.Count >= 5);
    }
}
