#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for ConfigurationLoaderExtensions
// =============================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using RoslynGuardAnalyzer.Configuration;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class ConfigurationLoaderExtensionsTests
{
    [Fact]
    public async Task LoadFromFileAsync_ValidFile_ReturnsParsedConfig()
    {
        // Arrange
        var loader = new ConfigurationLoader();
        using var dir = new TempDir();
        var path = Path.Combine(dir.Path, "config.json");
        File.WriteAllText(path, "{ \"enableCaching\": false }");

        // Act
        var config = await loader.LoadFromFileAsync(path);

        // Assert
        config.EnableCaching.Should().BeFalse();
        config.MinimumSeverity.Should().Be("Low");
        config.MaxViolationsToReport.Should().Be(1000);
        config.OutputFormat.Should().Be("text");
    }

    [Fact]
    public async Task LoadFromFileAsync_BlankOrMissingPath_Throws()
    {
        // Arrange
        var loader = new ConfigurationLoader();
        using var dir = new TempDir();
        string?[] blankPaths = { null, "", "   " };

        // Act & Assert
        foreach (var blankPath in blankPaths)
        {
            Func<Task> act = () => loader.LoadFromFileAsync(blankPath!);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        Func<Task> missing = () => loader.LoadFromFileAsync(Path.Combine(dir.Path, "missing.json"));
        await missing.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task LoadFromFileAsync_InvalidSeverity_ThrowsUnlessValidationDisabled()
    {
        // Arrange
        var loader = new ConfigurationLoader();
        using var dir = new TempDir();
        var path = Path.Combine(dir.Path, "config.json");
        File.WriteAllText(path, "{ \"severity\": \"Extreme\" }");

        // Act
        Func<Task> validated = () => loader.LoadFromFileAsync(path, validate: true);
        var unvalidated = await loader.LoadFromFileAsync(path, validate: false);

        // Assert
        await validated.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Configuration validation failed*");
        unvalidated.Should().NotBeNull();
    }

    [Fact]
    public async Task MergeWithDefaultAsync_DefaultFileExists_DedupesAndFillsGaps()
    {
        // Arrange
        var loader = new ConfigurationLoader();
        using var dir = new TempDir();
        File.WriteAllText(Path.Combine(dir.Path, ".roslyn-guard.json"), "{}");

        var baseConfig = new AnalysisConfig { MaxViolationsToReport = 0, OutputFormat = "" };
        baseConfig.EnabledRules.AddRange(new[] { "NAM001", "NAM001", "ASY001" });
        baseConfig.ExcludePatterns.AddRange(new[] { "bin/", "bin/" });

        // Act
        var merged = await loader.MergeWithDefaultAsync(baseConfig, dir.Path);
        var overridden = await loader.MergeWithDefaultAsync(
            new AnalysisConfig { MaxViolationsToReport = 25, OutputFormat = "json" }, dir.Path);

        // Assert
        merged.EnabledRules.Should().Equal("NAM001", "ASY001");
        merged.ExcludePatterns.Should().Equal("bin/");
        merged.MaxViolationsToReport.Should().Be(1000);
        merged.OutputFormat.Should().Be("text");
        overridden.MaxViolationsToReport.Should().Be(25);
        overridden.OutputFormat.Should().Be("json");
    }

    [Fact]
    public async Task MergeWithDefaultAsync_NoDefaultFileOrNullBase_ReturnsBaseOrThrows()
    {
        // Arrange
        var loader = new ConfigurationLoader();
        using var dir = new TempDir();
        var baseConfig = new AnalysisConfig();

        // Act
        var merged = await loader.MergeWithDefaultAsync(baseConfig, dir.Path);
        Func<Task> nullBase = () => loader.MergeWithDefaultAsync(null!, dir.Path);

        // Assert
        merged.Should().BeSameAs(baseConfig);
        await nullBase.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void IsRuleEnabled_EmptyListAndCaseInsensitiveMatch_BehavesAsExpected()
    {
        // Arrange
        var loader = new ConfigurationLoader();
        var config = new AnalysisConfig();
        config.EnabledRules.Add("NAM001");

        // Act
        var emptyListResult = loader.IsRuleEnabled(new AnalysisConfig(), "ANY001");
        var caseInsensitiveResult = loader.IsRuleEnabled(config, "nam001");
        var unlistedResult = loader.IsRuleEnabled(config, "LYR001");
        Action blankRuleId = () => loader.IsRuleEnabled(config, " ");

        // Assert
        emptyListResult.Should().BeTrue();
        caseInsensitiveResult.Should().BeTrue();
        unlistedResult.Should().BeFalse();
        blankRuleId.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsPathExcluded_PrefixExactAndWildcardPatterns_MatchExpectedPaths()
    {
        // Arrange
        var loader = new ConfigurationLoader();
        var config = new AnalysisConfig();
        config.ExcludePatterns.Add("obj/");
        config.ExcludePatterns.Add("*.g.cs");
        config.ExcludePatterns.Add("Legacy.cs");

        // Act & Assert
        loader.IsPathExcluded(new AnalysisConfig(), "src/A.cs").Should().BeFalse();
        loader.IsPathExcluded(config, "obj/Debug/A.cs").Should().BeTrue();
        loader.IsPathExcluded(config, "src/obj/A.cs").Should().BeFalse();
        loader.IsPathExcluded(config, "src/Foo.g.cs").Should().BeTrue();
        loader.IsPathExcluded(config, "src/Foo.cs").Should().BeFalse();
        loader.IsPathExcluded(config, "src/legacy.CS").Should().BeTrue();
    }

    [Fact]
    public void Clone_And_ShouldEnableCaching_CopyIsIndependentAndForceDisableWins()
    {
        // Arrange
        var loader = new ConfigurationLoader();
        var original = new AnalysisConfig { OutputFormat = "csv", MaxViolationsToReport = 7, EnableCaching = true };
        original.EnabledRules.Add("LYR001");
        original.ExcludePatterns.Add("obj/");

        // Act
        var clone = loader.Clone(original);
        clone.EnabledRules.Add("NAM001");
        clone.ExcludePatterns.Add("bin/");
        clone.MaxViolationsToReport = 1;

        // Assert
        clone.Should().NotBeSameAs(original);
        original.EnabledRules.Should().Equal("LYR001");
        original.ExcludePatterns.Should().Equal("obj/");
        original.MaxViolationsToReport.Should().Be(7);
        clone.OutputFormat.Should().Be("csv");

        loader.ShouldEnableCaching(original).Should().BeTrue();
        loader.ShouldEnableCaching(original, forceDisable: true).Should().BeFalse();
        Action nullConfig = () => loader.Clone(null!);
        nullConfig.Should().Throw<ArgumentNullException>();
    }

    /// <summary>Temporary directory that is removed when disposed.</summary>
    private sealed class TempDir : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), "rga-config-" + Guid.NewGuid().ToString("N"));

        public TempDir() => Directory.CreateDirectory(Path);

        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}
