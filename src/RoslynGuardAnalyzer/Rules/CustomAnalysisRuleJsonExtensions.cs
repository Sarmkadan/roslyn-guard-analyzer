#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using RoslynGuardAnalyzer.Domain.Models;
using RoslynGuardAnalyzer.Core;

namespace RoslynGuardAnalyzer.Rules;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="CustomAnalysisRule"/>.
/// Includes strict validation and sandboxing for user-provided rule definitions.
/// </summary>
public static class CustomAnalysisRuleJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    // Regex timeout to prevent ReDoS attacks (1 second timeout for user-provided patterns)
    internal static readonly TimeSpan RegexCompilationTimeout = TimeSpan.FromSeconds(1);

    // Maximum allowed regex complexity metrics
    private const int MaxRegexPatternLength = 500;
    private const int MaxRegexAlternations = 20;
    private const int MaxRegexQuantifiers = 30;
    private const int MaxRegexGroups = 15;

    /// <summary>
    /// Serializes the specified <see cref="CustomAnalysisRule"/> to a JSON string.
    /// </summary>
    /// <param name="value">The rule to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the rule.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this CustomAnalysisRule value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="CustomAnalysisRule"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized rule.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or consists only of white-space characters.</exception>
    /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized, or contains invalid rule definitions.</exception>
    /// <exception cref="InvalidOperationException">The rule definition contains security violations or invalid configuration.</exception>
    public static CustomAnalysisRule FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        CustomAnalysisRule? rule;
        try
        {
            rule = JsonSerializer.Deserialize<CustomAnalysisRule>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new JsonException("Failed to parse rule JSON. Ensure the JSON is valid and all required properties are present.", ex);
        }

        if (rule is null)
        {
            throw new JsonException("Deserialized rule is null. The JSON may represent a null value.");
        }

        // Validate the deserialized rule with detailed error messages
        ValidateRule(rule, json);

        return rule;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="CustomAnalysisRule"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized rule if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string json, out CustomAnalysisRule? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
        catch (InvalidOperationException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Validates a custom rule definition with comprehensive security and correctness checks.
    /// </summary>
    /// <param name="rule">The rule to validate.</param>
    /// <param name="sourceJson">The original JSON source for error reporting.</param>
    /// <exception cref="JsonException">Thrown when validation fails with detailed error information.</exception>
    /// <exception cref="InvalidOperationException">Thrown when security violations are detected.</exception>
    private static void ValidateRule(CustomAnalysisRule rule, string sourceJson)
    {
        ArgumentNullException.ThrowIfNull(rule);

        var errors = new List<string>();
        var lineNumber = 1;

        // Validate rule ID
        if (string.IsNullOrWhiteSpace(rule.Id))
        {
            errors.Add($"Line {lineNumber}: Rule ID cannot be null or empty.");
        }
        else
        {
            // Check ID length and format
            if (rule.Id.Length < 3)
            {
                errors.Add($"Line {lineNumber}: Rule ID '{rule.Id}' is too short. Minimum length is 3 characters.");
            }
            else if (rule.Id.Length > 50)
            {
                errors.Add($"Line {lineNumber}: Rule ID '{rule.Id}' is too long. Maximum length is 50 characters.");
            }

            // Check ID format (alphanumeric, underscores, hyphens only)
            if (!System.Text.RegularExpressions.Regex.IsMatch(rule.Id, "^[a-zA-Z0-9_-]+$", RegexOptions.CultureInvariant))
            {
                errors.Add($"Line {lineNumber}: Rule ID '{rule.Id}' contains invalid characters. Only alphanumeric, underscore, and hyphen are allowed.");
            }
        }

        // Validate rule name
        if (string.IsNullOrWhiteSpace(rule.Name))
        {
            errors.Add($"Line {lineNumber}: Rule name cannot be null or empty.");
        }
        else
        {
            if (rule.Name.Length < 5)
            {
                errors.Add($"Line {lineNumber}: Rule name '{rule.Name}' is too short. Minimum length is 5 characters.");
            }
            else if (rule.Name.Length > 200)
            {
                errors.Add($"Line {lineNumber}: Rule name '{rule.Name}' is too long. Maximum length is 200 characters.");
            }
        }

        // Validate rule description
        if (string.IsNullOrWhiteSpace(rule.Description))
        {
            errors.Add($"Line {lineNumber}: Rule description cannot be null or empty.");
        }
        else
        {
            if (rule.Description.Length > 1000)
            {
                errors.Add($"Line {lineNumber}: Rule description is too long. Maximum length is 1000 characters.");
            }
        }

        // Validate severity level
        if (!Enum.IsDefined(typeof(SeverityLevel), rule.DefaultSeverity))
        {
            errors.Add($"Line {lineNumber}: Invalid severity level '{rule.DefaultSeverity}'. Valid values are: Info, Warning, Error, Critical.");
        }

        // Validate category
        if (!Enum.IsDefined(typeof(RuleCategory), rule.Category))
        {
            errors.Add($"Line {lineNumber}: Invalid category '{rule.Category}'. Valid values are defined in RuleCategory enum.");
        }

        // Validate RulePattern if present (regex sandboxing)
        if (!string.IsNullOrEmpty(rule.RulePattern))
        {
            ValidateRulePattern(rule.RulePattern, errors, ref lineNumber);
        }

        // Validate configuration
        ValidateConfiguration(rule.Configuration, errors, ref lineNumber);

        // If we have validation errors, throw a comprehensive JsonException
        if (errors.Count > 0)
        {
            var errorMessage = "Custom rule validation failed:\n" + string.Join("\n", errors);
            throw new JsonException(errorMessage);
        }
    }

    /// <summary>
    /// Validates the regex pattern for potential ReDoS vulnerabilities and complexity.
    /// </summary>
    private static void ValidateRulePattern(string pattern, List<string> errors, ref int lineNumber)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return;
        }

        // Check pattern length
        if (pattern.Length > MaxRegexPatternLength)
        {
            errors.Add($"Line {lineNumber}: Regex pattern is too complex. Length {pattern.Length} exceeds maximum of {MaxRegexPatternLength} characters.");
            return; // Don't analyze further if too long
        }

        // Compile with timeout to detect catastrophic backtracking at validation time
        try
        {
            // Test the pattern with a simple match to detect obvious issues
            // Use a timeout to prevent hanging during validation
            using var cts = new CancellationTokenSource(RegexCompilationTimeout);
            var matchTask = System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    // Try a simple match to see if pattern is valid
                    _ = Regex.Match("test", pattern, RegexOptions.None, RegexCompilationTimeout);
                    return true;
                }
                catch (RegexMatchTimeoutException)
                {
                    return false; // Pattern is too complex
                }
                catch (ArgumentException)
                {
                    return false; // Invalid regex syntax
                }
            }, cts.Token);

            if (!matchTask.Result)
            {
                errors.Add($"Line {lineNumber}: Regex pattern is too complex or contains invalid syntax. It may cause ReDoS vulnerabilities.");
                return;
            }
        }
        catch (OperationCanceledException)
        {
            errors.Add($"Line {lineNumber}: Regex pattern caused timeout during validation. It may contain catastrophic backtracking patterns (ReDoS vulnerability).");
            return;
        }
        catch (AggregateException ae) when (ae.InnerException is RegexMatchTimeoutException)
        {
            errors.Add($"Line {lineNumber}: Regex pattern validation timed out. Pattern may contain catastrophic backtracking: {ae.InnerException.Message}");
            return;
        }

        // Analyze pattern complexity (simple static analysis)
        var complexityScore = CalculateRegexComplexity(pattern);
        if (complexityScore > 100)
        {
            errors.Add($"Line {lineNumber}: Regex pattern complexity score {complexityScore} exceeds safe threshold. Simplify the pattern.");
        }

        // Check for dangerous patterns
        var dangerousPatterns = new[] { @"\p{", @"\P{", @"(?(", @"(?(?", @"(*", @"(*F", @"(*COMMIT)", @"(*FAIL)", @"(*ACCEPT)" };
        foreach (var dangerousPattern in dangerousPatterns)
        {
            if (pattern.Contains(dangerousPattern, StringComparison.Ordinal))
            {
                errors.Add($"Line {lineNumber}: Regex pattern contains dangerous construct '{dangerousPattern}'. These can cause security issues.");
                break;
            }
        }
    }

    /// <summary>
    /// Calculates a simple complexity score for regex patterns.
    /// Higher scores indicate more complex patterns that may impact performance.
    /// </summary>
    private static int CalculateRegexComplexity(string pattern)
    {
        var score = 0;

        // Count alternations (|)
        var alternations = pattern.Split('|').Length - 1;
        if (alternations > MaxRegexAlternations)
        {
            score += (alternations - MaxRegexAlternations) * 5;
        }
        else
        {
            score += alternations * 2;
        }

        // Count quantifiers (*, +, ?, {n,m})
        var quantifiers = pattern.Split(new[] { '*', '+', '?', '{' }, StringSplitOptions.None).Length - 1;
        if (quantifiers > MaxRegexQuantifiers)
        {
            score += (quantifiers - MaxRegexQuantifiers) * 3;
        }
        else
        {
            score += quantifiers * 2;
        }

        // Count groups (())
        var groups = pattern.Split(new[] { '(', ')' }, StringSplitOptions.None).Length / 2;
        if (groups > MaxRegexGroups)
        {
            score += (groups - MaxRegexGroups) * 10;
        }
        else
        {
            score += groups * 3;
        }

        // Add length-based penalty
        score += pattern.Length / 10;

        return score;
    }

    /// <summary>
    /// Validates rule configuration dictionary.
    /// </summary>
    private static void ValidateConfiguration(Dictionary<string, object> configuration, List<string> errors, ref int lineNumber)
    {
        if (configuration is null || configuration.Count == 0)
        {
            return;
        }

        foreach (var kvp in configuration)
        {
            if (string.IsNullOrWhiteSpace(kvp.Key))
            {
                errors.Add($"Line {lineNumber}: Configuration key cannot be null or empty.");
            }
            else if (kvp.Key.Length > 100)
            {
                errors.Add($"Line {lineNumber}: Configuration key '{kvp.Key}' is too long (>{100} chars).");
            }

            if (kvp.Value is null)
            {
                errors.Add($"Line {lineNumber}: Configuration value for key '{kvp.Key}' cannot be null.");
            }
            else if (kvp.Value is string strValue && strValue.Length > 1000)
            {
                errors.Add($"Line {lineNumber}: Configuration value for key '{kvp.Key}' is too long (>{1000} chars).");
            }
        }
    }
}
