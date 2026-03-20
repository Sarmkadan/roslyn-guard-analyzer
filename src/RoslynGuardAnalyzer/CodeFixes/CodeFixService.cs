// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.CodeFixes;

/// <summary>
/// Defines operations for generating and applying auto-fix actions for detected violations.
/// </summary>
public interface ICodeFixService
{
    /// <summary>
    /// Generates the available <see cref="CodeFix"/> actions for the supplied violations.
    /// </summary>
    /// <param name="violations">The violations to generate fixes for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A task that resolves to the read-only collection of generated code fixes.</returns>
    Task<IReadOnlyList<CodeFix>> GetFixesAsync(
        IEnumerable<RuleViolation> violations,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies the specified <see cref="CodeFix"/> actions to their target source files.
    /// </summary>
    /// <param name="fixes">The fixes to apply.</param>
    /// <param name="dryRun">
    /// When <see langword="true"/> the operation is simulated without persisting any file changes.
    /// </param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A task that resolves to the result of the fix operation.</returns>
    Task<CodeFixResult> ApplyFixesAsync(
        IEnumerable<CodeFix> fixes,
        bool dryRun = false,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Generates and applies auto-fix code actions that resolve architectural rule violations
/// detected during analysis. Fix providers are registered statically, keyed by rule identifier,
/// so new rules can expose fixes simply by adding an entry to <see cref="_fixProviders"/>.
/// </summary>
public sealed class CodeFixService : ICodeFixService
{
    private readonly ILogger<CodeFixService> _logger;

    private static readonly Dictionary<string, Func<RuleViolation, CodeFix?>> _fixProviders =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["RG-N001"] = BuildInterfacePrefixFix,
            ["RG-N002"] = BuildAsyncSuffixFix,
            ["RG-A001"] = BuildConfigureAwaitFix,
            ["RG-A002"] = BuildAsyncVoidFix,
        };

    /// <summary>
    /// Initialises a new instance of <see cref="CodeFixService"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is <see langword="null"/>.</exception>
    public CodeFixService(ILogger<CodeFixService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CodeFix>> GetFixesAsync(
        IEnumerable<RuleViolation> violations,
        CancellationToken cancellationToken = default)
    {
        if (violations == null)
            throw new ArgumentNullException(nameof(violations));

        var violationList = violations.ToList();

        return await Task.Run(() =>
        {
            var fixes = new List<CodeFix>();

            foreach (var violation in violationList)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_fixProviders.TryGetValue(violation.RuleId, out var provider))
                {
                    _logger.LogDebug("No fix provider registered for rule {RuleId}.", violation.RuleId);
                    continue;
                }

                try
                {
                    var fix = provider(violation);
                    if (fix is not null)
                        fixes.Add(fix);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Fix provider for rule {RuleId} failed on violation {ViolationId}.",
                        violation.RuleId, violation.Id);
                }
            }

            _logger.LogInformation(
                "Generated {FixCount} fix(es) for {ViolationCount} violation(s).",
                fixes.Count, violationList.Count);

            return (IReadOnlyList<CodeFix>)fixes.AsReadOnly();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<CodeFixResult> ApplyFixesAsync(
        IEnumerable<CodeFix> fixes,
        bool dryRun = false,
        CancellationToken cancellationToken = default)
    {
        if (fixes == null)
            throw new ArgumentNullException(nameof(fixes));

        var fixList = fixes.ToList();
        var result = new CodeFixResult();

        if (fixList.Count == 0)
        {
            result.IsSuccess = true;
            return result;
        }

        // Group fixes by file so each source file is read and written at most once.
        var byFile = fixList.GroupBy(f => f.FilePath, StringComparer.OrdinalIgnoreCase);

        foreach (var fileGroup in byFile)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var filePath = fileGroup.Key;

            if (!File.Exists(filePath))
            {
                var msg = $"File not found — skipping all fixes for '{filePath}'.";
                _logger.LogWarning(msg);
                result.Messages.Add(msg);
                result.FailedFixes.AddRange(fileGroup);
                continue;
            }

            await ApplyFixesToFileAsync(filePath, fileGroup.ToList(), dryRun, result, cancellationToken);
        }

        result.IsSuccess = result.FailedFixes.Count == 0;
        return result;
    }

    // -------------------------------------------------------------------------
    // File application helper
    // -------------------------------------------------------------------------

    private async Task ApplyFixesToFileAsync(
        string filePath,
        List<CodeFix> fixes,
        bool dryRun,
        CodeFixResult result,
        CancellationToken cancellationToken)
    {
        try
        {
            var lines = await File.ReadAllLinesAsync(filePath, cancellationToken);
            var appliedCountBefore = result.AppliedFixes.Count;

            // Process in reverse line order to avoid index drift when replacements
            // change line lengths (single-line replacements only, so no offset needed).
            var ordered = fixes.OrderByDescending(f => f.StartLine).ToList();

            foreach (var fix in ordered)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var idx = fix.StartLine - 1;

                if (idx < 0 || idx >= lines.Length)
                {
                    _logger.LogWarning(
                        "Fix {FixId} references out-of-range line {Line} in '{File}'. Skipped.",
                        fix.Id, fix.StartLine, filePath);
                    result.FailedFixes.Add(fix);
                    continue;
                }

                if (!lines[idx].Contains(fix.OriginalCode, StringComparison.Ordinal))
                {
                    _logger.LogWarning(
                        "Original token not found at line {Line} in '{File}'. Fix {FixId} skipped.",
                        fix.StartLine, filePath, fix.Id);
                    result.FailedFixes.Add(fix);
                    continue;
                }

                lines[idx] = lines[idx].Replace(
                    fix.OriginalCode,
                    fix.ReplacementCode,
                    StringComparison.Ordinal);

                result.AppliedFixes.Add(fix);
                result.Messages.Add($"Applied: {fix.Title} — {filePath}:{fix.StartLine}");
            }

            var anyApplied = result.AppliedFixes.Count > appliedCountBefore;

            if (!dryRun && anyApplied)
            {
                await File.WriteAllLinesAsync(filePath, lines, cancellationToken);
                _logger.LogInformation(
                    "Wrote {Count} fix(es) to '{File}'.",
                    result.AppliedFixes.Count - appliedCountBefore,
                    filePath);
            }
            else if (dryRun && anyApplied)
            {
                result.Messages.Add($"Dry-run: changes to '{filePath}' were NOT written to disk.");
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to apply fixes to '{File}'.", filePath);
            result.FailedFixes.AddRange(fixes.Where(f => !result.AppliedFixes.Contains(f)));
        }
    }

    // -------------------------------------------------------------------------
    // Built-in fix providers
    // -------------------------------------------------------------------------

    private static CodeFix? BuildInterfacePrefixFix(RuleViolation violation)
    {
        if (string.IsNullOrWhiteSpace(violation.CodeSnippet))
            return null;

        var match = Regex.Match(violation.CodeSnippet, @"interface\s+(?<name>\w+)");
        if (!match.Success)
            return null;

        var name = match.Groups["name"].Value;
        if (name.StartsWith('I'))
            return null;

        var fixedName = "I" + name;

        return new CodeFix
        {
            ViolationId = violation.Id,
            RuleId = violation.RuleId,
            Title = $"Prefix interface '{name}' with 'I'",
            Description = $"Rename '{name}' to '{fixedName}' to satisfy the interface naming convention (RG-N001).",
            FilePath = violation.FilePath,
            StartLine = violation.LineNumber,
            EndLine = violation.LineNumber,
            OriginalCode = $"interface {name}",
            ReplacementCode = $"interface {fixedName}",
            Severity = violation.Severity,
            IsBreakingChange = true,
        };
    }

    private static CodeFix? BuildAsyncSuffixFix(RuleViolation violation)
    {
        if (string.IsNullOrWhiteSpace(violation.CodeSnippet))
            return null;

        var match = Regex.Match(
            violation.CodeSnippet,
            @"(?:public|private|protected|internal)\s+\S+\s+(?<name>\w+)\s*\(");

        if (!match.Success)
            return null;

        var name = match.Groups["name"].Value;
        if (name.EndsWith("Async", StringComparison.OrdinalIgnoreCase))
            return null;

        var fixedName = name + "Async";

        return new CodeFix
        {
            ViolationId = violation.Id,
            RuleId = violation.RuleId,
            Title = $"Add 'Async' suffix to method '{name}'",
            Description = $"Rename '{name}' to '{fixedName}' to follow the async method naming convention (RG-N002).",
            FilePath = violation.FilePath,
            StartLine = violation.LineNumber,
            EndLine = violation.LineNumber,
            OriginalCode = name + "(",
            ReplacementCode = fixedName + "(",
            Severity = violation.Severity,
            IsBreakingChange = true,
        };
    }

    private static CodeFix? BuildConfigureAwaitFix(RuleViolation violation)
    {
        if (string.IsNullOrWhiteSpace(violation.CodeSnippet))
            return null;

        var match = Regex.Match(violation.CodeSnippet, @"await\s+(?<expr>[^;]+);");
        if (!match.Success)
            return null;

        var expr = match.Groups["expr"].Value.Trim();
        if (expr.Contains(".ConfigureAwait", StringComparison.OrdinalIgnoreCase))
            return null;

        return new CodeFix
        {
            ViolationId = violation.Id,
            RuleId = violation.RuleId,
            Title = "Add .ConfigureAwait(false) to awaited expression",
            Description = "Appending .ConfigureAwait(false) prevents deadlocks when the caller does not require the original synchronisation context (RG-A001).",
            FilePath = violation.FilePath,
            StartLine = violation.LineNumber,
            EndLine = violation.LineNumber,
            OriginalCode = $"await {expr};",
            ReplacementCode = $"await {expr}.ConfigureAwait(false);",
            Severity = violation.Severity,
            IsBreakingChange = false,
        };
    }

    private static CodeFix? BuildAsyncVoidFix(RuleViolation violation)
    {
        if (string.IsNullOrWhiteSpace(violation.CodeSnippet))
            return null;

        if (!violation.CodeSnippet.Contains("async void", StringComparison.Ordinal))
            return null;

        return new CodeFix
        {
            ViolationId = violation.Id,
            RuleId = violation.RuleId,
            Title = "Replace 'async void' with 'async Task'",
            Description = "Changing the return type to Task allows exceptions to be observed and the method to be awaited, eliminating silent failure risks (RG-A002).",
            FilePath = violation.FilePath,
            StartLine = violation.LineNumber,
            EndLine = violation.LineNumber,
            OriginalCode = "async void",
            ReplacementCode = "async Task",
            Severity = violation.Severity,
            IsBreakingChange = false,
        };
    }
}
