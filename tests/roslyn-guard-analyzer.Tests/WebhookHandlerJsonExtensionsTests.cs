// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using System;
using Xunit;
using RoslynGuardAnalyzer.Integration;

namespace RoslynGuardAnalyzer.Tests;

public class WebhookHandlerJsonExtensionsTests
{
    [Fact]
    public void ToJson_Returns_NonEmptyString()
    {
        // Arrange
        var handler = new WebhookHandler();

        // Act
        var json = handler.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    [Fact]
    public void ToJson_WithIndentation_Produces_IndentedJson()
    {
        // Arrange
        var handler = new WebhookHandler();

        // Act
        var json = handler.ToJson(indented: true);

        // Assert
        // Indented JSON contains line breaks
        Assert.Contains(Environment.NewLine, json);
    }

    [Fact]
    public void ToJson_NullArgument_Throws_ArgumentNullException()
    {
        // Arrange
        WebhookHandler? handler = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => handler!.ToJson());
    }

    [Fact]
    public void FromJson_ValidJson_Returns_Object()
    {
        // Arrange
        var original = new WebhookHandler();
        var json = original.ToJson();

        // Act
        var result = WebhookHandlerJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<WebhookHandler>(result);
    }

    [Fact]
    public void FromJson_InvalidJson_Returns_Null()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act
        var result = WebhookHandlerJsonExtensions.FromJson(invalidJson);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_ValidJson_Returns_True_And_Object()
    {
        // Arrange
        var original = new WebhookHandler();
        var json = original.ToJson();

        // Act
        var success = WebhookHandlerJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.IsType<WebhookHandler>(result);
    }

    [Fact]
    public void TryFromJson_InvalidJson_Returns_False()
    {
        // Arrange
        var invalidJson = "not a json";

        // Act
        var success = WebhookHandlerJsonExtensions.TryFromJson(invalidJson, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_NullOrWhiteSpace_Returns_False()
    {
        // Arrange
        string nullJson = null!;
        string emptyJson = "";
        string whitespaceJson = "   ";

        // Act & Assert
        Assert.False(WebhookHandlerJsonExtensions.TryFromJson(nullJson, out var _));
        Assert.False(WebhookHandlerJsonExtensions.TryFromJson(emptyJson, out var _));
        Assert.False(WebhookHandlerJsonExtensions.TryFromJson(whitespaceJson, out var _));
    }
}
