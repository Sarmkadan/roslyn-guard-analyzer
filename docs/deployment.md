# Deployment Guide

This guide covers deploying Roslyn Guard Analyzer in production environments.

## Prerequisites

- **.NET 10.0 Runtime** or higher
- **4GB RAM** for medium-sized projects
- **100MB disk space** for application and cache
- Network access (for webhook notifications, optional)

## Deployment Strategies

### Strategy 1: CLI Tool Installation

Best for: Teams using the analyzer manually or in local CI/CD

```bash
# Build release binary
dotnet build -c Release

# Copy to system path
sudo cp src/RoslynGuardAnalyzer/bin/Release/net10.0/RoslynGuardAnalyzer \
  /usr/local/bin/roslyn-guard-analyzer

# Verify installation
roslyn-guard-analyzer --version
```

### Strategy 2: Docker Container

Best for: Containerized CI/CD pipelines, cloud deployments

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

COPY src/RoslynGuardAnalyzer/bin/Release/net10.0 .

ENTRYPOINT ["./RoslynGuardAnalyzer"]
```

Build and deploy:

```bash
docker build -t roslyn-guard-analyzer:latest .
docker tag roslyn-guard-analyzer:latest myregistry/roslyn-guard-analyzer:1.2.0
docker push myregistry/roslyn-guard-analyzer:1.2.0
```

Run in container:

```bash
docker run --rm \
  -v $(pwd):/workspace \
  myregistry/roslyn-guard-analyzer:1.2.0 \
  /workspace/MyProject.csproj \
  --format json
```

### Strategy 3: NuGet Global Tool

Best for: Wide distribution, easy updates

```bash
dotnet pack src/RoslynGuardAnalyzer -c Release -o ./nupkg
nuget push ./nupkg/RoslynGuardAnalyzer.1.2.0.nupkg -Source nuget.org
```

Users install with:

```bash
dotnet tool install --global roslyn-guard-analyzer
roslyn-guard-analyzer .
```

### Strategy 4: CI/CD Integrated

Best for: Automated analysis on every commit

**GitHub Actions**:

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
      
      - name: Build Analyzer
        run: dotnet build src/RoslynGuardAnalyzer -c Release
      
      - name: Run Analysis
        run: |
          dotnet run --project src/RoslynGuardAnalyzer -- ./src \
            --format json \
            --output analysis.json
      
      - name: Check Results
        run: |
          ERROR_COUNT=$(jq '.violations | map(select(.severity=="error")) | length' analysis.json)
          if [ $ERROR_COUNT -gt 0 ]; then
            echo "❌ Architecture violations found: $ERROR_COUNT"
            exit 1
          fi
          echo "✅ Analysis passed"
```

**GitLab CI**:

```yaml
analyze:
  image: mcr.microsoft.com/dotnet/sdk:10.0
  script:
    - dotnet build -c Release
    - dotnet run --project src/RoslynGuardAnalyzer -- ./src
  artifacts:
    reports:
      sast: analysis.json
  allow_failure: true
```

**Azure Pipelines**:

```yaml
- task: DotNetCoreCLI@2
  displayName: 'Build Analyzer'
  inputs:
    command: 'build'
    arguments: '-c Release'

- task: DotNetCoreCLI@2
  displayName: 'Run Analysis'
  inputs:
    command: 'run'
    arguments: '--project src/RoslynGuardAnalyzer -- ./src --format json --output analysis.json'

- task: PublishBuildArtifacts@1
  inputs:
    PathtoPublish: 'analysis.json'
    ArtifactName: 'code-analysis'
```

## Configuration Management

### Environment-Based Configuration

```json
{
  "projectPath": "${PROJECT_PATH}",
  "analysisTimeout": "${ANALYSIS_TIMEOUT:600}",
  "maxViolationsToReport": "${MAX_VIOLATIONS:1000}",
  "logLevel": "${LOG_LEVEL:2}",
  "rules": {
    "LYR001": {
      "enabled": "${LYR001_ENABLED:true}",
      "severity": "${LYR001_SEVERITY:error}"
    }
  }
}
```

Usage:

```bash
export PROJECT_PATH=./src
export ANALYSIS_TIMEOUT=900
export LOG_LEVEL=3
roslyn-guard-analyzer --config .roslyn-guard.json
```

### Secrets Management

For sensitive configuration (webhook URLs, tokens):

```bash
# Store in environment
export WEBHOOK_URL=https://api.example.com/analyze
export WEBHOOK_TOKEN=secret_token_here

# Reference in config
{
  "webhookUrl": "${WEBHOOK_URL}",
  "webhookToken": "${WEBHOOK_TOKEN}"
}
```

## Performance Tuning

### Memory Optimization

For large projects:

```bash
# Set max memory usage
export DOTNET_GCHeapCount=2
export DOTNET_GCRetainVM=1

roslyn-guard-analyzer ./src
```

### Timeout Configuration

Adjust for your project size:

```json
{
  "analysisTimeout": 1800
}
```

Typical timeouts:
- Small projects (< 50 files): 300s
- Medium projects (50-200 files): 600s
- Large projects (200+ files): 1800s+

### Parallel Processing

Analyzer automatically uses all available CPU cores:

```bash
# Limit to 4 cores
export DOTNET_ThreadPool_MinThreads=4
export DOTNET_ThreadPool_MaxThreads=4
roslyn-guard-analyzer ./src
```

## Monitoring and Logging

### Log Levels

```json
{
  "logLevel": 0
}
```

| Level | Output |
|-------|--------|
| 0 | None |
| 1 | Errors only |
| 2 | Warnings + Errors |
| 3 | Info + Warnings + Errors |
| 4 | Debug (verbose) |

### Structured Logging

Enable structured logs for log aggregation:

```bash
export STRUCTURED_LOGS=true
roslyn-guard-analyzer ./src 2>&1 | jq -s '[.[] | fromjson]'
```

### Exit Codes

```
0 - Success (no violations)
1 - Success (violations found, non-strict mode)
2 - Configuration error
3 - Analysis error
-1 - Fatal error
```

## Health Checks

### Smoke Tests

```bash
#!/bin/bash
set -e

echo "Running smoke tests..."

# Test 1: CLI help works
roslyn-guard-analyzer --help > /dev/null
echo "✓ CLI help works"

# Test 2: Can analyze test project
dotnet new console -o /tmp/test-project
roslyn-guard-analyzer /tmp/test-project > /dev/null
echo "✓ Can analyze projects"

# Test 3: JSON output works
roslyn-guard-analyzer /tmp/test-project --format json > /dev/null
echo "✓ JSON formatter works"

# Test 4: Can process large projects
# (use your actual large project path)
roslyn-guard-analyzer ./src --timeout 120
echo "✓ Can handle production workloads"

echo "All smoke tests passed!"
```

Run regularly:

```bash
0 * * * * /home/ci/health-check.sh >> /var/log/analyzer-health.log
```

## Scaling

### Horizontal Scaling

For distributed analysis:

```bash
#!/bin/bash
# Analyze different modules in parallel

analysis_module() {
  local module=$1
  roslyn-guard-analyzer ./src/$module \
    --format json \
    --output ./reports/$module.json
}

export -f analysis_module

find ./src -maxdepth 1 -type d | \
  parallel analysis_module {}

# Merge reports
jq -s 'reduce .[] as $item ({}; . + $item)' ./reports/*.json > combined.json
```

### Caching Strategy

```bash
# Run incremental analysis
if [ -f .analysis-cache ]; then
  CACHE_ARGS="--cache .analysis-cache"
fi

roslyn-guard-analyzer ./src $CACHE_ARGS --output report.json

# Update cache
mv report.json .analysis-cache
```

## Backup and Recovery

### Backup Configuration

```bash
# Backup analyzer configuration
tar -czf analyzer-config-backup.tar.gz .roslyn-guard.json

# Backup analysis reports
tar -czf analysis-reports-backup.tar.gz ./reports/

# Keep 30 days of backups
find . -name "*.tar.gz" -mtime +30 -delete
```

### Disaster Recovery

```bash
# Restore from backup
tar -xzf analyzer-config-backup.tar.gz

# Re-run analysis
roslyn-guard-analyzer ./src --output recovery-report.json
```

## Troubleshooting Production Issues

### Issue: Analysis Timeout

**Solution 1**: Increase timeout
```json
{
  "analysisTimeout": 3600
}
```

**Solution 2**: Exclude large files
```json
{
  "excludePatterns": [
    "**/bin/**",
    "**/obj/**",
    "**/Generated/**"
  ]
}
```

**Solution 3**: Analyze in batches
```bash
for dir in src/*/; do
  echo "Analyzing $dir..."
  roslyn-guard-analyzer "$dir" --output "reports/$(basename $dir).json"
done
```

### Issue: Out of Memory

**Solution 1**: Limit garbage collection
```bash
export DOTNET_GCHeapCount=1
export DOTNET_GCRetainVM=0
```

**Solution 2**: Reduce cache size
```json
{
  "maxViolationsToReport": 100
}
```

**Solution 3**: Run on larger instance
```bash
# Increase available memory
docker run --memory=8g roslyn-guard-analyzer ./src
```

### Issue: Inconsistent Results

**Causes**:
- Concurrent analysis conflicts
- Missing dependencies
- External service unavailability

**Solutions**:
```bash
# Serialize execution
roslyn-guard-analyzer ./src --serial

# Enable debug logging
roslyn-guard-analyzer ./src --verbose

# Verify dependencies
dotnet restore
```

## Security Hardening

### Least Privilege

```bash
# Create dedicated user
useradd -r -s /bin/false analyzer

# Run with restricted permissions
sudo -u analyzer roslyn-guard-analyzer ./src
```

### Input Validation

```bash
# Validate project path exists
if [ ! -f "$PROJECT_PATH/$(basename $PROJECT_PATH).csproj" ]; then
  echo "Invalid project path"
  exit 1
fi

# Validate config file
if [ ! -f .roslyn-guard.json ]; then
  echo "Missing required config"
  exit 1
fi
```

### Audit Logging

```bash
# Log all analysis runs
roslyn-guard-analyzer ./src 2>&1 | tee -a /var/log/analyzer.log

# Monitor for suspicious patterns
tail -f /var/log/analyzer.log | grep -i "error\|warning\|critical"
```

## Maintenance

### Regular Updates

```bash
# Check for updates
dotnet tool update roslyn-guard-analyzer --global

# Update Docker image
docker pull mcr.microsoft.com/dotnet/runtime:10.0
docker build -t roslyn-guard-analyzer:latest .
```

### Cleanup

```bash
# Remove old analysis reports
find ./reports -mtime +90 -delete

# Clear cache
rm -rf .roslyn-guard-cache

# Clean build artifacts
dotnet clean
```

## Support and Monitoring

### Metrics to Monitor

- Analysis duration (target: < 5 minutes)
- Violations per file (track trends)
- Rule distribution (identify patterns)
- Cache hit ratio (> 80% for incremental runs)

### Alerting

```bash
#!/bin/bash
# Alert if analysis takes too long

START=$(date +%s)
roslyn-guard-analyzer ./src
END=$(date +%s)
DURATION=$((END - START))

if [ $DURATION -gt 600 ]; then
  echo "Analysis took ${DURATION}s (threshold: 600s)" | \
    mail -s "Analyzer Performance Alert" ops@example.com
fi
```
