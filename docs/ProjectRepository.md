# ProjectRepository

A utility class that provides access to and management of a collection of `AnalysisProject` instances, enabling filtering, searching, statistical analysis, and persistence operations for project analysis scenarios.

## API

### `ProjectRepository()`
Initializes a new empty instance of the `ProjectRepository` class.

### `public IReadOnlyList<AnalysisProject> GetByTargetFramework(string targetFramework)`
Returns all projects whose target framework matches the specified value.
- **targetFramework**: The target framework identifier to match (e.g., `net8.0`).
- Returns: A read-only list of matching projects. Never `null`.
- Throws: `ArgumentNullException` if `targetFramework` is `null`.

### `public IReadOnlyList<AnalysisProject> GetModernDotNetProjects()`
Returns all projects targeting modern .NET (i.e., .NET Core or .NET 5+).
- Returns: A read-only list of modern .NET projects. Never `null`.

### `public IReadOnlyList<AnalysisProject> GetByLanguage(string language)`
Returns all projects whose primary language matches the specified value.
- **language**: The language identifier to match (e.g., `C#`).
- Returns: A read-only list of matching projects. Never `null`.
- Throws: `ArgumentNullException` if `language` is `null`.

### `public IReadOnlyList<AnalysisProject> GetWithMoreFilesThan(int fileCountThreshold)`
Returns all projects that contain more files than the specified threshold.
- **fileCountThreshold**: The minimum number of files a project must contain to be included.
- Returns: A read-only list of matching projects. Never `null`.

### `public IReadOnlyList<AnalysisProject> GetAnalyzedAfter(DateTime cutoff)`
Returns all projects that were analyzed after the specified cutoff date and time.
- **cutoff**: The date and time threshold for analysis completion.
- Returns: A read-only list of matching projects. Never `null`.

### `public IReadOnlyList<AnalysisProject> SearchByName(string namePattern)`
Returns all projects whose name matches the specified pattern using simple substring matching.
- **namePattern**: The substring to search for in project names.
- Returns: A read-only list of matching projects. Never `null`.
- Throws: `ArgumentNullException` if `namePattern` is `null`.

### `public AnalysisProject? FindByPath(string projectPath)`
Finds and returns the project located at the specified file system path, if present.
- **projectPath**: The absolute or relative path to the project file.
- Returns: The matching project, or `null` if not found.
- Throws: `ArgumentNullException` if `projectPath` is `null`.

### `public IReadOnlyList<AnalysisProject> GetWithReferences()`
Returns all projects that have at least one project reference.
- Returns: A read-only list of projects with references. Never `null`.

### `public async Task SaveAsync(Stream outputStream)`
Serializes the repository and all contained projects to the specified output stream.
- **outputStream**: The stream to write the serialized data to.
- Throws: `ArgumentNullException` if `outputStream` is `null`.
- Throws: `InvalidOperationException` if the repository is in an invalid state for saving.

### `public async Task LoadAsync(Stream inputStream)`
Deserializes and loads project data from the specified input stream into the repository.
- **inputStream**: The stream containing the serialized repository data.
- Throws: `ArgumentNullException` if `inputStream` is `null`.
- Throws: `InvalidOperationException` if the repository is not empty or in an invalid state for loading.

### `public async Task ExportAsync(Stream outputStream, IReadOnlyList<AnalysisProject> projects)`
Exports the specified subset of projects to the given output stream in a portable format.
- **outputStream**: The stream to write the exported data to.
- **projects**: The list of projects to export.
- Throws: `ArgumentNullException` if either parameter is `null`.
- Throws: `InvalidOperationException` if the repository is in an invalid state for export.

### `public async Task ImportAsync(Stream inputStream)`
Imports projects from the specified input stream into the repository, merging with existing data.
- **inputStream**: The stream containing the projects to import.
- Throws: `ArgumentNullException` if `inputStream` is `null`.
- Throws: `InvalidOperationException` if the repository is in an invalid state for import.

### `public ProjectRepositoryStatistics GetStatistics()`
Computes and returns aggregated statistics about the projects in the repository.
- Returns: A `ProjectRepositoryStatistics` object containing counts and metrics. Never `null`.

### `public async Task RemoveProjectAsync(AnalysisProject project)`
Removes the specified project from the repository.
- **project**: The project to remove.
- Throws: `ArgumentNullException` if `project` is `null`.
- Throws: `KeyNotFoundException` if the project is not present.

### `public void ValidateAndCleanup()`
Validates the integrity of the repository and removes any invalid or orphaned entries.
- Any project failing validation is removed from the repository.

### `public int TotalProjects`
Gets the total number of projects currently in the repository.
- Returns: The count of projects. Always non-negative.

### `public int ModernDotNetProjects`
Gets the number of projects targeting modern .NET frameworks.
- Returns: The count of modern .NET projects. Always non-negative.

### `public double AverageFileCount`
Gets the average number of files per project across the repository.
- Returns: The average file count. May be fractional.

### `public int TotalFiles`
Gets the total number of files across all projects in the repository.
- Returns: The total file count. Always non-negative.

## Usage

### Example 1: Filtering and Exporting Modern .NET Projects
