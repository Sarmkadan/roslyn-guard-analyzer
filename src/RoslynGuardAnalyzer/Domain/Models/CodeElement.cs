// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

using RoslynGuardAnalyzer.Core;
namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Represents a code element (class, method, property, etc.) being analyzed.
/// Contains metadata about the element's location and characteristics.
/// </summary>
public sealed class CodeElement
{
    public string Id { get; set; }
    public string Name { get; set; }
    public CodeElementType ElementType { get; set; }
    public string FilePath { get; set; }
    public int StartLineNumber { get; set; }
    public int EndLineNumber { get; set; }
    public string Namespace { get; set; }
    public string? ParentName { get; set; }
    public string? FullyQualifiedName { get; set; }
    public List<string> Attributes { get; set; }
    public List<string> Dependencies { get; set; }
    public bool IsPublic { get; set; }
    public bool IsAsync { get; set; }
    public bool IsStatic { get; set; }
    public bool IsAbstract { get; set; }
    public string? ReturnType { get; set; }
    public List<string> Parameters { get; set; }
    public int Complexity { get; set; }
    public DateTime AnalyzedAt { get; set; }

    public CodeElement()
    {
        Id = Guid.NewGuid().ToString();
        Name = string.Empty;
        ElementType = CodeElementType.Class;
        FilePath = string.Empty;
        Namespace = string.Empty;
        StartLineNumber = 0;
        EndLineNumber = 0;
        Attributes = new List<string>();
        Dependencies = new List<string>();
        Parameters = new List<string>();
        Complexity = 1;
        AnalyzedAt = DateTime.UtcNow;
    }

    public CodeElement(string name, CodeElementType elementType, string filePath)
        : this()
    {
        Name = name;
        ElementType = elementType;
        FilePath = filePath;
    }

    /// <summary>
    /// Gets the fully qualified name, building it if not already set.
    /// </summary>
    /// <returns>The fully qualified name of the element.</returns>
    public string GetFullyQualifiedName()
    {
        if (!string.IsNullOrWhiteSpace(FullyQualifiedName))
            return FullyQualifiedName;

        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(Namespace))
            parts.Add(Namespace);

        if (!string.IsNullOrWhiteSpace(ParentName))
            parts.Add(ParentName);

        parts.Add(Name);

        return string.Join(".", parts);
    }

    /// <summary>
    /// Adds a dependency reference from this element to another.
    /// </summary>
    public void AddDependency(string dependencyName)
    {
        if (!string.IsNullOrWhiteSpace(dependencyName) && !Dependencies.Contains(dependencyName))
        {
            Dependencies.Add(dependencyName);
        }
    }

    /// <summary>
    /// Checks if this element has a specific attribute.
    /// </summary>
    public bool HasAttribute(string attributeName)
    {
        return Attributes.Any(a => a.Contains(attributeName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Adds an attribute to this element.
    /// </summary>
    public void AddAttribute(string attributeName)
    {
        if (!string.IsNullOrWhiteSpace(attributeName) && !Attributes.Contains(attributeName))
        {
            Attributes.Add(attributeName);
        }
    }

    /// <summary>
    /// Gets the location information in a formatted string.
    /// </summary>
    /// <returns>Location string (e.g., "file.cs(42-56)").</returns>
    public string GetLocation()
    {
        var fileName = Path.GetFileName(FilePath);
        return $"{fileName}({StartLineNumber}-{EndLineNumber})";
    }

    /// <summary>
    /// Checks if this element is in a specific namespace hierarchy.
    /// </summary>
    /// <returns>True if the element's namespace starts with the given prefix.</returns>
    public bool IsInNamespace(string namespacePrefix)
    {
        return !string.IsNullOrWhiteSpace(Namespace)
            && Namespace.StartsWith(namespacePrefix, StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines if this element represents a container (class, interface, namespace).
    /// </summary>
    /// <returns>True if the element is a container type.</returns>
    public bool IsContainer()
    {
        return ElementType switch
        {
            CodeElementType.Class or CodeElementType.Interface or CodeElementType.Struct
                or CodeElementType.Namespace => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets a human-readable description of the element.
    /// </summary>
    /// <returns>Formatted description string.</returns>
    public string GetDescription()
    {
        var desc = $"{ElementType} {GetFullyQualifiedName()}";
        if (!string.IsNullOrWhiteSpace(ReturnType))
            desc += $" : {ReturnType}";
        return desc;
    }

    /// <summary>
    /// Validates that the element has required information.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name)
            && !string.IsNullOrWhiteSpace(FilePath)
            && StartLineNumber > 0
            && EndLineNumber >= StartLineNumber;
    }
}
