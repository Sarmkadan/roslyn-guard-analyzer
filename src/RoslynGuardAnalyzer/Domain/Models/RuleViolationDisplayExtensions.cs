using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RoslynGuardAnalyzer.Core;

namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Extension methods for <see cref="RuleViolation"/> that provide console-friendly
/// representations and grouping helpers.
/// </summary>
public static class RuleViolationDisplayExtensions
{
    /// <summary>
    /// Returns a console-friendly line that contains the severity, id, file:line and message.
    /// Example: "Error 1234 MyClass.cs:42 Something went wrong"
    /// </summary>
    /// <param name="violation">The rule violation to format.</param>
    /// <returns>A formatted console line string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="violation"/> is null.</exception>
    public static string ToConsoleLine(this RuleViolation violation)
    {
        ArgumentNullException.ThrowIfNull(violation);

        var fileName = Path.GetFileName(violation.FilePath ?? string.Empty);
        var location = $"{fileName}:{violation.LineNumber}";
        var severity = violation.Severity.ToString();

        return $"{severity} {violation.Id} {location} {violation.Message}";
    }

    /// <summary>
    /// Maps a <see cref="SeverityLevel"/> to a <see cref="ConsoleColor"/> that can be used
    /// when writing to the console.
    /// </summary>
    /// <param name="severity">The severity level to map.</param>
    /// <returns>The corresponding console color.</returns>
    public static ConsoleColor GetSeverityColor(this SeverityLevel severity) =>
        severity switch
        {
            SeverityLevel.Critical => ConsoleColor.Red,
            SeverityLevel.Error => ConsoleColor.DarkRed,
            SeverityLevel.Warning => ConsoleColor.Yellow,
            SeverityLevel.Info => ConsoleColor.Cyan,
            _ => ConsoleColor.White
        };

    /// <summary>
    /// Groups a sequence of <see cref="RuleViolation"/> instances by the file path
    /// they belong to.
    /// </summary>
    /// <param name="violations">The violations to group.</param>
    /// <returns>An enumerable of groupings by file path.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="violations"/> is null.</exception>
    public static IEnumerable<IGrouping<string, RuleViolation>> GroupByFile(
        this IEnumerable<RuleViolation> violations)
    {
        ArgumentNullException.ThrowIfNull(violations);

        return violations.GroupBy(v => v.FilePath);
    }
}
