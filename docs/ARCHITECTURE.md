# ARCHITECTURE

This is the "how it actually works" doc, grounded in the current code under
`src/RoslynGuardAnalyzer/`. The older `docs/architecture.md` describes the
intended design in broader strokes; when the two disagree, trust this file
and the source.

## Overview

Roslyn Guard Analyzer is a single-project .NET 10 console app
(`src/RoslynGuardAnalyzer/RoslynGuardAnalyzer.csproj`) that scans a C#
project or file, extracts a lightweight model of the code
(`CodeElement`s), runs a set of architectural/style rules over that model,
and prints or writes a report. Exit code is `1` when `--strict` is set and
violations were found.

```
args ──> Program.Main
           │  builds IConfiguration (appsettings.json + env RoslynGuardAnalyzer__ + cmdline)
           │  builds ServiceCollection via ServiceCollectionExtensions.RegisterAnalyzerServices()
           ▼
        IAnalysisService (AnalysisService)
           │  LoadProjectMetadata: enumerate *.cs (skip bin/obj/.git)
           │  ExtractCodeElements*: line-based parse -> List<CodeElement>
           │  merge partial classes across files
           ▼
        IRuleEngine (RuleEngine)
           │  for each enabled rule in IRuleRegistry:
           │    filter out suppressed elements ([SuppressRoslynGuard], GUARD_SKIP comments)
           │    dispatch on RuleCategory (LayerDependency / NamingConvention / AsyncPattern / NullSafety)
           │    or CustomAnalysisRule.EvaluateAsync for user rules
           ▼
        AnalysisResult (violations + stats)
           ▼
        IReportingService (ReportingService) -> text report -> stdout or --output file
```

## Module breakdown

| Folder | What lives there | Actually wired into the CLI run? |
|---|---|---|
| `Program.cs` | Entry point, config bootstrap, its own inline arg parser, help/version | yes |
| `Infrastructure/` | `ServiceCollectionExtensions` - the DI composition root | yes |
| `Services/` | `AnalysisService`, `RuleEngine`, `RuleRegistry`, `ValidationService`, `ReportingService`, plus `OutputWriter`, `ResultAggregator`, `AnalysisStatisticsService`, `BackgroundTaskQueue`, `DiagnosticsService` | first five yes; the rest are registered/available but not on the main path |
| `Domain/Models/` | `AnalysisRule`, `AnalysisResult`, `AnalysisProject`, `CodeElement`, `RuleViolation`, `ViolationReport`, `RuleConfiguration` | yes (core data model) |
| `Core/` | `AnalyzerConstants` (rule ids, layer suffixes, messages), enums, `SuppressRoslynGuardAttribute` | yes |
| `Rules/` | `CustomAnalysisRule`, `CustomRuleBuilder`, `CustomRuleEngine`, `CustomRuleRegistry` | extension point; `RuleEngine` delegates to `CustomAnalysisRule.EvaluateAsync` when it meets one |
| `Suppressions/` | `SuppressionManager` + `SuppressionRecord` - file/line/rule-scoped suppressions with expiry | registered in DI; `RuleEngine` also does its own attribute/`GUARD_SKIP` check inline |
| `Cli/` | `CliArgumentParser`, `CliOptions`, `CommandLineProcessor`, `HelpGenerator` | `CliOptions` yes; note `Program.Main` currently parses args itself instead of using `CliArgumentParser` |
| `Formatters/` | `IOutputFormatter` + `JsonFormatter`, `CsvFormatter`, `HtmlFormatter`, `XmlFormatter`(via registry), `FormatterRegistry` | used through `OutputWriter`, not through the default `Main` path (which uses `ReportingService`'s text report) |
| `Configuration/` | `RoslynGuardAnalyzerOptions` (+validator), `ConfigurationLoader`, `RuleConfigurationBuilder` | options binding/validation yes |
| `Middleware/` | `AnalysisPipeline` + `IMiddleware` (logging, error handling, perf metrics) | standalone composable pipeline; not inserted into the default run |
| `Events/` | `EventBus`, analysis lifecycle events | available, not published from the default run |
| `Caching/` | `CacheService`, `CacheKeyGenerator` | available, not on the default path |
| `Data/` | `RepositoryBase` + rule/result/project repositories (JSON-on-disk persistence) | registered as singletons; used when a data directory is supplied via the `RegisterAnalyzerServices(string dataDirectory)` overload |
| `Integration/` | `WebhookHandler`, `HttpClientFactory` | opt-in integration surface, not part of the CLI flow |
| `CodeFixes/` | `CodeFixService`, `FixAllProvider` | registered in DI, exposed for programmatic use |
| `Exceptions/` | `RoslynGuardException` hierarchy (`AnalysisException`, `ConfigurationException`, `FileAccessException`, `ParseException`) | yes |
| `Utilities/` | `TypeNameMatcher`, `PathNormalizer`, `AnalysisFilterBuilder`, string/collection helpers | supporting code |

Tests live in `tests/roslyn-guard-analyzer.Tests` (xUnit) and
`tests/roslyn-guard-analyzer.Benchmarks` (BenchmarkDotNet over `RuleEngine`).

## Key design decisions and trade-offs

**1. Line-based extraction instead of full Roslyn semantic analysis.**
`AnalysisService.ExtractCodeElementsFromFileAsync` splits files into lines
and pattern-matches `namespace `, `class `, `interface `, and
public-method-looking lines into `CodeElement`s. The
`Microsoft.CodeAnalysis.CSharp` package is referenced and is the intended
long-term parser, but the current extraction deliberately avoids building
a `Compilation`: it keeps a cold run fast, needs no MSBuild workspace
resolution, and works on broken/partial code. The trade-off is real -
multi-line declarations, expression-bodied members, generics with
constraints, and anything requiring symbol resolution can be missed or
misclassified. Rules that need "who references whom" get only the
string-level `Dependencies` list, not a semantic graph.

**2. Rule dispatch by category enum, not polymorphism, for built-in rules.**
`RuleEngine.ExecuteRuleAsync` switches on `RuleCategory` and calls a
private checker per category. Built-in checks stay in one file where the
shared layer/naming heuristics (`AnalyzerConstants.LayerPatterns`) are
visible together; the open/closed escape hatch is `CustomAnalysisRule`,
which carries its own `EvaluateAsync` delegate and bypasses the switch
entirely. If you add a new built-in category you must touch the switch -
accepted cost, the category set changes rarely.

**3. Rules execute sequentially, and rule checks are synchronous.**
`ExecuteAllRulesAsync` awaits rules one by one. Checks are CPU-bound
in-memory list scans, so `Task.Run` fan-out was consciously avoided (see
the comment on `ExecuteRuleAsync`); parallelism knobs
(`--max-threads`, `MaxParallelThreads`) exist in options but the current
engine does not use them. Parallelizing at the file-extraction level would
pay off before parallelizing rules.

**4. Two DI registration overloads instead of an options object.**
`ServiceCollectionExtensions.RegisterAnalyzerServices()` and
`RegisterAnalyzerServices(string dataDirectory)` differ only in how the
`Data/` repositories and `AnalyzerConfiguration` are constructed. Simple
call sites stay simple (the CLI uses the parameterless one); embedding
hosts that want persistence pass a directory. The shared middle is kept in
one private method so the two lists cannot drift.

**5. Everything is a singleton.**
The process is one-shot: build container, run one analysis, exit. Scopes
would add ceremony with no benefit. If this ever becomes a long-running
server (see `Integration/WebhookHandler`), `RuleRegistry` and the
repositories are the types to re-examine for shared mutable state.

**6. Configuration: Options pattern with double validation.**
`RoslynGuardAnalyzerOptions` binds from the `RoslynGuardAnalyzer` config
section (json + `RoslynGuardAnalyzer__*` env vars + command line), is
validated once at startup, then CLI flags are merged on top
(`MergeWithCliOptions`) and validated again. Rationale: CLI must win over
files, but a CLI flag can also make a valid config invalid, so validating
only before the merge is not enough.

**7. Suppression is layered.**
Three mechanisms, from closest-to-code outward:
`[SuppressRoslynGuard("RULE_ID")]` attributes, `// GUARD_SKIP:RULE_ID`
comments (both checked inline by `RuleEngine`), and
`SuppressionManager` records with optional expiry for
suppress-without-touching-the-code workflows (e.g. baseline files).

## Data flow summary

`string projectPath` -> `AnalysisProject` (file list + properties) ->
`List<CodeElement>` (with partial-class merge) -> `List<RuleViolation>`
per rule -> accumulated into `AnalysisResult` (counts, per-severity map,
timings) -> `ReportingService.GenerateReport` text, or
`OutputWriter`/`FormatterRegistry` for json/csv/html when driven
programmatically.

## Extension points

- **Custom rules**: build with `CustomRuleBuilder`, register through
  `IRuleRegistry.RegisterRule` (or `ICustomRuleRegistry`). See
  `docs/custom-rule-development.md`.
- **Output formats**: implement `IOutputFormatter`, add via
  `FormatterRegistry.Register`.
- **Cross-cutting behavior**: implement `IMiddleware` and compose with
  `AnalysisPipeline.Use(...)`.
- **Events**: subscribe to analysis lifecycle events on `EventBus`.
- **Persistence**: use the `RegisterAnalyzerServices(dataDirectory)`
  overload to get JSON-backed repositories for rules/results/projects.

## Known limitations

- The parser is heuristic (see decision 1). Do not expect semantic-level
  accuracy; `LYR001` layer detection is suffix-based
  (`*Repository`/`*Service`/`*Controller`).
- `Program.Main` has its own switch-based arg parser; `Cli/CliArgumentParser`
  is a parallel implementation used by `CommandLineProcessor`. Keeping both
  in sync is manual today.
- `--max-threads` and `--timeout` are parsed and stored but not enforced by
  the engine yet.
- The default CLI path emits only the text report; `--format json/csv/html`
  routes through the same text `ReportingService` unless you drive
  `OutputWriter` yourself. Unifying these is the obvious next refactor.
- Several subsystems (EventBus, CacheService, Middleware, Webhooks) are
  built and tested but not attached to the default CLI run - they exist for
  embedding scenarios.
