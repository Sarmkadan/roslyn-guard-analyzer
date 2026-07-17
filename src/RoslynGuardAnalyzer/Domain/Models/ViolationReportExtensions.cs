using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RoslynGuardAnalyzer.Domain.Models
{
    /// <summary>
    /// Extension methods that provide convenient, read‑only operations over <see cref="ViolationReport"/>.
    /// </summary>
    public static class ViolationReportExtensions
    {
        /// <summary>
        /// Returns a flat sequence of all <see cref="RuleViolation"/> instances contained in the report,
        /// regardless of severity or grouping.
        /// </summary>
        /// <param name="report">The report to query.</param>
        /// <returns>An <see cref="IEnumerable{RuleViolation}"/> that enumerates every violation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is <c>null</c>.</exception>
        public static IEnumerable<RuleViolation> GetAllViolations(this ViolationReport report)
        {
            ArgumentNullException.ThrowIfNull(report);

            return report.GetViolationsBySeverity().Values.SelectMany(v => v);
        }

        /// <summary>
        /// Retrieves all violations that originate from the specified source file.
        /// </summary>
        /// <param name="report">The report to query.</param>
        /// <param name="filePath">The full or relative path of the source file.</param>
        /// <returns>An <see cref="IEnumerable{RuleViolation}"/> containing the violations for the file.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is <c>null</c> or empty.</exception>
        public static IEnumerable<RuleViolation> GetViolationsByFile(this ViolationReport report, string filePath)
        {
            ArgumentNullException.ThrowIfNull(report);
            ArgumentException.ThrowIfNullOrEmpty(filePath);

            return report.GetViolationsFromFile(filePath);
        }

        /// <summary>
        /// Generates a concise Markdown representation of the report, suitable for inclusion in
        /// documentation, emails, or pull‑request comments.
        /// </summary>
        /// <param name="report">The report to render.</param>
        /// <returns>A Markdown formatted string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is <c>null</c>.</exception>
        public static string ToMarkdown(this ViolationReport report)
        {
            ArgumentNullException.ThrowIfNull(report);

            var sb = new StringBuilder();

            // Header
            sb.AppendLine($"# {report.Title ?? "Violation Report"}");
            sb.AppendLine();

            // Basic metadata
            sb.AppendLine($"*Project*: {report.ProjectName ?? "Unknown"}");
            sb.AppendLine($"*Generated*: {report.GeneratedAt.ToString("u", CultureInfo.InvariantCulture)}");
            sb.AppendLine();

            // Summary (if any)
            if (!string.IsNullOrWhiteSpace(report.Summary))
            {
                sb.AppendLine(report.Summary.Trim());
                sb.AppendLine();
            }

            // Statistics
            sb.AppendLine($"**Total Violations**: {report.GetTotalViolationCount()}");
            sb.AppendLine($"**Violation Groups**: {report.ViolationGroups?.Count ?? 0}");
            
            if (report.Statistics is not null)
            {
                sb.AppendLine($"**Critical**: {report.Statistics.CriticalCount}");
                sb.AppendLine($"**Errors**: {report.Statistics.ErrorCount}");
                sb.AppendLine($"**Warnings**: {report.Statistics.WarningCount}");
                sb.AppendLine($"**Info**: {report.Statistics.InfoCount}");
            }

            sb.AppendLine();

            // Optional detailed content
            if (!string.IsNullOrWhiteSpace(report.DetailedContent))
            {
                sb.AppendLine("## Details");
                sb.AppendLine();
                sb.AppendLine(report.DetailedContent.Trim());
            }

            return sb.ToString();
        }
    }
}
