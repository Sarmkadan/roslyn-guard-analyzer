# Contributing to RoslynGuardAnalyzer

Thanks for contributing!

## Prerequisites

- **.NET SDK 10.0.100** or later. `global.json` pins `10.0.100` with `rollForward: latestMinor`; the projects target `net10.0`.
- Git.

Check your SDK with `dotnet --version`.

## Building

```bash
dotnet restore RoslynGuardAnalyzer.sln
dotnet build RoslynGuardAnalyzer.sln -c Debug   # or: make build
```

Release: `dotnet build RoslynGuardAnalyzer.sln -c Release` (or `make release`).

## Running tests

Tests live in `tests/roslyn-guard-analyzer.Tests/` (xUnit, FluentAssertions, NSubstitute).

```bash
dotnet test RoslynGuardAnalyzer.sln -c Debug   # or: make test
dotnet test tests/roslyn-guard-analyzer.Tests/roslyn-guard-analyzer.Tests.csproj \
  --filter "FullyQualifiedName~RuleViolationTests"   # single test class
```

Do not add tests under `src/RoslynGuardAnalyzer.Tests/` or `src/tests/`; those are stray copies outside the solution.

## Coding conventions

- **Formatting:** follow `.editorconfig` (4 spaces; 2 for csproj/props/json/yml; LF; UTF-8; final newline). Verify with `make format-check`.
- **Nullable:** `Nullable` is enabled. Avoid `!` suppression unless unavoidable.
- **Warnings:** do not add new build warnings. The current baseline is 0 errors and 76 warnings; keep it from growing.
- **Structure:** namespaces mirror folders (`RoslynGuardAnalyzer.<Folder>`), file-scoped namespaces, one type per file, `sealed` by default, interfaces prefixed `I`, private fields `_camelCase`.
- **Analyzer diagnostic IDs:** built-in rules are `LYR001` (layer dependencies), `NAM001` (naming), `ASY001` (async patterns), `NUL001` (null safety), defined in `src/RoslynGuardAnalyzer/Rules/DiagnosticDescriptors.cs`. Error codes `ERR001`–`ERR006` are in `Core/Constants.cs`. Pick a new unique ID for a new rule and never renumber existing ones.
- **Tests:** file `<Type>Tests.cs`, `public sealed class`, method names `Method_Scenario_Expected`, Arrange/Act/Assert comments, FluentAssertions `.Should()`.
- **Docs:** public APIs get XML `<summary>` comments, and each public type has a `docs/<Type>.md` to update when its API changes.

## Pull request process

1. Fork the repository and branch from `main` (e.g. `feature/my-change`).
2. Keep each PR to one change.
3. Add or update tests for every behavior change.
4. Update `docs/` and `CHANGELOG.md` for user-facing changes.
5. Make sure CI passes. `ci.yml` runs restore, Release build, and `dotnet test --no-build -c Release`. `build.yml` runs self-analysis and fails on error-severity violations, so run `make analyze` locally.
6. In the PR description, explain what changed and why, and reference issues with `Fixes #<issue>`.

## Reporting issues

Use [GitHub Issues](https://github.com/sarmkadan/roslyn-guard-analyzer/issues). Include:

- A clear description of the problem.
- Minimal reproduction steps or a failing test case.
- Expected vs. actual behavior.
- SDK version (`dotnet --version`) and OS.

## License

By contributing, you agree that your contributions will be licensed under the project's **MIT License**.
