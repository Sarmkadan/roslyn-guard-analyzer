#!/bin/bash

# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================
# Example: Basic Project Analysis
# This script demonstrates the most common usage pattern: analyzing a project
# and displaying results in a human-readable format.
# =============================================================================

set -e

PROJECT_PATH="${1:-.}"

echo "╔════════════════════════════════════════════════════════════╗"
echo "║  Roslyn Guard Analyzer - Basic Analysis Example            ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""

# Verify project path
if [ ! -d "$PROJECT_PATH" ]; then
    echo "❌ Error: Project path not found: $PROJECT_PATH"
    echo ""
    echo "Usage: $0 [project-path]"
    echo "Example: $0 ./src"
    exit 1
fi

echo "📂 Analyzing project: $PROJECT_PATH"
echo ""

# Run analysis
if roslyn-guard-analyzer "$PROJECT_PATH"; then
    echo ""
    echo "✅ Analysis completed with no violations!"
    exit 0
else
    echo ""
    echo "⚠️ Analysis found violations"
    echo "Review the output above for details"
    exit 1
fi
