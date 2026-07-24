#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for AnalysisProjectJsonExtensions
// =============================================================================

using System;
using System.Collections.Generic;
using FluentAssertions;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class AnalysisProjectJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithValidProject_ReturnsNonEmptyJson()
    {
        // Arrange
        var project = new AnalysisProject("TestProject", "/path/to/project.csproj")
        {
            TargetFramework = "net8.0",
            Language = "C#",
            SourceFiles = { "/path/to/Program.cs", "/path/to/Class1.cs" },
            ReferencedProjects = { "/path/to/Dependency.csproj" },
            Properties = { { "Version", "1.0.0" }, { "Author", "Test Author" } }
        };

        // Act
        string json = project.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("TestProject");
        json.Should().Contain("net8.0");
    }

    [Fact]
    public void ToJson_WithIndented_ReturnsFormattedJson()
    {
        // Arrange
        var project = new AnalysisProject("TestProject", "/path/to/project.csproj");

        // Act
        string json = project.ToJson(indented: true);

        // Assert
        json.Should().Contain("\n");
        json.Should().Contain("  "); // indentation
    }

    [Fact]
    public void ToJson_WithNullProject_ThrowsArgumentNullException()
    {
        // Arrange
        AnalysisProject? project = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => project!.ToJson());
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsDeserializedProject()
    {
        // Arrange
        var originalProject = new AnalysisProject("TestProject", "/path/to/project.csproj")
        {
            TargetFramework = "net8.0",
            Language = "C#"
        };
        string json = originalProject.ToJson();

        // Act
        var deserializedProject = AnalysisProjectJsonExtensions.FromJson(json);

        // Assert
        deserializedProject.Should().NotBeNull();
        deserializedProject!.Name.Should().Be("TestProject");
        deserializedProject.Path.Should().Be("/path/to/project.csproj");
        deserializedProject.TargetFramework.Should().Be("net8.0");
        deserializedProject.Language.Should().Be("C#");
    }

    [Fact]
    public void FromJson_WithEmptyJson_ThrowsArgumentException()
    {
        // Arrange
        string emptyJson = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => AnalysisProjectJsonExtensions.FromJson(emptyJson));
    }

    [Fact]
    public void FromJson_WithWhitespaceOnlyJson_ThrowsArgumentException()
    {
        // Arrange
        string whitespaceJson = "   \n\t  ";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => AnalysisProjectJsonExtensions.FromJson(whitespaceJson));
        exception.Message.Should().Contain("whitespace");
    }

    [Fact]
    public void FromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullJson = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AnalysisProjectJsonExtensions.FromJson(nullJson!));
    }

    [Fact]
    public void FromJson_WithInvalidJson_ReturnsNull()
    {
        // Arrange
        string invalidJson = "{ this is not valid json";

        // Act
        var result = AnalysisProjectJsonExtensions.FromJson(invalidJson);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializedProject()
    {
        // Arrange
        var originalProject = new AnalysisProject("TestProject", "/path/to/project.csproj");
        string json = originalProject.ToJson();

        // Act
        bool result = AnalysisProjectJsonExtensions.TryFromJson(json, out var deserializedProject);

        // Assert
        result.Should().BeTrue();
        deserializedProject.Should().NotBeNull();
        deserializedProject!.Name.Should().Be("TestProject");
    }

    [Fact]
    public void TryFromJson_WithEmptyJson_ThrowsArgumentException()
    {
        // Arrange
        string emptyJson = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => AnalysisProjectJsonExtensions.TryFromJson(emptyJson, out _));
    }

    [Fact]
    public void TryFromJson_WithWhitespaceOnlyJson_ReturnsFalseAndNull()
    {
        // Arrange
        string whitespaceJson = "   \n\t  ";

        // Act
        bool result = AnalysisProjectJsonExtensions.TryFromJson(whitespaceJson, out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullJson = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AnalysisProjectJsonExtensions.TryFromJson(nullJson!, out _));
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        string invalidJson = "invalid json string";

        // Act
        bool result = AnalysisProjectJsonExtensions.TryFromJson(invalidJson, out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }


}
