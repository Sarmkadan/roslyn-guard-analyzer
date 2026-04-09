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
/// Application-wide constants and configuration values.
/// </summary>
public static class AnalyzerConstants
{
    public const string ApplicationName = "Roslyn Guard Analyzer";
    public const string ApplicationVersion = "1.0.0";
    public const string Author = "Vladyslav Zaiets";
    public const string RepositoryUrl = "https://github.com/sarmkadan/roslyn-guard-analyzer";

    public static class Naming
    {
        public const string InterfacePrefix = "I";
        public const string AsyncSuffix = "Async";
        public const string PrivateFieldPrefix = "_";
        public const string ConstantPascalCase = "PascalCase";
    }

    public static class LayerPatterns
    {
        public const string RepositoryLayerSuffix = "Repository";
        public const string ServiceLayerSuffix = "Service";
        public const string ControllerLayerSuffix = "Controller";
        public const string ModelLayerPattern = "*.Models";
        public const string ApiLayerPattern = "*.Api";
    }

    public static class Analysis
    {
        public const int DefaultMaxViolationsToReport = 1000;
        public const int DefaultTimeoutSeconds = 300;
        public const string DefaultConfigFileName = "roslyn-guard.json";
        public const int MaxConcurrentAnalyses = 10;
    }

    public static class Messages
    {
        public const string AnalysisStarted = "Starting architectural analysis...";
        public const string AnalysisCompleted = "Analysis completed successfully.";
        public const string ConfigurationLoaded = "Configuration loaded from {0}";
        public const string NoViolationsFound = "No rule violations found.";
        public const string ViolationsFound = "{0} violation(s) found during analysis.";
    }

    public static class FileExtensions
    {
        public const string CSharpExtension = ".cs";
        public const string ProjectExtension = ".csproj";
        public const string SolutionExtension = ".sln";
        public const string ConfigExtension = ".json";
        public const string ReportExtension = ".txt";
    }

    public static class DefaultRules
    {
        public const string LayerDependencyRule = "LYR001";
        public const string NamingConventionRule = "NAM001";
        public const string AsyncPatternRule = "ASY001";
        public const string NullSafetyRule = "NUL001";
    }
}

/// <summary>
/// Error codes used throughout the application.
/// </summary>
public static class ErrorCodes
{
    public const string InvalidConfiguration = "ERR001";
    public const string AnalysisFailed = "ERR002";
    public const string RuleNotFound = "ERR003";
    public const string IoException = "ERR004";
    public const string ParseException = "ERR005";
    public const string TimeoutException = "ERR006";
}
