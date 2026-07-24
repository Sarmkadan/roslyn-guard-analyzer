// =============================================================================
// Tests for AnalysisResultJsonExtensions
// =============================================================================

using System;
using System.Text.Json;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Core;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class AnalysisResultJsonExtensionsTests
{
    [Fact]
    public void ToJson_ReturnsNonEmptyJson()
    {
        // Arrange
        var result = new AnalysisResult
        {
            ProjectName = "TestProject",
            ProjectPath = "/path/to/project"
        };

        // Act
        string json = result.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.StartsWith("{", json);
        Assert.EndsWith("}", json);
    }

    [Fact]
    public void ToJson_WithIndentation_ContainsNewLine()
    {
        // Arrange
        var result = new AnalysisResult
        {
            ProjectName = "TestProject",
            ProjectPath = "/path/to/project"
        };

        // Act
        string json = result.ToJson(indented: true);

        // Assert
        Assert.Contains('\n', json);
    }

    [Fact]
    public void ToJson_ProducesValidJsonThatCanBeDeserialized()
    {
        // Arrange
        var originalResult = new AnalysisResult
        {
            ProjectName = "TestProject",
            ProjectPath = "/path/to/project",
            AnalysisSucceeded = true,
            TotalFilesAnalyzed = 42,
            TotalElementsAnalyzed = 1000
        };

        // Act
        string json = originalResult.ToJson();
        var deserializedResult = AnalysisResultJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserializedResult);
        Assert.Equal(originalResult.ProjectName, deserializedResult.ProjectName);
        Assert.Equal(originalResult.ProjectPath, deserializedResult.ProjectPath);
        Assert.Equal(originalResult.AnalysisSucceeded, deserializedResult.AnalysisSucceeded);
        Assert.Equal(originalResult.TotalFilesAnalyzed, deserializedResult.TotalFilesAnalyzed);
        Assert.Equal(originalResult.TotalElementsAnalyzed, deserializedResult.TotalElementsAnalyzed);
    }

    [Fact]
    public void ToJson_WithViolations_ProducesValidJson()
    {
        // Arrange
        var result = new AnalysisResult
        {
            ProjectName = "TestProject",
            ProjectPath = "/path/to/project"
        };

        result.AddViolation(new RuleViolation
        {
            RuleId = "RULE001",
            FilePath = "/path/to/file.cs",
            LineNumber = 10,
            Message = "Test violation",
            Severity = SeverityLevel.Warning,
            Category = RuleCategory.CodeStructure
        });

        // Act
        string json = result.ToJson();
        var deserializedResult = AnalysisResultJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserializedResult);
        Assert.Single(deserializedResult.Violations);
        Assert.Equal("RULE001", deserializedResult.Violations[0].RuleId);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AnalysisResultJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void FromJson_EmptyString_ReturnsNull()
    {
        // Act
        var result = AnalysisResultJsonExtensions.FromJson(string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_WhitespaceOnly_ReturnsNull()
    {
        // Act
        var result = AnalysisResultJsonExtensions.FromJson("   ");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsAnalysisResult()
    {
        // Arrange
        var originalResult = new AnalysisResult
        {
            ProjectName = "TestProject",
            ProjectPath = "/path/to/project",
            AnalysisSucceeded = false,
            ErrorMessage = "Test error"
        };
        string json = originalResult.ToJson();

        // Act
        var result = AnalysisResultJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestProject", result.ProjectName);
        Assert.Equal("/path/to/project", result.ProjectPath);
        Assert.False(result.AnalysisSucceeded);
        Assert.Equal("Test error", result.ErrorMessage);
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        const string invalidJson = "{ this is not valid json }";

        // Act & Assert
        Assert.Throws<JsonException>(() => AnalysisResultJsonExtensions.FromJson(invalidJson));
    }

    [Fact]
    public void FromJson_JsonWithNullValues_DeserializesCorrectly()
    {
        // Arrange
        string json = @"{ " +
            "\"projectName\": \"TestProject\"," +
            "\"projectPath\": \"/path/to/project\"," +
            "\"violations\": null" +
            "}";

        // Act
        var result = AnalysisResultJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestProject", result.ProjectName);
    }

    [Fact]
    public void TryFromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => AnalysisResultJsonExtensions.TryFromJson(null!, out _));
    }

    [Fact]
    public void TryFromJson_EmptyString_ReturnsFalseAndNull()
    {
        // Act
        bool result = AnalysisResultJsonExtensions.TryFromJson(string.Empty, out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void TryFromJson_WhitespaceOnly_ReturnsFalseAndNull()
    {
        // Act
        bool result = AnalysisResultJsonExtensions.TryFromJson("   ", out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        const string invalidJson = "{ this is not valid json }";

        // Act
        bool result = AnalysisResultJsonExtensions.TryFromJson(invalidJson, out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndAnalysisResult()
    {
        // Arrange
        var originalResult = new AnalysisResult
        {
            ProjectName = "TestProject",
            ProjectPath = "/path/to/project"
        };
        string json = originalResult.ToJson();

        // Act
        bool result = AnalysisResultJsonExtensions.TryFromJson(json, out var deserialized);

        // Assert
        Assert.True(result);
        Assert.NotNull(deserialized);
        Assert.Equal("TestProject", deserialized.ProjectName);
    }

    [Fact]
    public void TryFromJson_LargeJson_ReturnsFalse()
    {
        // Arrange
        // Create a JSON string that exceeds the 10MB limit
        string largeJson = new string('x', 11 * 1024 * 1024);

        // Act
        bool result = AnalysisResultJsonExtensions.TryFromJson(largeJson, out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void RoundTripSerialization_PreservesAllProperties()
    {
        // Arrange
        var originalResult = new AnalysisResult
        {
            ProjectName = "CompleteTestProject",
            ProjectPath = "/full/path/to/project",
            AnalysisSucceeded = true,
            ErrorMessage = null,
            TotalFilesAnalyzed = 123,
            TotalElementsAnalyzed = 4567,
            Id = "test-id-12345"
        };

        originalResult.AddViolation(new RuleViolation
        {
            RuleId = "TEST001",
            FilePath = "/path/to/file.cs",
            LineNumber = 42,
            Message = "Test message",
            Severity = SeverityLevel.Error,
            Category = RuleCategory.LayerDependency
        });

        // Act
        string json = originalResult.ToJson();
        var deserializedResult = AnalysisResultJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserializedResult);
        Assert.Equal(originalResult.ProjectName, deserializedResult.ProjectName);
        Assert.Equal(originalResult.ProjectPath, deserializedResult.ProjectPath);
        Assert.Equal(originalResult.AnalysisSucceeded, deserializedResult.AnalysisSucceeded);
        Assert.Equal(originalResult.TotalFilesAnalyzed, deserializedResult.TotalFilesAnalyzed);
        Assert.Equal(originalResult.TotalElementsAnalyzed, deserializedResult.TotalElementsAnalyzed);
        Assert.Equal(originalResult.Id, deserializedResult.Id);
        Assert.Single(deserializedResult.Violations);
        Assert.Equal("TEST001", deserializedResult.Violations[0].RuleId);
        Assert.Equal(42, deserializedResult.Violations[0].LineNumber);
    }

    [Fact]
    public void FromJson_MinimalValidJson_ReturnsAnalysisResult()
    {
        // Arrange
        string json = @"{ " +
            "\"projectName\": \"Minimal\"," +
            "\"projectPath\": \"/minimal/path\"" +
            "}";

        // Act
        var result = AnalysisResultJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Minimal", result.ProjectName);
        Assert.Equal("/minimal/path", result.ProjectPath);
    }

    [Fact]
    public void TryFromJson_MinimalValidJson_ReturnsTrue()
    {
        // Arrange
        string json = @"{ " +
            "\"projectName\": \"Minimal\"," +
            "\"projectPath\": \"/minimal/path\"" +
            "}";

        // Act
        bool result = AnalysisResultJsonExtensions.TryFromJson(json, out var value);

        // Assert
        Assert.True(result);
        Assert.NotNull(value);
    }
}
