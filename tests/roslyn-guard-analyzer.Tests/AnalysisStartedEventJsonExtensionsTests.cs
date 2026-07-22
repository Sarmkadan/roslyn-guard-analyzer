#nullable enable

using System;
using System.Text.Json;
using FluentAssertions;
using RoslynGuardAnalyzer.Events;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="AnalysisStartedEventJsonExtensions"/>.
/// Tests JSON serialization and deserialization of AnalysisStartedEvent.
/// </summary>
public class AnalysisStartedEventJsonExtensionsTests
{
    private static readonly string _sampleProjectPath = "/path/to/sample/project.csproj";
    private static readonly string _sampleAnalysisId = Guid.NewGuid().ToString();
    private static readonly string _sampleConfigPath = "/path/to/config.json";

    #region ToJson Tests

    [Fact]
    public void ToJson_WithValidEvent_ReturnsNonEmptyJsonString()
    {
        // Arrange
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = _sampleProjectPath,
            AnalysisId = _sampleAnalysisId,
            ConfigFilePath = _sampleConfigPath
        };

        // Act
        var json = @event.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("projectPath");
        json.Should().Contain("analysisId");
        json.Should().Contain("configFilePath");
    }

    [Fact]
    public void ToJson_WithValidEvent_ProducesValidJson()
    {
        // Arrange
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = _sampleProjectPath,
            AnalysisId = _sampleAnalysisId,
            ConfigFilePath = _sampleConfigPath
        };

        // Act
        var json = @event.ToJson();

        // Assert
        Action act = () => Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = _sampleProjectPath,
            AnalysisId = _sampleAnalysisId
        };

        // Act
        var json = @event.ToJson(indented: true);

        // Assert
        json.Should().Contain("\n"); // Should have newlines for formatting
        json.Should().Contain("  "); // Should have indentation spaces
    }

    [Fact]
    public void ToJson_WithIndentedFalse_ReturnsCompactJson()
    {
        // Arrange
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = _sampleProjectPath,
            AnalysisId = _sampleAnalysisId
        };

        // Act
        var json = @event.ToJson(indented: false);

        // Assert
        json.Should().NotContain("\n"); // Should not have newlines
        json.Should().NotContain("  "); // Should not have indentation
    }

    [Fact]
    public void ToJson_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        AnalysisStartedEvent? @event = null;

        // Act
        Action act = () => @event!.ToJson();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToJson_WithEventWithoutConfigFilePath_SerializesCorrectly()
    {
        // Arrange
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = _sampleProjectPath,
            AnalysisId = _sampleAnalysisId
            // ConfigFilePath is null
        };

        // Act
        var json = @event.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("projectPath");
        json.Should().Contain("analysisId");
        // When writing null, it should be omitted due to DefaultIgnoreCondition.WhenWritingNull
        json.Should().NotContain("configFilePath");
    }

    #endregion

    #region FromJson Tests

    [Fact]
    public void FromJson_WithValidJson_ReturnsDeserializedEvent()
    {
        // Arrange
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = _sampleProjectPath,
            AnalysisId = _sampleAnalysisId,
            ConfigFilePath = _sampleConfigPath
        };
        var json = @event.ToJson();

        // Act
        var deserializedEvent = AnalysisStartedEventJsonExtensions.FromJson(json);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent.ProjectPath.Should().Be(_sampleProjectPath);
        deserializedEvent.AnalysisId.Should().Be(_sampleAnalysisId);
        deserializedEvent.ConfigFilePath.Should().Be(_sampleConfigPath);
        deserializedEvent.EventType.Should().Be("AnalysisStarted");
        deserializedEvent.EventId.Should().NotBeNullOrWhiteSpace();
        deserializedEvent.TimestampUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void FromJson_WithValidJsonWithoutConfigFilePath_ReturnsDeserializedEvent()
    {
        // Arrange
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = _sampleProjectPath,
            AnalysisId = _sampleAnalysisId
            // ConfigFilePath is null
        };
        var json = @event.ToJson();

        // Act
        var deserializedEvent = AnalysisStartedEventJsonExtensions.FromJson(json);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent.ProjectPath.Should().Be(_sampleProjectPath);
        deserializedEvent.AnalysisId.Should().Be(_sampleAnalysisId);
        deserializedEvent.ConfigFilePath.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act
        Action act = () => AnalysisStartedEventJsonExtensions.FromJson(json!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        var json = string.Empty;

        // Act
        Action act = () => AnalysisStartedEventJsonExtensions.FromJson(json);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromJson_WithWhitespaceString_ThrowsArgumentException()
    {
        // Arrange
        var json = "   \n\t  ";

        // Act
        Action act = () => AnalysisStartedEventJsonExtensions.FromJson(json);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromJson_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var json = "{ invalid json";

        // Act
        Action act = () => AnalysisStartedEventJsonExtensions.FromJson(json);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void FromJson_WithEmptyObjectJson_ThrowsJsonException()
    {
        // Arrange
        var json = "{}";

        // Act
        Action act = () => AnalysisStartedEventJsonExtensions.FromJson(json);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void FromJson_WithMissingRequiredFields_ThrowsJsonException()
    {
        // Arrange
        var json = "{\"projectPath\": \"/some/path\"}";

        // Act
        Action act = () => AnalysisStartedEventJsonExtensions.FromJson(json);

        // Assert
        act.Should().Throw<JsonException>();
    }

    #endregion

    #region TryFromJson Tests

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializesEvent()
    {
        // Arrange
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = _sampleProjectPath,
            AnalysisId = _sampleAnalysisId,
            ConfigFilePath = _sampleConfigPath
        };
        var json = @event.ToJson();

        // Act
        var result = AnalysisStartedEventJsonExtensions.TryFromJson(json, out var deserializedEvent);

        // Assert
        result.Should().BeTrue();
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.ProjectPath.Should().Be(_sampleProjectPath);
        deserializedEvent.AnalysisId.Should().Be(_sampleAnalysisId);
        deserializedEvent.ConfigFilePath.Should().Be(_sampleConfigPath);
    }

    [Fact]
    public void TryFromJson_WithValidJsonWithoutConfigFilePath_ReturnsTrueAndDeserializesEvent()
    {
        // Arrange
        var @event = new AnalysisStartedEvent
        {
            ProjectPath = _sampleProjectPath,
            AnalysisId = _sampleAnalysisId
        };
        var json = @event.ToJson();

        // Act
        var result = AnalysisStartedEventJsonExtensions.TryFromJson(json, out var deserializedEvent);

        // Assert
        result.Should().BeTrue();
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.ProjectPath.Should().Be(_sampleProjectPath);
        deserializedEvent.AnalysisId.Should().Be(_sampleAnalysisId);
        deserializedEvent.ConfigFilePath.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithNullJson_ReturnsFalseAndNullValue()
    {
        // Arrange
        string? json = null;

        // Act
        var result = AnalysisStartedEventJsonExtensions.TryFromJson(json, out var deserializedEvent);

        // Assert
        result.Should().BeFalse();
        deserializedEvent.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithEmptyString_ReturnsFalseAndNullValue()
    {
        // Arrange
        var json = string.Empty;

        // Act
        var result = AnalysisStartedEventJsonExtensions.TryFromJson(json, out var deserializedEvent);

        // Assert
        result.Should().BeFalse();
        deserializedEvent.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithWhitespaceString_ReturnsFalseAndNullValue()
    {
        // Arrange
        var json = "   \n\t  ";

        // Act
        var result = AnalysisStartedEventJsonExtensions.TryFromJson(json, out var deserializedEvent);

        // Assert
        result.Should().BeFalse();
        deserializedEvent.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNullValue()
    {
        // Arrange
        var json = "{ invalid json";

        // Act
        var result = AnalysisStartedEventJsonExtensions.TryFromJson(json, out var deserializedEvent);

        // Assert
        result.Should().BeFalse();
        deserializedEvent.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithEmptyObjectJson_ReturnsFalseAndNullValue()
    {
        // Arrange
        var json = "{}";

        // Act
        var result = AnalysisStartedEventJsonExtensions.TryFromJson(json, out var deserializedEvent);

        // Assert
        result.Should().BeFalse();
        deserializedEvent.Should().BeNull();
    }

    #endregion

    #region Round-trip Tests

    [Fact]
    public void RoundTrip_ToJsonThenFromJson_PreservesCustomProperties()
    {
        // Arrange
        var originalEvent = new AnalysisStartedEvent
        {
            ProjectPath = "/custom/project/path/project.csproj",
            AnalysisId = Guid.NewGuid().ToString(),
            ConfigFilePath = "/custom/config/settings.json"
        };

        // Act
        var json = originalEvent.ToJson();
        var deserializedEvent = AnalysisStartedEventJsonExtensions.FromJson(json);

        // Assert - Check that custom properties are preserved
        deserializedEvent.ProjectPath.Should().Be(originalEvent.ProjectPath);
        deserializedEvent.AnalysisId.Should().Be(originalEvent.AnalysisId);
        deserializedEvent.ConfigFilePath.Should().Be(originalEvent.ConfigFilePath);
        deserializedEvent.EventType.Should().Be(originalEvent.EventType);
        // EventId and TimestampUtc are auto-generated, so we don't compare them
    }

    [Fact]
    public void RoundTrip_WithNullConfigFilePath_PreservesCustomProperties()
    {
        // Arrange
        var originalEvent = new AnalysisStartedEvent
        {
            ProjectPath = "/custom/project/path/project.csproj",
            AnalysisId = Guid.NewGuid().ToString()
            // ConfigFilePath is null
        };

        // Act
        var json = originalEvent.ToJson();
        var deserializedEvent = AnalysisStartedEventJsonExtensions.FromJson(json);

        // Assert - Check that custom properties are preserved
        deserializedEvent.ProjectPath.Should().Be(originalEvent.ProjectPath);
        deserializedEvent.AnalysisId.Should().Be(originalEvent.AnalysisId);
        deserializedEvent.ConfigFilePath.Should().Be(originalEvent.ConfigFilePath);
        deserializedEvent.EventType.Should().Be(originalEvent.EventType);
        // EventId and TimestampUtc are auto-generated, so we don't compare them
    }

    #endregion
}