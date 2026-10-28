#nullable enable

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using RoslynGuardAnalyzer.Rules;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Integration tests for <see cref="BoundsCheckAnalyzer"/> using the Roslyn analyzer testing framework.
/// </summary>
public sealed class BoundsCheckAnalyzerTests
{
    static BoundsCheckAnalyzerTests()
    {
        // ReferenceAssemblies.Net.Net80 restores reference packages through NuGet, which throws
        // when HOME is unset (e.g. minimal CI or service environments). Fall back to the user profile.
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOME")))
        {
            Environment.SetEnvironmentVariable("HOME", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }
    }

    private static CSharpAnalyzerTest<BoundsCheckAnalyzer, DefaultVerifier> CreateTest(string source)
    {
        var test = new CSharpAnalyzerTest<BoundsCheckAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        return test;
    }

    // ==================== Positive tests (should report diagnostics) ====================

    [Fact]
    public async Task ArrayAccess_WithParameterIndex_ReportsDiagnostic()
    {
        // Arrange
        var source = """
            using System;
            class C
            {
                void M(int[] arr, int index)
                {
                    var x = {|#0:arr[index]|};
                }
            }
            """;

        var test = CreateTest(source);
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(BoundsCheckAnalyzer.Rule)
                .WithLocation(0)
                .WithArguments("arr"));

        // Act & Assert
        await test.RunAsync();
    }

    [Fact]
    public async Task ListAccess_WithParameterIndex_ReportsDiagnostic()
    {
        // Arrange
        var source = """
            using System;
            using System.Collections.Generic;
            class C
            {
                void M(List<string> list, int index)
                {
                    var x = {|#0:list[index]|};
                }
            }
            """;

        var test = CreateTest(source);
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(BoundsCheckAnalyzer.Rule)
                .WithLocation(0)
                .WithArguments("list"));

        await test.RunAsync();
    }

    [Fact]
    public async Task SpanAccess_WithParameterIndex_ReportsDiagnostic()
    {
        // Arrange
        var source = """
            using System;
            class C
            {
                void M(Span<int> span, int index)
                {
                    var x = {|#0:span[index]|};
                }
            }
            """;

        var test = CreateTest(source);
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(BoundsCheckAnalyzer.Rule)
                .WithLocation(0)
                .WithArguments("span"));

        await test.RunAsync();
    }

    [Fact]
    public async Task ReadOnlySpanAccess_WithParameterIndex_ReportsDiagnostic()
    {
        // Arrange
        var source = """
            using System;
            class C
            {
                void M(ReadOnlySpan<byte> span, int i)
                {
                    var x = {|#0:span[i]|};
                }
            }
            """;

        var test = CreateTest(source);
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(BoundsCheckAnalyzer.Rule)
                .WithLocation(0)
                .WithArguments("span"));

        await test.RunAsync();
    }

    [Fact]
    public async Task ArrayAccess_UnboundedForLoop_ReportsDiagnostic()
    {
        // A for loop that does NOT compare against the array's Length
        var source = """
            using System;
            class C
            {
                void M(int[] arr, int max)
                {
                    for (int i = 0; i < max; i++)
                    {
                        var x = {|#0:arr[i]|};
                    }
                }
            }
            """;

        var test = CreateTest(source);
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(BoundsCheckAnalyzer.Rule)
                .WithLocation(0)
                .WithArguments("arr"));

        await test.RunAsync();
    }

    // ==================== Negative tests (no diagnostics expected) ====================

    [Fact]
    public async Task ArrayAccess_ConstantIndex_NoDiagnostic()
    {
        // Arrange - constant index should not trigger
        var source = """
            using System;
            class C
            {
                void M(int[] arr)
                {
                    var x = arr[0];
                    var y = arr[1];
                }
            }
            """;

        var test = CreateTest(source);
        // No expected diagnostics
        await test.RunAsync();
    }

    [Fact]
    public async Task ArrayAccess_InsideBoundedForLoop_NoDiagnostic()
    {
        // Arrange - for loop bounded by arr.Length
        var source = """
            using System;
            class C
            {
                void M(int[] arr)
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        var x = arr[i];
                    }
                }
            }
            """;

        var test = CreateTest(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task ListAccess_InsideBoundedForLoop_NoDiagnostic()
    {
        // Arrange - for loop bounded by list.Count
        var source = """
            using System;
            using System.Collections.Generic;
            class C
            {
                void M(List<int> list)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var x = list[i];
                    }
                }
            }
            """;

        var test = CreateTest(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task ArrayAccess_InsideForeachLoop_NoDiagnostic()
    {
        // Arrange - foreach iteration implies bounds
        var source = """
            using System;
            using System.Linq;
            class C
            {
                void M(int[] arr)
                {
                    foreach (var item in arr)
                    {
                        var x = arr[0];
                    }
                }
            }
            """;

        var test = CreateTest(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task ArrayAccess_GuardedByIfLengthCheck_NoDiagnostic()
    {
        // Arrange - if (i < arr.Length) guards the access
        var source = """
            using System;
            class C
            {
                void M(int[] arr, int i)
                {
                    if (i < arr.Length)
                    {
                        var x = arr[i];
                    }
                }
            }
            """;

        var test = CreateTest(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task ArrayAccess_GuardedByPrecedingIfWithReturn_NoDiagnostic()
    {
        // Arrange - a preceding if check with early return guards the access
        var source = """
            using System;
            class C
            {
                void M(int[] arr, int i)
                {
                    if (i >= arr.Length)
                        return;
                    var x = arr[i];
                }
            }
            """;

        var test = CreateTest(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task ArrayAccess_GuardedByPrecedingIfWithThrow_NoDiagnostic()
    {
        // Arrange - a preceding if check with throw guards the access
        var source = """
            using System;
            class C
            {
                void M(int[] arr, int i)
                {
                    if (i >= arr.Length)
                        throw new ArgumentOutOfRangeException(nameof(i));
                    var x = arr[i];
                }
            }
            """;

        var test = CreateTest(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task IndexExpression_HatOperator_NoDiagnostic()
    {
        // Arrange - ^1 index expressions are runtime-bounded
        var source = """
            using System;
            class C
            {
                void M(int[] arr)
                {
                    var last = arr[^1];
                    var secondLast = arr[^2];
                }
            }
            """;

        var test = CreateTest(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task RangeExpression_NoDiagnostic()
    {
        // Arrange - range expressions are bounded by runtime
        var source = """
            using System;
            class C
            {
                void M(int[] arr)
                {
                    var slice = arr[1..3];
                    var fromEnd = arr[..^1];
                }
            }
            """;

        var test = CreateTest(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task DictionaryAccess_NotIndexable_NoDiagnostic()
    {
        // Arrange - Dictionary is not in the indexable type list
        var source = """
            using System;
            using System.Collections.Generic;
            class C
            {
                void M(Dictionary<string, int> dict, string key)
                {
                    var x = dict[key];
                }
            }
            """;

        var test = CreateTest(source);
        await test.RunAsync();
    }

    [Fact]
    public async Task ArrayAccess_PatternMatchedLength_NoDiagnostic()
    {
        // Arrange - length comparison in enclosing if guards the access
        var source = """
            using System;
            class C
            {
                void M(int[] arr)
                {
                    if (arr.Length > 3)
                    {
                        var x = arr[3];
                    }
                }
            }
            """;

        var test = CreateTest(source);
        await test.RunAsync();
    }

    // ==================== Diagnostic descriptor validation ====================

    [Fact]
    public void Rule_HasExpectedProperties()
    {
        BoundsCheckAnalyzer.Rule.Id.Should().Be("BND001");
        BoundsCheckAnalyzer.Rule.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
        BoundsCheckAnalyzer.Rule.IsEnabledByDefault.Should().BeTrue();
        BoundsCheckAnalyzer.Rule.Category.Should().Be("RoslynGuardAnalyzer.BoundsCheck");
    }

    [Fact]
    public void Analyzer_SupportsBND001()
    {
        var analyzer = new BoundsCheckAnalyzer();
        analyzer.SupportedDiagnostics.Should().ContainSingle()
            .Which.Id.Should().Be("BND001");
    }
}
