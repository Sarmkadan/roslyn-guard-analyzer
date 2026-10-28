---
name: Bug report
about: Report a rule that reports incorrectly, crashes, or misses a violation
title: "[Bug]: "
labels: bug
---

## Description

<!-- A clear, concise description of the problem. Which rule or diagnostic (e.g. LYR001, NAM001, ASY001, NUL001, or a custom rule) is affected? -->

## Minimal code sample

<!--
Smallest C# source that triggers the diagnostic, or that should trigger it but does not.
Keep it self-contained (no external packages). If the problem is a missing diagnostic, say so explicitly.
-->

```csharp

```

**Command used:**

```bash

```

## Expected diagnostic behavior

<!-- What should the analyzer report (rule ID, severity, location)? Or should it report nothing? -->

## Actual behavior

<!-- What happens instead? Paste the analyzer output, exception stack trace, or the report (text/JSON/SARIF) if relevant. -->

```text

```

## Environment

- Analyzer version: <!-- e.g. 1.0.0, or the commit SHA if built from source -->
- .NET SDK: <!-- output of `dotnet --version` -->
- OS: <!-- e.g. Ubuntu 24.04, Windows 11, macOS 15 -->
- IDE / host (if applicable): <!-- e.g. Visual Studio 2022 17.x, VS Code + C# Dev Kit, Rider 2025.x, CI (GitHub Actions) -->
- Installation method: <!-- dotnet tool, NuGet package, Docker image, built from source -->
