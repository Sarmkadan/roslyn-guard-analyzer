#nullable enable

using System;
using FluentAssertions;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="AnalysisProjectExtensions"/>.
/// </summary>
public class AnalysisProjectExtensionsTests
{
    private static AnalysisProject CreateTestProject(string targetFramework = null, string[] sourceFiles = null, string[] properties = null)
    {
        var project = new AnalysisProject("TestProject", "/path/to/project");
        if (targetFramework != null) project.TargetFramework = targetFramework;
        if (sourceFiles != null) foreach (var file in sourceFiles) project.AddSourceFile(file);
        if (properties != null) foreach (var prop in properties) project.SetProperty(prop.Split(':')[0], prop.Split(':')[1]);
        return project;
    }

    #region HasProperty

    [Fact] public void HasProperty_Existing_ReturnsTrue() =>
        CreateTestProject(properties: ["TargetFramework:net8.0"]).HasProperty("TargetFramework").Should().BeTrue();

    [Fact] public void HasProperty_NonExisting_ReturnsFalse() =>
        CreateTestProject(properties: ["TargetFramework:net8.0"]).HasProperty("Version").Should().BeFalse();

    [Fact] public void HasProperty_NullProject_Throws() =>
        Assert.Throws<ArgumentNullException>(() => ((AnalysisProject)null!).HasProperty("key"));

    [Fact] public void HasProperty_NullKey_Throws() =>
        Assert.Throws<ArgumentNullException>(() => CreateTestProject().HasProperty(null!));

    #endregion

    #region GetAllCSharpFiles

    [Fact] public void GetAllCSharpFiles_ReturnsOnlyCSharpFiles()
    {
        var project = CreateTestProject(sourceFiles: [
            "/path/to/project/Program.cs",
            "/path/to/project/Class1.cs",
            "/path/to/project/ReadMe.md",
            "/path/to/project/Config.json"
        ]);
        var result = project.GetAllCSharpFiles();
        result.Should().HaveCount(2);
        result.Should().Contain("/path/to/project/Program.cs");
        result.Should().Contain("/path/to/project/Class1.cs");
    }

    [Fact] public void GetAllCSharpFiles_NullProject_Throws() =>
        Assert.Throws<ArgumentNullException>(() => ((AnalysisProject)null!).GetAllCSharpFiles());

    #endregion

    #region HasSameTargetFramework

    [Fact] public void HasSameTargetFramework_Same_ReturnsTrue()
    {
        var p1 = CreateTestProject("net8.0");
        var p2 = CreateTestProject("net8.0");
        p1.HasSameTargetFramework(p2).Should().BeTrue();
    }

    [Fact] public void HasSameTargetFramework_Different_ReturnsFalse()
    {
        var p1 = CreateTestProject("net8.0");
        var p2 = CreateTestProject("net6.0");
        p1.HasSameTargetFramework(p2).Should().BeFalse();
    }

    [Fact] public void HasSameTargetFramework_NullProject_Throws() =>
        Assert.Throws<ArgumentNullException>(() => ((AnalysisProject)null!).HasSameTargetFramework(CreateTestProject()));

    #endregion

    #region GetRequiredProperty

    [Fact] public void GetRequiredProperty_Existing_ReturnsValue()
    {
        var project = CreateTestProject(properties: ["TargetFramework:net8.0"]);
        project.GetRequiredProperty("TargetFramework").Should().Be("net8.0");
    }

    [Fact] public void GetRequiredProperty_NonExisting_Throws()
    {
        var project = CreateTestProject(properties: ["TargetFramework:net8.0"]);
        Assert.Throws<KeyNotFoundException>(() => project.GetRequiredProperty("Version"));
    }

    [Fact] public void GetRequiredProperty_NullProject_Throws() =>
        Assert.Throws<ArgumentNullException>(() => ((AnalysisProject)null!).GetRequiredProperty("key"));

    #endregion

    #region IsModernDotNetProject

    [Fact] public void IsModernDotNetProject_Modern_ReturnsTrue() =>
        CreateTestProject("net8.0").IsModernDotNetProject().Should().BeTrue();

    [Fact] public void IsModernDotNetProject_Legacy_ReturnsFalse() =>
        CreateTestProject("netframework4.8").IsModernDotNetProject().Should().BeFalse();

    [Fact] public void IsModernDotNetProject_NullTargetFramework_ReturnsFalse() =>
        CreateTestProject(null).IsModernDotNetProject().Should().BeFalse();

    [Fact] public void IsModernDotNetProject_NullProject_Throws() =>
        Assert.Throws<ArgumentNullException>(() => ((AnalysisProject)null!).IsModernDotNetProject());

    #endregion

    #region GetCSharpFileCount

    [Fact] public void GetCSharpFileCount_ReturnsCorrectCount()
    {
        var project = CreateTestProject(sourceFiles: [
            "/path/to/project/Program.cs",
            "/path/to/project/Class1.cs",
            "/path/to/project/ReadMe.md"
        ]);
        project.GetCSharpFileCount().Should().Be(2);
    }

    [Fact] public void GetCSharpFileCount_NullProject_Throws() =>
        Assert.Throws<ArgumentNullException>(() => ((AnalysisProject)null!).GetCSharpFileCount());

    #endregion

    #region HasCSharpFiles

    [Fact] public void HasCSharpFiles_WithFiles_ReturnsTrue() =>
        CreateTestProject(sourceFiles: ["/path/to/project/Program.cs"]).HasCSharpFiles().Should().BeTrue();

    [Fact] public void HasCSharpFiles_NoFiles_ReturnsFalse() =>
        CreateTestProject(sourceFiles: ["/path/to/project/ReadMe.md"]).HasCSharpFiles().Should().BeFalse();

    #endregion

    #region GetTargetFrameworkDisplay

    [Fact] public void GetTargetFrameworkDisplay_ReturnsFramework() =>
        CreateTestProject("net8.0").GetTargetFrameworkDisplay().Should().Be("net8.0");

    [Fact] public void GetTargetFrameworkDisplay_NullTargetFramework_ReturnsUnknown() =>
        CreateTestProject(null).GetTargetFrameworkDisplay().Should().Be("Unknown");

    #endregion
}