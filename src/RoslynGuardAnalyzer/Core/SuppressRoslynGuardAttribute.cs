using System;

namespace RoslynGuardAnalyzer
{
    /// <summary>
    /// Indicates that RoslynGuard analysis is suppressed for a class, method, or property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class SuppressRoslynGuardAttribute : Attribute
    {
        /// <summary>
        /// Gets the identifier of the RoslynGuard rule to suppress.
        /// </summary>
        public string RuleId { get; }

        /// <summary>
        /// Gets or sets the reason for suppressing the RoslynGuard rule.
        /// </summary>
        public string? Justification { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuppressRoslynGuardAttribute"/> class for the specified rule.
        /// </summary>
        /// <param name="ruleId">The identifier of the RoslynGuard rule to suppress.</param>
        public SuppressRoslynGuardAttribute(string ruleId)
        {
            RuleId = ruleId;
        }
    }
}
