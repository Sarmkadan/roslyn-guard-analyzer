using System;
using RoslynGuardAnalyzer.Events;
using Xunit;

namespace RoslynGuardAnalyzer.Tests
{
    public class AnalysisStartedEventExtensionsTests
    {
        [Fact]
        public void HasConfigFilePath_ReturnsTrue_WhenConfigFilePathIsNotNullOrEmpty()
        {
            // Arrange
            var @event = new AnalysisStartedEvent
            {
                ConfigFilePath = "/path/to/config.json",
                ProjectPath = "/path/to/project",
                AnalysisId = Guid.NewGuid().ToString()
            };

            // Act
            var result = @event.HasConfigFilePath();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasConfigFilePath_ReturnsFalse_WhenConfigFilePathIsNullOrEmpty()
        {
            // Arrange
            var @event = new AnalysisStartedEvent
            {
                ConfigFilePath = null,
                ProjectPath = "/path/to/project",
                AnalysisId = Guid.NewGuid().ToString()
            };

            // Act
            var result = @event.HasConfigFilePath();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetAnalysisSummary_ReturnsFormattedString()
        {
            // Arrange
            var projectPath = "/path/to/project";
            var analysisId = Guid.NewGuid().ToString();
            var @event = new AnalysisStartedEvent
            {
                ConfigFilePath = "/path/to/config.json",
                ProjectPath = projectPath,
                AnalysisId = analysisId
            };

            // Act
            var result = @event.GetAnalysisSummary();

            // Assert
            Assert.Equal($"Analysis started for project '{projectPath}' with ID '{analysisId}'", result);
        }

        [Fact]
        public void IsValid_ReturnsTrue_WhenProjectPathAndAnalysisIdAreNotNullOrEmpty()
        {
            // Arrange
            var @event = new AnalysisStartedEvent
            {
                ConfigFilePath = "/path/to/config.json",
                ProjectPath = "/path/to/project",
                AnalysisId = Guid.NewGuid().ToString()
            };

            // Act
            var result = @event.IsValid();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_ReturnsFalse_WhenProjectPathIsNullOrEmpty()
        {
            // Arrange
            var @event = new AnalysisStartedEvent
            {
                ConfigFilePath = "/path/to/config.json",
                ProjectPath = null,
                AnalysisId = Guid.NewGuid().ToString()
            };

            // Act
            var result = @event.IsValid();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValid_ReturnsFalse_WhenAnalysisIdIsNullOrEmpty()
        {
            // Arrange
            var @event = new AnalysisStartedEvent
            {
                ConfigFilePath = "/path/to/config.json",
                ProjectPath = "/path/to/project",
                AnalysisId = null
            };

            // Act
            var result = @event.IsValid();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasConfigFilePath_ThrowsArgumentNullException_WhenEventIsNull()
        {
            // Arrange
            AnalysisStartedEvent @event = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => @event.HasConfigFilePath());
        }

        [Fact]
        public void GetAnalysisSummary_ThrowsArgumentNullException_WhenEventIsNull()
        {
            // Arrange
            AnalysisStartedEvent @event = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => @event.GetAnalysisSummary());
        }

        [Fact]
        public void IsValid_ThrowsArgumentNullException_WhenEventIsNull()
        {
            // Arrange
            AnalysisStartedEvent @event = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => @event.IsValid());
        }
    }
}