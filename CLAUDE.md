# CLAUDE.md

## Overview

Roslyn Guard Analyzer: a .NET 10 console CLI that scans C# sources, runs architectural/style rules (layer dependencies, naming, async patterns, null safety, custom rules) and emits a report (text, JSON, CSV, HTML, SARIF).

## Build

- SDK pinned in `global.json` (10.0.100, rollForward latestMinor).
- `dotnet build RoslynGuardAnalyzer.sln -c Debug` (or `make build`)
- `dotnet build -c Release` (or `make release`)
- `dotnet publish src/RoslynGuardAnalyzer -c Release -o ./publish` (or `make publish`)
- `dotnet pack src/RoslynGuardAnalyzer -c Release -o ./nupkg` (or `make package`)
- Run analyzer on itself: `dotnet run --project src/RoslynGuardAnalyzer -- ./src --format json --output analysis.json` (or `make analyze`)
- Docker: `make docker-build` / `docker-compose.yml`

## Tests

- Framework: xUnit + FluentAssertions + NSubstitute; benchmarks via BenchmarkDotNet.
- `dotnet test` (or `make test`); CI uses `dotnet test --no-build -c Release`.
- Single test: `dotnet test tests/roslyn-guard-analyzer.Tests --filter "FullyQualifiedName~RuleViolationTests"`
- Benchmarks: `dotnet run --project tests/roslyn-guard-analyzer.Benchmarks -c Release`
- Test project: `tests/roslyn-guard-analyzer.Tests/` (the only test project in the .sln). `src/RoslynGuardAnalyzer.Tests/` and `src/tests/` are stray copies not in the solution; do not add tests there.

## Lint / Format

- `dotnet format RoslynGuardAnalyzer.sln` (or `make format`); verify with `make format-check`.
- `make lint` = format-check + self-analysis. `make ci` = clean, restore, build, test, analyze, format-check.
- Style from `.editorconfig`: 4 spaces (2 for csproj/props/json/yml), LF, UTF-8, final newline.
- `Directory.Build.props`: `Nullable` and `ImplicitUsings` enabled, `LangVersion` latest, warnings not errors. Main project generates XML docs (CS1591 suppressed).

## Key directories and entry points

- `src/RoslynGuardAnalyzer/Program.cs` - entry point; builds `IConfiguration` (appsettings.json + env `RoslynGuardAnalyzer__*` + args), then DI.
- `src/RoslynGuardAnalyzer/Infrastructure/ServiceCollectionExtensions.cs` - DI composition root (`RegisterAnalyzerServices()`).
- `Services/` - `AnalysisService` -> `RuleEngine` -> `ReportingService` is the main run path; `RuleRegistry`, `ValidationService`, `OutputWriter`, `ResultAggregator` etc.
- `Domain/Models/` - `AnalysisRule`, `AnalysisResult`, `CodeElement`, `RuleViolation`, `ViolationReport`, `RuleConfiguration`.
- `Core/` - `Constants.cs` (rule ids `LYR001`, `NAM001`, `ASY001`, `NUL001`; `GUARD_SKIP` markers), enums, `SuppressRoslynGuardAttribute`.
- `Rules/` - built-in rules and the custom-rule extension point (`CustomAnalysisRule`, `CustomRuleBuilder`, `CustomRuleEngine`, `CustomRuleRegistry`).
- `Formatters/` - `IOutputFormatter` implementations + `FormatterRegistry`.
- `Cli/`, `Configuration/`, `Suppressions/`, `Middleware/`, `Events/`, `Caching/`, `Data/`, `Integration/`, `CodeFixes/`, `Exceptions/`, `Utilities/` - see `docs/ARCHITECTURE.md` for which are on the default CLI path vs. opt-in.
- `docs/` - per-class reference docs plus guides; `docs/ARCHITECTURE.md` is authoritative over `docs/architecture.md`.
- `examples/` - usage samples and CI scripts.
- `.github/workflows/` - ci.yml (build + test), build.yml (self-analysis gate: fails on error-severity violations), codeql, docker, nuget-publish, release.
- Repo-root loose files (`build/`, `QuickTest/`, `EdgeCaseTester/`, `*.cs` at root, `Domain/`, `Rules/`, `Infrastructure/` at root) are scratch/leftovers, not part of the solution. Ignore them; do not extend them.

## Conventions

- Namespaces mirror folders: `RoslynGuardAnalyzer.<Folder>` (e.g. `RoslynGuardAnalyzer.Domain.Models`, `RoslynGuardAnalyzer.Services`).
- File-scoped namespaces, `#nullable enable`, one type per file, `sealed` classes by default, interfaces prefixed `I`.
- Extension methods live in `<Type>Extensions.cs`; JSON helpers in `<Type>JsonExtensions.cs`; validation helpers in `<Type>Validation.cs`.
- Services are interface + implementation pairs registered in `ServiceCollectionExtensions`; options bound via `AddOptions<RoslynGuardAnalyzerOptions>()` with validation.
- Custom exceptions derive from `RoslynGuardException`.
- Tests: `tests/roslyn-guard-analyzer.Tests/<Type>Tests.cs`, namespace `RoslynGuardAnalyzer.Tests`, `public sealed class`, method names `Method_Scenario_Expected` (or `Method_Expected`), Arrange/Act/Assert comments, FluentAssertions `.Should()`.
- Each public type has a matching `docs/<Type>.md`; update it when the public API changes.
- Version is set in `src/RoslynGuardAnalyzer/RoslynGuardAnalyzer.csproj` (`<Version>`); user-facing changes go in `CHANGELOG.md`.
