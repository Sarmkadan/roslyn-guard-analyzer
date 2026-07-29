// SPDX-License-Identifier: MIT
// Tests for JsonFormatter
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Formatters;
using Xunit;

namespace RoslynGuardAnalyzer.Tests
{
    public class JsonFormatterTests
    {
        [Fact]
        public void CanFormat_ReturnsTrue_ForJson_AndFalse_ForOther()
        {
            var formatter = new JsonFormatter();

            Assert.True(formatter.CanFormat("json"));
            Assert.True(formatter.CanFormat("JSON"));
            Assert.False(formatter.CanFormat("xml"));
            Assert.False(formatter.CanFormat(string.Empty));
        }

        [Fact]
        public void FormatResult_WithSingleViolation_ReturnsValidJson()
        {
            var result = new AnalysisResult
            {
                ProjectName = "TestProject",
                ProjectPath = "/path/to/project",
                AnalysisSucceeded = true,
                ErrorMessage = null,
                TotalFilesAnalyzed = 1,
                TotalElementsAnalyzed = 10,
                Violations = new List<RuleViolation>
                {
                    new RuleViolation
                    {
                        RuleId = "R001",
                        RuleName = "TestRule",
                        Severity = SeverityLevel.Warning,
                        Message = "Test message",
                        FilePath = "/path/file.cs",
                        LineNumber = 5,
                        ColumnNumber = 10,
                        CodeSnippet = "var x = 1;"
                    }
                }
            };

            var formatter = new JsonFormatter();
            var json = formatter.FormatResult(result);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal("TestProject", root.GetProperty("ProjectName").GetString());
            Assert.Equal("/path/to/project", root.GetProperty("ProjectPath").GetString());
            Assert.True(root.GetProperty("AnalysisSucceeded").GetBoolean());
            Assert.Equal(1, root.GetProperty("TotalFilesAnalyzed").GetInt32());
            Assert.Equal(10, root.GetProperty("TotalElementsAnalyzed").GetInt32());
            Assert.Equal(1, root.GetProperty("ViolationCount").GetInt32());

            var violations = root.GetProperty("Violations");
            Assert.Equal(1, violations.GetArrayLength());

            var v = violations[0];
            Assert.Equal("R001", v.GetProperty("RuleId").GetString());
            Assert.Equal("TestRule", v.GetProperty("RuleName").GetString());
            Assert.Equal("Warning", v.GetProperty("Severity").GetString());
            Assert.Equal("Test message", v.GetProperty("Message").GetString());
            Assert.Equal("/path/file.cs", v.GetProperty("FilePath").GetString());
            Assert.Equal(5, v.GetProperty("LineNumber").GetInt32());
            Assert.Equal(10, v.GetProperty("ColumnNumber").GetInt32());
            Assert.Equal("var x = 1;", v.GetProperty("CodeSnippet").GetString());
        }

        [Fact]
        public void FormatViolations_EmptyCollection_ReturnsCountZero()
        {
            var formatter = new JsonFormatter();
            var json = formatter.FormatViolations(new List<RuleViolation>());
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal(0, root.GetProperty("Count").GetInt32());
            Assert.Equal(0, root.GetProperty("Violations").GetArrayLength());
        }

        [Fact]
        public void FormatViolations_WithMultipleViolations_ReturnsCorrectCount()
        {
            var violations = new List<RuleViolation>
            {
                new RuleViolation
                {
                    RuleId = "R1",
                    RuleName = "RuleA",
                    Severity = SeverityLevel.Critical,
                    Message = "msg",
                    FilePath = "file1.cs",
                    LineNumber = 1,
                    ColumnNumber = 1,
                    CodeSnippet = null
                },
                new RuleViolation
                {
                    RuleId = "R2",
                    RuleName = "RuleB",
                    Severity = SeverityLevel.Info,
                    Message = "msg2",
                    FilePath = "file2.cs",
                    LineNumber = 2,
                    ColumnNumber = 2,
                    CodeSnippet = null
                }
            };

            var formatter = new JsonFormatter();
            var json = formatter.FormatViolations(violations);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal(2, root.GetProperty("Count").GetInt32());
            Assert.Equal(2, root.GetProperty("Violations").GetArrayLength());
        }

        [Fact]
        public void FormatReport_WithEmptyViolationGroups_ReturnsSeveritySummaryZeros()
        {
            var report = new ViolationReport
            {
                Title = "Test Report",
                ProjectName = "Proj",
                GeneratedAt = DateTime.UtcNow,
                Summary = "summary",
                DetailedContent = "details",
                ViolationGroups = new List<ViolationGroup>()
            };

            var formatter = new JsonFormatter();
            var json = formatter.FormatReport(report);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var severitySummary = root.GetProperty("SeveritySummary");
            Assert.Equal(0, severitySummary.GetProperty("Critical").GetInt32());
            Assert.Equal(0, severitySummary.GetProperty("High").GetInt32());
            Assert.Equal(0, severitySummary.GetProperty("Medium").GetInt32());
            Assert.Equal(0, severitySummary.GetProperty("Low").GetInt32());
        }

        [Fact]
        public void FormatReport_WithViolations_ProducesCorrectSeverityCounts()
        {
            var violationCritical = new RuleViolation
            {
                RuleId = "RC",
                RuleName = "CriticalRule",
                Severity = SeverityLevel.Critical,
                Message = "critical issue",
                FilePath = "file.cs",
                LineNumber = 10,
                ColumnNumber = 5,
                CodeSnippet = null
            };

            var violationError = new RuleViolation
            {
                RuleId = "RE",
                RuleName = "ErrorRule",
                Severity = SeverityLevel.Error,
                Message = "error issue",
                FilePath = "file.cs",
                LineNumber = 20,
                ColumnNumber = 15,
                CodeSnippet = null
            };

            var group = new ViolationGroup
            {
                Violations = new List<RuleViolation> { violationCritical, violationError }
            };

            var report = new ViolationReport
            {
                Title = "Full Report",
                ProjectName = "Proj",
                GeneratedAt = DateTime.UtcNow,
                Summary = "summary",
                DetailedContent = "details",
                ViolationGroups = new List<ViolationGroup> { group }
            };

            var formatter = new JsonFormatter();
            var json = formatter.FormatReport(report);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var severitySummary = root.GetProperty("SeveritySummary");
            Assert.Equal(1, severitySummary.GetProperty("Critical").GetInt32());
            Assert.Equal(1, severitySummary.GetProperty("High").GetInt32()); // High maps to SeverityLevel.Error
            Assert.Equal(0, severitySummary.GetProperty("Medium").GetInt32());
            Assert.Equal(0, severitySummary.GetProperty("Low").GetInt32());
        }
    }
}
