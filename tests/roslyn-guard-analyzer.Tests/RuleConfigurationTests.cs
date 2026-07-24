#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="RuleConfiguration"/>.
/// </summary>
public class RuleConfigurationTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_Default_InitializesWithDefaultValues()
    {
        // Act
        var config = new RuleConfiguration();

        // Assert
        config.Id.Should().NotBeNullOrWhiteSpace();
        config.Name.Should().BeEmpty();
        config.Description.Should().BeEmpty();
        config.EnabledRules.Should().NotBeNull().And.BeEmpty();
        config.ExcludedNamespaces.Should().NotBeNull().And.BeEmpty();
        config.ExcludedFiles.Should().NotBeNull().And.BeEmpty();
        config.MaxViolationsToReport.Should().Be(AnalyzerConstants.Analysis.DefaultMaxViolationsToReport);
        config.AnalysisTimeoutSeconds.Should().Be(AnalyzerConstants.Analysis.DefaultTimeoutSeconds);
        config.MinimumReportedSeverity.Should().Be(SeverityLevel.Warning);
        config.FailOnError.Should().BeFalse();
        config.GenerateDetailedReport.Should().BeTrue();
        config.CustomSettings.Should().NotBeNull().And.BeEmpty();
        config.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
        config.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNameAndDescription_SetsNameAndDescription()
    {
        // Arrange
        const string name = "Test Configuration";
        const string description = "A test configuration for unit testing";

        // Act
        var config = new RuleConfiguration(name, description);

        // Assert
        config.Id.Should().NotBeNullOrWhiteSpace();
        config.Name.Should().Be(name);
        config.Description.Should().Be(description);
        config.EnabledRules.Should().NotBeNull().And.BeEmpty();
        config.ExcludedNamespaces.Should().NotBeNull().And.BeEmpty();
        config.ExcludedFiles.Should().NotBeNull().And.BeEmpty();
        config.MaxViolationsToReport.Should().Be(AnalyzerConstants.Analysis.DefaultMaxViolationsToReport);
        config.AnalysisTimeoutSeconds.Should().Be(AnalyzerConstants.Analysis.DefaultTimeoutSeconds);
        config.MinimumReportedSeverity.Should().Be(SeverityLevel.Warning);
        config.FailOnError.Should().BeFalse();
        config.GenerateDetailedReport.Should().BeTrue();
        config.CustomSettings.Should().NotBeNull().And.BeEmpty();
        config.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
        config.UpdatedAt.Should().BeNull();
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Id_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var config = new RuleConfiguration();
        var expectedId = Guid.NewGuid().ToString();

        // Act
        config.Id = expectedId;

        // Assert
        config.Id.Should().Be(expectedId);
    }

    [Fact]
    public void Name_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var config = new RuleConfiguration();
        const string expectedName = "Test Config";

        // Act
        config.Name = expectedName;

        // Assert
        config.Name.Should().Be(expectedName);
    }

    [Fact]
    public void Description_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var config = new RuleConfiguration();
        const string expectedDescription = "Test Description";

        // Act
        config.Description = expectedDescription;

        // Assert
        config.Description.Should().Be(expectedDescription);
    }

    [Fact]
    public void EnabledRules_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var config = new RuleConfiguration();
        var expectedRules = new List<AnalysisRule>
        {
            new AnalysisRule("R001", "Test Rule 1", "Description 1", RuleCategory.CodeStructure),
            new AnalysisRule("R002", "Test Rule 2", "Description 2", RuleCategory.NamingConvention)
        };

        // Act
        config.EnabledRules = expectedRules;

        // Assert
        config.EnabledRules.Should().BeEquivalentTo(expectedRules);
    }

    [Fact]
    public void ExcludedNamespaces_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var config = new RuleConfiguration();
        var expectedNamespaces = new List<string> { "System.IO", "System.Net", "Microsoft.AspNetCore" };

        // Act
        config.ExcludedNamespaces = expectedNamespaces;

        // Assert
        config.ExcludedNamespaces.Should().BeEquivalentTo(expectedNamespaces);
    }

    [Fact]
    public void ExcludedFiles_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var config = new RuleConfiguration();
        var expectedFiles = new List<string> { "*.generated.cs", "AssemblyInfo.cs", "*/bin/*" };

        // Act
        config.ExcludedFiles = expectedFiles;

        // Assert
        config.ExcludedFiles.Should().BeEquivalentTo(expectedFiles);
    }

    [Fact]
    public void MaxViolationsToReport_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var config = new RuleConfiguration();
        const int expectedValue = 500;

        // Act
        config.MaxViolationsToReport = expectedValue;

        // Assert
        config.MaxViolationsToReport.Should().Be(expectedValue);
    }

    [Fact]
    public void AnalysisTimeoutSeconds_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var config = new RuleConfiguration();
        const int expectedValue = 600;

        // Act
        config.AnalysisTimeoutSeconds = expectedValue;

        // Assert
        config.AnalysisTimeoutSeconds.Should().Be(expectedValue);
    }

    [Fact]
    public void MinimumReportedSeverity_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var config = new RuleConfiguration();
        var expectedValue = SeverityLevel.Error;

        // Act
        config.MinimumReportedSeverity = expectedValue;

        // Assert
        config.MinimumReportedSeverity.Should().Be(expectedValue);
    }

    [Fact]
    public void FailOnError_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var config = new RuleConfiguration();
        const bool expectedValue = true;

        // Act
        config.FailOnError = expectedValue;

        // Assert
        config.FailOnError.Should().Be(expectedValue);
    }

    [Fact]
    public void GenerateDetailedReport_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var config = new RuleConfiguration();
        const bool expectedValue = false;

        // Act
        config.GenerateDetailedReport = expectedValue;

        // Assert
        config.GenerateDetailedReport.Should().Be(expectedValue);
    }

    [Fact]
    public void CustomSettings_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var config = new RuleConfiguration();
        var expectedSettings = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        // Act
        config.CustomSettings = expectedSettings;

        // Assert
        config.CustomSettings.Should().BeEquivalentTo(expectedSettings);
    }

    [Fact]
    public void CreatedAt_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var config = new RuleConfiguration();
        var expectedDate = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        config.CreatedAt = expectedDate;

        // Assert
        config.CreatedAt.Should().Be(expectedDate);
    }

    [Fact]
    public void UpdatedAt_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var config = new RuleConfiguration();
        var expectedDate = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        config.UpdatedAt = expectedDate;

        // Assert
        config.UpdatedAt.Should().Be(expectedDate);
    }

    #endregion

    #region Method Tests

    [Fact]
    public void AddRule_WithValidRule_AddsRuleToCollection()
    {
        // Arrange
        var config = new RuleConfiguration();
        var rule = new AnalysisRule("R001", "Test Rule", "Test Description", RuleCategory.CodeStructure);

        // Act
        config.AddRule(rule);

        // Assert
        config.EnabledRules.Should().ContainSingle();
        config.EnabledRules.First().Should().Be(rule);
    }

    [Fact]
    public void AddRule_WithNullRule_DoesNotAddRule()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        config.AddRule(null!);

        // Assert
        config.EnabledRules.Should().BeEmpty();
    }

    [Fact]
    public void AddRule_WithDuplicateRuleId_DoesNotAddDuplicate()
    {
        // Arrange
        var config = new RuleConfiguration();
        var rule1 = new AnalysisRule("R001", "Test Rule 1", "Description 1", RuleCategory.CodeStructure);
        var rule2 = new AnalysisRule("R001", "Test Rule 2", "Description 2", RuleCategory.NamingConvention);

        // Act
        config.AddRule(rule1);
        config.AddRule(rule2);

        // Assert
        config.EnabledRules.Should().ContainSingle();
        config.EnabledRules.First().Should().Be(rule1);
    }

    [Fact]
    public void RemoveRule_WithExistingRuleId_ReturnsTrueAndRemovesRule()
    {
        // Arrange
        var config = new RuleConfiguration();
        var rule = new AnalysisRule("R001", "Test Rule", "Test Description", RuleCategory.CodeStructure);
        config.AddRule(rule);

        // Act
        var result = config.RemoveRule("R001");

        // Assert
        result.Should().BeTrue();
        config.EnabledRules.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRule_WithNonExistingRuleId_ReturnsFalse()
    {
        // Arrange
        var config = new RuleConfiguration();
        var rule = new AnalysisRule("R001", "Test Rule", "Test Description", RuleCategory.CodeStructure);
        config.AddRule(rule);

        // Act
        var result = config.RemoveRule("R999");

        // Assert
        result.Should().BeFalse();
        config.EnabledRules.Should().ContainSingle();
    }

    [Fact]
    public void RemoveRule_WithNullOrEmptyRuleId_ReturnsFalse()
    {
        // Arrange
        var config = new RuleConfiguration();
        var rule = new AnalysisRule("R001", "Test Rule", "Test Description", RuleCategory.CodeStructure);
        config.AddRule(rule);

        // Act
        var result1 = config.RemoveRule(null!);
        var result2 = config.RemoveRule(string.Empty);
        var result3 = config.RemoveRule("   ");

        // Assert
        result1.Should().BeFalse();
        result2.Should().BeFalse();
        result3.Should().BeFalse();
        config.EnabledRules.Should().ContainSingle();
    }

    [Fact]
    public void GetRule_WithExistingRuleId_ReturnsRule()
    {
        // Arrange
        var config = new RuleConfiguration();
        var rule = new AnalysisRule("R001", "Test Rule", "Test Description", RuleCategory.CodeStructure);
        config.AddRule(rule);

        // Act
        var result = config.GetRule("R001");

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(rule);
    }

    [Fact]
    public void GetRule_WithNonExistingRuleId_ReturnsNull()
    {
        // Arrange
        var config = new RuleConfiguration();
        var rule = new AnalysisRule("R001", "Test Rule", "Test Description", RuleCategory.CodeStructure);
        config.AddRule(rule);

        // Act
        var result = config.GetRule("R999");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetRule_WithNullOrEmptyRuleId_ReturnsNull()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        var result1 = config.GetRule(null!);
        var result2 = config.GetRule(string.Empty);
        var result3 = config.GetRule("   ");

        // Assert
        result1.Should().BeNull();
        result2.Should().BeNull();
        result3.Should().BeNull();
    }

    [Fact]
    public void ExcludeNamespace_WithValidNamespace_AddsToExclusionList()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        config.ExcludeNamespace("System.IO");

        // Assert
        config.ExcludedNamespaces.Should().ContainSingle().Which.Should().Be("System.IO");
    }

    [Fact]
    public void ExcludeNamespace_WithNullOrEmptyNamespace_DoesNotAddToList()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        config.ExcludeNamespace(null!);
        config.ExcludeNamespace(string.Empty);
        config.ExcludeNamespace("   ");

        // Assert
        config.ExcludedNamespaces.Should().BeEmpty();
    }

    [Fact]
    public void ExcludeNamespace_WithDuplicateNamespace_DoesNotAddDuplicate()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        config.ExcludeNamespace("System.IO");
        config.ExcludeNamespace("System.IO");

        // Assert
        config.ExcludedNamespaces.Should().ContainSingle().Which.Should().Be("System.IO");
    }

    [Fact]
    public void ExcludeFile_WithValidFilePattern_AddsToExclusionList()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        config.ExcludeFile("*.generated.cs");

        // Assert
        config.ExcludedFiles.Should().ContainSingle().Which.Should().Be("*.generated.cs");
    }

    [Fact]
    public void ExcludeFile_WithNullOrEmptyFilePattern_DoesNotAddToList()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        config.ExcludeFile(null!);
        config.ExcludeFile(string.Empty);
        config.ExcludeFile("   ");

        // Assert
        config.ExcludedFiles.Should().BeEmpty();
    }

    [Fact]
    public void ExcludeFile_WithDuplicateFilePattern_DoesNotAddDuplicate()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        config.ExcludeFile("*.generated.cs");
        config.ExcludeFile("*.generated.cs");

        // Assert
        config.ExcludedFiles.Should().ContainSingle().Which.Should().Be("*.generated.cs");
    }

    [Fact]
    public void ShouldAnalyzeFile_WithNullFilePath_ReturnsFalse()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        var result = config.ShouldAnalyzeFile(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldAnalyzeFile_WithEmptyFilePath_ReturnsFalse()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        var result = config.ShouldAnalyzeFile(string.Empty);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldAnalyzeFile_WithWhitespaceFilePath_ReturnsFalse()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        var result = config.ShouldAnalyzeFile("   ");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldAnalyzeFile_WithMatchingExcludedPattern_ReturnsFalse()
    {
        // Arrange
        var config = new RuleConfiguration();
        config.ExcludeFile("Generated");

        // Act
        var result = config.ShouldAnalyzeFile("MyClass.Generated.cs");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldAnalyzeFile_WithNonMatchingExcludedPattern_ReturnsTrue()
    {
        // Arrange
        var config = new RuleConfiguration();
        config.ExcludeFile("Generated");

        // Act
        var result = config.ShouldAnalyzeFile("MyClass.cs");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldAnalyzeFile_WithCaseInsensitiveMatching_ReturnsFalse()
    {
        // Arrange
        var config = new RuleConfiguration();
        config.ExcludeFile("generated");

        // Act
        var result = config.ShouldAnalyzeFile("MyClass.GENERATED.cs");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldAnalyzeNamespace_WithNullNamespace_ReturnsTrue()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        var result = config.ShouldAnalyzeNamespace(null!);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldAnalyzeNamespace_WithEmptyNamespace_ReturnsTrue()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        var result = config.ShouldAnalyzeNamespace(string.Empty);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldAnalyzeNamespace_WithWhitespaceNamespace_ReturnsTrue()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        var result = config.ShouldAnalyzeNamespace("   ");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldAnalyzeNamespace_WithNoExcludedNamespaces_ReturnsTrue()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        var result = config.ShouldAnalyzeNamespace("System.IO");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldAnalyzeNamespace_WithMatchingExcludedNamespace_ReturnsFalse()
    {
        // Arrange
        var config = new RuleConfiguration();
        config.ExcludeNamespace("System.IO");

        // Act
        var result = config.ShouldAnalyzeNamespace("System.IO.File");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldAnalyzeNamespace_WithNonMatchingExcludedNamespace_ReturnsTrue()
    {
        // Arrange
        var config = new RuleConfiguration();
        config.ExcludeNamespace("System.IO");

        // Act
        var result = config.ShouldAnalyzeNamespace("System.Text.Json");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldAnalyzeNamespace_WithPartialMatchExcludedNamespace_ReturnsFalse()
    {
        // Arrange
        var config = new RuleConfiguration();
        config.ExcludeNamespace("System");

        // Act
        var result = config.ShouldAnalyzeNamespace("System.IO.File");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldAnalyzeNamespace_WithCaseInsensitiveMatching_ReturnsFalse()
    {
        // Arrange
        var config = new RuleConfiguration();
        config.ExcludeNamespace("system.io");

        // Act
        var result = config.ShouldAnalyzeNamespace("SYSTEM.IO.FILE");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SetCustomSetting_WithNullOrEmptyKey_ThrowsArgumentException()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        Action act1 = () => config.SetCustomSetting(null!, "value");
        Action act2 = () => config.SetCustomSetting(string.Empty, "value");
        Action act3 = () => config.SetCustomSetting("   ", "value");

        // Assert
        act1.Should().Throw<ArgumentException>();
        act2.Should().Throw<ArgumentException>();
        act3.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetCustomSetting_WithValidKeyAndValue_AddsSetting()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        config.SetCustomSetting("testKey", "testValue");

        // Assert
        config.CustomSettings.Should().ContainKey("testKey").WhoseValue.Should().Be("testValue");
    }

    [Fact]
    public void SetCustomSetting_WithValidKeyAndNullValue_AddsEmptyStringValue()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        config.SetCustomSetting("testKey", null);

        // Assert
        config.CustomSettings.Should().ContainKey("testKey").WhoseValue.Should().Be(string.Empty);
    }

    [Fact]
    public void SetCustomSetting_WithExistingKey_OverwritesValue()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        config.SetCustomSetting("testKey", "oldValue");
        config.SetCustomSetting("testKey", "newValue");

        // Assert
        config.CustomSettings.Should().ContainKey("testKey").WhoseValue.Should().Be("newValue");
    }

    [Fact]
    public void GetCustomSetting_WithExistingKey_ReturnsValue()
    {
        // Arrange
        var config = new RuleConfiguration();
        config.SetCustomSetting("testKey", "testValue");

        // Act
        var result = config.GetCustomSetting("testKey");

        // Assert
        result.Should().Be("testValue");
    }

    [Fact]
    public void GetCustomSetting_WithNonExistingKey_ReturnsNull()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        var result = config.GetCustomSetting("nonExistentKey");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCustomSetting_WithNonExistingKeyAndDefaultValue_ReturnsDefaultValue()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        var result = config.GetCustomSetting("nonExistentKey", "defaultValue");

        // Assert
        result.Should().Be("defaultValue");
    }

    [Fact]
    public void GetCustomSetting_WithExistingKeyAndDefaultValue_ReturnsActualValue()
    {
        // Arrange
        var config = new RuleConfiguration();
        config.SetCustomSetting("testKey", "actualValue");

        // Act
        var result = config.GetCustomSetting("testKey", "defaultValue");

        // Assert
        result.Should().Be("actualValue");
    }

    [Fact]
    public void IsValid_WithValidConfiguration_ReturnsTrue()
    {
        // Arrange
        var config = new RuleConfiguration("Valid Config", "A valid configuration for testing")
        {
            MaxViolationsToReport = 100,
            AnalysisTimeoutSeconds = 300
        };
        config.AddRule(new AnalysisRule("R001", "Test Rule", "Test Description", RuleCategory.CodeStructure));

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyName_ReturnsFalse()
    {
        // Arrange
        var config = new RuleConfiguration(string.Empty, "Valid Description")
        {
            MaxViolationsToReport = 100,
            AnalysisTimeoutSeconds = 300
        };
        config.AddRule(new AnalysisRule("R001", "Test Rule", "Test Description", RuleCategory.CodeStructure));

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithShortName_ReturnsFalse()
    {
        // Arrange
        var config = new RuleConfiguration("Ab", "Valid Description")
        {
            MaxViolationsToReport = 100,
            AnalysisTimeoutSeconds = 300
        };
        config.AddRule(new AnalysisRule("R001", "Test Rule", "Test Description", RuleCategory.CodeStructure));

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNullDescription_ReturnsTrue()
    {
        // Arrange
        var config = new RuleConfiguration("Valid Name", null!)
        {
            MaxViolationsToReport = 100,
            AnalysisTimeoutSeconds = 300
        };
        config.AddRule(new AnalysisRule("R001", "Test Rule", "Test Description", RuleCategory.CodeStructure));

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithZeroMaxViolationsToReport_ReturnsFalse()
    {
        // Arrange
        var config = new RuleConfiguration("Valid Name", "Valid Description")
        {
            MaxViolationsToReport = 0,
            AnalysisTimeoutSeconds = 300
        };
        config.AddRule(new AnalysisRule("R001", "Test Rule", "Test Description", RuleCategory.CodeStructure));

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNegativeMaxViolationsToReport_ReturnsFalse()
    {
        // Arrange
        var config = new RuleConfiguration("Valid Name", "Valid Description")
        {
            MaxViolationsToReport = -1,
            AnalysisTimeoutSeconds = 300
        };
        config.AddRule(new AnalysisRule("R001", "Test Rule", "Test Description", RuleCategory.CodeStructure));

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithZeroAnalysisTimeoutSeconds_ReturnsFalse()
    {
        // Arrange
        var config = new RuleConfiguration("Valid Name", "Valid Description")
        {
            MaxViolationsToReport = 100,
            AnalysisTimeoutSeconds = 0
        };
        config.AddRule(new AnalysisRule("R001", "Test Rule", "Test Description", RuleCategory.CodeStructure));

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNegativeAnalysisTimeoutSeconds_ReturnsFalse()
    {
        // Arrange
        var config = new RuleConfiguration("Valid Name", "Valid Description")
        {
            MaxViolationsToReport = 100,
            AnalysisTimeoutSeconds = -1
        };
        config.AddRule(new AnalysisRule("R001", "Test Rule", "Test Description", RuleCategory.CodeStructure));

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNoEnabledRules_ReturnsFalse()
    {
        // Arrange
        var config = new RuleConfiguration("Valid Name", "Valid Description")
        {
            MaxViolationsToReport = 100,
            AnalysisTimeoutSeconds = 300
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetEnabledRuleCount_WithNoRules_ReturnsZero()
    {
        // Arrange
        var config = new RuleConfiguration();

        // Act
        var result = config.GetEnabledRuleCount();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetEnabledRuleCount_WithDisabledRules_ReturnsZero()
    {
        // Arrange
        var config = new RuleConfiguration();
        var disabledRule = new AnalysisRule("R001", "Disabled Rule", "Description", RuleCategory.CodeStructure) { IsEnabled = false };
        config.AddRule(disabledRule);

        // Act
        var result = config.GetEnabledRuleCount();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetEnabledRuleCount_WithEnabledRules_ReturnsCount()
    {
        // Arrange
        var config = new RuleConfiguration();
        var enabledRule1 = new AnalysisRule("R001", "Enabled Rule 1", "Description", RuleCategory.CodeStructure) { IsEnabled = true };
        var enabledRule2 = new AnalysisRule("R002", "Enabled Rule 2", "Description", RuleCategory.NamingConvention) { IsEnabled = true };
        var disabledRule = new AnalysisRule("R003", "Disabled Rule", "Description", RuleCategory.AsyncPattern) { IsEnabled = false };
        config.AddRule(enabledRule1);
        config.AddRule(enabledRule2);
        config.AddRule(disabledRule);

        // Act
        var result = config.GetEnabledRuleCount();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void CreateCopy_CreatesDeepCopyWithNewId()
    {
        // Arrange
        var original = new RuleConfiguration("Original Config", "Original Description")
        {
            MaxViolationsToReport = 500,
            AnalysisTimeoutSeconds = 600,
            MinimumReportedSeverity = SeverityLevel.Error,
            FailOnError = true,
            GenerateDetailedReport = false
        };
        original.AddRule(new AnalysisRule("R001", "Test Rule 1", "Description 1", RuleCategory.CodeStructure));
        original.AddRule(new AnalysisRule("R002", "Test Rule 2", "Description 2", RuleCategory.NamingConvention));
        original.ExcludeNamespace("System.IO");
        original.ExcludeFile("*.generated.cs");
        original.SetCustomSetting("key1", "value1");
        original.SetCustomSetting("key2", "value2");

        // Act
        var copy = original.CreateCopy();

        // Assert
        copy.Should().NotBeSameAs(original);
        copy.Id.Should().NotBe(original.Id);
        copy.Name.Should().Be($"{original.Name} (Copy)");
        copy.Description.Should().Be(original.Description);
        copy.MaxViolationsToReport.Should().Be(original.MaxViolationsToReport);
        copy.AnalysisTimeoutSeconds.Should().Be(original.AnalysisTimeoutSeconds);
        copy.MinimumReportedSeverity.Should().Be(original.MinimumReportedSeverity);
        copy.FailOnError.Should().Be(original.FailOnError);
        copy.GenerateDetailedReport.Should().Be(original.GenerateDetailedReport);
        copy.EnabledRules.Should().BeEquivalentTo(original.EnabledRules);
        copy.ExcludedNamespaces.Should().BeEquivalentTo(original.ExcludedNamespaces);
        copy.ExcludedFiles.Should().BeEquivalentTo(original.ExcludedFiles);
        copy.CustomSettings.Should().BeEquivalentTo(original.CustomSettings);
        copy.CreatedAt.Should().BeCloseTo(original.CreatedAt, precision: TimeSpan.FromSeconds(5));
        copy.UpdatedAt.Should().BeNull(); // Copy should have null UpdatedAt
    }

    [Fact]
    public void MarkAsUpdated_SetsUpdatedAtToCurrentTime()
    {
        // Arrange
        var config = new RuleConfiguration();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        config.MarkAsUpdated();
        var afterUpdate = DateTime.UtcNow;

        // Assert
        config.UpdatedAt.Should().NotBeNull();
        config.UpdatedAt.Value.Should().BeOnOrAfter(beforeUpdate);
        config.UpdatedAt.Value.Should().BeOnOrBefore(afterUpdate);
    }

    #endregion
}
