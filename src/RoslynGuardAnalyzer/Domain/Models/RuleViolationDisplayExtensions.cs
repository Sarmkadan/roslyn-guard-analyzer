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
    public static string ToConsoleLine(this RuleViolation violation)
    {
        if (violation == null) throw new ArgumentNullException(nameof(violation));

        var fileName = Path.GetFileName(violation.FilePath);
        var location = $"{fileName}:{violation.LineNumber}";
        var severity = violation.Severity.ToString();

        return $"{severity} {violation.Id} {location} {violation.Message}";
    }

    /// <summary>
    /// Maps a <see cref="SeverityLevel"/> to a <see cref="ConsoleColor"/> that can be used
    /// when writing to the console.
    /// </summary>
    public static ConsoleColor GetSeverityColor(this SeverityLevel severity)
    {
        return severity switch
        {
            SeverityLevel.Critical => ConsoleColor.Red,
            SeverityLevel.Error    => ConsoleColor.DarkRed,
            SeverityLevel.Warning  => ConsoleColor.Yellow,
            SeverityLevel.Info     => ConsoleColor.Cyan,
            _                      => ConsoleColor.White
        };
    }

    /// <summary>
    /// Groups a sequence of <see cref="RuleViolation"/> instances by the file path
    /// they belong to.
    /// </summary>
    public static IEnumerable<IGrouping<string, RuleViolation>> GroupByFile(
        this IEnumerable<RuleViolation> violations)
    {
        if (violations == null) throw new ArgumentNullException(nameof(violations));

        return violations.GroupBy(v => v.FilePath);
    }
}
