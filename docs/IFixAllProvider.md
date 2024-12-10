# IFixAllProvider

`IFixAllProvider` orchestrates batch application of Roslyn code fixes across an entire solution, project, or document. It surfaces diagnostic severity filtering, rule scoping, breaking-change protection, and fix caps so that callers can preview or apply corrections in a controlled, auditable manner. The interface exposes both the configuration state and the outcome of a fix-all operation, including timing, violation counts, and per-rule messages.

## API

### Properties

- **`bool DryRun`**  
  Indicates whether the provider is configured to simulate fixes without persisting changes. When `true`, `ApplyAllAsync` computes results but does not modify source files.

- **`SeverityLevel? MinimumSeverity`**  
  The lowest diagnostic severity that qualifies a violation for fixing. Violations below this threshold are ignored. A `null` value means no severity filter is applied.

- **`IReadOnlyList<string>? RuleIds`**  
  An explicit whitelist of diagnostic IDs to fix. When `null`, all supported rules are eligible. An empty list suppresses all fixes.

- **`bool SkipBreakingChanges`**  
  When `true`, fixes that the analyzer classifies as potentially breaking (e.g., signature changes) are excluded from both preview and application.

- **`int MaxFixes`**  
  The maximum number of individual fix operations the provider will attempt. Once this limit is reached, remaining violations are left untouched and reported as unfixable.

- **`int TotalViolations`**  
  The total number of rule violations discovered during the most recent analysis pass, before any filtering or capping.

- **`int FixableViolations`**  
  The subset of `TotalViolations` that passed severity, rule-ID, breaking-change, and other filters and are eligible for fixing.

- **`CodeFixResult FixResult`**  
  An enum summarizing the aggregate outcome of the fix-all attempt (e.g., `Success`, `Partial`, `Failed`). Set after `ApplyAllAsync` completes.

- **`IReadOnlyList<RuleViolation> UnfixableViolations`**  
  Violations that were identified but could not be fixed, either because they exceeded `MaxFixes`, were blocked by `SkipBreakingChanges`, or had no registered code fix.

- **`TimeSpan Duration`**  
  The wall-clock time consumed by the most recent `PreviewAllAsync` or `ApplyAllAsync` call.

- **`bool IsSuccess`**  
  Convenience flag that is `true` when `FixResult` equals `CodeFixResult.Success` and no errors were recorded.

- **`IReadOnlyList<string> Messages`**  
  Human-readable diagnostic messages produced during the operation (warnings, per-file errors, fixer logs). May be empty on a clean run.

- **`FixAllProvider`**  
  The underlying Roslyn `FixAllProvider` instance that this wrapper delegates to for scoping and fix computation.

### Methods

- **`async Task<IReadOnlyList<CodeFix>> PreviewAllAsync()`**  
  Computes all eligible fixes without applying them. Returns a list of `CodeFix` objects describing each suggested change, its target document, and the diagnostic that triggered it.  
  *Throws* `InvalidOperationException` if the provider has not been initialized with a valid `FixAllContext`.

- **`async Task<FixAllResult> ApplyAllAsync()`**  
  Executes all eligible fixes and persists changes to the workspace. Returns a `FixAllResult` that bundles the final `CodeFixResult`, updated violation counts, timing, and any post-apply messages.  
  *Throws* `InvalidOperationException` when called without a valid context, or `OperationCanceledException` if the underlying operation is canceled.

## Usage

### Example 1: Preview fixes with severity and rule filters

```csharp
var provider = workspace.GetService<IFixAllProvider>();
provider.MinimumSeverity = SeverityLevel.Warning;
provider.RuleIds = new[] { "CS8618", "CA1822" };
provider.SkipBreakingChanges = true;
provider.MaxFixes = 200;

IReadOnlyList<CodeFix> preview = await provider.PreviewAllAsync();

Console.WriteLine($"Previewed {preview.Count} fixes across {provider.TotalViolations} total violations.");
foreach (var fix in preview)
{
    Console.WriteLine($"[{fix.Diagnostic.Id}] {fix.Document.Name}: {fix.Description}");
}
```

### Example 2: Apply fixes with a dry-run safety check

```csharp
var provider = workspace.GetService<IFixAllProvider>();
provider.DryRun = true;

FixAllResult dryResult = await provider.ApplyAllAsync();
if (!dryResult.IsSuccess)
{
    Console.WriteLine($"Dry-run failed: {string.Join(", ", provider.Messages)}");
    return;
}

// Review unfixable violations before committing
foreach (var v in provider.UnfixableViolations)
{
    Console.WriteLine($"Unfixable {v.Diagnostic.Id} in {v.Document.Name}: {v.Reason}");
}

provider.DryRun = false;
FixAllResult result = await provider.ApplyAllAsync();
Console.WriteLine($"Applied {result.FixesApplied} fixes in {result.Duration.TotalSeconds:F2}s.");
```

## Notes

- **Ordering of operations:** `PreviewAllAsync` and `ApplyAllAsync` are independent entry points. Calling `PreviewAllAsync` does not automatically update `TotalViolations` or `FixableViolations` unless the implementation refreshes analysis internally. Rely on the properties only after the corresponding async method completes.
- **Thread safety:** The properties (`TotalViolations`, `FixableViolations`, `Duration`, etc.) reflect the outcome of the last completed async operation and are not updated atomically during execution. Concurrent calls to `ApplyAllAsync` from different threads may overwrite state; serialize access if consistent property reads are required.
- **`MaxFixes` and `UnfixableViolations`:** When `MaxFixes` is exceeded, the excess violations appear in `UnfixableViolations` even if a code fix exists for them. The provider does not guarantee which subset of violations gets fixed—ordering depends on document enumeration.
- **`SkipBreakingChanges` classification:** Breaking-change detection is delegated to the underlying analyzer metadata. A fix is skipped only if the analyzer explicitly marks it as breaking; fixes without metadata are always considered safe.
- **Empty `RuleIds` list:** Passing an empty `IReadOnlyList<string>` for `RuleIds` disables all rules, resulting in zero fixes and `FixableViolations == 0`. Use `null` to opt into all supported rules.
- **`DryRun` and workspace state:** When `DryRun` is `true`, `ApplyAllAsync` must not mutate the workspace. If an implementation does not fully honor this, callers should clone the workspace or solution before calling.
