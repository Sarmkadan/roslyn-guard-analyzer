// AnalysisProjectTests.cs
using FluentAssertions;
using RoslynGuardAnalyzer.Domain.Models;
using System;
using System.IO;
using Xunit;

namespace RoslynGuardAnalyzer.Tests
{
    public class AnalysisProjectTests
    {
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

        [Fact]
        public void Ctor_WithNameAndPath_SetsValues()
        {
            // Arrange
            var name = "TestProject";
            var path = "/path/to/project";

            // Act
            var project = new AnalysisProject(name, path);

            // Assert
            project.Name.Should().Be(name);
            project.Path.Should().Be(path);
        }

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

        [Fact]
        public void GetProperty_Existing_ReturnsValue()
        {
            // Arrange
            var project = new AnalysisProject();
            project.SetProperty("key", "value");

            // Act
            var value = project.GetProperty("key");

            // Assert
            value.Should().Be("value");
        }

        [Fact]
        public void GetProperty_NonExisting_ReturnsDefault()
        {
            // Arrange
            var project = new AnalysisProject();

            // Act
            var value = project.GetProperty("key", "default");

            // Assert
            value.Should().Be("default");
        }

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
}
