using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class AnalysisProjectTests
{
    [Fact]
    public void ToString_ReturnsNameAndPath()
    {
        // Arrange
        var project = new AnalysisProject("MyApp", "/src/MyApp/MyApp.csproj");

        // Act
        var result = project.ToString();

        // Assert
        Assert.Equal("MyApp (/src/MyApp/MyApp.csproj)", result);
    }

    [Fact]
    public void ToString_DoesNotIncludeProperties()
    {
        // Arrange
        var project = new AnalysisProject("MyApp", "/src/MyApp/MyApp.csproj");
        project.SetProperty("ConnectionString", "Server=secret;Password=hunter2");

        // Act
        var result = project.ToString();

        // Assert
        Assert.DoesNotContain("ConnectionString", result);
        Assert.DoesNotContain("hunter2", result);
    }
}
