using System;
using System.Collections.Generic;
using System.Linq;

namespace RoslynGuardAnalyzer.Suppressions
{
    /// <summary>
    /// Validation helpers for <see cref="SuppressionRecord"/>.
    /// </summary>
    public static class SuppressionRecordValidation
    {
        /// <summary>
        /// Validates the supplied <see cref="SuppressionRecord"/> and returns a list of human‑readable problems.
        /// </summary>
        /// <param name="value">The record to validate.</param>
        /// <returns>A read‑only list of validation error messages. Empty if the record is valid.</returns>
        public static IReadOnlyList<string> Validate(this SuppressionRecord value)
        {
            var errors = new List<string>();

            if (value is null)
            {
                errors.Add("SuppressionRecord instance is null.");
                return errors;
            }

            // String properties – must be non‑null and non‑whitespace
            if (string.IsNullOrWhiteSpace(value.Id))
                errors.Add("Id must not be null, empty, or whitespace.");

            if (string.IsNullOrWhiteSpace(value.RuleId))
                errors.Add("RuleId must not be null, empty, or whitespace.");

            if (string.IsNullOrWhiteSpace(value.Justification))
                errors.Add("Justification must not be null, empty, or whitespace.");

            if (string.IsNullOrWhiteSpace(value.Author))
                errors.Add("Author must not be null, empty, or whitespace.");

            // Optional string properties – if supplied, they must contain something meaningful
            if (value.TargetFile is not null && string.IsNullOrWhiteSpace(value.TargetFile))
                errors.Add("TargetFile, when provided, must not be empty or whitespace.");

            if (value.TargetElement is not null && string.IsNullOrWhiteSpace(value.TargetElement))
                errors.Add("TargetElement, when provided, must not be empty or whitespace.");

            // DateTime properties – CreatedAt must be set, ExpiresAt (if set) must be after CreatedAt
            if (value.CreatedAt == default)
                errors.Add("CreatedAt must be a valid date and time.");

            if (value.ExpiresAt.HasValue)
            {
                if (value.ExpiresAt.Value == default)
                    errors.Add("ExpiresAt, when provided, must be a valid date and time.");
                else if (value.ExpiresAt.Value <= value.CreatedAt)
                    errors.Add("ExpiresAt must be later than CreatedAt.");
            }

            // No validation needed for boolean properties (IsActive, Matches) – they are always valid.

            return errors;
        }

        /// <summary>
        /// Returns <c>true</c> if the <see cref="SuppressionRecord"/> passes all validation checks.
        /// </summary>
        public static bool IsValid(this SuppressionRecord value) => !value.Validate().Any();

        /// <summary>
        /// Ensures the <see cref="SuppressionRecord"/> is valid, otherwise throws an <see cref="ArgumentException"/>
        /// containing the validation problems.
        /// </summary>
        public static void EnsureValid(this SuppressionRecord value)
        {
            var problems = value.Validate();
            if (problems.Count > 0)
            {
                var message = $"SuppressionRecord is invalid: {string.Join("; ", problems)}";
                throw new ArgumentException(message, nameof(value));
            }
        }
    }
}
