// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using System;
using Xunit;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Tests;

public class CodeElementExtensionsTests
{
    // Note: CodeElement also declares an instance HasAttribute(string) method with different
    // (case-insensitive, non-validating) semantics, so the extension method under test is
    // invoked explicitly as a static call to avoid overload resolution picking the instance member.

    [Fact]
    public void HasAttribute_Returns_True_When_Attribute_Present()
    {
        var element = new CodeElement("MyClass", CodeElementType.Class, "file.cs");
        element.AddAttribute("Obsolete");

        Assert.True(CodeElementExtensions.HasAttribute(element, "Obsolete"));
    }

    [Fact]
    public void HasAttribute_Returns_False_When_Attribute_Missing()
    {
        var element = new CodeElement("MyClass", CodeElementType.Class, "file.cs");

        Assert.False(CodeElementExtensions.HasAttribute(element, "Obsolete"));
    }

    [Fact]
    public void HasAttribute_Throws_When_Element_Is_Null()
    {
        CodeElement? element = null;

        Assert.Throws<ArgumentNullException>(() => CodeElementExtensions.HasAttribute(element!, "Obsolete"));
    }

    [Fact]
    public void HasAttribute_Throws_When_AttributeName_Is_Null_Or_Empty()
    {
        var element = new CodeElement("MyClass", CodeElementType.Class, "file.cs");

        Assert.Throws<ArgumentException>(() => CodeElementExtensions.HasAttribute(element, string.Empty));
    }

    [Fact]
    public void GetDisplayName_Combines_Namespace_Parent_And_Name()
    {
        var element = new CodeElement("MyMethod", CodeElementType.Method, "file.cs")
        {
            Namespace = "MyNamespace",
            ParentName = "MyClass"
        };

        Assert.Equal("MyNamespace.MyClass.MyMethod", element.GetDisplayName());
    }

    [Fact]
    public void GetDisplayName_Omits_Missing_Namespace_And_Parent()
    {
        var element = new CodeElement("MyClass", CodeElementType.Class, "file.cs");
        element.Namespace = string.Empty;

        Assert.Equal("MyClass", element.GetDisplayName());
    }

    [Fact]
    public void GetDisplayName_Throws_When_Element_Is_Null()
    {
        CodeElement? element = null;

        Assert.Throws<ArgumentNullException>(() => element!.GetDisplayName());
    }

    [Fact]
    public void IsTopLevelElement_Returns_True_When_No_Parent()
    {
        var element = new CodeElement("MyClass", CodeElementType.Class, "file.cs");

        Assert.True(element.IsTopLevelElement());
    }

    [Fact]
    public void IsTopLevelElement_Returns_False_When_Parent_Present()
    {
        var element = new CodeElement("MyMethod", CodeElementType.Method, "file.cs")
        {
            ParentName = "MyClass"
        };

        Assert.False(element.IsTopLevelElement());
    }

    [Fact]
    public void GetCodeLocation_Returns_FormattedString()
    {
        var element = new CodeElement("MyClass", CodeElementType.Class, "file.cs")
        {
            StartLineNumber = 10,
            EndLineNumber = 20
        };

        Assert.Equal("file.cs (10-20)", element.GetCodeLocation());
    }

    [Fact]
    public void GetCodeLocation_Throws_When_Element_Is_Null()
    {
        CodeElement? element = null;

        Assert.Throws<ArgumentNullException>(() => element!.GetCodeLocation());
    }
}
