// AdvancedUsage.cs
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;
using RoslynGuardAnalyzer.Rules;
using RoslynGuardAnalyzer.Suppressions;
using RoslynGuardAnalyzer.CodeFixes;

// Demonstrating configuration, custom options, and error handling
public class AdvancedUsage
{
    public async Task RunAdvancedAnalysis(
        IAnalysisService analysisService,
        IRuleRegistry ruleRegistry,
        ISuppressionManager suppressionManager,
        IFixAllProvider fixAllProvider)
    {
        try
        {
            // 1. Define custom configuration
            var config = new RuleConfiguration
            {
                Enabled = true,
                Severity = RuleSeverity.Warning
            };

            // 2. Register custom rules
            var customRule = CustomRuleBuilder.Create("CUS001", "Custom Naming Rule")
                .For(RuleCategory.Naming)
                .When(element => element.Name.StartsWith("Temp"))
                .WithMessage(element => $"Element '{element.Name}' should not start with Temp")
                .Build();
            
            ruleRegistry.RegisterRule(customRule);

            // 3. Run analysis
            var result = await analysisService.AnalyzeWithConfigAsync("./src", config);

            // 4. Filter results with Suppression Manager
            var activeViolations = suppressionManager.FilterSuppressed(result.Violations);

            // 5. Apply automatic fixes
            var fixOptions = new FixAllOptions { DryRun = true };
            var fixResult = await fixAllProvider.ApplyAllAsync(activeViolations, fixOptions);
            
            Console.WriteLine($"Applied {fixResult.TotalFixesApplied} fixes.");
        }
        catch (RoslynGuardException ex)
        {
            Console.WriteLine($"Analysis error: {ex.Message}");
        }
    }
}
