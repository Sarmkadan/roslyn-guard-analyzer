using System;

namespace RoslynGuardAnalyzer
{
    /// <summary>
    /// Attribute used to suppress RoslynGuard analysis for a specific class, method, or property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class SuppressRoslynGuardAttribute : Attribute
    {
        /// <summary>
        /// Gets the ID of the rule being suppressed.
        /// </summary>
        public string RuleId { get; }

        /// <summary>
        /// Gets or sets the justification for suppressing the rule.
        /// </summary>
        public string? Justification { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuppressRoslynGuardAttribute"/> class.
        /// </summary>
        /// <param name="ruleId">The ID of the rule being suppressed.</param>
        public SuppressRoslynGuardAttribute(string ruleId)
        {
            RuleId = ruleId;
        }
    }
}
