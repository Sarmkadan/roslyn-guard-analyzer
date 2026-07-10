using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Services
{
    public static class ResultAggregatorExtensions
    {
        /// <summary>
        /// Adds multiple analysis results to the aggregator in a single operation.
        /// </summary>
        /// <param name="aggregator">The result aggregator instance.</param>
        /// <param name="results">The analysis results to add.</param>
        public static void AddRange(this ResultAggregator aggregator, IEnumerable<AnalysisResult> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            foreach (var result in results)
            {
                aggregator.Add(result);
            }
        }

        /// <summary>
        /// Gets the total count of violations across all analysis results.
        /// </summary>
        /// <param name="aggregator">The result aggregator instance.</param>
        /// <returns>The total violation count.</returns>
        public static int GetTotalViolations(this ResultAggregator aggregator)
        {
            return aggregator.GetAllViolations().Count();
        }

        /// <summary>
        /// Gets a dictionary mapping file paths to their violations.
        /// </summary>
        /// <param name="aggregator">The result aggregator instance.</param>
        /// <returns>A dictionary with file paths as keys and lists of violations as values.</returns>
        public static Dictionary<string, List<RuleViolation>> GetViolationsByFile(this ResultAggregator aggregator)
        {
            var result = new Dictionary<string, List<RuleViolation>>(StringComparer.OrdinalIgnoreCase);

            foreach (var violation in aggregator.GetAllViolations())
            {
                if (violation.FilePath != null)
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
        /// <param name="aggregator">The result aggregator instance.</param>
        /// <returns>A dictionary with rule IDs as keys and lists of violations as values.</returns>
        public static Dictionary<string, List<RuleViolation>> GetViolationsByRule(this ResultAggregator aggregator)
        {
            var result = new Dictionary<string, List<RuleViolation>>();

            foreach (var violation in aggregator.GetAllViolations())
            {
                if (violation.RuleId != null)
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
        /// <param name="aggregator">The result aggregator instance.</param>
        /// <returns>A dictionary with severity levels as keys and lists of violations as values.</returns>
        public static Dictionary<string, List<RuleViolation>> GetViolationsBySeverity(this ResultAggregator aggregator)
        {
            var result = new Dictionary<string, List<RuleViolation>>();

            foreach (var violation in aggregator.GetAllViolations())
            {
                var severity = violation.Severity.ToString();
                if (!result.TryGetValue(severity, out var violationsList))
                {
                    violationsList = new List<RuleViolation>();
                    result[severity] = violationsList;
                }

                violationsList.Add(violation);
            }

            return result;
        }
    }
}