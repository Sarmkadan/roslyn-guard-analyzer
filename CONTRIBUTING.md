# Contributing to RoslynGuardAnalyzer

Thank you for considering contributing to RoslynGuardAnalyzer!

## Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- Git

## Building Locally

```bash
# Clone your fork
git clone https://github.com/your-username/roslyn-guard-analyzer.git
cd roslyn-guard-analyzer

# Restore dependencies
dotnet restore

# Build in Release configuration
dotnet build -c Release

# Or use the Makefile
make build
```

## Running Tests

```bash
# Run all tests
dotnet test --verbosity normal

# Run tests with detailed output and generate a TRX report
dotnet test --verbosity normal --logger "trx" --results-directory TestResults

# Run a specific test project
dotnet test tests/roslyn-guard-analyzer.Tests/
```

## Code Style

This project follows standard .NET conventions enforced via `.editorconfig`:

- **Naming**: PascalCase for public types and members, camelCase for parameters and locals, `_camelCase` for private fields, `I` prefix for interfaces, `Async` suffix for async methods.
- **Braces**: Always use braces for control flow blocks.
- **Nullability**: Enable nullable reference types (`<Nullable>enable</Nullable>`); avoid `null` suppression operators unless unavoidable.
- **XML docs**: Add or update `<summary>` documentation for all public APIs.

Run `dotnet format --verify-no-changes` to confirm your changes comply before opening a PR.

## Branching & Pull Requests

1. **Fork** the repository and create a branch from `main`:
   ```bash
   git checkout -b feature/my-feature
   ```
2. Make focused, well-scoped changes. One feature or fix per PR.
3. Ensure all tests pass and no new build warnings are introduced.
4. Write a clear PR description explaining _what_ changed and _why_.
5. Reference any related issues with `Fixes #<issue>` in the PR body.

## Reporting Issues

Use [GitHub Issues](https://github.com/sarmkadan/roslyn-guard-analyzer/issues). When reporting a bug, include:

- A clear description of the problem.
- Minimal reproduction steps or a failing test case.
- Expected vs. actual behaviour.
- SDK version (`dotnet --version`) and OS.

## License

By contributing, you agree that your contributions will be licensed under the project's **MIT License**.
