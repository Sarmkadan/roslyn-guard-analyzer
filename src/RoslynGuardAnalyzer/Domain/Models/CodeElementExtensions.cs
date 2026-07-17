using System;
using System.Collections.Generic;

namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Provides extension methods for the <see cref="CodeElement"/> class.
/// </summary>
public static class CodeElementExtensions
{
    /// <summary>
    /// Determines whether the code element has the specified attribute.
    /// </summary>
    /// <param name="element">The code element to check.</param>
    /// <param name="attributeName">Name of the attribute to search for.</param>
    /// <returns><c>true</c> if the attribute exists; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="element"/> or <paramref name="attributeName"/> is <c>null</c>.</exception>
    public static bool HasAttribute(this CodeElement element, string attributeName)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentException.ThrowIfNullOrEmpty(attributeName);
        return element.Attributes.Contains(attributeName);
    }

    /// <summary>
    /// Gets a display name combining namespace, parent name, and element name.
    /// </summary>
    /// <param name="element">The code element.</param>
    /// <returns>A formatted display name in the format "Namespace.ParentName.ElementName".</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="element"/> is <c>null</c>.</exception>
    public static string GetDisplayName(this CodeElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        var parts = new List<string>(3);

        if (!string.IsNullOrEmpty(element.Namespace))
        {
            parts.Add(element.Namespace);
        }

        if (!string.IsNullOrEmpty(element.ParentName))
        {
            parts.Add(element.ParentName);
        }

        parts.Add(element.Name);

        return string.Join(".", parts);
    }

    /// <summary>
    /// Determines whether the code element is a top-level element (no parent).
    /// </summary>
    /// <param name="element">The code element to check.</param>
    /// <returns><c>true</c> if the element is top-level; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="element"/> is <c>null</c>.</exception>
    public static bool IsTopLevelElement(this CodeElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return string.IsNullOrEmpty(element.ParentName);
    }

    /// <summary>
    /// Gets the code location as a formatted string.
    /// </summary>
    /// <param name="element">The code element.</param>
    /// <returns>A string in the format "FilePath (StartLine-EndLine)".</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="element"/> is <c>null</c>.</exception>
    public static string GetCodeLocation(this CodeElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return $"{element.FilePath} ({element.StartLineNumber}-{element.EndLineNumber})";
    }
}