// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using System;
using RoslynGuardAnalyzer.CodeFixes;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class CodeFixTests
{
    [Fact]
    public void Constructor_Initializes_Defaults()
    {
        // Arrange & Act
        var fix = new CodeFix();

        // Assert
        // Id should be a non‑empty GUID string
        Assert.False(string.IsNullOrWhiteSpace(fix.Id));
        Assert.True(Guid.TryParse(fix.Id, out _));

        // String properties default to empty string
        Assert.Equal(string.Empty, fix.ViolationId);
        Assert.Equal(string.Empty, fix.RuleId);
        Assert.Equal(string.Empty, fix.Title);
        Assert.Equal(string.Empty, fix.Description);
        Assert.Equal(string.Empty, fix.FilePath);
        Assert.Equal(string.Empty, fix.OriginalCode);
        Assert.Equal(string.Empty, fix.ReplacementCode);

        // Numeric defaults
        Assert.Equal(0, fix.StartLine);
        Assert.Equal(0, fix.EndLine);

        // Severity default
        Assert.Equal(SeverityLevel.Warning, fix.Severity);

        // GeneratedAt should be recent (within the last minute)
        var now = DateTime.UtcNow;
        Assert.InRange(fix.GeneratedAt, now.AddMinutes(-1), now);
    }

    [Fact]
    public void GetSummary_Returns_Expected_Format()
    {
        // Arrange
        var fix = new CodeFix
        {
            RuleId = "RG001",
            Title = "Rename method",
            FilePath = "/src/Program.cs",
            StartLine = 42
        };

        // Act
        var summary = fix.GetSummary();

        // Assert
        Assert.Equal("[RG001] Rename method — /src/Program.cs:42", summary);
    }

    [Fact]
    public void IsValid_Returns_True_When_All_Required_Fields_Are_Present()
    {
        // Arrange
        var fix = new CodeFix
        {
            RuleId = "RG002",
            FilePath = "C:\\Project\\File.cs",
            OriginalCode = "var x = 1;",
            StartLine = 10
        };

        // Act
        var result = fix.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_Returns_False_When_RuleId_Is_Missing()
    {
        // Arrange
        var fix = new CodeFix
        {
            // RuleId left empty
            FilePath = "C:\\Project\\File.cs",
            OriginalCode = "var x = 1;",
            StartLine = 10
        };

        // Act
        var result = fix.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_Returns_False_When_StartLine_Is_Not_Positive()
    {
        // Arrange
        var fix = new CodeFix
        {
            RuleId = "RG003",
            FilePath = "C:\\Project\\File.cs",
            OriginalCode = "var x = 1;",
            StartLine = 0 // invalid
        };

        // Act
        var result = fix.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_Returns_False_When_FilePath_Is_Empty()
    {
        // Arrange
        var fix = new CodeFix
        {
            RuleId = "RG004",
            // FilePath left empty
            OriginalCode = "var x = 1;",
            StartLine = 5
        };

        // Act
        var result = fix.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_Returns_False_When_OriginalCode_Is_Empty()
    {
        // Arrange
        var fix = new CodeFix
        {
            RuleId = "RG005",
            FilePath = "C:\\Project\\File.cs",
            // OriginalCode left empty
            StartLine = 5
        };

        // Act
        var result = fix.IsValid();

        // Assert
        Assert.False(result);
    }
}
