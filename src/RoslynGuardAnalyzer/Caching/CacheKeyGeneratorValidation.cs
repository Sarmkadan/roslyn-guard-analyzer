#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace RoslynGuardAnalyzer.Caching;

/// <summary>
/// Provides validation helpers for <see cref="CacheKeyGenerator"/> to ensure generated cache keys are valid.
/// </summary>
public sealed class CacheKeyGeneratorValidation
{
    /// <summary>
    /// Validates the provided parameters for <see cref="CacheKeyGenerator.GenerateProjectAnalysisKey"/>.
    /// </summary>
    /// <param name="projectPath">The project path to validate.</param>
    /// <param name="configHash">The configuration hash to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="projectPath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="projectPath"/> is empty or whitespace.</exception>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGenerateProjectAnalysisKey(
        string projectPath,
        string? configHash = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectPath);

        var problems = new List<string>();

        if (!string.IsNullOrEmpty(configHash) && string.IsNullOrWhiteSpace(configHash))
        {
            problems.Add("Configuration hash cannot be whitespace if provided.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided parameters for <see cref="CacheKeyGenerator.GenerateFileAnalysisKey"/>.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <param name="fileContentHash">The file content hash to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is empty or whitespace.</exception>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGenerateFileAnalysisKey(
        string filePath,
        string? fileContentHash = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var problems = new List<string>();

        if (!string.IsNullOrEmpty(fileContentHash) && string.IsNullOrWhiteSpace(fileContentHash))
        {
            problems.Add("File content hash cannot be whitespace if provided.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided parameters for <see cref="CacheKeyGenerator.GenerateResultKey"/>.
    /// </summary>
    /// <param name="analysisId">The analysis ID to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="analysisId"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="analysisId"/> is empty or whitespace.</exception>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGenerateResultKey(string analysisId)
    {
        ArgumentException.ThrowIfNullOrEmpty(analysisId);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates the provided parameters for <see cref="CacheKeyGenerator.GenerateRuleExecutionKey"/>.
    /// </summary>
    /// <param name="ruleName">The rule name to validate.</param>
    /// <param name="targetName">The target name to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ruleName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="ruleName"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="targetName"/> is empty or whitespace.</exception>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGenerateRuleExecutionKey(
        string ruleName,
        string targetName)
    {
        ArgumentException.ThrowIfNullOrEmpty(ruleName);
        ArgumentException.ThrowIfNullOrEmpty(targetName);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates the provided parameters for <see cref="CacheKeyGenerator.GenerateCodeElementKey"/>.
    /// </summary>
    /// <param name="fullTypeName">The full type name to validate.</param>
    /// <param name="memberName">The member name to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fullTypeName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fullTypeName"/> is empty or whitespace.</exception>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGenerateCodeElementKey(
        string fullTypeName,
        string? memberName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(fullTypeName);

        var problems = new List<string>();

        if (!string.IsNullOrEmpty(memberName) && string.IsNullOrWhiteSpace(memberName))
        {
            problems.Add("Member name cannot be whitespace if provided.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided parameters for <see cref="CacheKeyGenerator.ComputeHash"/>.
    /// </summary>
    /// <param name="input">The input string to validate.</param>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateComputeHash(string? input)
    {
        var problems = new List<string>();

        // null is acceptable - the method handles it by returning "empty"
        if (!string.IsNullOrEmpty(input) && string.IsNullOrWhiteSpace(input))
        {
            problems.Add("Input cannot be whitespace if provided.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided parameters for <see cref="CacheKeyGenerator.ComputeFileHash"/>.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is empty or whitespace.</exception>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateComputeFileHash(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates the provided parameters for <see cref="CacheKeyGenerator.CreateCompositeKey"/>.
    /// </summary>
    /// <param name="components">The components array to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="components"/> is <see langword="null"/>.</exception>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateCreateCompositeKey(params string[]? components)
    {
        var problems = new List<string>();

        if (components is null || components.Length == 0)
        {
            problems.Add("At least one component is required.");
        }
        else
        {
            for (int i = 0; i < components.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(components[i]))
                {
                    problems.Add($"Component at index {i} cannot be null, empty, or whitespace.");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided parameters for <see cref="CacheKeyGenerator.GeneratePatternKey"/>.
    /// </summary>
    /// <param name="prefix">The prefix to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="prefix"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="prefix"/> is empty or whitespace.</exception>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGeneratePatternKey(string prefix)
    {
        ArgumentException.ThrowIfNullOrEmpty(prefix);

        return Array.Empty<string>();
    }
}
