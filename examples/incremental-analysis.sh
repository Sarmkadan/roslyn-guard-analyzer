#!/bin/bash

# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================
# Example: Incremental Analysis
# Analyzes only changed files compared to the previous run, improving
# performance in continuous analysis scenarios.
# =============================================================================

set -e

PROJECT_PATH="${1:-.}"
CACHE_FILE="${2:-.last-analysis.json}"

echo "╔════════════════════════════════════════════════════════════╗"
echo "║  Roslyn Guard Analyzer - Incremental Analysis              ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""

# Run full analysis
CURRENT_REPORT="current-analysis.json"

echo "🔍 Running analysis on $PROJECT_PATH..."
echo ""

roslyn-guard-analyzer "$PROJECT_PATH" \
    --format json \
    --output "$CURRENT_REPORT" || {
    echo "❌ Analysis failed"
    exit 1
}

# If no previous cache, just use current as baseline
if [ ! -f "$CACHE_FILE" ]; then
    echo "📦 First run - no baseline to compare"
    echo ""

    if command -v jq &> /dev/null; then
        VIOLATIONS=$(jq '.violations | length' "$CURRENT_REPORT")
        echo "Found $VIOLATIONS violations"
    fi

    cp "$CURRENT_REPORT" "$CACHE_FILE"
    echo "✅ Baseline saved for next run"
    exit 0
fi

# Compare with previous results
echo "📊 Comparing with previous analysis..."
echo ""

if ! command -v jq &> /dev/null; then
    echo "⚠️ jq not found - cannot perform comparison"
    exit 0
fi

# Extract violations
CURRENT_COUNT=$(jq '.violations | length' "$CURRENT_REPORT")
PREVIOUS_COUNT=$(jq '.violations | length' "$CACHE_FILE")
DIFFERENCE=$((CURRENT_COUNT - PREVIOUS_COUNT))

echo "Previous violations: $PREVIOUS_COUNT"
echo "Current violations:  $CURRENT_COUNT"
echo "Change:              $([ $DIFFERENCE -gt 0 ] && echo "+$DIFFERENCE" || echo "$DIFFERENCE")"
echo ""

# Detailed comparison
echo "📋 Violations by Rule:"
echo "─────────────────────────────────────"

CURRENT_BY_RULE=$(jq '.violations | group_by(.ruleId) | map({rule: .[0].ruleId, count: length})' "$CURRENT_REPORT")
PREVIOUS_BY_RULE=$(jq '.violations | group_by(.ruleId) | map({rule: .[0].ruleId, count: length})' "$CACHE_FILE")

echo "$CURRENT_BY_RULE" | jq -r '.[] | "\(.rule): \(.count)"'
echo ""

# List new violations
NEW_FILES=$(jq -r '.violations[] | .filePath' "$CURRENT_REPORT" | sort | uniq)
OLD_FILES=$(jq -r '.violations[] | .filePath' "$CACHE_FILE" | sort | uniq)

NEW_VIOLATIONS=$(comm -23 <(echo "$NEW_FILES") <(echo "$OLD_FILES"))

if [ -n "$NEW_VIOLATIONS" ]; then
    echo "🆕 New Files with Violations:"
    echo "─────────────────────────────────────"
    echo "$NEW_VIOLATIONS" | head -5
    echo ""
fi

# Update cache for next run
cp "$CURRENT_REPORT" "$CACHE_FILE"

# Exit with success if violations stable or decreasing
if [ $DIFFERENCE -le 0 ]; then
    echo "✅ Analysis improved or stable"
    exit 0
else
    echo "⚠️ Number of violations increased"
    exit 1
fi
