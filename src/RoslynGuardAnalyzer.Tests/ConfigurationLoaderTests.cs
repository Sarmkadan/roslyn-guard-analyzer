#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RoslynGuardAnalyzer.Configuration;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public sealed class ConfigurationLoaderTests
{
    private static string WriteTempConfig(string json)
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, json);
        return tempFile;
    }

    [Fact]
    public async Task LoadFromFileAsync_HappyPath_ParsesAllProperties()
    {
        // Arrange
        var json = @"
        {
            ""enabledRules"": [""RuleA"", ""RuleB""],
            ""excludePatterns"": [""*.g.cs"", ""bin/*""],
            ""severity"": ""High"",
            ""maxViolations"": 500,
            ""enableCaching"": true,
            ""outputFormat"": ""json""
        }";
        var path = WriteTempConfig(json);

        try
        {
            // Act
            var config = await ConfigurationLoader.LoadFromFileAsync(path);

            // Assert
            Assert.Equal(new[] { "RuleA", "RuleB" }, config.EnabledRules);
            Assert.Equal(new[] { "*.g.cs", "bin/*" }, config.ExcludePatterns);
            Assert.Equal("High", config.MinimumSeverity);
            Assert.Equal(500, config.MaxViolationsToReport);
            Assert.True(config.EnableCaching);
            Assert.Equal("json", config.OutputFormat);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadFromFileAsync_NullOrWhiteSpacePath_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await ConfigurationLoader.LoadFromFileAsync(null!));

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await ConfigurationLoader.LoadFromFileAsync(string.Empty));

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await ConfigurationLoader.LoadFromFileAsync("   "));
    }

    [Fact]
    public async Task LoadFromFileAsync_NonExistentFile_ThrowsFileNotFoundException()
    {
        var nonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "missing.json");
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await ConfigurationLoader.LoadFromFileAsync(nonExistent));
    }

    [Fact]
    public async Task TryLoadDefaultAsync_FindsConfigInParentDirectory()
    {
        // Arrange: create a temporary directory hierarchy
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var child = Path.Combine(root, "child");
        Directory.CreateDirectory(child);

        var json = @"{ ""severity"": ""Low"" }";
        var configPath = Path.Combine(root, ".roslyn-guard.json");
        File.WriteAllText(configPath, json);

        try
        {
            // Act
            var config = await ConfigurationLoader.TryLoadDefaultAsync(child);

            // Assert
            Assert.NotNull(config);
            Assert.Equal("Low", config!.MinimumSeverity);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TryLoadDefaultAsync_NoConfigFound_ReturnsNull()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var result = await ConfigurationLoader.TryLoadDefaultAsync(tempDir);
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void AnalysisConfig_Validate_ValidConfiguration_ReturnsTrue()
    {
        var config = new AnalysisConfig
        {
            MinimumSeverity = "Medium",
            MaxViolationsToReport = 10,
            EnableCaching = false,
            OutputFormat = "xml"
        };
        config.EnabledRules.Add("RuleX");
        config.ExcludePatterns.Add("obj/*");

        var isValid = config.Validate(out var errors);

        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("")]
    public void AnalysisConfig_Validate_InvalidSeverity_ReturnsFalse(string severity)
    {
        var config = new AnalysisConfig
        {
            MinimumSeverity = severity,
            MaxViolationsToReport = 10,
            OutputFormat = "json"
        };

        var isValid = config.Validate(out var errors);

        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("minimum severity"));
    }

    [Fact]
    public void AnalysisConfig_Validate_InvalidOutputFormat_ReturnsFalse()
    {
        var config = new AnalysisConfig
        {
            MinimumSeverity = "Low",
            MaxViolationsToReport = 10,
            OutputFormat = "yaml"
        };

        var isValid = config.Validate(out var errors);

        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("output format"));
    }

    [Fact]
    public void AnalysisConfig_Validate_NonPositiveMaxViolations_ReturnsFalse()
    {
        var config = new AnalysisConfig
        {
            MinimumSeverity = "Low",
            MaxViolationsToReport = 0,
            OutputFormat = "text"
        };

        var isValid = config.Validate(out var errors);

        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("greater than 0"));
    }
}
