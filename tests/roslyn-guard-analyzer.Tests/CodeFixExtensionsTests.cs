using System;
using RoslynGuardAnalyzer.CodeFixes;
using RoslynGuardAnalyzer.Core;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class CodeFixExtensionsTests
{
    // Helper to create a basic valid CodeFix
    private static CodeFix CreateFix(
        SeverityLevel severity = SeverityLevel.Warning,
        bool isBreaking = false,
        string ruleId = "RG001",
        string title = "Sample fix",
        string filePath = "/src/Program.cs",
        int startLine = 10,
        int endLine = 10,
        string original = "var x = 1;",
        string replacement = "var x = 2;",
        string description = "Fix description")
    {
        return new CodeFix
        {
            Severity = severity,
            IsBreakingChange = isBreaking,
            RuleId = ruleId,
            Title = title,
            FilePath = filePath,
            StartLine = startLine,
            EndLine = endLine,
            OriginalCode = original,
            ReplacementCode = replacement,
            Description = description,
            GeneratedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void IsMoreSevereThan_Returns_True_When_Higher_Severity()
    {
        var higher = CreateFix(severity: SeverityLevel.Critical);
        var lower = CreateFix(severity: SeverityLevel.Info);

        Assert.True(higher.IsMoreSevereThan(lower));
        Assert.False(lower.IsMoreSevereThan(higher));
    }

    [Fact]
    public void IsMoreSevereThan_Throws_On_Null_Fix()
    {
        var other = CreateFix();
        Assert.Throws<ArgumentNullException>(() => ((CodeFix)null!).IsMoreSevereThan(other));
        Assert.Throws<ArgumentNullException>(() => other.IsMoreSevereThan(null!));
    }

    [Fact]
    public void IsLessSevereThan_Returns_True_When_Lower_Severity()
    {
        var low = CreateFix(severity: SeverityLevel.Info);
        var high = CreateFix(severity: SeverityLevel.Error);

        Assert.True(low.IsLessSevereThan(high));
        Assert.False(high.IsLessSevereThan(low));
    }

    [Fact]
    public void GetSeverityString_Returns_Correct_Text()
    {
        Assert.Equal("Info", CreateFix(severity: SeverityLevel.Info).GetSeverityString());
        Assert.Equal("Warning", CreateFix(severity: SeverityLevel.Warning).GetSeverityString());
        Assert.Equal("Error", CreateFix(severity: SeverityLevel.Error).GetSeverityString());
        Assert.Equal("Critical", CreateFix(severity: SeverityLevel.Critical).GetSeverityString());
    }

    [Fact]
    public void GetSeverityString_Throws_On_Unknown_Value()
    {
        var fix = CreateFix(severity: (SeverityLevel)99);
        Assert.Throws<InvalidOperationException>(() => fix.GetSeverityString());
    }

    [Fact]
    public void IsBreaking_Returns_Property_Value()
    {
        var breaking = CreateFix(isBreaking: true);
        var notBreaking = CreateFix(isBreaking: false);

        Assert.True(breaking.IsBreaking());
        Assert.False(notBreaking.IsBreaking());
    }

    [Fact]
    public void GetDisplaySummary_Includes_All_Parts()
    {
        var fix = CreateFix(
            severity: SeverityLevel.Error,
            isBreaking: true,
            ruleId: "RG123",
            title: "Rename method",
            filePath: "/src/Program.cs",
            startLine: 42,
            description: "Rename for clarity");

        var summary = fix.GetDisplaySummary();

        // Expected format:
        // [RG123] Rename method — Error 🔴 BREAKING
        // /src/Program.cs:42
        // Rename for clarity
        var expectedFirstLine = "[RG123] Rename method — Error 🔴 BREAKING";
        var expectedSecondLine = "/src/Program.cs:42";
        var expectedThirdLine = "Rename for clarity";

        var lines = summary.Split(Environment.NewLine);
        Assert.Equal(expectedFirstLine, lines[0]);
        Assert.Equal(expectedSecondLine, lines[1]);
        Assert.Equal(expectedThirdLine, lines[2]);
    }

    [Fact]
    public void GetAge_Returns_Seconds_Ago_For_Recent_Fix()
    {
        var fix = CreateFix();
        fix.GeneratedAt = DateTime.UtcNow.AddSeconds(-30); // 30 seconds ago

        var age = fix.GetAge();
        Assert.Equal("30s ago", age);
    }

    [Fact]
    public void TargetsFile_Is_Case_Insensitive()
    {
        var fix = CreateFix(filePath: "C:\\Project\\File.cs");
        Assert.True(fix.TargetsFile("c:\\project\\file.cs"));
        Assert.False(fix.TargetsFile("C:\\Other\\File.cs"));
    }

    [Fact]
    public void TargetsFile_Throws_On_Null_Or_Empty_Arguments()
    {
        var fix = CreateFix();
        Assert.Throws<ArgumentNullException>(() => ((CodeFix)null!).TargetsFile("a.cs"));
        Assert.Throws<ArgumentException>(() => fix.TargetsFile(string.Empty));
        Assert.Throws<ArgumentException>(() => fix.TargetsFile("   "));
    }

    [Fact]
    public void GetFileExtension_Returns_Correct_Value()
    {
        var fix = CreateFix(filePath: "/src/Program.cs");
        Assert.Equal(".cs", fix.GetFileExtension());

        var noExt = CreateFix(filePath: "README");
        Assert.Equal(string.Empty, noExt.GetFileExtension());

        var emptyPath = CreateFix(filePath: string.Empty);
        Assert.Equal(string.Empty, emptyPath.GetFileExtension());
    }

    [Fact]
    public void IsInLineRange_Handles_Overlap_And_Normalization()
    {
        var fix = CreateFix(startLine: 10, endLine: 20);

        // Overlap
        Assert.True(fix.IsInLineRange(15, 25));
        // Inside
        Assert.True(fix.IsInLineRange(5, 12));
        // Exact match
        Assert.True(fix.IsInLineRange(10, 20));
        // No overlap
        Assert.False(fix.IsInLineRange(21, 30));

        // Normalized range (start > end)
        Assert.True(fix.IsInLineRange(20, 10));
    }

    [Fact]
    public void IsInLineRange_Throws_On_Invalid_Range()
    {
        var fix = CreateFix();
        Assert.Throws<ArgumentOutOfRangeException>(() => fix.IsInLineRange(0, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => fix.IsInLineRange(5, 0));
    }

    [Fact]
    public void GetCodeContext_Returns_Combined_When_Both_Are_Present()
    {
        var fix = CreateFix(
            original: "var a = 1;",
            replacement: "var a = 2;",
            startLine: 3);

        var context = fix.GetCodeContext();

        var expected = string.Join(Environment.NewLine, new[]
        {
            "Original (3):",
            "var a = 1;",
            "",
            "Replacement (3):",
            "var a = 2;"
        });

        Assert.Equal(expected, context);
    }

    [Fact]
    public void GetCodeContext_Returns_Only_Original_When_Replacement_Missing()
    {
        var fix = CreateFix(original: "var a = 1;", replacement: string.Empty, startLine: 7);
        var context = fix.GetCodeContext();

        var expected = string.Join(Environment.NewLine, new[]
        {
            "Original (7):",
            "var a = 1;"
        });

        Assert.Equal(expected, context);
    }

    [Fact]
    public void GetCodeContext_Returns_Only_Replacement_When_Original_Missing()
    {
        var fix = CreateFix(original: string.Empty, replacement: "var a = 2;", startLine: 9);
        var context = fix.GetCodeContext();

        var expected = string.Join(Environment.NewLine, new[]
        {
            "Replacement (9):",
            "var a = 2;"
        });

        Assert.Equal(expected, context);
    }

    [Fact]
    public void GetCodeContext_Returns_Message_When_Nothing_Available()
    {
        var fix = CreateFix(original: string.Empty, replacement: string.Empty);
        var context = fix.GetCodeContext();

        Assert.Equal("No code context available.", context);
    }

    [Fact]
    public void GetCodeContext_Throws_On_Null_Fix()
    {
        Assert.Throws<ArgumentNullException>(() => ((CodeFix)null!).GetCodeContext());
    }
}
