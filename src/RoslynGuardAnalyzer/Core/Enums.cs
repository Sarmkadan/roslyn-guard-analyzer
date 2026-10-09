#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace RoslynGuardAnalyzer.Core;

/// <summary>
/// Defines the severity level of an architectural rule violation.
/// </summary>
public enum SeverityLevel
{
    /// <summary>Informational message, no action required.</summary>
    Info = 0,

    /// <summary>Warning that should be addressed.</summary>
    Warning = 1,

    /// <summary>Error that must be fixed.</summary>
    Error = 2,

    /// <summary>Critical issue that prevents compilation or execution.</summary>
    Critical = 3
}

/// <summary>
/// Categorizes architectural rules by domain.
/// </summary>
public enum RuleCategory
{
    /// <summary>Rules enforcing layer and dependency boundaries.</summary>
    LayerDependency = 0,

    /// <summary>Rules enforcing naming conventions.</summary>
    NamingConvention = 1,

    /// <summary>Rules enforcing async/await patterns.</summary>
    AsyncPattern = 2,

    /// <summary>Rules enforcing null safety and nullable reference types.</summary>
    NullSafety = 3,

    /// <summary>Rules enforcing general code structure.</summary>
    CodeStructure = 4
}

/// <summary>
/// Indicates the type of code element being analyzed.
/// </summary>
public enum CodeElementType
{
    /// <summary>A namespace declaration.</summary>
    Namespace = 0,

    /// <summary>A class declaration.</summary>
    Class = 1,

    /// <summary>An interface declaration.</summary>
    Interface = 2,

    /// <summary>A struct declaration.</summary>
    Struct = 3,

    /// <summary>An enum declaration.</summary>
    Enum = 4,

    /// <summary>A method declaration.</summary>
    Method = 5,

    /// <summary>A property declaration.</summary>
    Property = 6,

    /// <summary>A field declaration.</summary>
    Field = 7,

    /// <summary>A method or constructor parameter.</summary>
    Parameter = 8,

    /// <summary>The return type of a method.</summary>
    ReturnType = 9,

    /// <summary>A catch block in a try statement.</summary>
    CatchBlock = 10
}

/// <summary>
/// Specifies the analysis scope.
/// </summary>
public enum AnalysisScope
{
    /// <summary>Analyze a single file.</summary>
    File = 0,

    /// <summary>Analyze a project.</summary>
    Project = 1,

    /// <summary>Analyze a solution.</summary>
    Solution = 2
}

/// <summary>
/// Defines the output format for analysis reports.
/// </summary>
public enum ReportFormat
{
    /// <summary>Plain text format.</summary>
    Text = 0,

    /// <summary>JSON format.</summary>
    Json = 1,

    /// <summary>XML format.</summary>
    Xml = 2,

    /// <summary>CSV format.</summary>
    Csv = 3,

    /// <summary>SARIF format.</summary>
    Sarif = 4
}
