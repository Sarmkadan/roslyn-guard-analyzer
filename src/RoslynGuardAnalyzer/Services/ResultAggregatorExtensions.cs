using System;
using System.Collections.Generic;
using System.Linq;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Services
{
    /// <summary>
    /// Provides extension methods for <see cref="ResultAggregator"/> to aggregate and query analysis results.
    /// </summary>
    public static class ResultAggregatorExtensions
    {
        /// <summary>
        /// Adds multiple analysis results to the aggregator in a single operation.
        /// </summary>
        /// <param name="aggregator">The result aggregator instance. Cannot be null.</param>
        /// <param name="results">The analysis results to add. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregator"/> or <paramref name="results"/> is null.</exception>
        public static void AddRange(this ResultAggregator aggregator, IEnumerable<AnalysisResult> results)
        {
            ArgumentNullException.ThrowIfNull(aggregator);
            ArgumentNullException.ThrowIfNull(results);

            foreach (var result in results)
            {
                aggregator.Add(result);
            }
        }

        /// <summary>
        /// Gets the total count of violations across all analysis results.
        /// </summary>
        /// <param name="aggregator">The result aggregator instance. Cannot be null.</param>
        /// <returns>The total violation count.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregator"/> is null.</exception>
        public static int GetTotalViolations(this ResultAggregator aggregator)
        {
            ArgumentNullException.ThrowIfNull(aggregator);
            return aggregator.GetAllViolations().Count();
        }

        /// <summary>
        /// Gets a dictionary mapping file paths to their violations.
        /// </summary>
        /// <param name="aggregator">The result aggregator instance. Cannot be null.</param>
        /// <returns>A dictionary with file paths as keys and lists of violations as values.
        /// Keys use ordinal case-insensitive comparison.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregator"/> is null.</exception>
        public static Dictionary<string, List<RuleViolation>> GetViolationsByFile(this ResultAggregator aggregator)
        {
            ArgumentNullException.ThrowIfNull(aggregator);

            var result = new Dictionary<string, List<RuleViolation>>(StringComparer.OrdinalIgnoreCase);

            foreach (var violation in aggregator.GetAllViolations())
            {
                if (violation?.FilePath is not null)
                {
                    if (!result.TryGetValue(violation.FilePath, out var violationsList))
                    {
                        violationsList = new List<RuleViolation>();
                        result[violation.FilePath] = violationsList;
                    }

                    violationsList.Add(violation);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a dictionary mapping rule IDs to their violations.
        /// </summary>
        /// <param name="aggregator">The result aggregator instance. Cannot be null.</param>
        /// <returns>A dictionary with rule IDs as keys and lists of violations as values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregator"/> is null.</exception>
        public static Dictionary<string, List<RuleViolation>> GetViolationsByRule(this ResultAggregator aggregator)
        {
            ArgumentNullException.ThrowIfNull(aggregator);

            var result = new Dictionary<string, List<RuleViolation>>();

            foreach (var violation in aggregator.GetAllViolations())
            {
                if (violation?.RuleId is not null)
                {
                    if (!result.TryGetValue(violation.RuleId, out var violationsList))
                    {
                        violationsList = new List<RuleViolation>();
                        result[violation.RuleId] = violationsList;
                    }

                    violationsList.Add(violation);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a dictionary mapping severity levels to their violations.
        /// </summary>
        /// <param name="aggregator">The result aggregator instance. Cannot be null.</param>
        /// <returns>A dictionary with severity levels as keys and lists of violations as values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregator"/> is null.</exception>
        public static Dictionary<string, List<RuleViolation>> GetViolationsBySeverity(this ResultAggregator aggregator)
        {
            ArgumentNullException.ThrowIfNull(aggregator);

            var result = new Dictionary<string, List<RuleViolation>>();

            foreach (var violation in aggregator.GetAllViolations())
            {
                if (violation is not null)
                {
                    var severity = violation.Severity.ToString();
                    if (!result.TryGetValue(severity, out var violationsList))
                    {
                        violationsList = new List<RuleViolation>();
                        result[severity] = violationsList;
                    }

                    violationsList.Add(violation);
                }
            }

            return result;
        }
    }
}
