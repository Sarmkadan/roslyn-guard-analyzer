#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Rules;

/// <summary>
/// Rule that flags public types with meaningless or placeholder names.
/// Public types should have descriptive names that indicate their purpose.
/// Placeholder names like ResultA, Temp, Placeholder, Foo, Bar, Baz, etc.
/// indicate incomplete or auto-generated code that should be properly named.
/// </summary>
public static class MeaningfulTypeNameRule
{
    // List of placeholder/type patterns that indicate meaningless names
    private static readonly HashSet<string> PlaceholderPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "ResultA", "ResultB", "Result1", "Result2", // Sequential placeholders
        "Temp", "Temporary", "Placeholder", "Stub", "Dummy",
        "Foo", "Bar", "Baz", "Qux", "Quux", "Corge", "Grault", "Garply", "Waldo",
        "Thing", "Object", "Class", "Interface", "Struct",
        "Helper", "Utility", "Manager", "Handler", "Processor",
        "Model", "View", "Controller", "Service", "Repository",
        "Data", "Entity", "Item" // Overly generic terms
    };

    /// <summary>
    /// Creates and returns the MeaningfulTypeNameRule instance.
    /// </summary>
    public static CustomAnalysisRule Create()
    {
        return CustomRuleBuilder.Create("MTN001", "Public Types Must Have Meaningful Names")
            .For(RuleCategory.NamingConvention)
            .WithSeverity(SeverityLevel.Error)
            .WithDescription("Detects public types with placeholder or meaningless names. Public types should have descriptive names that indicate the type's purpose.")
            .When(HasMeaninglessTypeName)
            .WithMessage(CreateViolationMessage)
            .Build();
    }

    private static bool HasMeaninglessTypeName(CodeElement element)
    {
        // Only check types (classes, structs, interfaces, enums)
        if (element.ElementType != CodeElementType.Class &&
            element.ElementType != CodeElementType.Struct &&
            element.ElementType != CodeElementType.Interface &&
            element.ElementType != CodeElementType.Enum)
        {
            return false;
        }

        // Only check public types
        if (!element.IsPublic)
        {
            return false;
        }

        // Check if the type name matches any placeholder pattern
        if (MatchesPlaceholderPattern(element.Name))
        {
            return true;
        }

        return false;
    }

    private static bool MatchesPlaceholderPattern(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return false;
        }

        var normalizedName = typeName.Trim();

        // Check for common placeholder patterns
        foreach (var pattern in PlaceholderPatterns)
        {
            if (normalizedName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Check for generic placeholder patterns like Result{Letter} or Result{Number}
        if (normalizedName.StartsWith("Result", StringComparison.Ordinal) &&
            normalizedName.Length > 6)
        {
            var suffix = normalizedName[6..];
            if (char.IsLetter(suffix[0]) || char.IsDigit(suffix[0]))
            {
                return true;
            }
        }

        // Check for single letter type names (generic type parameters like T, K, V are OK)
        if (normalizedName.Length == 1 && char.IsLetter(normalizedName[0]))
        {
            // Allow common generic type parameter names
            var upperChar = char.ToUpperInvariant(normalizedName[0]);
            if (upperChar is 'T' or 'K' or 'V')
            {
                return false;
            }

            return true;
        }

        // Check for sequential patterns like Class1, Class2, etc.
        if (normalizedName.Length > 1 &&
            char.IsDigit(normalizedName[^1]))
        {
            return true;
        }

        return false;
    }

    private static string CreateViolationMessage(CodeElement element)
    {
        var typeName = element.Name;
        var typeKind = element.ElementType.ToString().ToLowerInvariant();
        var fileLocation = element.GetLocation();

        return $"Public {typeKind} '{typeName}' at {fileLocation} has a meaningless name. " +
               "Consider using a descriptive name that indicates the type's purpose.";
    }
}
