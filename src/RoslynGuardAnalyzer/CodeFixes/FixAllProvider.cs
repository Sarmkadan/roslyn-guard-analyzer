#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.CodeFixes;

/// <summary>
/// Applies or previews bulk code fixes for collections of rule violations.
/// </summary>
public interface IFixAllProvider
{
    /// <summary>
    /// Applies all eligible fixes for the supplied violations.
    /// </summary>
    Task<FixAllResult> ApplyAllAsync(IEnumerable<RuleViolation> violations, FixAllOptions options, CancellationToken ct = default);

    /// <summary>
    /// Returns the fixes that would be applied for the supplied violations.
    /// </summary>
    Task<IReadOnlyList<CodeFix>> PreviewAllAsync(IEnumerable<RuleViolation> violations, FixAllOptions options, CancellationToken ct = default);
}

/// <summary>
/// Controls how bulk fixes are filtered and executed.
/// </summary>
public sealed class FixAllOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation should simulate changes only.
    /// </summary>
    public bool DryRun { get; set; }

    /// <summary>
    /// Gets or sets the minimum severity a violation must have to be considered.
    /// </summary>
    public SeverityLevel? MinimumSeverity { get; set; }

    /// <summary>
    /// Gets or sets the optional set of rule identifiers to include.
    /// </summary>
    public IReadOnlyList<string>? RuleIds { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether fixes marked as breaking changes should be skipped.
    /// </summary>
    public bool SkipBreakingChanges { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of fixes to apply. Zero means unlimited.
    /// </summary>
    public int MaxFixes { get; set; }
}

/// <summary>
/// Describes the outcome of a bulk fix operation.
/// </summary>
public sealed class FixAllResult
{
    /// <summary>
    /// Gets or sets the total number of input violations.
    /// </summary>
    public int TotalViolations { get; set; }

    /// <summary>
    /// Gets or sets the number of violations for which a fix was available.
    /// </summary>
    public int FixableViolations { get; set; }

    /// <summary>
    /// Gets or sets the underlying code-fix service result.
    /// </summary>
    public CodeFixResult FixResult { get; set; } = new();

    /// <summary>
    /// Gets or sets the violations that could not be fixed.
    /// </summary>
    public IReadOnlyList<RuleViolation> UnfixableViolations { get; set; } = Array.Empty<RuleViolation>();

    /// <summary>
    /// Gets or sets the total execution duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the bulk fix completed successfully.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets informational messages produced during execution.
    /// </summary>
    public IReadOnlyList<string> Messages { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Coordinates bulk preview and application of code fixes.
/// </summary>
public sealed class FixAllProvider : IFixAllProvider
{
    private readonly ICodeFixService _codeFixService;
    private readonly ILogger<FixAllProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FixAllProvider"/> class.
    /// </summary>
    public FixAllProvider(ICodeFixService codeFixService, ILogger<FixAllProvider> logger)
    {
        _codeFixService = codeFixService ?? throw new ArgumentNullException(nameof(codeFixService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CodeFix>> PreviewAllAsync(IEnumerable<RuleViolation> violations, FixAllOptions options, CancellationToken ct = default)
    {
        var filteredViolations = FilterViolations(violations, options).ToList();
        LogViolationGroups(filteredViolations);

        var fixes = await _codeFixService.GetFixesAsync(filteredViolations, ct).ConfigureAwait(false);
        var filteredFixes = ApplyFixFilters(fixes, options).ToList().AsReadOnly();

        _logger.LogInformation("Previewed {ViolationCount} violations and found {FixCount} fixes.", filteredViolations.Count, filteredFixes.Count);
        return filteredFixes;
    }

    /// <inheritdoc/>
    public async Task<FixAllResult> ApplyAllAsync(IEnumerable<RuleViolation> violations, FixAllOptions options, CancellationToken ct = default)
    {
        if (violations is null)
            throw new ArgumentNullException(nameof(violations));

        if (options is null)
            throw new ArgumentNullException(nameof(options));

        var stopwatch = Stopwatch.StartNew();
        var violationList = violations.ToList();
        var filteredViolations = FilterViolations(violationList, options).ToList();
        var fixes = await PreviewAllAsync(filteredViolations, options, ct).ConfigureAwait(false);
        var fixableViolationIds = fixes.Select(fix => fix.ViolationId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unfixableViolations = filteredViolations.Where(violation => !fixableViolationIds.Contains(violation.Id)).ToList().AsReadOnly();
        var fixResult = await _codeFixService.ApplyFixesAsync(fixes, options.DryRun, ct).ConfigureAwait(false);
        stopwatch.Stop();

        var messages = fixResult.Messages.ToList();
        messages.Add($"{filteredViolations.Count} violations found, {fixes.Count} fixable, {fixResult.AppliedFixes.Count} fixes applied");

        _logger.LogInformation("{Summary}", messages[^1]);

        return new FixAllResult
        {
            TotalViolations = violationList.Count,
            FixableViolations = fixes.Count,
            FixResult = fixResult,
            UnfixableViolations = unfixableViolations,
            Duration = stopwatch.Elapsed,
            IsSuccess = fixResult.IsSuccess,
            Messages = messages.AsReadOnly()
        };
    }

    private static IEnumerable<RuleViolation> FilterViolations(IEnumerable<RuleViolation> violations, FixAllOptions options)
    {
        if (violations is null)
            throw new ArgumentNullException(nameof(violations));

        if (options is null)
            throw new ArgumentNullException(nameof(options));

        var query = violations;

        if (options.MinimumSeverity.HasValue)
            query = query.Where(violation => violation.Severity >= options.MinimumSeverity.Value);

        if (options.RuleIds is { Count: > 0 })
        {
            var allowedRules = new HashSet<string>(options.RuleIds, StringComparer.OrdinalIgnoreCase);
            query = query.Where(violation => allowedRules.Contains(violation.RuleId));
        }

        return query;
    }

    private static IEnumerable<CodeFix> ApplyFixFilters(IEnumerable<CodeFix> fixes, FixAllOptions options)
    {
        var query = fixes;

        if (options.SkipBreakingChanges)
            query = query.Where(fix => !fix.IsBreakingChange);

        if (options.MaxFixes > 0)
            query = query.Take(options.MaxFixes);

        return query;
    }

    private void LogViolationGroups(IEnumerable<RuleViolation> violations)
    {
        foreach (var group in violations.GroupBy(violation => violation.RuleId))
        {
            _logger.LogInformation("Rule {RuleId} has {Count} candidate violations.", group.Key, group.Count());
        }
    }
}
