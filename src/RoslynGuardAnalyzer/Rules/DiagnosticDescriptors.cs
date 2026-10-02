using Microsoft.CodeAnalysis;

namespace RoslynGuardAnalyzer.Rules
{
    /// <summary>
    /// Provides a collection of diagnostic descriptors for Roslyn Guard Analyzer rules.
    /// </summary>
    public static class DiagnosticDescriptors
    {
        /// <summary>
        /// Gets the diagnostic descriptor for the rule that enforces layer dependencies.
        /// </summary>
        public static readonly DiagnosticDescriptor LayerDependencyRule = new DiagnosticDescriptor(
            id: "LYR001",
            title: "Layer Dependency Violation",
            messageFormat: "Type '{0}' in layer '{1}' depends on type '{2}' in layer '{3}'",
            category: "RoslynGuardAnalyzer.LayerDependency",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Ensures that types in a layer only depend on types in the same or lower layers.",
            helpLinkUri: "https://github.com/roslyn-guard-analyzer/roslyn-guard-analyzer/blob/main/docs/rules.md#LYR001");

        /// <summary>
        /// Gets the diagnostic descriptor for the rule that enforces naming conventions.
        /// </summary>
        public static readonly DiagnosticDescriptor NamingConventionRule = new DiagnosticDescriptor(
            id: "NAM001",
            title: "Naming Convention Violation",
            messageFormat: "Type '{0}' does not follow the naming convention '{1}'",
            category: "RoslynGuardAnalyzer.NamingConvention",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Ensures that types follow the specified naming conventions.",
            helpLinkUri: "https://github.com/roslyn-guard-analyzer/roslyn-guard-analyzer/blob/main/docs/rules.md#NAM001");

        /// <summary>
        /// Gets the diagnostic descriptor for the rule that enforces async patterns.
        /// </summary>
        public static readonly DiagnosticDescriptor AsyncPatternRule = new DiagnosticDescriptor(
            id: "ASY001",
            title: "Async Pattern Violation",
            messageFormat: "Method '{0}' does not follow the async pattern",
            category: "RoslynGuardAnalyzer.AsyncPattern",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Ensures that methods follow the async pattern by returning Task or Task<T>.",
            helpLinkUri: "https://github.com/roslyn-guard-analyzer/roslyn-guard-analyzer/blob/main/docs/rules.md#ASY001");

        /// <summary>
        /// Gets the diagnostic descriptor for the rule that enforces null safety.
        /// </summary>
        public static readonly DiagnosticDescriptor NullSafetyRule = new DiagnosticDescriptor(
            id: "NUL001",
            title: "Null Safety Violation",
            messageFormat: "Type '{0}' is not null-safe",
            category: "RoslynGuardAnalyzer.NullSafety",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Ensures that types are null-safe by using nullable reference types.",
            helpLinkUri: "https://github.com/roslyn-guard-analyzer/roslyn-guard-analyzer/blob/main/docs/rules.md#NUL001");
    }
}
