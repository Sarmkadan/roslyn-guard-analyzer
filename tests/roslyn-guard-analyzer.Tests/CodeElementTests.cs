using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class CodeElementTests
{
    [Fact]
    public void Constructor_SetsDefaultValuesCorrectly()
    {
        var element = new CodeElement("Test", CodeElementType.Class, "test.cs");

        Assert.Equal("Test", element.Name);
        Assert.Equal(CodeElementType.Class, element.ElementType);
        Assert.Equal("test.cs", element.FilePath);
        Assert.NotNull(element.Id);
        Assert.Empty(element.Attributes);
    }

    [Fact]
    public void GetFullyQualifiedName_ReturnsCorrectName()
    {
        var element = new CodeElement("MyClass", CodeElementType.Class, "file.cs")
        {
            Namespace = "MyNamespace",
            ParentName = "MyParent"
        };

        Assert.Equal("MyNamespace.MyParent.MyClass", element.GetFullyQualifiedName());
    }

    [Fact]
    public void AddDependency_AddsUniqueDependency()
    {
        var element = new CodeElement();
        element.AddDependency("Dep1");
        element.AddDependency("Dep1");

        Assert.Single(element.Dependencies);
        Assert.Contains("Dep1", element.Dependencies);
    }

    [Fact]
    public void HasAttribute_ReturnsTrueIfAttributeExists()
    {
        var element = new CodeElement();
        element.AddAttribute("TestAttribute");

        Assert.True(element.HasAttribute("Test"));
    }

    [Fact]
    public void GetLocation_ReturnsFormattedString()
    {
        var element = new CodeElement("Test", CodeElementType.Class, "/path/to/file.cs")
        {
            StartLineNumber = 10,
            EndLineNumber = 20
        };

        Assert.Equal("file.cs(10-20)", element.GetLocation());
    }

    [Fact]
    public void IsInNamespace_ReturnsTrueForCorrectPrefix()
    {
        var element = new CodeElement { Namespace = "MyNamespace.SubNamespace" };

        Assert.True(element.IsInNamespace("MyNamespace"));
    }

    [Fact]
    public void IsContainer_ReturnsTrueForClass()
    {
        var element = new CodeElement { ElementType = CodeElementType.Class };

        Assert.True(element.IsContainer());
    }

    [Fact]
    public void IsValid_ReturnsTrueWhenValid()
    {
        var element = new CodeElement("Class", CodeElementType.Class, "file.cs")
        {
            StartLineNumber = 1,
            EndLineNumber = 5
        };

        Assert.True(element.IsValid());
    }

    [Fact]
    public void IsValid_ReturnsFalseWhenInvalid()
    {
        var element = new CodeElement("Class", CodeElementType.Class, "file.cs")
        {
            StartLineNumber = 5,
            EndLineNumber = 1
        };

        Assert.False(element.IsValid());
    }
}
