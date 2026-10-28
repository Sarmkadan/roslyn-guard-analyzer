#nullable enable

using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RoslynGuardAnalyzer.Rules;

/// <summary>
/// Roslyn diagnostic analyzer that detects potentially unsafe array, list, and span
/// indexing operations that are not guarded by a bounds check.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BoundsCheckAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for unguarded element access.
    /// </summary>
    public const string DiagnosticId = "BND001";

    private static readonly LocalizableString Title =
        "Potentially unguarded element access";

    private static readonly LocalizableString MessageFormat =
        "Element access on '{0}' may be out of bounds without a preceding length/bounds check";

    private static readonly LocalizableString Description =
        "Array, List, and Span indexing should be guarded by a bounds check to prevent IndexOutOfRangeException or ArgumentOutOfRangeException.";

    /// <summary>
    /// The diagnostic descriptor for unguarded bounds access.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        "RoslynGuardAnalyzer.BoundsCheck",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    /// <summary>
    /// Copies of <see cref="Rule"/> with each severity that <c>guard.bounds_check.severity</c> can select.
    /// They share the ID, so the analyzer still advertises only <see cref="Rule"/>.
    /// </summary>
    private static readonly ImmutableDictionary<DiagnosticSeverity, DiagnosticDescriptor> SeverityRules =
        new[] { DiagnosticSeverity.Hidden, DiagnosticSeverity.Info, DiagnosticSeverity.Warning, DiagnosticSeverity.Error }
            .ToImmutableDictionary(
                severity => severity,
                severity => new DiagnosticDescriptor(
                    Rule.Id,
                    Rule.Title,
                    Rule.MessageFormat,
                    Rule.Category,
                    severity,
                    Rule.IsEnabledByDefault,
                    Rule.Description,
                    Rule.HelpLinkUri,
                    Rule.CustomTags.ToArray()));

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule, DiagnosticDescriptors.GuardConfigurationRule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        // Validate every distinct .editorconfig section once per compilation. Diagnostics can only be
        // reported from the compilation-end context, so invalid values are collected here and reported there.
        var provider = context.Options.AnalyzerConfigOptionsProvider;
        var configs = new ConcurrentDictionary<AnalyzerConfigOptions, GuardOptions>(ReferenceEqualityComparer.Instance);
        var errors = new List<GuardConfigurationError>();

        foreach (var tree in context.Compilation.SyntaxTrees)
        {
            var config = provider.GetOptions(tree);
            if (configs.ContainsKey(config))
                continue;

            var result = GuardOptions.Parse(ReadGuardValues(config));
            configs[config] = result.Options;
            errors.AddRange(result.Errors);
        }

        if (errors.Count > 0)
        {
            // Sections with identical text can yield identical errors, so report each distinct error once.
            context.RegisterCompilationEndAction(endContext =>
            {
                foreach (var error in errors.Distinct())
                {
                    endContext.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.GuardConfigurationRule,
                        Location.None,
                        error.Key,
                        error.Value,
                        error.Reason));
                }
            });
        }

        context.RegisterSyntaxNodeAction(
            nodeContext => AnalyzeElementAccess(nodeContext, configs),
            SyntaxKind.ElementAccessExpression);
    }

    private static void AnalyzeElementAccess(
        SyntaxNodeAnalysisContext context,
        ConcurrentDictionary<AnalyzerConfigOptions, GuardOptions> configs)
    {
        var elementAccess = (ElementAccessExpressionSyntax)context.Node;

        var config = context.Options.AnalyzerConfigOptionsProvider.GetOptions(elementAccess.SyntaxTree);
        var guardOptions = configs.GetOrAdd(config, static c => GuardOptions.Parse(ReadGuardValues(c)).Options);

        if (IsInIgnoredNamespace(context, guardOptions.IgnoredNamespaces))
            return;

        // Determine the collection expression being indexed
        var expression = elementAccess.Expression;
        var expressionText = expression.ToString();

        // Skip if the index is a constant (e.g. arr[0])
        if (IsConstantIndex(elementAccess, context.SemanticModel))
            return;

        // Skip if inside a for loop that is bounded by the collection's Length/Count
        if (IsInsideBoundedForLoop(elementAccess, context.SemanticModel))
            return;

        // Skip if inside a foreach loop over the same collection
        if (IsInsideForeachOverSameCollection(elementAccess))
            return;

        // Skip if preceded by an if-check on Length/Count in the same block or an enclosing if
        if (IsGuardedByLengthCheck(elementAccess, context.SemanticModel, guardOptions.BoundsCheckMaxDepth))
            return;

        // Skip if the index is a hat expression (^1) - these are range/index expressions
        // that are inherently bounded by the runtime
        if (IsIndexExpression(elementAccess))
            return;

        // Skip if this is a range expression (e.g. arr[1..3])
        if (IsRangeExpression(elementAccess))
            return;

        // Check if the target is one of the configured indexable types (array, List, Span, etc.)
        var typeInfo = context.SemanticModel.GetTypeInfo(expression);
        if (!IsCheckedType(typeInfo.Type, guardOptions))
            return;

        var diagnostic = Diagnostic.Create(GetRule(guardOptions.BoundsCheckSeverity), elementAccess.GetLocation(), expressionText);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsConstantIndex(ElementAccessExpressionSyntax elementAccess, SemanticModel model)
    {
        if (elementAccess.ArgumentList.Arguments.Count != 1)
            return false;

        var argument = elementAccess.ArgumentList.Arguments[0].Expression;
        var constantValue = model.GetConstantValue(argument);
        return constantValue.HasValue;
    }

    private static bool IsInsideBoundedForLoop(ElementAccessExpressionSyntax elementAccess, SemanticModel model)
    {
        var forStatement = elementAccess.FirstAncestorOrSelf<ForStatementSyntax>();
        if (forStatement is null)
            return false;

        // Check if the condition references .Length or .Count of the same expression
        if (forStatement.Condition is BinaryExpressionSyntax binary)
        {
            var conditionText = binary.ToString();
            var expressionText = elementAccess.Expression.ToString();

            if (conditionText.Contains($"{expressionText}.Length", StringComparison.Ordinal) ||
                conditionText.Contains($"{expressionText}.Count", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInsideForeachOverSameCollection(ElementAccessExpressionSyntax elementAccess)
    {
        var foreachStatement = elementAccess.FirstAncestorOrSelf<ForEachStatementSyntax>();
        if (foreachStatement is null)
            return false;

        // A foreach implies the collection is being iterated - indexing inside is typically
        // bounded by the iteration itself
        return true;
    }

    private static bool IsInIgnoredNamespace(SyntaxNodeAnalysisContext context, ImmutableArray<string> ignoredNamespaces)
    {
        if (ignoredNamespaces.IsDefaultOrEmpty)
            return false;

        var containingNamespace = context.ContainingSymbol?.ContainingNamespace;
        if (containingNamespace is null || containingNamespace.IsGlobalNamespace)
            return false;

        var namespaceName = containingNamespace.ToDisplayString();
        foreach (var ignored in ignoredNamespaces)
        {
            if (namespaceName == ignored || namespaceName.StartsWith(ignored + ".", StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static Dictionary<string, string> ReadGuardValues(AnalyzerConfigOptions config)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var key in GuardOptions.KnownKeys)
        {
            if (config.TryGetValue(key, out var value))
                values[key] = value;
        }

        return values;
    }

    private static bool IsGuardedByLengthCheck(ElementAccessExpressionSyntax elementAccess, SemanticModel model, int maxDepth)
    {
        var expressionText = elementAccess.Expression.ToString();

        // Walk up to find an enclosing if statement, examining at most maxDepth ancestor levels
        var current = elementAccess.Parent;
        var depth = 0;
        while (current is not null && depth++ < maxDepth)
        {
            if (current is IfStatementSyntax ifStatement)
            {
                var conditionText = ifStatement.Condition.ToString();
                if (conditionText.Contains($"{expressionText}.Length", StringComparison.Ordinal) ||
                    conditionText.Contains($"{expressionText}.Count", StringComparison.Ordinal))
                {
                    // Ensure the element access is inside the if block (true branch), not the else
                    if (ifStatement.Statement.Contains(elementAccess))
                        return true;
                }
            }

            // Check for a preceding if statement in the same block
            if (current is BlockSyntax block)
            {
                var statements = block.Statements;
                for (var i = 0; i < statements.Count; i++)
                {
                    if (statements[i].Contains(elementAccess))
                        break;

                    if (statements[i] is IfStatementSyntax precedingIf)
                    {
                        var conditionText = precedingIf.Condition.ToString();
                        if (conditionText.Contains($"{expressionText}.Length", StringComparison.Ordinal) ||
                            conditionText.Contains($"{expressionText}.Count", StringComparison.Ordinal))
                        {
                            // If the preceding if has a return/throw in its body, the access is guarded
                            if (ContainsReturnOrThrow(precedingIf.Statement))
                                return true;
                        }
                    }
                }
            }

            current = current.Parent;
        }

        return false;
    }

    private static bool ContainsReturnOrThrow(StatementSyntax statement)
    {
        return statement.DescendantNodesAndSelf()
            .Any(n => n is ReturnStatementSyntax or ThrowStatementSyntax or ThrowExpressionSyntax);
    }

    private static bool IsIndexExpression(ElementAccessExpressionSyntax elementAccess)
    {
        if (elementAccess.ArgumentList.Arguments.Count != 1)
            return false;

        var argument = elementAccess.ArgumentList.Arguments[0].Expression;
        return argument is PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.IndexExpression };
    }

    private static bool IsRangeExpression(ElementAccessExpressionSyntax elementAccess)
    {
        if (elementAccess.ArgumentList.Arguments.Count != 1)
            return false;

        var argument = elementAccess.ArgumentList.Arguments[0].Expression;
        return argument is RangeExpressionSyntax;
    }

    private static DiagnosticDescriptor GetRule(DiagnosticSeverity severity) =>
        severity == Rule.DefaultSeverity ? Rule : SeverityRules[severity];

    private static bool IsCheckedType(ITypeSymbol? type, GuardOptions options)
    {
        if (type is null)
            return false;

        if (type is IArrayTypeSymbol)
            return options.BoundsCheckAllowedIndexerTypes.Contains(GuardOptions.ArrayIndexerTypeEntry);

        // Compare by namespace-qualified name without type arguments, e.g. System.Collections.Generic.List
        var definition = type.OriginalDefinition;
        var name = definition.ContainingNamespace is { IsGlobalNamespace: false } ns
            ? $"{ns.ToDisplayString()}.{definition.Name}"
            : definition.Name;

        if (!options.BoundsCheckAllowedIndexerTypes.Contains(name))
            return false;

        var isSpan = name is "System.Span" or "System.ReadOnlySpan";
        return options.BoundsCheckCheckSpans || !isSpan;
    }
}
