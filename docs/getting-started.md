# Getting Started with Roslyn Guard Analyzer

This guide will help you get up and running with Roslyn Guard Analyzer in minutes.

## Prerequisites

- **.NET 10.0 SDK** or higher ([download](https://dotnet.microsoft.com/download))
- **Git** for cloning the repository
- **2GB RAM** minimum for analysis of medium-sized projects
- Any operating system: Windows, macOS, or Linux

## Step 1: Installation

### Option A: Clone and Build from Source (Recommended for Development)

```bash
# Clone the repository
git clone https://github.com/sarmkadan/roslyn-guard-analyzer.git
cd roslyn-guard-analyzer

# Build the project
dotnet build -c Release

# Verify installation
dotnet run --project src/RoslynGuardAnalyzer -- --help
```

### Option B: Use Pre-built Binary

Download the latest release from [GitHub Releases](https://github.com/sarmkadan/roslyn-guard-analyzer/releases):

```bash
# Extract to a convenient location
tar -xzf roslyn-guard-analyzer-1.2.0-linux-x64.tar.gz
cd roslyn-guard-analyzer

# Run analyzer
./RoslynGuardAnalyzer --help
```

### Option C: Docker Container

```bash
# Build Docker image
docker build -t roslyn-guard-analyzer .

# Run analysis
docker run --rm -v $(pwd):/workspace \
  roslyn-guard-analyzer /workspace/MyProject.csproj
```

## Step 2: First Analysis

### 2a. Analyze Your Project

Navigate to your .NET project directory and run:

```bash
cd ~/MyProject
roslyn-guard-analyzer .

# Output:
# === Roslyn Guard Analyzer ===
# Starting architecture rule analysis...
#
# Total files analyzed: 45
# Violations found: 0
#
# Analysis completed successfully!
```

### 2b. Interpret the Results

When violations are found:

```
File: src/Services/UserService.cs:25
Rule: NAM001 (Naming Convention)
Message: Private field 'userData' should use snake_case naming: '_user_data'
Severity: warning
```

Each violation includes:
- **File path** and **line number** for quick navigation
- **Rule ID** and **category** for filtering
- **Message** explaining the violation
- **Severity** level (error/warning)

## Step 3: Understanding the Rules

Roslyn Guard Analyzer includes four built-in rules:

### LYR001 - Layer Dependency

Prevents layers from depending on upper layers:

```csharp
// ❌ VIOLATION: Repository depending on Service
public class UserRepository
{
    private readonly IUserService _userService; // Layer violation
}

// ✅ CORRECT: Repository depends only on Data layer
public class UserRepository
{
    private readonly IDbConnection _connection;
}
```

**When it applies**: Classes named `*Repository` cannot depend on classes ending with `*Service` or `*Controller`

### NAM001 - Naming Convention

Enforces consistent naming patterns:

```csharp
// ❌ VIOLATIONS
private string userName;        // Should be _user_name
public string userName { get; } // Should be UserName
class userService { }           // Should be UserService
public void getUserById() { }   // Should be GetUserById

// ✅ CORRECT
private string _user_name;
public string UserName { get; }
class UserService { }
public void GetUserById() { }
```

**Rules**:
- **Classes/Methods**: PascalCase (`UserService`, `GetById`)
- **Properties**: PascalCase (`UserName`, `IsActive`)
- **Private Fields**: snake_case with underscore prefix (`_user_name`, `_is_active`)
- **Parameters**: camelCase (`userId`, `userName`)

### ASY001 - Async Pattern

Detects improper async/await usage:

```csharp
// ❌ VIOLATIONS
public async void DownloadFile() { }  // Async void (use Task instead)
public async Task ProcessData()       
{
    var result = LongRunningSync();  // Blocking call in async method
}

// ✅ CORRECT
public async Task DownloadFileAsync() { }
public async Task ProcessDataAsync()
{
    var result = await LongRunningAsync();
}
```

**Common patterns detected**:
- `async void` (except event handlers)
- `.Result` or `.Wait()` on Tasks
- Synchronous calls in async methods
- Missing `Async` suffix

### NUL001 - Null Safety

Enforces nullable reference type patterns:

```csharp
// ❌ VIOLATIONS
public class UserService
{
    public string GetUserName(User? user)
    {
        return user.Name; // Potential null reference
    }
}

// ✅ CORRECT
public class UserService
{
    public string? GetUserName(User? user)
    {
        return user?.Name; // Proper null handling
    }
}
```

**Checks**:
- Dereference of nullable types without checks
- Missing null-coalescing operators
- Missing null-conditional operators

## Step 4: Customize Configuration

Create `.roslyn-guard.json` in your project root to customize behavior:

```json
{
  "projectPath": "./src",
  "analysisTimeout": 600,
  "maxViolationsToReport": 1000,
  "logLevel": 2,
  "rules": {
    "LYR001": {
      "enabled": true,
      "severity": "error"
    },
    "NAM001": {
      "enabled": true,
      "severity": "warning"
    },
    "ASY001": {
      "enabled": true,
      "severity": "error"
    },
    "NUL001": {
      "enabled": true,
      "severity": "warning"
    }
  },
  "excludePatterns": [
    "**/bin/**",
    "**/obj/**",
    "**/*.Generated.cs"
  ]
}
```

Then run with your config:

```bash
roslyn-guard-analyzer --config .roslyn-guard.json
```

## Step 5: Export Reports

Generate reports in various formats for integration with other tools:

### Text Report (Default)

```bash
roslyn-guard-analyzer . --format text
```

Human-readable output with visual formatting.

### JSON Report

```bash
roslyn-guard-analyzer . --format json --output report.json
```

Perfect for CI/CD integration and programmatic processing.

### CSV Report

```bash
roslyn-guard-analyzer . --format csv --output violations.csv
```

Open in Excel or Google Sheets for analysis and filtering.

### HTML Report

```bash
roslyn-guard-analyzer . --format html --output report.html
open report.html
```

Beautiful interactive report with filtering and statistics.

## Step 6: CI/CD Integration

### GitHub Actions

Add to `.github/workflows/analyze.yml`:

```yaml
name: Code Analysis

on: [push, pull_request]

jobs:
  analyze:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Run Roslyn Guard Analyzer
        run: |
          dotnet build
          dotnet run --project src/RoslynGuardAnalyzer -- ./src \
            --format json \
            --output analysis.json
      
      - name: Check results
        run: |
          ERRORS=$(jq '.violations | map(select(.severity=="error")) | length' analysis.json)
          if [ $ERRORS -gt 0 ]; then
            echo "Architecture violations found: $ERRORS"
            exit 1
          fi
```

### GitLab CI

Add to `.gitlab-ci.yml`:

```yaml
analyze:
  image: mcr.microsoft.com/dotnet/sdk:10.0
  script:
    - dotnet build
    - dotnet run --project src/RoslynGuardAnalyzer -- ./src
  artifacts:
    reports:
      sast: analysis.json
```

## Step 7: Fix Violations

When violations are reported, follow these steps to fix them:

### 1. Identify the Issue

Read the violation message carefully:

```
File: src/Services/AuthService.cs:42
Rule: NAM001
Message: Method name 'getUserToken' should be PascalCase: 'GetUserToken'
```

### 2. Locate in Code

Open the file at the specified line number.

### 3. Apply the Fix

```csharp
// Before
public string getUserToken(string username)
{
    // ...
}

// After
public string GetUserToken(string username)
{
    // ...
}
```

### 4. Re-run Analysis

```bash
roslyn-guard-analyzer . --rules NAM001
```

### 5. Verify No Regressions

```bash
roslyn-guard-analyzer .
```

## Troubleshooting

### Issue: "No violations found" but expected some

**Check 1**: Verify the file path exists
```bash
ls -la /path/to/file.cs
```

**Check 2**: Verify rule is enabled
```bash
roslyn-guard-analyzer . --verbose
# Look for "NAM001 enabled: true"
```

**Check 3**: Check if excluded by pattern
```json
{
  "excludePatterns": ["**/*.cs"] // This would exclude everything!
}
```

### Issue: Analysis is slow on large projects

**Solution 1**: Exclude build outputs
```json
{
  "excludePatterns": [
    "**/bin/**",
    "**/obj/**",
    "**/node_modules/**"
  ]
}
```

**Solution 2**: Increase timeout
```json
{
  "analysisTimeout": 1800
}
```

**Solution 3**: Analyze subset of code
```bash
roslyn-guard-analyzer ./src/Domain
roslyn-guard-analyzer ./src/Services
```

### Issue: False positives in generated code

**Solution**: Exclude generated files
```json
{
  "excludePatterns": [
    "**/*.Generated.cs",
    "**/*.Designer.cs"
  ]
}
```

## Next Steps

- Read the [Architecture Guide](./architecture.md) for deep dive
- Check [API Reference](./api-reference.md) for custom rule development
- Review [FAQ](./faq.md) for common questions
- Explore [Deployment Guide](./deployment.md) for production setup

## Support

- **Documentation**: See [README.md](../README.md)
- **Issues**: [GitHub Issues](https://github.com/sarmkadan/roslyn-guard-analyzer/issues)
- **Discussions**: [GitHub Discussions](https://github.com/sarmkadan/roslyn-guard-analyzer/discussions)
