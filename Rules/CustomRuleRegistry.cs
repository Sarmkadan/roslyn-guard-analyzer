using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RoslynGuard.Analyzers.Rules
{
    public class CustomRuleRegistry
    {
        private readonly ConcurrentDictionary<string, CustomRule> _customRules = new ConcurrentDictionary<string, CustomRule>();

        public void RegisterCustomRule(CustomRule rule)
        {
            // ... rest of the code remains the same ...
        }
    }
}