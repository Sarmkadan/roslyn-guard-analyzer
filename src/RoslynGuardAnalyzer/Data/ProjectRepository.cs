// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System;
using RoslynGuardAnalyzer.Domain.Models;

namespace RoslynGuardAnalyzer.Data;

/// <summary>
/// Repository for managing persistence of analyzed projects.
/// </summary>
public sealed class ProjectRepository : RepositoryBase<AnalysisProject>
{
    private const string ProjectsFileName = "projects.json";
    private readonly string _dataDirectory;

    public ProjectRepository(string? dataDirectory = null)
    {
        _dataDirectory = dataDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RoslynGuardAnalyzer",
            "Data");

        EnsureDirectoryExists();
    }

    /// <summary>
    /// Gets projects by target framework.
    /// </summary>
    public IReadOnlyList<AnalysisProject> GetByTargetFramework(string targetFramework)
    {
        return Find(p => p.TargetFramework?.Equals(targetFramework, StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Gets .NET Core/.NET 5+ projects only.
    /// </summary>
    public IReadOnlyList<AnalysisProject> GetModernDotNetProjects()
    {
        return Find(p => p.IsModernDotNet());
    }

    /// <summary>
    /// Gets projects by language.
    /// </summary>
    public IReadOnlyList<AnalysisProject> GetByLanguage(string language)
    {
        return Find(p => p.Language?.Equals(language, StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Gets projects with more than a specified number of files.
    /// </summary>
    public IReadOnlyList<AnalysisProject> GetWithMoreFilesThan(int fileCount)
    {
        return Find(p => p.FileCount > fileCount);
    }

    /// <summary>
    /// Gets projects analyzed after a specific date.
    /// </summary>
    public IReadOnlyList<AnalysisProject> GetAnalyzedAfter(DateTime date)
    {
        return Find(p => p.AnalyzedAt > date);
    }

    /// <summary>
    /// Searches for projects by name pattern.
    /// </summary>
    public IReadOnlyList<AnalysisProject> SearchByName(string pattern)
    {
        return Find(p => p.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Finds a project by path.
    /// </summary>
    public AnalysisProject? FindByPath(string path)
    {
        return Find(p => p.Path.Equals(path, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
    }

    /// <summary>
    /// Gets projects with referenced dependencies.
    /// </summary>
    public IReadOnlyList<AnalysisProject> GetWithReferences()
    {
        return Find(p => p.ReferencedProjects.Any());
    }

    /// <summary>
    /// Saves all projects to disk asynchronously.
    /// </summary>
    public async Task SaveAsync()
    {
        var projectsPath = Path.Combine(_dataDirectory, ProjectsFileName);

        try
        {
            var projects = GetAll();
            var json = System.Text.Json.JsonSerializer.Serialize(
                projects,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(projectsPath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save projects: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads all projects from disk asynchronously.
    /// </summary>
    public async Task LoadAsync()
    {
        var projectsPath = Path.Combine(_dataDirectory, ProjectsFileName);

        if (!File.Exists(projectsPath))
            return;

        try
        {
            var json = await File.ReadAllTextAsync(projectsPath);
            var projects = System.Text.Json.JsonSerializer.Deserialize<List<AnalysisProject>>(json);

            Clear();

            if (projects != null)
            {
                foreach (var project in projects)
                {
                    Add(project.Id, project);
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load projects: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Exports projects to a JSON file.
    /// </summary>
    public async Task ExportAsync(string filePath)
    {
        try
        {
            var projects = GetAll();
            var json = System.Text.Json.JsonSerializer.Serialize(
                projects,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to export projects: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Imports projects from a JSON file.
    /// </summary>
    public async Task ImportAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Import file not found: {filePath}");

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var projects = System.Text.Json.JsonSerializer.Deserialize<List<AnalysisProject>>(json);

            if (projects != null)
            {
                foreach (var project in projects)
                {
                    if (Exists(project.Id))
                    {
                        Update(project.Id, project);
                    }
                    else
                    {
                        Add(project.Id, project);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to import projects: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets statistics about stored projects.
    /// </summary>
    public ProjectRepositoryStatistics GetStatistics()
    {
        var allProjects = GetAll();

        if (!allProjects.Any())
        {
            return new ProjectRepositoryStatistics();
        }

        return new ProjectRepositoryStatistics
        {
            TotalProjects = allProjects.Count,
            ModernDotNetProjects = allProjects.Count(p => p.IsModernDotNet()),
            AverageFileCount = allProjects.Average(p => p.FileCount),
            TotalFiles = allProjects.Sum(p => p.FileCount),
            ProjectsByFramework = allProjects
                .GroupBy(p => p.TargetFramework ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count()),
            UniqueLanguages = allProjects.Select(p => p.Language).Distinct().Count()
        };
    }

    /// <summary>
    /// Removes a project and optionally its source files from disk.
    /// </summary>
    public async Task RemoveProjectAsync(string projectId, bool deleteSourceFiles = false)
    {
        var project = GetById(projectId);
        if (project == null)
            return;

        if (deleteSourceFiles && Directory.Exists(project.GetDirectoryPath()))
        {
            try
            {
                Directory.Delete(project.GetDirectoryPath(), recursive: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not delete project directory: {ex.Message}");
            }
        }

        Remove(projectId);
    }

    /// <summary>
    /// Validates all projects and removes invalid ones.
    /// </summary>
    public void ValidateAndCleanup()
    {
        var invalidProjects = Find(p => !p.IsValid()).ToList();

        foreach (var project in invalidProjects)
        {
            Remove(project.Id);
        }
    }

    /// <summary>
    /// Ensures the data directory exists.
    /// </summary>
    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }
    }
}

/// <summary>
/// Statistics about the projects repository.
/// </summary>
public sealed class ProjectRepositoryStatistics
{
    public int TotalProjects { get; set; }
    public int ModernDotNetProjects { get; set; }
    public double AverageFileCount { get; set; }
    public int TotalFiles { get; set; }
    public Dictionary<string, int> ProjectsByFramework { get; set; } = new();
    public int UniqueLanguages { get; set; }

    public double GetModernDotNetPercentage()
    {
        if (TotalProjects == 0)
            return 0;

        return (ModernDotNetProjects / (double)TotalProjects) * 100;
    }
}
