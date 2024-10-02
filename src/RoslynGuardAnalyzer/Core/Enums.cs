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
    Namespace = 0,
    Class = 1,
    Interface = 2,
    Struct = 3,
    Enum = 4,
    Method = 5,
    Property = 6,
    Field = 7,
    Parameter = 8,
    ReturnType = 9
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
    Csv = 3
}
