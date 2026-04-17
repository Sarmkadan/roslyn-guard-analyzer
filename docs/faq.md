# Frequently Asked Questions

Common questions about Roslyn Guard Analyzer usage, configuration, and development.

## Installation & Setup

### Q: What .NET versions are supported?

**A**: Roslyn Guard Analyzer requires **.NET 10.0 or higher**. It uses the latest C# language features and latest Roslyn API. We don't support older frameworks (.NET 6, 7, 8, 9) as they lack necessary language and compiler features.

### Q: Can I install it as a global tool?

**A**: Yes! Once we publish to NuGet, installation is simple:

```bash
dotnet tool install --global roslyn-guard-analyzer
roslyn-guard-analyzer --version
```

Currently, build from source:

```bash
git clone https://github.com/sarmkadan/roslyn-guard-analyzer.git
cd roslyn-guard-analyzer
dotnet build -c Release
```

### Q: Does it work on Windows, macOS, and Linux?

**A**: Yes! Roslyn Guard Analyzer is fully cross-platform:
- **Windows**: `.exe` binary or dotnet CLI
- **macOS**: ARM64 and Intel native binaries
- **Linux**: x64 and ARM64 support

### Q: How much disk space does it need?

**A**: Minimal:
- **Runtime**: ~300MB (.NET SDK)
- **Application**: ~50MB binary
- **Cache**: ~100MB (configurable)
- **Total**: ~500MB for complete setup

## Usage Questions

### Q: Can I analyze only specific files?

**A**: Yes! Provide individual file paths:

```bash
roslyn-guard-analyzer ./src/Services/UserService.cs
roslyn-guard-analyzer ./src/Domain/Models/
```

Or use patterns in configuration:

```json
{
  "includePatterns": [
    "**/Services/*.cs",
    "**/Domain/**/*.cs"
  ]
}
```

### Q: How do I exclude generated code?

**A**: Use `excludePatterns` in configuration:

```json
{
  "excludePatterns": [
    "**/*.Generated.cs",
    "**/*.Designer.cs",
    "**/obj/**",
    "**/bin/**"
  ]
}
```

### Q: What's the output format I should use for CI/CD?

**A**: **JSON** format is ideal for CI/CD:

```bash
roslyn-guard-analyzer ./src --format json --output report.json
```

Then parse with `jq` for decision-making:

```bash
VIOLATIONS=$(jq '.violations | length' report.json)
if [ $VIOLATIONS -gt 0 ]; then
  echo "Architecture violations found: $VIOLATIONS"
  exit 1
fi
```

### Q: How can I suppress specific violations?

**A**: Currently, violations can't be suppressed per-file. Instead:

**Option 1**: Disable the rule entirely
```json
{
  "rules": {
    "NAM001": { "enabled": false }
  }
}
```

**Option 2**: Make rule severity less critical
```json
{
  "rules": {
    "NAM001": { "severity": "warning" }
  }
}
```

We're considering a `[AnalyzerSuppression]` attribute for future versions.

### Q: Can I run analysis on multiple projects simultaneously?

**A**: No, the analyzer processes one project path at a time. For multiple projects:

```bash
#!/bin/bash
for project in project1 project2 project3; do
  echo "Analyzing $project..."
  roslyn-guard-analyzer ./src/$project --output reports/$project.json &
done
wait
```

## Configuration Questions

### Q: Where should I put .roslyn-guard.json?

**A**: Place it in the project root (where you run the analyzer):

```
MyProject/
├── .roslyn-guard.json
├── .git/
├── src/
└── README.md
```

You can also specify a path:

```bash
roslyn-guard-analyzer ./src --config custom-config.json
```

### Q: Can I override configuration from command line?

**A**: Yes, command-line arguments take precedence:

```bash
# Override format and output
roslyn-guard-analyzer ./src --format csv --output report.csv

# Analyze specific rules only
roslyn-guard-analyzer ./src --rules LYR001,NAM001
```

### Q: What happens if configuration is invalid?

**A**: The analyzer validates configuration on startup and reports clear errors:

```
Error: Invalid configuration
  - Rule "INVALID001" not found
  - Timeout must be between 10 and 3600 seconds
  - Project path does not exist: /invalid/path
```

### Q: Can I use environment variables in configuration?

**A**: Yes! Reference with `${VAR_NAME}` or `${VAR_NAME:default}`:

```json
{
  "projectPath": "${PROJECT_PATH:./src}",
  "analysisTimeout": "${TIMEOUT:600}",
  "excludePatterns": "${EXCLUDE_PATTERNS:['**/bin/**','**/obj/**']}"
}
```

Then set before running:

```bash
export PROJECT_PATH=./my-project
export TIMEOUT=900
roslyn-guard-analyzer --config .roslyn-guard.json
```

## Rule Questions

### Q: What's the difference between rule severity levels?

**A**:

| Severity | CI/CD | Display | Use Case |
|----------|-------|---------|----------|
| **Info** | ℹ️ Continue | Blue | Informational only |
| **Warning** | ⚠️ Continue | Yellow | Should fix but not critical |
| **Error** | ❌ Fail | Red | Enforce architectural rules |
| **Critical** | 🚨 Fail | Dark Red | Never allow |

### Q: How do I disable a specific rule?

**A**: Use the configuration file:

```json
{
  "rules": {
    "ASY001": { "enabled": false }
  }
}
```

Or keep it enabled but make it informational:

```json
{
  "rules": {
    "ASY001": {
      "enabled": true,
      "severity": "info"
    }
  }
}
```

### Q: Can I create custom rules?

**A**: Yes! Extend `AnalysisRule` and register:

```csharp
public class MyCustomRule : AnalysisRule
{
    public override string Id => "CUSTOM001";
    public override string Category => "Custom";
    
    public override async Task<IEnumerable<RuleViolation>> ValidateAsync(
        CodeElement element,
        RuleConfiguration config)
    {
        // Your logic here
        return new List<RuleViolation>();
    }
}

// Register
services.AddSingleton<AnalysisRule, MyCustomRule>();
```

See [API Reference](./api-reference.md) for complete guide.

### Q: Why is NAM001 complaining about my method names?

**A**: NAM001 enforces PascalCase for methods:

```csharp
// ❌ Wrong
public void getUserName() { }

// ✅ Correct
public void GetUserName() { }
```

Exception: Properties can use `_prefixed_case` for backing fields:

```csharp
private string _user_name;
public string UserName { get; set; }
```

### Q: How strict should I be with rule severity?

**A**: Recommended approach:

- **Error**: Architecture violations (LYR001) - enforce in CI/CD
- **Warning**: Best practices (NAM001, ASY001) - informational
- **Info**: Metrics only (complexity, code smells)

## Performance Questions

### Q: Why is analysis slow on my project?

**A**: Common causes and solutions:

**Cause 1**: Too many files
```bash
# Solution: Exclude build artifacts
"excludePatterns": ["**/bin/**", "**/obj/**"]
```

**Cause 2**: Large files
```bash
# Solution: Increase timeout
"analysisTimeout": 1800
```

**Cause 3**: Limited memory
```bash
# Solution: Increase available memory
export DOTNET_GCHeapCount=2
```

### Q: How can I speed up analysis?

**A**: Performance tips:

1. **Exclude known safe directories**
   ```json
   { "excludePatterns": ["**/bin/**", "**/obj/**", "**/*.Generated.cs"] }
   ```

2. **Disable unused rules**
   ```json
   { "rules": { "NUL001": { "enabled": false } } }
   ```

3. **Use caching** (if available)
   ```bash
   roslyn-guard-analyzer ./src --use-cache
   ```

4. **Limit violations**
   ```json
   { "maxViolationsToReport": 100 }
   ```

### Q: What's typical analysis time?

**A**: Depends on project size:

| Project Size | Files | Typical Time |
|--------------|-------|--------------|
| Tiny | < 50 | < 1s |
| Small | 50-200 | 2-5s |
| Medium | 200-500 | 5-15s |
| Large | 500-1000 | 15-60s |
| Huge | > 1000 | 60-300s |

## Integration Questions

### Q: How do I integrate with GitHub Actions?

**A**: Add this workflow:

```yaml
name: Analyze

on: [push, pull_request]

jobs:
  analyze:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - run: |
          dotnet build -c Release
          dotnet run --project src/RoslynGuardAnalyzer -- ./src \
            --format json --output analysis.json
      
      - run: |
          ERRORS=$(jq '.violations | map(select(.severity=="error")) | length' analysis.json)
          [ $ERRORS -eq 0 ] && echo "✅ Analysis passed" || exit 1
```

### Q: Can I integrate with SonarQube?

**A**: Yes! Export JSON and use SonarQube's external issues API:

```bash
roslyn-guard-analyzer ./src --format json --output report.json

# Import into SonarQube
curl -X POST "http://sonarqube:9000/api/issues/import" \
  -F "report=@report.json"
```

### Q: How do I integrate with Azure DevOps?

**A**: Use the SARIF format (work in progress). Currently, publish JSON artifact:

```yaml
- task: PublishBuildArtifacts@1
  inputs:
    PathtoPublish: 'analysis.json'
    ArtifactName: 'analyzer-results'
```

## Troubleshooting Questions

### Q: Analysis reports "No violations found" but I see violations

**A**: Check these:

1. **Rules are enabled**
   ```bash
   roslyn-guard-analyzer ./src --verbose
   # Look for "Rule XXX: enabled"
   ```

2. **Files aren't excluded**
   ```bash
   # Check your excludePatterns
   cat .roslyn-guard.json | grep -A5 excludePatterns
   ```

3. **Rule configuration is correct**
   ```json
   {
     "rules": {
       "NAM001": { "enabled": true }
     }
   }
   ```

### Q: Getting "OutOfMemoryException"

**A**: Solutions:

1. **Exclude large files**
   ```json
   {
     "excludePatterns": ["**/*.cs.txt", "**/huge-generated.cs"]
   }
   ```

2. **Reduce violation limit**
   ```json
   {
     "maxViolationsToReport": 50
   }
   ```

3. **Increase GC heap**
   ```bash
   export DOTNET_GCHeapCount=4
   roslyn-guard-analyzer ./src
   ```

4. **Analyze in smaller batches**
   ```bash
   for dir in src/*/; do
     roslyn-guard-analyzer "$dir"
   done
   ```

### Q: Getting "Project not found" error

**A**: Verify the path:

```bash
# Show actual path
pwd

# Verify project file exists
ls -la ./src/Project.csproj

# Use absolute path
roslyn-guard-analyzer /full/path/to/project
```

## Development Questions

### Q: How do I build from source?

**A**:

```bash
git clone https://github.com/sarmkadan/roslyn-guard-analyzer.git
cd roslyn-guard-analyzer
dotnet build -c Release

# Run tests (if present)
dotnet test

# Create release binary
dotnet publish -c Release -o ./release
```

### Q: Can I contribute custom rules?

**A**: Yes! Guidelines:

1. Create rule in `src/RoslynGuardAnalyzer/Rules/`
2. Extend `AnalysisRule` base class
3. Add unit tests with 80%+ coverage
4. Document in README
5. Submit pull request

### Q: What's the contribution process?

**A**:

1. Fork repository
2. Create feature branch: `git checkout -b feature/my-rule`
3. Commit changes: `git commit -am "Add custom rule"`
4. Push branch: `git push origin feature/my-rule`
5. Create Pull Request

See CONTRIBUTING.md for guidelines.

## Licensing Questions

### Q: Is this project open source?

**A**: Yes! MIT License - use freely for commercial and private projects.

### Q: Can I use this in closed-source projects?

**A**: Yes! MIT License allows commercial use. Just include the license text.

### Q: Do I need to give attribution?

**A**: Optional but appreciated. The project is maintained by Vladyslav Zaiets.

---

**Still have questions?**

- 📖 [Read Documentation](../README.md)
- 💬 [GitHub Discussions](https://github.com/sarmkadan/roslyn-guard-analyzer/discussions)
- 🐛 [Report Issues](https://github.com/sarmkadan/roslyn-guard-analyzer/issues)
- 🧑‍💻 [See Examples](../examples/)
