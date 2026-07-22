#nullable enable

using FluentAssertions;
using RoslynGuardAnalyzer.Events;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="AnalysisStartedEvent"/>.
/// Tests the event that fires when analysis starts for a project.
/// </summary>
public class AnalysisStartedEventTests
{
    #region Constructor and Property Initialization

    [Fact]
    public void Constructor_WithRequiredProperties_InitializesCorrectly()
    {
        // Arrange & Act
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = "/path/to/project.csproj",
            AnalysisId = Guid.NewGuid().ToString(),
            ConfigFilePath = "/path/to/config.json"
        };

        // Assert
        @event.Should().NotBeNull();
        @event.ProjectPath.Should().Be("/path/to/project.csproj");
        @event.AnalysisId.Should().NotBeNullOrWhiteSpace();
        @event.ConfigFilePath.Should().Be("/path/to/config.json");
        @event.EventType.Should().Be("AnalysisStarted");
        @event.EventId.Should().NotBeNullOrWhiteSpace();
        @event.TimestampUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.Metadata.Should().NotBeNull();
        @event.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithRequiredPropertiesOnly_InitializesCorrectly()
    {
        // Arrange & Act
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = "/path/to/project.csproj",
            AnalysisId = Guid.NewGuid().ToString()
        };

        // Assert
        @event.Should().NotBeNull();
        @event.ProjectPath.Should().Be("/path/to/project.csproj");
        @event.AnalysisId.Should().NotBeNullOrWhiteSpace();
        @event.ConfigFilePath.Should().BeNull();
        @event.EventType.Should().Be("AnalysisStarted");
    }

    [Fact]
    public void Constructor_WithEmptyProjectPath_CompilesSuccessfully()
    {
        // Arrange
        var projectPath = string.Empty;
        var analysisId = Guid.NewGuid().ToString();

        // Act
        var act = () => new AnalysisStartedEvent
        {
            ProjectPath = projectPath,
            AnalysisId = analysisId
        };

        // Assert - With init-only setters and nullable reference types, empty strings compile but may not be ideal
        // The class uses 'required' properties which are validated at compile time with nullable reference types
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithWhitespaceProjectPath_CompilesSuccessfully()
    {
        // Arrange
        var projectPath = "   ";
        var analysisId = Guid.NewGuid().ToString();

        // Act
        var act = () => new AnalysisStartedEvent
        {
            ProjectPath = projectPath,
            AnalysisId = analysisId
        };

        // Assert - With init-only setters and nullable reference types, whitespace strings compile
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithNullProjectPath_CompilesSuccessfully()
    {
        // Arrange
        string? projectPath = null;
        var analysisId = Guid.NewGuid().ToString();

        // Act
        var act = () => new AnalysisStartedEvent
        {
            ProjectPath = projectPath!,
            AnalysisId = analysisId
        };

        // Assert - With init-only setters and nullable reference types, null compiles but violates 'required'
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithEmptyAnalysisId_CompilesSuccessfully()
    {
        // Arrange
        var projectPath = "/path/to/project.csproj";
        var analysisId = string.Empty;

        // Act
        var act = () => new AnalysisStartedEvent
        {
            ProjectPath = projectPath,
            AnalysisId = analysisId
        };

        // Assert - With init-only setters and nullable reference types, empty strings compile
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithWhitespaceAnalysisId_CompilesSuccessfully()
    {
        // Arrange
        var projectPath = "/path/to/project.csproj";
        var analysisId = "   ";

        // Act
        var act = () => new AnalysisStartedEvent
        {
            ProjectPath = projectPath,
            AnalysisId = analysisId
        };

        // Assert - With init-only setters and nullable reference types, whitespace strings compile
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithNullAnalysisId_CompilesSuccessfully()
    {
        // Arrange
        var projectPath = "/path/to/project.csproj";
        string? analysisId = null;

        // Act
        var act = () => new AnalysisStartedEvent
        {
            ProjectPath = projectPath,
            AnalysisId = analysisId!
        };

        // Assert - With init-only setters and nullable reference types, null compiles but violates 'required'
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithVeryLongPaths_InitializesCorrectly()
    {
        // Arrange
        var longProjectPath = new string('a', 1000) + ".csproj";
        var longAnalysisId = Guid.NewGuid().ToString();
        var longConfigPath = new string('b', 1000) + ".json";

        // Act
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = longProjectPath,
            AnalysisId = longAnalysisId,
            ConfigFilePath = longConfigPath
        };

        // Assert
        @event.ProjectPath.Should().Be(longProjectPath);
        @event.AnalysisId.Should().Be(longAnalysisId);
        @event.ConfigFilePath.Should().Be(longConfigPath);
    }

    #endregion

    #region Property Validation

    [Fact]
    public void ProjectPath_Getter_ReturnsCorrectValue()
    {
        // Arrange
        var expectedPath = "/custom/project/path.csproj";
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = expectedPath,
            AnalysisId = Guid.NewGuid().ToString()
        };

        // Act
        var actualPath = @event.ProjectPath;

        // Assert
        actualPath.Should().Be(expectedPath);
    }

    [Fact]
    public void AnalysisId_Getter_ReturnsCorrectValue()
    {
        // Arrange
        var expectedId = Guid.NewGuid().ToString();
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = "/path/to/project.csproj",
            AnalysisId = expectedId
        };

        // Act
        var actualId = @event.AnalysisId;

        // Assert
        actualId.Should().Be(expectedId);
    }

    [Fact]
    public void ConfigFilePath_Getter_ReturnsCorrectValue()
    {
        // Arrange
        var expectedConfigPath = "/path/to/config.json";
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = "/path/to/project.csproj",
            AnalysisId = Guid.NewGuid().ToString(),
            ConfigFilePath = expectedConfigPath
        };

        // Act
        var actualConfigPath = @event.ConfigFilePath;

        // Assert
        actualConfigPath.Should().Be(expectedConfigPath);
    }

    [Fact]
    public void ConfigFilePath_Getter_WhenNull_ReturnsNull()
    {
        // Arrange
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = "/path/to/project.csproj",
            AnalysisId = Guid.NewGuid().ToString()
        };

        // Act
        var configFilePath = @event.ConfigFilePath;

        // Assert
        configFilePath.Should().BeNull();
    }

    #endregion

    #region Event Inheritance

    [Fact]
    public void Event_InheritsFromEventBaseClass()
    {
        // Arrange & Act
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = "/path/to/project.csproj",
            AnalysisId = Guid.NewGuid().ToString()
        };

        // Assert
        @event.Should().BeAssignableTo<Event>();
    }

    [Fact]
    public void Event_ImplementsIEventInterface()
    {
        // Arrange & Act
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = "/path/to/project.csproj",
            AnalysisId = Guid.NewGuid().ToString()
        };

        // Assert
        @event.Should().BeAssignableTo<RoslynGuardAnalyzer.Events.IEvent>();
    }

    [Fact]
    public void Event_HasUniqueEventId()
    {
        // Arrange
        var @event1 = new AnalysisStartedEvent
        {
            ProjectPath = "/path/to/project1.csproj",
            AnalysisId = Guid.NewGuid().ToString()
        };
        var @event2 = new AnalysisStartedEvent
        {
            ProjectPath = "/path/to/project2.csproj",
            AnalysisId = Guid.NewGuid().ToString()
        };

        // Act
        var eventId1 = @event1.EventId;
        var eventId2 = @event2.EventId;

        // Assert
        eventId1.Should().NotBeNullOrWhiteSpace();
        eventId2.Should().NotBeNullOrWhiteSpace();
        eventId1.Should().NotBe(eventId2);
    }

    [Fact]
    public void Event_HasCorrectEventType()
    {
        // Arrange & Act
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = "/path/to/project.csproj",
            AnalysisId = Guid.NewGuid().ToString()
        };

        // Assert
        @event.EventType.Should().Be("AnalysisStarted");
    }

    [Fact]
    public void Event_HasTimestampUtc()
    {
        // Arrange & Act
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = "/path/to/project.csproj",
            AnalysisId = Guid.NewGuid().ToString()
        };

        // Assert
        @event.TimestampUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Event_HasMetadataDictionary()
    {
        // Arrange & Act
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = "/path/to/project.csproj",
            AnalysisId = Guid.NewGuid().ToString()
        };

        // Assert
        @event.Metadata.Should().NotBeNull();
        @event.Metadata.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_WithSpecialCharactersInPaths_InitializesCorrectly()
    {
        // Arrange
        var specialPath = @"/path/with spaces/and-dashes_and.dots/project.csproj";
        var specialAnalysisId = Guid.NewGuid().ToString();
        var specialConfigPath = @"/path/with spaces/config.json";

        // Act
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = specialPath,
            AnalysisId = specialAnalysisId,
            ConfigFilePath = specialConfigPath
        };

        // Assert
        @event.ProjectPath.Should().Be(specialPath);
        @event.AnalysisId.Should().Be(specialAnalysisId);
        @event.ConfigFilePath.Should().Be(specialConfigPath);
    }

    [Fact]
    public void Constructor_WithRelativePaths_InitializesCorrectly()
    {
        // Arrange
        var relativePath = "./src/project.csproj";
        var relativeAnalysisId = Guid.NewGuid().ToString();

        // Act
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = relativePath,
            AnalysisId = relativeAnalysisId
        };

        // Assert
        @event.ProjectPath.Should().Be(relativePath);
        @event.AnalysisId.Should().Be(relativeAnalysisId);
    }

    [Fact]
    public void Constructor_WithAbsolutePaths_InitializesCorrectly()
    {
        // Arrange
        var absolutePath = "/home/user/projects/myapp/src/project.csproj";
        var absoluteAnalysisId = Guid.NewGuid().ToString();

        // Act
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = absolutePath,
            AnalysisId = absoluteAnalysisId
        };

        // Assert
        @event.ProjectPath.Should().Be(absolutePath);
        @event.AnalysisId.Should().Be(absoluteAnalysisId);
    }

    [Fact]
    public void Metadata_CanBeModifiedAfterCreation()
    {
        // Arrange
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = "/path/to/project.csproj",
            AnalysisId = Guid.NewGuid().ToString()
        };

        // Act
        @event.Metadata["customKey"] = "customValue";
        @event.Metadata["anotherKey"] = 123;

        // Assert
        @event.Metadata.Should().ContainKey("customKey").WhoseValue.Should().Be("customValue");
        @event.Metadata.Should().ContainKey("anotherKey").WhoseValue.Should().Be(123);
    }

    #endregion
}