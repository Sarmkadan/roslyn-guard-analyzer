# Baselines

A baseline is a JSON snapshot of violations that are already known and accepted
for the moment. On a later analysis, Roslyn Guard compares current violations
with that snapshot and reports only violations that do not match a baseline
entry. This lets a team enforce "no new violations" while it pays down existing
findings separately.

Baselining is a reporting filter, not a code fix or a rule suppression. The
analyzer still discovers the violation, and removing or expiring its baseline
entry makes it visible again.

The workflow is implemented by
`src/RoslynGuardAnalyzer/Services/BaselineService.cs`. The persisted model and
matching rules live in
`src/RoslynGuardAnalyzer/Domain/Models/BaselineViolation.cs`, which defines both
`BaselineViolation` and the containing `Baseline` document.

## Typical workflow

1. Analyze the project and create a baseline from the current violations.
2. Review and commit the generated JSON so changes to accepted findings are
   visible in code review.
3. In later runs, load that file and pass it to `FilterNewViolations`.
4. Fail or report based on the returned list, which contains only unmatched
   violations.
5. Regenerate or deliberately edit the baseline as accepted debt is removed,
   changed, or approved.

The CLI exposes this flow with `--create-baseline`, `--baseline`, and `--output`:

```bash
# Capture the current findings.
dotnet run --project src/RoslynGuardAnalyzer -- \
  ./src/MyProject/MyProject.csproj \
  --create-baseline \
  --output .roslyn-guard-baseline.json

# On later runs, report only findings not represented in the snapshot.
dotnet run --project src/RoslynGuardAnalyzer -- \
  ./src/MyProject/MyProject.csproj \
  --baseline .roslyn-guard-baseline.json
```

When `--baseline` and `--create-baseline` are supplied together, the CLI loads
and applies the old baseline first, then writes a new baseline containing only
the violations left after filtering. It does not merge the old entries into the
new file. Baseline creation also requires `--output`; without an output path,
the CLI continues with normal report generation.

## Service workflow

`BaselineService` is registered as the singleton implementation of
`IBaselineService`. Its operations can also be used directly:

```csharp
var previous = await baselineService.LoadBaselineAsync(baselinePath);
var result = await analysisService.AnalyzeProjectAsync(projectPath);

var newViolations = baselineService.FilterNewViolations(
    result.Violations,
    previous,
    TimeSpan.FromDays(90));

// When the reviewed current result should become the next snapshot:
var replacement = baselineService.CreateBaseline(result);
await baselineService.SaveBaselineAsync(replacement, baselinePath);
```

The two `CreateBaseline` overloads accept either an `AnalysisResult` or a project
name plus a list of `RuleViolation` objects. Each violation is converted with
`BaselineViolation.FromRuleViolation`, which assigns a new ID and creation time,
copies the message into `Description`, and computes the content hash.

`SaveBaselineAsync` serializes indented JSON and creates the parent directory if
needed. `LoadBaselineAsync` reads and deserializes the document. A missing file,
invalid JSON, or read failure is logged and returns `null`; a save failure is
logged and rethrown. Both operations reject blank paths, `..` path segments, and
characters reported as invalid by the current platform. This validation prevents
explicit traversal segments but does not constrain an absolute path to a
particular directory, so callers remain responsible for choosing an approved
location.

`FilterNewViolations` returns the input list unchanged when it is empty, the
baseline is `null`, or the baseline has no entries. Otherwise it tests every
current violation with `Baseline.Contains` and returns a new list containing the
non-matches. If a non-default expiration interval is supplied, expired entries
are removed from the in-memory `Baseline` before matching; this mutates the
loaded object but does not rewrite its file unless it is subsequently saved.

## File format

A baseline document contains format metadata, the project name, its creation
time, and an array of entries:

```json
{
  "version": "1.0",
  "schemaVersion": "1.0",
  "projectName": "MyProject",
  "baselineCreatedAt": "2026-09-16T10:00:00Z",
  "violations": [
    {
      "id": "980bd8dc-8758-4d43-9c70-8fc6cdf53ba4",
      "ruleId": "RG001",
      "filePath": "src/MyProject/LegacyService.cs",
      "lineNumber": 42,
      "contentHash": "hW7KQx2X6v+exampleFingerprint==",
      "createdAt": "2026-09-16T10:00:00Z",
      "description": "Replace the legacy implementation"
    }
  ]
}
```

`Version` and `SchemaVersion` currently default to `1.0`. `Id` identifies the
stored record, but it is not part of current-violation matching. `Description`
is informational. `CreatedAt` controls expiration, while `RuleId`, `FilePath`,
`LineNumber`, and `ContentHash` participate in matching as described below.

## Fingerprints and matching

For new entries, `ComputeContentHash` builds a SHA-256 fingerprint from:

- the rule ID;
- the normalized file path;
- the normalized violation message; and
- the code snippet, or an empty string when no snippet is available.

The digest is stored as Base64. Message normalization collapses line endings,
trims surrounding whitespace, and replaces path-like text, timestamps in
`yyyy-MM-dd HH:mm:ss` form, and digit sequences with placeholders. File paths
are normalized by `PathNormalizer`, which makes persisted paths portable across
common slash and case differences.

`BaselineViolation.Matches` first requires the same rule ID and an equivalent
normalized file path. If the stored entry has a content hash, the freshly
computed hash must also match. A matching hash is accepted even when the line
number has moved, making current-format baselines resilient to inserted or
deleted lines. If an older entry has no content hash, matching falls back to the
rule ID, equivalent path, and exact line number.

This means changes to the rule, logical path, normalized message, or code snippet
can cause a finding to appear as new. Moving unchanged content within the same
file generally does not. Renaming or moving the file changes the fingerprint and
path comparison, so the baseline should be reviewed or regenerated.

`BaselineViolation.Equals` uses rule ID, equivalent path, and content hash; it
does not use the generated ID, line number, creation time, or description.
`GetHashCode` uses the same fields.

## Expiration and maintenance

`IsValid(maxAge)` considers an entry valid while
`DateTime.UtcNow - CreatedAt <= maxAge`. `RemoveExpired` replaces the baseline's
violation list with only valid entries. Expiration is available through the
service API, but the current CLI calls `FilterNewViolations` without an
expiration interval, so CLI-loaded entries do not expire automatically.

Treat a baseline update as an explicit acceptance decision. Avoid regenerating
it blindly in the same gate that is meant to detect regressions: doing so would
turn newly introduced findings into accepted ones. A useful CI pattern is to
keep baseline creation as a reviewed maintenance action and use `--baseline` in
the regular validation job.
