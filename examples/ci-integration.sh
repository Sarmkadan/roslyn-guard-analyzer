#!/bin/bash

# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================
# Example: CI/CD Integration
# Demonstrates how to integrate Roslyn Guard Analyzer into CI/CD pipelines
# with JSON export, violation counting, and exit codes for CI systems.
# =============================================================================

set -e

PROJECT_PATH="${1:-.}"
OUTPUT_FILE="${2:-analysis.json}"
STRICT_MODE="${STRICT_MODE:-0}"

echo "=== Roslyn Guard Analyzer - CI Integration ==="
echo ""

# Run analysis and capture JSON output
echo "🔍 Running analysis on $PROJECT_PATH..."
echo ""

if ! roslyn-guard-analyzer "$PROJECT_PATH" \
    --format json \
    --output "$OUTPUT_FILE"; then
    echo "❌ Analysis execution failed"
    exit 1
fi

echo "✅ Analysis complete"
echo ""

# Parse results using jq
if ! command -v jq &> /dev/null; then
    echo "⚠️ jq not found, skipping detailed analysis"
    exit 0
fi

# Count violations by severity
echo "📊 Violation Summary:"
echo "─────────────────────────────────────"

TOTAL=$(jq '.violations | length' "$OUTPUT_FILE")
ERRORS=$(jq '.violations | map(select(.severity=="error")) | length' "$OUTPUT_FILE")
WARNINGS=$(jq '.violations | map(select(.severity=="warning")) | length' "$OUTPUT_FILE")
INFO=$(jq '.violations | map(select(.severity=="info")) | length' "$OUTPUT_FILE")

echo "Total violations:    $TOTAL"
echo "  🔴 Errors:        $ERRORS"
echo "  🟡 Warnings:      $WARNINGS"
echo "  🔵 Info:          $INFO"
echo ""

# Display violations grouped by rule
echo "📋 Violations by Rule:"
echo "─────────────────────────────────────"
jq -r '.violations | group_by(.ruleId) | .[] | "\(.[] | .ruleId) (\(length))"' "$OUTPUT_FILE" | \
    sort | uniq || true
echo ""

# Top violated files
echo "📁 Most Problematic Files:"
echo "─────────────────────────────────────"
jq -r '.violations | group_by(.filePath) | sort_by(length) | reverse | .[0:5][] | "\(.[0].filePath) (\(length))"' "$OUTPUT_FILE" || true
echo ""

# Determine exit code based on severity
if [ "$ERRORS" -gt 0 ]; then
    echo "❌ Build FAILED: $ERRORS architectural errors found"
    echo ""
    echo "Violations with error severity:"
    jq -r '.violations[] | select(.severity=="error") | "\(.filePath):\(.line) - \(.message)"' "$OUTPUT_FILE" | head -10
    exit 1
elif [ "$WARNINGS" -gt 0 ] && [ "$STRICT_MODE" -eq 1 ]; then
    echo "⚠️ Build FAILED: $WARNINGS warnings found (strict mode)"
    exit 1
else
    echo "✅ Build PASSED"
    exit 0
fi
