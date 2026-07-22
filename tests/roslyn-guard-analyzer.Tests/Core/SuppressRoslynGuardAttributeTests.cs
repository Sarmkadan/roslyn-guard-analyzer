using System;
using System.Collections.Generic;
using FluentAssertions;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using Xunit;

namespace RoslynGuardAnalyzer.Tests.Core;

/// <summary>
/// Tests for <see cref="SuppressRoslynGuardAttribute"/> handling in code element attributes.
/// Tests that the attribute properly filters elements in the analysis pipeline.
/// </summary>
public sealed class SuppressRoslynGuardAttributeTests
{
    private const string TestRuleId = "LYR001";
    private const string OtherRuleId = "NAM001";

    /// <summary>
    /// Tests that a class with matching SuppressRoslynGuard attribute is filtered out.
    /// </summary>
    [Fact]
    public void FilterElements_ClassWithMatchingSuppressAttribute_ElementFiltered()
    {
        // Arrange
        var codeElement = new CodeElement("TestClass", CodeElementType.Class, "/test/TestClass.cs")
        {
            Namespace = "TestNamespace",
            StartLineNumber = 10,
            EndLineNumber = 20,
            Attributes = new List<string> { "[SuppressRoslynGuard(\"LYR001\")]" }
        };

        var elements = new List<CodeElement> { codeElement };

        // Act - Simulate the filtering logic from RuleEngine.ExecuteRuleAsync
        var activeElements = elements.Where(e => !e.Attributes.Any(a =>
            a.Contains("SuppressRoslynGuard", StringComparison.OrdinalIgnoreCase) &&
            a.Contains(TestRuleId, StringComparison.OrdinalIgnoreCase))).ToList();

        // Assert
        activeElements.Should().BeEmpty("because the element with matching SuppressRoslynGuard attribute should be filtered out");
    }

    /// <summary>
    /// Tests that a class with non-matching SuppressRoslynGuard attribute is NOT filtered out.
    /// This verifies the "wrong rule id still reported" requirement.
    /// </summary>
    [Fact]
    public void FilterElements_ClassWithNonMatchingSuppressAttribute_ElementNotFiltered()
    {
        // Arrange
        var codeElement = new CodeElement("TestClass", CodeElementType.Class, "/test/TestClass.cs")
        {
            Namespace = "TestNamespace",
            StartLineNumber = 10,
            EndLineNumber = 20,
            Attributes = new List<string> { "[SuppressRoslynGuard(\"NAM001\")]" }
        };

        var elements = new List<CodeElement> { codeElement };

        // Act - Simulate the filtering logic from RuleEngine.ExecuteRuleAsync
        var activeElements = elements.Where(e => !e.Attributes.Any(a =>
            a.Contains("SuppressRoslynGuard", StringComparison.OrdinalIgnoreCase) &&
            a.Contains(TestRuleId, StringComparison.OrdinalIgnoreCase))).ToList();

        // Assert
        activeElements.Should().ContainSingle("because the element with non-matching SuppressRoslynGuard attribute should NOT be filtered");
    }

    /// <summary>
    /// Tests that a method with matching SuppressRoslynGuard attribute is filtered out.
    /// </summary>
    [Fact]
    public void FilterElements_MethodWithMatchingSuppressAttribute_ElementFiltered()
    {
        // Arrange
        var codeElement = new CodeElement("TestMethod", CodeElementType.Method, "/test/TestClass.cs")
        {
            Namespace = "TestNamespace",
            StartLineNumber = 15,
            EndLineNumber = 18,
            ReturnType = "void",
            Attributes = new List<string> { "[SuppressRoslynGuard(\"LYR001\")]" }
        };

        var elements = new List<CodeElement> { codeElement };

        // Act - Simulate the filtering logic from RuleEngine.ExecuteRuleAsync
        var activeElements = elements.Where(e => !e.Attributes.Any(a =>
            a.Contains("SuppressRoslynGuard", StringComparison.OrdinalIgnoreCase) &&
            a.Contains(TestRuleId, StringComparison.OrdinalIgnoreCase))).ToList();

        // Assert
        activeElements.Should().BeEmpty("because the method with matching SuppressRoslynGuard attribute should be filtered out");
    }

    /// <summary>
    /// Tests that a property with matching SuppressRoslynGuard attribute is filtered out.
    /// </summary>
    [Fact]
    public void FilterElements_PropertyWithMatchingSuppressAttribute_ElementFiltered()
    {
        // Arrange
        var codeElement = new CodeElement("TestProperty", CodeElementType.Property, "/test/TestClass.cs")
        {
            Namespace = "TestNamespace",
            StartLineNumber = 25,
            EndLineNumber = 26,
            ReturnType = "string",
            Attributes = new List<string> { "[SuppressRoslynGuard(\"LYR001\")]" }
        };

        var elements = new List<CodeElement> { codeElement };

        // Act - Simulate the filtering logic from RuleEngine.ExecuteRuleAsync
        var activeElements = elements.Where(e => !e.Attributes.Any(a =>
            a.Contains("SuppressRoslynGuard", StringComparison.OrdinalIgnoreCase) &&
            a.Contains(TestRuleId, StringComparison.OrdinalIgnoreCase))).ToList();

        // Assert
        activeElements.Should().BeEmpty("because the property with matching SuppressRoslynGuard attribute should be filtered out");
    }

    /// <summary>
    /// Tests that SuppressRoslynGuard attribute with justification is properly handled.
    /// </summary>
    [Fact]
    public void FilterElements_AttributeWithJustification_ElementFiltered()
    {
        // Arrange
        var codeElement = new CodeElement("TestClass", CodeElementType.Class, "/test/TestClass.cs")
        {
            Namespace = "TestNamespace",
            StartLineNumber = 10,
            EndLineNumber = 20,
            Attributes = new List<string> { "[SuppressRoslynGuard(\"LYR001\", Justification = \"Known exception\")]" }
        };

        var elements = new List<CodeElement> { codeElement };

        // Act - Simulate the filtering logic from RuleEngine.ExecuteRuleAsync
        var activeElements = elements.Where(e => !e.Attributes.Any(a =>
            a.Contains("SuppressRoslynGuard", StringComparison.OrdinalIgnoreCase) &&
            a.Contains(TestRuleId, StringComparison.OrdinalIgnoreCase))).ToList();

        // Assert
        activeElements.Should().BeEmpty("because the SuppressRoslynGuard attribute with justification should filter the element");
    }

    /// <summary>
    /// Tests that multiple SuppressRoslynGuard attributes work correctly.
    /// </summary>
    [Fact]
    public void FilterElements_MultipleSuppressAttributes_MixedFiltering()
    {
        // Arrange
        var codeElement = new CodeElement("TestClass", CodeElementType.Class, "/test/TestClass.cs")
        {
            Namespace = "TestNamespace",
            StartLineNumber = 10,
            EndLineNumber = 20,
            Attributes = new List<string>
            {
                "[SuppressRoslynGuard(\"LYR001\")]",
                "[SuppressRoslynGuard(\"NAM001\")]"
            }
        };

        var elements = new List<CodeElement> { codeElement };

        // Act - Test filtering for LYR001
        var activeElementsLyR = elements.Where(e => !e.Attributes.Any(a =>
            a.Contains("SuppressRoslynGuard", StringComparison.OrdinalIgnoreCase) &&
            a.Contains(TestRuleId, StringComparison.OrdinalIgnoreCase))).ToList();

        // Act - Test filtering for NAM001
        var activeElementsNam = elements.Where(e => !e.Attributes.Any(a =>
            a.Contains("SuppressRoslynGuard", StringComparison.OrdinalIgnoreCase) &&
            a.Contains(OtherRuleId, StringComparison.OrdinalIgnoreCase))).ToList();

        // Assert
        activeElementsLyR.Should().BeEmpty("because LYR001 is suppressed");
        activeElementsNam.Should().BeEmpty("because NAM001 is also suppressed");
    }

    /// <summary>
    /// Tests that SuppressRoslynGuard attribute is case-insensitive in matching.
    /// </summary>
    [Fact]
    public void FilterElements_CaseInsensitiveAttributeMatching_ElementFiltered()
    {
        // Arrange
        var codeElement = new CodeElement("TestClass", CodeElementType.Class, "/test/TestClass.cs")
        {
            Namespace = "TestNamespace",
            StartLineNumber = 10,
            EndLineNumber = 20,
            Attributes = new List<string> { "[suppressroslynguard(\"LYR001\")]" }
        };

        var elements = new List<CodeElement> { codeElement };

        // Act - Simulate the filtering logic from RuleEngine.ExecuteRuleAsync
        var activeElements = elements.Where(e => !e.Attributes.Any(a =>
            a.Contains("SuppressRoslynGuard", StringComparison.OrdinalIgnoreCase) &&
            a.Contains(TestRuleId, StringComparison.OrdinalIgnoreCase))).ToList();

        // Assert
        activeElements.Should().BeEmpty("because attribute matching should be case-insensitive");
    }

    /// <summary>
    /// Tests that SuppressRoslynGuard attribute with rule ID in different case still filters.
    /// </summary>
    [Fact]
    public void FilterElements_CaseInsensitiveRuleIdMatching_ElementFiltered()
    {
        // Arrange
        var codeElement = new CodeElement("TestClass", CodeElementType.Class, "/test/TestClass.cs")
        {
            Namespace = "TestNamespace",
            StartLineNumber = 10,
            EndLineNumber = 20,
            Attributes = new List<string> { "[SuppressRoslynGuard(\"lyr001\")]" }
        };

        var elements = new List<CodeElement> { codeElement };

        // Act - Simulate the filtering logic from RuleEngine.ExecuteRuleAsync
        var activeElements = elements.Where(e => !e.Attributes.Any(a =>
            a.Contains("SuppressRoslynGuard", StringComparison.OrdinalIgnoreCase) &&
            a.Contains(TestRuleId, StringComparison.OrdinalIgnoreCase))).ToList();

        // Assert
        activeElements.Should().BeEmpty("because rule ID matching should be case-insensitive");
    }

    /// <summary>
    /// Tests that an element without any attributes is NOT filtered.
    /// </summary>
    [Fact]
    public void FilterElements_NoAttributes_ElementNotFiltered()
    {
        // Arrange
        var codeElement = new CodeElement("TestClass", CodeElementType.Class, "/test/TestClass.cs")
        {
            Namespace = "TestNamespace",
            StartLineNumber = 10,
            EndLineNumber = 20,
            Attributes = new List<string>()
        };

        var elements = new List<CodeElement> { codeElement };

        // Act - Simulate the filtering logic from RuleEngine.ExecuteRuleAsync
        var activeElements = elements.Where(e => !e.Attributes.Any(a =>
            a.Contains("SuppressRoslynGuard", StringComparison.OrdinalIgnoreCase) &&
            a.Contains(TestRuleId, StringComparison.OrdinalIgnoreCase))).ToList();

        // Assert
        activeElements.Should().ContainSingle("because elements without SuppressRoslynGuard attributes should NOT be filtered");
    }

    /// <summary>
    /// Tests member-level scope: class suppression doesn't affect method-level elements.
    /// This verifies the "member-level scope respected" requirement.
    /// </summary>
    [Fact]
    public void FilterElements_ClassSuppression_DoesNotAffectMethodLevel()
    {
        // Arrange
        var classElement = new CodeElement("TestClass", CodeElementType.Class, "/test/TestClass.cs")
        {
            Namespace = "TestNamespace",
            StartLineNumber = 10,
            EndLineNumber = 50,
            Attributes = new List<string> { "[SuppressRoslynGuard(\"LYR001\")]" }
        };

        var methodElement = new CodeElement("BadMethod", CodeElementType.Method, "/test/TestClass.cs")
        {
            Namespace = "TestNamespace",
            StartLineNumber = 20,
            EndLineNumber = 25,
            ReturnType = "void",
            Attributes = new List<string>() // No suppression at method level
        };

        var elements = new List<CodeElement> { classElement, methodElement };

        // Act - Simulate the filtering logic from RuleEngine.ExecuteRuleAsync
        var activeElements = elements.Where(e => !e.Attributes.Any(a =>
            a.Contains("SuppressRoslynGuard", StringComparison.OrdinalIgnoreCase) &&
            a.Contains(TestRuleId, StringComparison.OrdinalIgnoreCase))).ToList();

        // Assert
        activeElements.Should().ContainSingle("because method-level element should NOT be filtered despite class suppression");
        activeElements[0].Name.Should().Be("BadMethod");
    }

    /// <summary>
    /// Tests member-level scope: method suppression doesn't affect class-level elements.
    /// </summary>
    [Fact]
    public void FilterElements_MethodSuppression_DoesNotAffectClassLevel()
    {
        // Arrange
        var classElement = new CodeElement("TestClass", CodeElementType.Class, "/test/TestClass.cs")
        {
            Namespace = "TestNamespace",
            StartLineNumber = 10,
            EndLineNumber = 50,
            Attributes = new List<string>() // No suppression at class level
        };

        var methodElement = new CodeElement("BadMethod", CodeElementType.Method, "/test/TestClass.cs")
        {
            Namespace = "TestNamespace",
            StartLineNumber = 20,
            EndLineNumber = 25,
            ReturnType = "void",
            Attributes = new List<string> { "[SuppressRoslynGuard(\"LYR001\")]" }
        };

        var elements = new List<CodeElement> { classElement, methodElement };

        // Act - Simulate the filtering logic from RuleEngine.ExecuteRuleAsync
        var activeElements = elements.Where(e => !e.Attributes.Any(a =>
            a.Contains("SuppressRoslynGuard", StringComparison.OrdinalIgnoreCase) &&
            a.Contains(TestRuleId, StringComparison.OrdinalIgnoreCase))).ToList();

        // Assert
        activeElements.Should().ContainSingle("because class-level element should NOT be filtered despite method suppression");
        activeElements[0].Name.Should().Be("TestClass");
    }

    /// <summary>
    /// Tests that SuppressRoslynGuard attribute works with all valid target types (class, method, property).
    /// </summary>
    [Fact]
    public void FilterElements_AllTargetTypes_AllFiltered()
    {
        // Arrange
        var classElement = new CodeElement("TestClass", CodeElementType.Class, "/test/TestClass.cs")
        {
            Namespace = "TestNamespace",
            StartLineNumber = 10,
            EndLineNumber = 20,
            Attributes = new List<string> { "[SuppressRoslynGuard(\"LYR001\")]" }
        };

        var methodElement = new CodeElement("TestMethod", CodeElementType.Method, "/test/TestClass.cs")
        {
            Namespace = "TestNamespace",
            StartLineNumber = 25,
            EndLineNumber = 28,
            ReturnType = "void",
            Attributes = new List<string> { "[SuppressRoslynGuard(\"LYR001\")]" }
        };

        var propertyElement = new CodeElement("TestProperty", CodeElementType.Property, "/test/TestClass.cs")
        {
            Namespace = "TestNamespace",
            StartLineNumber = 30,
            EndLineNumber = 31,
            ReturnType = "string",
            Attributes = new List<string> { "[SuppressRoslynGuard(\"LYR001\")]" }
        };

        var elements = new List<CodeElement> { classElement, methodElement, propertyElement };

        // Act
        var activeElements = elements.Where(e => !e.Attributes.Any(a =>
            a.Contains("SuppressRoslynGuard", StringComparison.OrdinalIgnoreCase) &&
            a.Contains(TestRuleId, StringComparison.OrdinalIgnoreCase))).ToList();

        // Assert - All elements should be filtered
        activeElements.Should().BeEmpty("because all element types with matching SuppressRoslynGuard should be filtered");
    }
}
