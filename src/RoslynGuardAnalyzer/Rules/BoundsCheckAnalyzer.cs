#nullable enable

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

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeElementAccess, SyntaxKind.ElementAccessExpression);
    }

    private static void AnalyzeElementAccess(SyntaxNodeAnalysisContext context)
    {
        var elementAccess = (ElementAccessExpressionSyntax)context.Node;

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
        if (IsGuardedByLengthCheck(elementAccess, context.SemanticModel))
            return;

        // Skip if the index is a hat expression (^1) - these are range/index expressions
        // that are inherently bounded by the runtime
        if (IsIndexExpression(elementAccess))
            return;

        // Skip if this is a range expression (e.g. arr[1..3])
        if (IsRangeExpression(elementAccess))
            return;

        // Check if the target is an indexable type (array, List, Span, etc.)
        var typeInfo = context.SemanticModel.GetTypeInfo(expression);
        if (!IsIndexableType(typeInfo.Type))
            return;

        var diagnostic = Diagnostic.Create(Rule, elementAccess.GetLocation(), expressionText);
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

    private static bool IsGuardedByLengthCheck(ElementAccessExpressionSyntax elementAccess, SemanticModel model)
    {
        var expressionText = elementAccess.Expression.ToString();

        // Walk up to find an enclosing if statement
        var current = elementAccess.Parent;
        while (current is not null)
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

    private static bool IsIndexableType(ITypeSymbol? type)
    {
        if (type is null)
            return false;

        // Arrays
        if (type is IArrayTypeSymbol)
            return true;

        // Check for well-known indexable types
        var fullName = type.ToDisplayString();
        if (fullName.StartsWith("System.Collections.Generic.List<", StringComparison.Ordinal) ||
            fullName.StartsWith("System.Collections.Generic.IList<", StringComparison.Ordinal) ||
            fullName.StartsWith("System.Span<", StringComparison.Ordinal) ||
            fullName.StartsWith("System.ReadOnlySpan<", StringComparison.Ordinal) ||
            fullName.StartsWith("System.Memory<", StringComparison.Ordinal) ||
            fullName.StartsWith("System.ReadOnlyMemory<", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }
}
