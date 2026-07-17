# AnalysisProjectExtensions

Provides extension methods for the `AnalysisProject` class to facilitate common project analysis operations such as property checking, file enumeration, and target framework comparison.

## API

### HasProperty

Determines whether the project contains a specific property in its properties dictionary.

- **Parameters:**
  - `project` (AnalysisProject): The project to check.
  - `key` (string): The key of the property to look for.

- **Returns:** `bool` - `true` if the project has the property; otherwise, `false`.

- **Exceptions:**
  - Throws `ArgumentNullException` if `project` is null.
  - Throws `ArgumentNullException` if `key` is null.

- **Usage:** Useful for checking if a project has specific metadata or configuration properties before attempting to retrieve them.

### GetAllCSharpFiles

Gets the names of all source files that are C# files in the project.

- **Parameters:**
  - `project` (AnalysisProject): The project to get the C# file names from.

- **Returns:** `IReadOnlyList<string>` - An immutable list of C# file names (including file paths).

- **Exceptions:**
  - Throws `ArgumentNullException` if `project` is null.

- **Usage:** Returns all `.cs` files in the project's source files collection as an immutable list.

### HasSameTargetFramework

Determines whether two projects have the same target framework.

- **Parameters:**
  - `project` (AnalysisProject): The first project to compare.
  - `otherProject` (AnalysisProject?): The second project to compare.

- **Returns:** `bool` - `true` if the projects have the same target framework; otherwise, `false`.

- **Exceptions:**
  - Throws `ArgumentNullException` if `project` is null.
  - Throws `ArgumentNullException` if `otherProject` is null.

- **Usage:** Useful for validating that projects in a solution share the same target framework before performing cross-project analysis.

### GetRequiredProperty

Gets the value of a project property or throws if the property is not found.

- **Parameters:**
  - `project` (AnalysisProject): The project to get the property from.
  - `key` (string): The key of the property to retrieve.

- **Returns:** `string` - The property value.

- **Exceptions:**
  - Throws `ArgumentNullException` if `project` is null.
  - Throws `ArgumentNullException` if `key` is null.
  - Throws `KeyNotFoundException` if the property does not exist in the project's properties dictionary.

- **Usage:** Retrieves a required project property and throws a descriptive exception if it's missing, ensuring mandatory configuration is present.


### IsModernDotNetProject

Determines whether the project targets a modern .NET version (net6.0 or later).


- **Parameters:**
  - `project` (AnalysisProject): The project to check.

- **Returns:** `bool` - `true` if the project targets a modern .NET version (net6.0+); otherwise, `false`.

- **Exceptions:**
  - Throws `ArgumentNullException` if `project` is null.


- **Usage:** Identifies projects that use modern .NET features and can leverage newer language and framework capabilities.


### GetCSharpFileCount

Gets the count of C# files in the project.

- **Parameters:**
  - `project` (AnalysisProject): The project to count files in.

- **Returns:** `int` - The number of C# files in the project.

- **Exceptions:**
  - Throws `ArgumentNullException` if `project` is null.

- **Usage:** Quickly determines the size of a project in terms of source files for analysis planning or progress tracking.

### HasCSharpFiles

Determines whether the project has any C# files.

- **Parameters:**
  - `project` (AnalysisProject): The project to check.

- **Returns:** `bool` - `true` if the project has at least one C# file; otherwise, `false`.

- **Exceptions:**
  - Throws `ArgumentNullException` if `project` is null.


- **Usage:** Checks if a project contains any source code before performing analysis operations that require source files.

### GetTargetFrameworkDisplay

Gets the project's target framework version as a normalized string.

- **Parameters:**
  - `project` (AnalysisProject): The project to get the target framework from.

- **Returns:** `string` - The normalized target framework version. Returns "Unknown" if the target framework is not set or is whitespace.

- **Exceptions:**
  - Throws `ArgumentNullException` if `project` is null.

- **Usage:** Provides a display-ready string representation of the target framework for logging or user interface display.


## Usage

### Example 1: Validating project properties before analysis
```csharp
var project = analysisContext.GetProject("MyProject");

if (project.HasProperty("AnalysisEnabled") && project.GetRequiredProperty("AnalysisEnabled") == "true")
{
    // Perform analysis only if explicitly enabled
    var files = project.GetAllCSharpFiles();
    Console.WriteLine($"Analyzing {files.Count} C# files...");
}
```

### Example 2: Checking framework compatibility between projects
```csharp
var projectA = analysisContext.GetProject("ProjectA");
var projectB = analysisContext.GetProject("ProjectB");

if (projectA.HasSameTargetFramework(projectB))
{
    Console.WriteLine($"Projects share target framework: {projectA.GetTargetFrameworkDisplay()}");
    
    if (projectA.IsModernDotNetProject())
    {
        Console.WriteLine("Both projects use modern .NET features");
    }
}
```

## Notes

- **Thread Safety:** All extension methods are thread-safe as they only read project state and do not modify the `AnalysisProject` instance. The underlying collections (`Properties`, `SourceFiles`) are not modified by these methods.


- **Null Safety:** Every method validates its parameters using `ArgumentNullException.ThrowIfNull()` and will throw immediately if null values are provided, preventing null reference exceptions downstream.


- **Performance:** Methods like `GetAllCSharpFiles()` and `GetCSharpFileCount()` enumerate the source files collection. For repeated calls, consider caching the results if the project state doesn't change.


- **Modern .NET Detection:** `IsModernDotNetProject()` checks if the target framework starts with "net" and doesn't start with "netframework", correctly identifying net6.0, net7.0, net8.0, and future versions.

- **Missing Properties:** `GetRequiredProperty()` throws `KeyNotFoundException` with a descriptive message including both the property key and project name, making it easier to diagnose configuration issues.

- **Empty Target Framework:** `GetTargetFrameworkDisplay()` returns "Unknown" for null, empty, or whitespace target frameworks, providing a safe default for display purposes.