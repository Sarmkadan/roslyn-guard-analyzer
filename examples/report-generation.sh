#!/bin/bash

# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================
# Example: Multi-Format Report Generation
# Generates analysis reports in multiple formats (JSON, CSV, HTML, Text)
# and opens them in appropriate viewers.
# =============================================================================

set -e

PROJECT_PATH="${1:-.}"
OUTPUT_DIR="${2:-.}"

echo "╔════════════════════════════════════════════════════════════╗"
echo "║  Roslyn Guard Analyzer - Report Generation                 ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""

# Create output directory
mkdir -p "$OUTPUT_DIR"

echo "🔍 Analyzing project: $PROJECT_PATH"
echo "📁 Output directory: $OUTPUT_DIR"
echo ""

# Generate reports in different formats
FORMATS=("text" "json" "csv" "html")

for FORMAT in "${FORMATS[@]}"; do
    OUTPUT_FILE="$OUTPUT_DIR/analysis-report.$FORMAT"

    echo "📝 Generating $FORMAT report..."

    if roslyn-guard-analyzer "$PROJECT_PATH" \
        --format "$FORMAT" \
        --output "$OUTPUT_FILE"; then

        SIZE=$(du -h "$OUTPUT_FILE" | cut -f1)
        echo "  ✅ Generated: $OUTPUT_FILE ($SIZE)"
    else
        echo "  ❌ Failed to generate $FORMAT report"
    fi
done

echo ""
echo "╔════════════════════════════════════════════════════════════╗"
echo "║  Report Generation Complete                                ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""

# Show quick statistics from JSON report
JSON_REPORT="$OUTPUT_DIR/analysis-report.json"
if [ -f "$JSON_REPORT" ]; then
    echo "📊 Quick Statistics:"
    echo "─────────────────────────────────────"

    if command -v jq &> /dev/null; then
        TOTAL=$(jq '.violations | length' "$JSON_REPORT")
        FILES=$(jq '.totalFilesAnalyzed' "$JSON_REPORT")
        echo "Files analyzed:  $FILES"
        echo "Total violations: $TOTAL"
        echo ""

        # Show violations by rule
        echo "Violations by Rule:"
        jq -r '.violations | group_by(.ruleId) | .[] | "  \(.[0].ruleId): \(length)"' "$JSON_REPORT"
    fi
fi

echo ""
echo "📂 All reports available in: $OUTPUT_DIR"
echo ""
echo "💡 You can now:"
echo "  • View text report:  cat $OUTPUT_DIR/analysis-report.text"
echo "  • Open HTML report:  open $OUTPUT_DIR/analysis-report.html"
echo "  • Import CSV:        open $OUTPUT_DIR/analysis-report.csv"
echo ""

exit 0
