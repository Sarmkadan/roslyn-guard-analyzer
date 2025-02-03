# Roslyn Guard Analyzer

...

## SuppressionManagerExtensions

The `SuppressionManagerExtensions` class provides utility methods to easily interact with suppression records. These extensions enable you to add, remove, and query suppressions for rule violations.

### Usage Example
```csharp
var suppressionManager = new SuppressionManager();

// Add a suppression
var record = suppressionManager.AddSuppression(
    new SuppressionRecord
    {
        RuleId = "LYR001",
        TargetFile = "src/Domain/UserRepository.cs",
        Justification = "Legacy dependency scheduled for refactor",
        Author = "team-maintainer",
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    });

// Check if there are suppressions for a rule
if (suppressionManager.HasActiveSuppressionsForRule("LYR001"))
{
    Console.WriteLine("There are active suppressions for LYR001");
}

// Export active suppressions
var activeSuppressions = suppressionManager.ExportActiveSuppressions();
foreach (var suppression in activeSuppressions)
{
    Console.WriteLine($"Rule {suppression.RuleId} suppressed in {suppression.TargetFile}");
}

// Cleanup expired suppressions
var removedCount = suppressionManager.CleanupExpiredSuppressions();
Console.WriteLine($"Removed {removedCount} expired suppressions");

// Get suppression count
var count = suppressionManager.GetSuppressionCount();
Console.WriteLine($"Total suppressions: {count}");
```
