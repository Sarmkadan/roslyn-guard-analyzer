#!/bin/bash

# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================
# Example: Batch Analysis of Multiple Modules
# Analyzes multiple projects or modules in parallel and generates a
# combined report for organization-wide code quality insights.
# =============================================================================

set -e

BASE_DIR="${1:-.}"
REPORTS_DIR="${2:-./analysis-reports}"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

echo "╔════════════════════════════════════════════════════════════╗"
echo "║  Roslyn Guard Analyzer - Batch Analysis                    ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""

# Create reports directory
mkdir -p "$REPORTS_DIR"

echo "🔍 Discovering modules in $BASE_DIR..."
echo ""

# Find all projects
MODULES=$(find "$BASE_DIR" -maxdepth 2 -name "*.csproj" -exec dirname {} \; | sort | uniq)
MODULE_COUNT=$(echo "$MODULES" | wc -l)

if [ "$MODULE_COUNT" -eq 0 ]; then
    echo "⚠️ No .csproj files found in $BASE_DIR"
    exit 1
fi

echo "Found $MODULE_COUNT module(s):"
echo ""

# Analyze each module
FAILED_COUNT=0
TOTAL_VIOLATIONS=0

for MODULE in $MODULES; do
    MODULE_NAME=$(basename "$MODULE")
    echo "📦 Analyzing $MODULE_NAME..."

    OUTPUT_FILE="$REPORTS_DIR/${MODULE_NAME}_${TIMESTAMP}.json"

    if roslyn-guard-analyzer "$MODULE" \
        --format json \
        --output "$OUTPUT_FILE" 2>/dev/null; then

        VIOLATIONS=$(jq '.violations | length' "$OUTPUT_FILE" 2>/dev/null || echo "0")
        echo "  ✅ Found $VIOLATIONS violations"
        TOTAL_VIOLATIONS=$((TOTAL_VIOLATIONS + VIOLATIONS))
    else
        echo "  ❌ Analysis failed"
        FAILED_COUNT=$((FAILED_COUNT + 1))
    fi
done

echo ""
echo "╔════════════════════════════════════════════════════════════╗"
echo "║  Batch Analysis Complete                                   ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""

# Generate combined report
echo "📊 Summary:"
echo "─────────────────────────────────────"
echo "Modules analyzed:  $((MODULE_COUNT - FAILED_COUNT)) / $MODULE_COUNT"
echo "Total violations:  $TOTAL_VIOLATIONS"
echo "Reports location:  $REPORTS_DIR"
echo ""

# Create combined JSON
COMBINED_FILE="$REPORTS_DIR/combined_${TIMESTAMP}.json"
echo "Creating combined report: $COMBINED_FILE"

jq -s '{
    timestamp: now | todate,
    modules: length,
    violations: (reduce .[] as $file (0; . + ($file.violations | length))),
    details: .
}' "$REPORTS_DIR"/*_${TIMESTAMP}.json > "$COMBINED_FILE"

# Summary by module
echo ""
echo "📋 Top Modules by Violations:"
echo "─────────────────────────────────────"
for REPORT in $(ls -S "$REPORTS_DIR"/*_${TIMESTAMP}.json 2>/dev/null | head -5); do
    MODULE=$(basename "$REPORT" | sed 's/_'$TIMESTAMP'.json//')
    COUNT=$(jq '.violations | length' "$REPORT")
    echo "  $MODULE: $COUNT"
done

echo ""
echo "✅ Batch analysis complete"
exit 0
