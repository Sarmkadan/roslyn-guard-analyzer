// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using FluentAssertions;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Services;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Tests for AnalysisStatisticsService null-argument handling.
/// </summary>
public sealed class AnalysisStatisticsServiceTests
{
    [Fact]
    public void CalculateStatistics_NullViolations_ThrowsArgumentNullException()
    {
        // Arrange
        var violations = (IEnumerable<RuleViolation>?)null;

        // Act
        Action act = () => AnalysisStatisticsService.CalculateStatistics(violations);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("violations");
    }

    [Fact]
    public void CalculateStatistics_NullAnalysisResult_ThrowsArgumentNullException()
    {
        // Arrange
        var result = (AnalysisResult?)null;

        // Act
        Action act = () => AnalysisStatisticsService.CalculateStatistics(result);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("result");
    }

    [Fact]
    public void GetTopRulesByViolations_NullViolations_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<RuleViolation>? violations = null;

        // Act
        Action act = () => AnalysisStatisticsService.GetTopRulesByViolations(violations!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("violations");
    }

    [Fact]
    public void GetTopFilesByViolations_NullViolations_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<RuleViolation>? violations = null;

        // Act
        Action act = () => AnalysisStatisticsService.GetTopFilesByViolations(violations!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("violations");
    }

    [Fact]
    public void GetSeverityDistribution_NullViolations_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<RuleViolation>? violations = null;

        // Act
        Action act = () => AnalysisStatisticsService.GetSeverityDistribution(violations!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("violations");
    }

    [Fact]
    public void GenerateSummaryReport_NullStats_ThrowsArgumentNullException()
    {
        // Arrange
        AnalysisStatisticsService.ViolationStatistics? stats = null;

        // Act
        Action act = () => AnalysisStatisticsService.GenerateSummaryReport(stats!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("stats");
    }

    [Fact]
    public void CalculateRiskScore_NullStats_ThrowsArgumentNullException()
    {
        // Arrange
        AnalysisStatisticsService.ViolationStatistics? stats = null;

        // Act
        Action act = () => AnalysisStatisticsService.CalculateRiskScore(stats!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("stats");
    }

    [Fact]
    public void GetHealthAssessment_NullStats_ThrowsArgumentNullException()
    {
        // Arrange
        AnalysisStatisticsService.ViolationStatistics? stats = null;

        // Act
        Action act = () => AnalysisStatisticsService.GetHealthAssessment(stats!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("stats");
    }
}
