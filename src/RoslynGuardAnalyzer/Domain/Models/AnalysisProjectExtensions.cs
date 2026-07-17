namespace RoslynGuardAnalyzer.Domain.Models;

/// <summary>
/// Provides extension methods for <see cref="AnalysisProject"/>.
/// </summary>
public static class AnalysisProjectExtensions
{
    /// <summary>
    /// Determines whether the project has a specific property.
    /// </summary>
    /// <param name="project">The project to check.</param>
    /// <param name="key">The key of the property to look for.</param>
    /// <returns>true if the project has the property; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="project"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
    public static bool HasProperty(this AnalysisProject project, string key)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(key);

        return project.Properties.ContainsKey(key);
    }

    /// <summary>
    /// Gets the names of all source files that are C# files.
    /// </summary>
    /// <param name="project">The project to get the C# file names from.</param>
    /// <returns>An enumerable collection of C# file names.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="project"/> is null.</exception>
    public static IReadOnlyList<string> GetAllCSharpFiles(this AnalysisProject project)
    {
        ArgumentNullException.ThrowIfNull(project);

        return project.GetCSharpFiles().ToList();
    }

    /// <summary>
    /// Determines whether two projects have the same target framework.
    /// </summary>
    /// <param name="project">The first project to compare.</param>
    /// <param name="otherProject">The second project to compare.</param>
    /// <returns>true if the projects have the same target framework; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="project"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="otherProject"/> is null.</exception>
    public static bool HasSameTargetFramework(this AnalysisProject project, AnalysisProject? otherProject)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(otherProject);

        return string.Equals(project.TargetFramework, otherProject.TargetFramework, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the value of a project property or throws if not found.
    /// </summary>
    /// <param name="project">The project to get the property from.</param>
    /// <param name="key">The key of the property to retrieve.</param>
    /// <returns>The property value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="project"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the property does not exist.</exception>
    public static string GetRequiredProperty(this AnalysisProject project, string key)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(key);

        if (!project.Properties.TryGetValue(key, out var value))
        {
            throw new KeyNotFoundException($"The required property '{key}' was not found in the project '{project.Name}'");
        }

        return value;
    }

    /// <summary>
    /// Determines whether the project targets a modern .NET version (net6.0+).
    /// </summary>
    /// <param name="project">The project to check.</param>
    /// <returns>true if the project targets a modern .NET version; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="project"/> is null.</exception>
    public static bool IsModernDotNetProject(this AnalysisProject project)
    {
        ArgumentNullException.ThrowIfNull(project);

        return project.IsModernDotNet();
    }

    /// <summary>
    /// Gets the count of C# files in the project.
    /// </summary>
    /// <param name="project">The project to count files in.</param>
    /// <returns>The number of C# files.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="project"/> is null.</exception>
    public static int GetCSharpFileCount(this AnalysisProject project)
    {
        ArgumentNullException.ThrowIfNull(project);

        return project.GetCSharpFiles().Count();
    }

    /// <summary>
    /// Determines whether the project has any C# files.
    /// </summary>
    /// <param name="project">The project to check.</param>
    /// <returns>true if the project has at least one C# file; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="project"/> is null.</exception>
    public static bool HasCSharpFiles(this AnalysisProject project)
    {
        ArgumentNullException.ThrowIfNull(project);

        return project.GetCSharpFiles().Any();
    }

    /// <summary>
    /// Gets the project's target framework version as a normalized string.
    /// Returns "Unknown" if the target framework is not set.
    /// </summary>
    /// <param name="project">The project to get the target framework from.</param>
    /// <returns>The normalized target framework version.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="project"/> is null.</exception>
    public static string GetTargetFrameworkDisplay(this AnalysisProject project)
    {
        ArgumentNullException.ThrowIfNull(project);

        return string.IsNullOrWhiteSpace(project.TargetFramework)
            ? "Unknown"
            : project.TargetFramework;
    }
}
