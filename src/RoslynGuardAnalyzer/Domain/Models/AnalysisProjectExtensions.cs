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
    public static bool HasProperty(this AnalysisProject project, string key)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentException.ThrowIfNullOrEmpty(key);

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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="project"/> or <paramref name="otherProject"/> is null.</exception>
    public static bool HasSameTargetFramework(this AnalysisProject project, AnalysisProject otherProject)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(otherProject);

        return project.TargetFramework == otherProject.TargetFramework;
    }
}
