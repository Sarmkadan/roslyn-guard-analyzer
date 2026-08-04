// SPDX-License-Identifier: MIT
// Tests for ResultAggregator
using System;
using System.Collections.Generic;
using System.Linq;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public sealed class ResultAggregatorTests
{
    private static AnalysisResult CreateResult(int violationCount, string ruleName, string filePath, Severity severity, bool succeeded = true)
    {
        var violation = new RuleViolation
        {
            RuleName = ruleName,
            FilePath = filePath,
            Severity = severity
        };

        return new AnalysisResult
        {
            ViolationCount = violationCount,
            Violations = new List<RuleViolation> { violation },
            TotalFilesAnalyzed = 1,
            TotalElementsAnalyzed = 1,
            AnalysisSucceeded = succeeded
        };
    }

    [Fact]
    public void Add_Null_ThrowsArgumentNullException()
    {
        var aggregator = new ResultAggregator();

        Assert.Throws<ArgumentNullException>(() => aggregator.Add(null!));
    }

    [Fact]
    public void Add_ValidResult_IncreasesCountAndTotals()
    {
        var aggregator = new ResultAggregator();
        var result = CreateResult(3, "R001", "src/File1.cs", Severity.Error);

        aggregator.Add(result);

        Assert.Equal(1, aggregator.Count);
        Assert.Equal(3, aggregator.GetTotalViolations());
        Assert.Equal(1, aggregator.GetTotalFilesAnalyzed());
        Assert.Equal(1, aggregator.GetTotalElementsAnalyzed());
    }

    [Fact]
    public void AddRange_EmptyCollection_DoesNotThrowAndKeepsZeroCounts()
    {
        var aggregator = new ResultAggregator();

        aggregator.AddRange(Array.Empty<AnalysisResult>());

        Assert.Equal(0, aggregator.Count);
        Assert.Empty(aggregator.GetAllViolations());
        Assert.Equal(0, aggregator.GetTotalViolations());
    }

    [Fact]
    public void GetViolationsByRule_GroupsCorrectly()
    {
        var aggregator = new ResultAggregator();
        aggregator.Add(CreateResult(1, "RuleA", "a.cs", Severity.Warning));
        aggregator.Add(CreateResult(1, "RuleB", "b.cs", Severity.Error));
        aggregator.Add(CreateResult(1, "RuleA", "c.cs", Severity.Info));

        var byRule = aggregator.GetViolationsByRule();

        Assert.Equal(2, byRule.Count);
        Assert.Contains("RuleA", byRule.Keys);
        Assert.Contains("RuleB", byRule.Keys);
        Assert.Equal(2, byRule["RuleA"].Count);
        Assert.Single(byRule["RuleB"]);
    }

    [Fact]
    public void GetViolationsBySeverity_GroupsCorrectly()
    {
        var aggregator = new ResultAggregator();
        aggregator.Add(CreateResult(1, "R1", "a.cs", Severity.Warning));
        aggregator.Add(CreateResult(1, "R2", "b.cs", Severity.Warning));
        aggregator.Add(CreateResult(1, "R3", "c.cs", Severity.Error));

        var bySeverity = aggregator.GetViolationsBySeverity();

        Assert.Equal(2, bySeverity.Count);
        Assert.Contains("Warning", bySeverity.Keys);
        Assert.Contains("Error", bySeverity.Keys);
        Assert.Equal(2, bySeverity["Warning"].Count);
        Assert.Single(bySeverity["Error"]);
    }

    [Fact]
    public void GetViolationsByFile_GroupsCorrectly()
    {
        var aggregator = new ResultAggregator();
        aggregator.Add(CreateResult(1, "R1", "file1.cs", Severity.Info));
        aggregator.Add(CreateResult(1, "R2", "file1.cs", Severity.Info));
        aggregator.Add(CreateResult(1, "R3", "file2.cs", Severity.Info));

        var byFile = aggregator.GetViolationsByFile();

        Assert.Equal(2, byFile.Count);
        Assert.Contains("file1.cs", byFile.Keys);
        Assert.Contains("file2.cs", byFile.Keys);
        Assert.Equal(2, byFile["file1.cs"].Count);
        Assert.Single(byFile["file2.cs"]);
    }

    [Fact]
    public void GenerateSummaryReport_IncludesAllViolations()
    {
        var aggregator = new ResultAggregator();
        aggregator.Add(CreateResult(1, "R1", "a.cs", Severity.Critical));
        aggregator.Add(CreateResult(1, "R2", "b.cs", Severity.Error));

        var report = aggregator.GenerateSummaryReport();

        // The report should contain a single group with all violations
        var allViolations = report.GetAllViolations().ToList();
        Assert.Equal(2, allViolations.Count);
        Assert.Contains(allViolations, v => v.RuleName == "R1");
        Assert.Contains(allViolations, v => v.RuleName == "R2");
    }
}
