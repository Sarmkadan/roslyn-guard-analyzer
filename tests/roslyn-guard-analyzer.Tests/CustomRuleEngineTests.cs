#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using RoslynGuardAnalyzer.Core;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Rules;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="CustomRuleEngine"/>.
/// </summary>
public sealed class CustomRuleEngineTests
{
    private static CodeElement CreateElement(string name)
    {
        return new CodeElement { Id = name, Name = name, FilePath = "Sample.cs", Namespace = "Sample" };
    }

    [Fact]
    public void Constructor_WithNullRegistry_ThrowsArgumentNullException()
    {
        var act = () => new CustomRuleEngine(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluateRuleAsync_WithMatchingElements_ReturnsViolations()
    {
        var registry = Substitute.For<ICustomRuleRegistry>();
        var engine = new CustomRuleEngine(registry);

        var rule = CustomRuleBuilder
            .Create("R100", "NoFooRule")
            .When(e => e.Name == "Foo")
            .WithMessage("Foo is not allowed")
            .Build();

        var elements = new List<CodeElement> { CreateElement("Foo"), CreateElement("Bar") };

        var violations = await engine.EvaluateRuleAsync(rule, elements);

        violations.Should().ContainSingle();
        violations[0].RuleId.Should().Be("R100");
        violations[0].Message.Should().Be("Foo is not allowed");
    }

    [Fact]
    public async Task EvaluateRuleAsync_WithNoMatchingElements_ReturnsEmptyList()
    {
        var registry = Substitute.For<ICustomRuleRegistry>();
        var engine = new CustomRuleEngine(registry);

        var rule = CustomRuleBuilder
            .Create("R101", "NeverMatches")
            .When(_ => false)
            .WithMessage("unused")
            .Build();

        var violations = await engine.EvaluateRuleAsync(rule, Array.Empty<CodeElement>());

        violations.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateRuleAsync_WithAlreadyCancelledToken_ThrowsOperationCanceledException()
    {
        var registry = Substitute.For<ICustomRuleRegistry>();
        var engine = new CustomRuleEngine(registry);

        var rule = CustomRuleBuilder
            .Create("R102", "CancelledRule")
            .When(_ => true)
            .WithMessage("unused")
            .Build();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await engine.EvaluateRuleAsync(rule, Array.Empty<CodeElement>(), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task EvaluateAsync_WithNullElements_ThrowsArgumentNullException()
    {
        var registry = Substitute.For<ICustomRuleRegistry>();
        var engine = new CustomRuleEngine(registry);

        var act = async () => await engine.EvaluateAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluateAsync_WithNoRegisteredRules_ReturnsEmptyList()
    {
        var registry = Substitute.For<ICustomRuleRegistry>();
        registry.GetCustomRules().Returns(new List<CustomAnalysisRule>());
        var engine = new CustomRuleEngine(registry);

        var violations = await engine.EvaluateAsync(new List<CodeElement> { CreateElement("Foo") });

        violations.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateAsync_WithMultipleRules_AggregatesViolationsAcrossRules()
    {
        var registry = Substitute.For<ICustomRuleRegistry>();

        var fooRule = CustomRuleBuilder
            .Create("R200", "NoFoo")
            .When(e => e.Name == "Foo")
            .WithMessage("Foo violation")
            .Build();

        var barRule = CustomRuleBuilder
            .Create("R201", "NoBar")
            .When(e => e.Name == "Bar")
            .WithMessage("Bar violation")
            .Build();

        registry.GetCustomRules().Returns(new List<CustomAnalysisRule> { fooRule, barRule });
        var engine = new CustomRuleEngine(registry);

        var elements = new List<CodeElement> { CreateElement("Foo"), CreateElement("Bar"), CreateElement("Baz") };

        var violations = await engine.EvaluateAsync(elements);

        violations.Should().HaveCount(2);
        violations.Should().Contain(v => v.RuleId == "R200");
        violations.Should().Contain(v => v.RuleId == "R201");
    }

    [Fact]
    public async Task EvaluateAsync_WithEmptyElementCollection_ReturnsEmptyList()
    {
        var registry = Substitute.For<ICustomRuleRegistry>();

        var rule = CustomRuleBuilder
            .Create("R202", "AlwaysMatches")
            .When(_ => true)
            .WithMessage("unused")
            .Build();

        registry.GetCustomRules().Returns(new List<CustomAnalysisRule> { rule });
        var engine = new CustomRuleEngine(registry);

        var violations = await engine.EvaluateAsync(Array.Empty<CodeElement>());

        violations.Should().BeEmpty();
    }
}
