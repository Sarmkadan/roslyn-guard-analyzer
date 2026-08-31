// SPDX-License-Identifier: MIT
// Tests for CsvFormatter
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Formatters;
using Xunit;

namespace RoslynGuardAnalyzer.Tests
{
    public class CsvFormatterTests
    {
        [Fact]
        public void CanFormat_ReturnsTrue_ForCsv_AndFalse_ForOther()
        {
            var formatter = new CsvFormatter();

            Assert.True(formatter.CanFormat("csv"));
            Assert.True(formatter.CanFormat("CSV"));
            Assert.False(formatter.CanFormat("json"));
        }

        [Fact]
        public void FormatViolations_WithMultipleViolations_ReturnsHeaderAndOneLinePerViolation()
        {
            var violations = new List<RuleViolation>
            {
                CreateViolation("RuleA", SeverityLevel.Warning, "message one", "file1.cs", 1, 2, "var x = 1;"),
                CreateViolation("RuleB", SeverityLevel.Info, "message two", "file2.cs", 3, 4, "var y = 2;")
            };

            var formatter = new CsvFormatter();
            var csv = formatter.FormatViolations(violations);
            var lines = csv.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(3, lines.Length);
            Assert.Equal("Rule,Severity,Message,File,Line,Column,Code", lines[0]);
            Assert.Equal("RuleA,Warning,message one,file1.cs,1,2,var x = 1;", lines[1]);
            Assert.Equal("RuleB,Info,message two,file2.cs,3,4,var y = 2;", lines[2]);
        }

        [Fact]
        public void FormatResult_WithSpecialCharacters_EscapesCsvValues()
        {
            var result = new AnalysisResult
            {
                ProjectName = "TestProject",
                ProjectPath = "/path/to/project",
                AnalysisSucceeded = true,
                TotalFilesAnalyzed = 1,
                TotalElementsAnalyzed = 1,
                Violations = new List<RuleViolation>
                {
                    CreateViolation(
                        "Rule, \"Quoted\"",
                        SeverityLevel.Error,
                        "First line\nSecond, \"quoted\" line",
                        "folder/file,one.cs",
                        10,
                        5,
                        "Console.WriteLine(\"a,b\");")
                }
            };

            var formatter = new CsvFormatter();
            var csv = formatter.FormatResult(result);

            Assert.Contains("\"Rule, \"\"Quoted\"\"\"", csv);
            Assert.Contains("\"First line\nSecond, \"\"quoted\"\" line\"", csv);
            Assert.Contains("\"folder/file,one.cs\"", csv);
            Assert.Contains("\"Console.WriteLine(\"\"a,b\"\");\"", csv);
        }

        [Fact]
        public void FormatViolations_NullOrEmptyCodeSnippet_UsesCurrentCsvRepresentation()
        {
            var violations = new List<RuleViolation>
            {
                CreateViolation("NullSnippet", SeverityLevel.Warning, "message", "null.cs", 1, 1, null),
                CreateViolation("EmptySnippet", SeverityLevel.Warning, "message", "empty.cs", 2, 2, string.Empty)
            };

            var formatter = new CsvFormatter();
            var csv = formatter.FormatViolations(violations);

            Assert.Contains("NullSnippet,Warning,message,null.cs,1,1,N/A", csv);
            Assert.Contains("EmptySnippet,Warning,message,empty.cs,2,2,\"\"", csv);
        }

        [Fact]
        public void FormatReport_WithViolations_ContainsAllSectionsAndCorrectCounts()
        {
            var report = new ViolationReport
            {
                Title = "Test Report",
                ProjectName = "Proj",
                GeneratedAt = new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                Summary = "summary",
                DetailedContent = "details",
                ViolationGroups = new List<ViolationGroup>
                {
                    new ViolationGroup
                    {
                        Violations = new List<RuleViolation>
                        {
                            CreateViolation("RuleA", SeverityLevel.Critical, "critical", "a.cs", 1, 1, null),
                            CreateViolation("RuleA", SeverityLevel.Error, "error", "b.cs", 2, 2, null),
                            CreateViolation("RuleB", SeverityLevel.Warning, "warning", "c.cs", 3, 3, null),
                            CreateViolation("RuleC", SeverityLevel.Info, "info", "d.cs", 4, 4, null)
                        }
                    }
                }
            };

            var formatter = new CsvFormatter();
            var csv = formatter.FormatReport(report);

            Assert.Contains("SUMMARY", csv);
            Assert.Contains("Total Violations,4", csv);
            Assert.Contains("SEVERITY SUMMARY", csv);
            Assert.Contains("Critical,1", csv);
            Assert.Contains("High,1", csv);
            Assert.Contains("Medium,1", csv);
            Assert.Contains("Low,1", csv);
            Assert.Contains("VIOLATIONS BY RULE", csv);
            Assert.Contains("RuleA,2,Critical", csv);
            Assert.Contains("RuleB,1,Warning", csv);
            Assert.Contains("RuleC,1,Info", csv);
            Assert.Contains("DETAILED VIOLATIONS", csv);
        }

        private static RuleViolation CreateViolation(
            string ruleName,
            SeverityLevel severity,
            string message,
            string filePath,
            int lineNumber,
            int columnNumber,
            string? codeSnippet)
        {
            return new RuleViolation
            {
                RuleId = $"{ruleName}-id",
                RuleName = ruleName,
                Severity = severity,
                Message = message,
                FilePath = filePath,
                LineNumber = lineNumber,
                ColumnNumber = columnNumber,
                CodeSnippet = codeSnippet
            };
        }
    }
}
