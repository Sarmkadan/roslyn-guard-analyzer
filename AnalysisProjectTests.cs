using FluentAssertions;
using RoslynGuardAnalyzer.Domain.Models;
using System;
using System.IO;
using Xunit;

/// <summary>
/// Tests for the AnalysisProject class.
/// </summary>
public class AnalysisProjectTests
{
    /// <summary>
    /// Verifies that the default constructor sets default values.
    /// </summary>
    [Fact]
    public void Ctor_Default_SetsDefaultValues()
    {
        // Arrange
        // Act
        var project = new AnalysisProject();

        // Assert
        project.Id.Should().NotBeEmpty();
        project.Name.Should().BeEmpty();
        project.Path.Should().BeEmpty();
        project.SourceFiles.Should().NotBeNull().And.BeEmpty();
        project.ReferencedProjects.Should().NotBeNull().And.BeEmpty();
        project.Properties.Should().NotBeNull().And.BeEmpty();
        project.IsNetCore.Should().BeFalse();
        project.Language.Should().Be("C#");
        project.AnalyzedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        project.FileCount.Should().Be(0);
    }

    /// <summary>
    /// Verifies that the constructor with name and path sets values.
    /// </summary>
    /// <param name="name">The name of the project.</param>
    /// <param name="path">The path of the project.</param>
    [Fact]
    public void Ctor_WithNameAndPath_SetsValues(string name, string path)
    {
        // Arrange
        // Act
        var project = new AnalysisProject(name, path);

        // Assert
        project.Name.Should().Be(name);
        project.Path.Should().Be(path);
    }

    /// <summary>
    /// Verifies that adding a duplicate source file does not add it.
    /// </summary>
    [Fact]
    public void AddSourceFile_Duplicate_DoesNotAdd()
    {
        // Arrange
        var project = new AnalysisProject();
        var filePath = "/path/to/file.cs";

        // Act
        project.AddSourceFile(filePath);
        project.AddSourceFile(filePath);

        // Assert
        project.SourceFiles.Should().ContainSingle().Which.Should().Be(filePath);
        project.FileCount.Should().Be(1);
    }

    /// <summary>
    /// Verifies that adding a duplicate referenced project does not add it.
    /// </summary>
    [Fact]
    public void AddReferencedProject_Duplicate_DoesNotAdd()
    {
        // Arrange
        var project = new AnalysisProject();
        var projectPath = "/path/to/referenced/project";

        // Act
        project.AddReferencedProject(projectPath);
        project.AddReferencedProject(projectPath);

        // Assert
        project.ReferencedProjects.Should().ContainSingle().Which.Should().Be(projectPath);
    }

    /// <summary>
    /// Verifies that getting C# files returns the correct files.
    /// </summary>
    [Fact]
    public void GetCSharpFiles_ReturnsCSharpFiles()
    {
        // Arrange
        var project = new AnalysisProject();
        project.AddSourceFile("/path/to/file.cs");
        project.AddSourceFile("/path/to/file.txt");

        // Act
        var csharpFiles = project.GetCSharpFiles().ToList();

        // Assert
        csharpFiles.Should().ContainSingle().Which.Should().Be("/path/to/file.cs");
    }

    /// <summary>
    /// Verifies that getting a property returns the correct value.
    /// </summary>
    /// <param name="key">The key of the property.</param>
    /// <param name="defaultValue">The default value to return if the property does not exist.</param>
    /// <returns>The value of the property, or the default value if it does not exist.</returns>
    [Fact]
    public void GetProperty_Existing_ReturnsValue(string key, string defaultValue)
    {
        // Arrange
        var project = new AnalysisProject();
        project.SetProperty(key, "value");

        // Act
        var value = project.GetProperty(key);

        // Assert
        value.Should().Be("value");
    }

    /// <summary>
    /// Verifies that getting a non-existent property returns the default value.
    /// </summary>
    /// <param name="key">The key of the property.</param>
    /// <param name="defaultValue">The default value to return if the property does not exist.</param>
    /// <returns>The default value.</returns>
    [Fact]
    public void GetProperty_NonExisting_ReturnsDefault(string key, string defaultValue)
    {
        // Arrange
        var project = new AnalysisProject();

        // Act
        var value = project.GetProperty(key, defaultValue);

        // Assert
        value.Should().Be(defaultValue);
    }

    /// <summary>
    /// Verifies that a valid project is considered valid.
    /// </summary>
    [Fact]
    public void IsValid_ValidProject_ReturnsTrue()
    {
        // Arrange
        var project = new AnalysisProject("TestProject", Directory.GetCurrentDirectory());

        // Act
        var isValid = project.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that an invalid project is considered invalid.
    /// </summary>
    [Fact]
    public void IsValid_InvalidProject_ReturnsFalse()
    {
        // Arrange
        var project = new AnalysisProject();

        // Act
        var isValid = project.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that getting the directory path returns the correct path.
    /// </summary>
    [Fact]
    public void GetDirectoryPath_ExistingPath_ReturnsPath()
    {
        // Arrange
        var project = new AnalysisProject("TestProject", Directory.GetCurrentDirectory());

        // Act
        var directoryPath = project.GetDirectoryPath();

        // Assert
        directoryPath.Should().Be(Directory.GetCurrentDirectory());
    }

    /// <summary>
    /// Verifies that getting the directory path returns the parent path when the project path does not exist.
    /// </summary>
    [Fact]
    public void GetDirectoryPath_NonExistingPath_ReturnsParentPath()
    {
        // Arrange
        var project = new AnalysisProject("TestProject", "/path/to/non/existing/project");

        // Act
        var directoryPath = project.GetDirectoryPath();

        // Assert
        directoryPath.Should().Be("/path");
    }
}
