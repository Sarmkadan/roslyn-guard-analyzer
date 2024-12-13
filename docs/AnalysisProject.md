# AnalysisProject

The `AnalysisProject` class serves as the primary data model representing a single .NET project within the Roslyn Guard Analyzer ecosystem. It encapsulates essential metadata such as project identity, file system location, target framework, and language, while maintaining collections of source files and project references. This type facilitates the aggregation of project-level statistics and provides mechanisms for dynamically modifying the project's configuration and file lists during the analysis lifecycle.

## API

### Properties

#### `public string Id`
Gets the unique identifier for the project. This value is typically derived from the project file GUID or a deterministic hash of the project path.

#### `public string Name`
Gets the human-readable name of the project, usually corresponding to the assembly name or the project file name without extension.

#### `public string Path`
Gets the absolute file system path to the project file (e.g., `.csproj` or `.vbproj`).

#### `public string? TargetFramework`
Gets the target framework moniker (TFM) specified for this project (e.g., `net6.0`, `netstandard2.0`). Returns `null` if the framework is not explicitly defined or could not be resolved.

#### `public List<string> SourceFiles`
Gets the list of absolute paths to all source code files included in this project. This list is mutable and reflects the current state of included items.

#### `public List<string> ReferencedProjects`
Gets the list of paths to other project files referenced by this project. This defines the intra-solution dependency graph.

#### `public Dictionary<string, string> Properties`
Gets the dictionary of MSBuild properties associated with this project. Keys are property names and values are their resolved string values.

#### `public bool IsNetCore`
Gets a value indicating whether the project targets a .NET Core or .NET 5+ runtime. Returns `false` for .NET Framework projects.

#### `public string? Language`
Gets the programming language of the project (e.g., `C#`, `VB`). Returns `null` if the language cannot be determined.

#### `public DateTime AnalyzedAt`
Gets the timestamp indicating when the analysis for this project was initiated or completed.

#### `public int FileCount`
Gets the total number of source files currently tracked in the `SourceFiles` collection.

#### `public bool IsModernDotNet`
Gets a value indicating whether the project targets a modern, supported version of .NET (typically .NET Core 3.1, .NET 5, or later). This property derives its value from `TargetFramework`.

### Constructors

#### `public AnalysisProject()`
Initializes a new instance of the `AnalysisProject` class with default values. Collections are initialized to empty lists, and nullable fields are set to `null`.

#### `public AnalysisProject(string path, string name)`
Initializes a new instance of the `AnalysisProject` class with the specified file path and name.
*   **Parameters**:
    *   `path`: The absolute path to the project file.
    *   `name`: The name of the project.
*   **Throws**: `ArgumentNullException` if `path` or `name` is null. `ArgumentException` if `path` is empty.

### Methods

#### `public void AddSourceFile(string filePath)`
Adds a source file path to the `SourceFiles` collection.
*   **Parameters**:
    *   `filePath`: The absolute path to the source file.
*   **Throws**: `ArgumentNullException` if `filePath` is null. `ArgumentException` if the file does not exist or has an invalid extension.

#### `public void AddReferencedProject(string projectPath)`
Adds a referenced project path to the `ReferencedProjects` collection.
*   **Parameters**:
    *   `projectPath`: The absolute path to the referenced project file.
*   **Throws**: `ArgumentNullException` if `projectPath` is null.

#### `public IEnumerable<string> GetCSharpFiles()`
Returns an enumerable collection of source file paths filtered to include only C# files (files ending in `.cs`).
*   **Returns**: `IEnumerable<string>` containing paths to C# files.
*   **Remarks**: The enumeration is lazy; filtering occurs during iteration.

#### `public string? GetProperty(string key)`
Retrieves the value of a specific MSBuild property.
*   **Parameters**:
    *   `key`: The name of the property to retrieve.
*   **Returns**: The property value if found; otherwise, `null`.
*   **Throws**: `ArgumentNullException` if `key` is null.

#### `public void SetProperty(string key, string value)`
Sets or updates a specific MSBuild property in the `Properties` dictionary.
*   **Parameters**:
    *   `key`: The name of the property.
    *   `value`: The value to assign.
*   **Throws**: `ArgumentNullException` if `key` is null.

#### `public ProjectStatistics GetStatistics()`
Calculates and returns statistical data regarding the project, such as line counts, complexity metrics, or dependency depth.
*   **Returns**: A `ProjectStatistics` object containing the computed metrics.
*   **Remarks**: This method may perform I/O operations or traverse the syntax tree depending on the implementation of `ProjectStatistics`.

## Usage

### Example 1: Initializing and Configuring a Project
The following example demonstrates creating an `AnalysisProject` instance, setting core properties, and adding source files.

```csharp
using RoslynGuardAnalyzer.Models;

// Initialize the project with path and name
var project = new AnalysisProject(
    path: "/src/MyApplication/MyApplication.csproj", 
    name: "MyApplication"
);

// Configure metadata
project.SetProperty("Configuration", "Release");
project.SetProperty("Platform", "AnyCPU");

// Add source files
project.AddSourceFile("/src/MyApplication/Program.cs");
project.AddSourceFile("/src/MyApplication/Services/DataService.cs");

// Verify C# files only
var csharpFiles = project.GetCSharpFiles();
foreach (var file in csharpFiles)
{
    Console.WriteLine($"Processing C# file: {file}");
}
```

### Example 2: Analyzing Dependencies and Statistics
This example illustrates how to link referenced projects and retrieve analysis statistics.

```csharp
using RoslynGuardAnalyzer.Models;

var mainProject = new AnalysisProject("/src/Main/Main.csproj", "Main");
var libProject = new AnalysisProject("/src/CoreLib/CoreLib.csproj", "CoreLib");

// Establish project reference
mainProject.AddReferencedProject(libProject.Path);

// Check framework compatibility
if (mainProject.IsModernDotNet)
{
    Console.WriteLine($"Project {mainProject.Name} targets a modern .NET runtime.");
}

// Retrieve statistics after analysis setup
var stats = mainProject.GetStatistics();
Console.WriteLine($"Total files: {stats.TotalFiles}");
Console.WriteLine($"Analysis completed at: {mainProject.AnalyzedAt}");
```

## Notes

*   **Thread Safety**: The `AnalysisProject` class is not thread-safe. The collections `SourceFiles`, `ReferencedProjects`, and `Properties` are mutable standard .NET collections (`List<T>` and `Dictionary<TKey, TValue>`). Concurrent modifications (e.g., calling `AddSourceFile` while iterating `GetCSharpFiles`) from multiple threads will result in undefined behavior or runtime exceptions. External synchronization is required when accessing an instance from multiple threads.
*   **Path Validity**: Methods accepting file paths (`AddSourceFile`, `AddReferencedProject`) assume absolute paths. Passing relative paths may lead to resolution errors during later stages of the analysis pipeline.
*   **Nullable Reference Types**: Properties such as `TargetFramework` and `Language` are nullable. Consumers must handle `null` values appropriately, particularly when `IsNetCore` or `IsModernDotNet` returns `false`, as the specific framework string might be unavailable.
*   **Lazy Evaluation**: The `GetCSharpFiles` method returns an `IEnumerable<string>`. The filtering logic is executed upon enumeration. If the underlying `SourceFiles` list is modified between the call to `GetCSharpFiles` and the iteration of the result, the enumeration may reflect those changes or throw an exception if the collection is modified during iteration.
