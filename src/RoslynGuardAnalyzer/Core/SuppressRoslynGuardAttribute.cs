using System;

namespace RoslynGuardAnalyzer
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class SuppressRoslynGuardAttribute : Attribute
    {
        public string RuleId { get; }
        public string? Justification { get; set; }

        public SuppressRoslynGuardAttribute(string ruleId)
        {
            RuleId = ruleId;
        }
    }
}
