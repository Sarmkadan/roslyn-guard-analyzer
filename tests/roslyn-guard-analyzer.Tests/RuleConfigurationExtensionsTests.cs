#nullable enable

using System;
using System.Collections.Generic;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class RuleConfigurationExtensionsTests
{
    private static RuleConfiguration CreateConfiguration() => new RuleConfiguration("Test", "Description");

    [Fact]
    public void HasRulesWithSeverityOrHigher_WhenConfigurationIsNull_ThrowsArgumentNullException()
    {
        Action act = () => RuleConfigurationExtensions.HasRulesWithSeverityOrHigher(null!, SeverityLevel.Warning);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HasRulesWithSeverityOrHigher_WhenRulesMatchSeverity_ReturnsTrue()
    {
        var config = CreateConfiguration();
        config.AddRule(new AnalysisRule("R001", "Rule 1", "Desc", RuleCategory.CodeStructure) { DefaultSeverity = SeverityLevel.Error });

        config.HasRulesWithSeverityOrHigher(SeverityLevel.Warning).Should().BeTrue();
    }

    [Fact]
    public void HasRulesWithSeverityOrHigher_WhenNoRulesMatchSeverity_ReturnsFalse()
    {
        var config = CreateConfiguration();
        config.AddRule(new AnalysisRule("R001", "Rule 1", "Desc", RuleCategory.CodeStructure) { DefaultSeverity = SeverityLevel.Info });

        config.HasRulesWithSeverityOrHigher(SeverityLevel.Warning).Should().BeFalse();
    }

    [Fact]
    public void GetEnabledRulesWithSeverityOrHigher_WhenConfigurationIsNull_ThrowsArgumentNullException()
    {
        Action act = () => RuleConfigurationExtensions.GetEnabledRulesWithSeverityOrHigher(null!, SeverityLevel.Warning);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetEnabledRulesWithSeverityOrHigher_ReturnsMatchingEnabledRules()
    {
        var config = CreateConfiguration();
        config.AddRule(new AnalysisRule("R001", "Rule 1", "Desc", RuleCategory.CodeStructure) { DefaultSeverity = SeverityLevel.Error, IsEnabled = true });
        config.AddRule(new AnalysisRule("R002", "Rule 2", "Desc", RuleCategory.NamingConvention) { DefaultSeverity = SeverityLevel.Info, IsEnabled = true });
        config.AddRule(new AnalysisRule("R003", "Rule 3", "Desc", RuleCategory.AsyncPattern) { DefaultSeverity = SeverityLevel.Warning, IsEnabled = false });

        var results = config.GetEnabledRulesWithSeverityOrHigher(SeverityLevel.Warning);

        results.Should().ContainSingle().Which.Id.Should().Be("R001");
    }

    [Fact]
    public void HasCustomSettings_WhenConfigurationIsNull_ThrowsArgumentNullException()
    {
        Action act = () => RuleConfigurationExtensions.HasCustomSettings(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HasCustomSettings_WhenSettingsExist_ReturnsTrue()
    {
        var config = CreateConfiguration();
        config.SetCustomSetting("key1", "value1");

        config.HasCustomSettings().Should().BeTrue();
    }

    [Fact]
    public void HasCustomSettings_WhenSettingsDoNotExist_ReturnsFalse()
    {
        var config = CreateConfiguration();

        config.HasCustomSettings().Should().BeFalse();
    }

    [Fact]
    public void GetCustomSettingKeysWithPrefix_WhenConfigurationIsNull_ThrowsArgumentNullException()
    {
        Action act = () => RuleConfigurationExtensions.GetCustomSettingKeysWithPrefix(null!, "prefix");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetCustomSettingKeysWithPrefix_WhenPrefixIsNull_ThrowsArgumentException()
    {
        var config = CreateConfiguration();
        Action act = () => config.GetCustomSettingKeysWithPrefix(null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetCustomSettingKeysWithPrefix_WhenPrefixIsEmpty_ThrowsArgumentException()
    {
        var config = CreateConfiguration();
        Action act = () => config.GetCustomSettingKeysWithPrefix(string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetCustomSettingKeysWithPrefix_ReturnsMatchingKeys()
    {
        var config = CreateConfiguration();
        config.SetCustomSetting("api.key", "value1");
        config.SetCustomSetting("api.url", "value2");
        config.SetCustomSetting("other.setting", "value3");

        var keys = config.GetCustomSettingKeysWithPrefix("api.");

        keys.Should().BeEquivalentTo("api.key", "api.url");
    }
}
