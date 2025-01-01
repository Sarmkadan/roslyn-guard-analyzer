# PathNormalizer

Provides a set of static helpers for working with file system paths in a platform‑agnostic, deterministic way. The methods normalize paths, compare them, extract components, and build new paths while handling the nuances of Windows and Unix separators, relative vs. absolute forms, and empty or malformed input.

## API

### Normalize(string path)

**Purpose**  
Returns a canonical representation of *path* by removing redundant directory separators, resolving `.` and `..` segments, and ensuring a consistent separator character for the current platform.

**Parameters**  
- `path`: The path to normalize. May be absolute or relative, and may contain mixed separators.

**Return value**  
A normalized string. If *path* is empty, returns an empty string. If *path* consists only of separator characters, returns a single separator.

**Exceptions**  
- `ArgumentNullException` if *path* is `null`.  
- `ArgumentException` if *path* contains invalid characters for the current platform.

### NormalizeMany(IEnumerable<string> paths)

**Purpose**  
Normalizes each element in *paths* and returns the results as a new array.

**Parameters**  
- `paths`: An enumerable of paths to normalize. May be empty; `null` elements are not allowed.

**Return value**  
A string array where each entry is the normalized form of the corresponding input element.

**Exceptions**  
- `ArgumentNullException` if *paths* is `null`.  
- `ArgumentNullException` if any element in *paths* is `null`.  
- `ArgumentException` if any element contains invalid path characters.

### ArePathsEqual(string path1, string path2)

**Purpose**  
Determines whether two paths refer to the same location after normalization. Comparison is case‑insensitive on Windows and case‑sensitive on other platforms.

**Parameters**  
- `path1`: First path to compare.  
- `path2`: Second path to compare.

**Return value**  
`true` if the normalized forms of *path1* and *path2* are equivalent; otherwise `false`.

**Exceptions**  
- `ArgumentNullException` if either *path1* or *path2* is `null`.  
- `ArgumentException` if either path contains invalid characters.

### GetRelativePath(string basePath, string targetPath)

**Purpose**  
Computes the relative path from *basePath* to *targetPath*. Both paths are normalized before the calculation.

**Parameters**  
- `basePath`: The starting point (must be a directory path).  
- `targetPath`: The destination path.

**Return value**  
A relative path that, when combined with *basePath*, yields *targetPath*. Returns an empty string if the two paths are identical after normalization.

**Exceptions**  
- `ArgumentNullException` if either argument is `null`.  
- `ArgumentException` if either path contains invalid characters or if *basePath* is not a valid directory (e.g., contains a file name without a trailing separator on platforms that require it).

### IsAbsolute(string path)

**Purpose**  
Indicates whether *path* is an absolute path (e.g., starts with a drive letter and colon on Windows, or begins with a separator on Unix).

**Parameters**  
- `path`: The path to test.

**Return value**  
`true` if *path* is absolute; otherwise `false`.

**Exceptions**  
- `ArgumentNullException` if *path* is `null`.  
- `ArgumentException` if *path* contains invalid characters.

### Combine(params string[] paths)

**Purpose**  
Combines one or more path fragments into a single path, inserting the appropriate separator between fragments and normalizing the result.

**Parameters**  
- `paths`: Path fragments to combine. At least one fragment must be supplied; fragments may be empty strings.

**Return value**  
A combined, normalized path string.

**Exceptions**  
- `ArgumentNullException` if *paths* is `null` or any element is `null`.  
- `ArgumentException` if the combined result would contain invalid characters.

### GetDirectoryName(string path)

**Purpose**  
Returns the directory portion of *path*. If *path* ends with a separator, the returned directory excludes that trailing separator.

**Parameters**  
- `path`: The path to query.

**Return value**  
The directory component, or `null` if *path* does not contain a directory part (e.g., a file name only). Returns an empty string if *path* consists solely of a separator.

**Exceptions**  
- `ArgumentNullException` if *path* is `null`.  
- `ArgumentException` if *path* contains invalid characters.

### GetFileName(string path)

**Purpose**  
Returns the file name and extension of *path*. If *path* ends with a separator, returns an empty string.

**Parameters**  
- `path`: The path to query.

**Return value**  
The file name component, or an empty string if *path* does not contain a file name.

**Exceptions**  
- `ArgumentNullException` if *path* is `null`.  
- `ArgumentException` if *path* contains invalid characters.

### GetExtension(string path)

**Purpose**  
Returns the extension (including the leading period) of the file name in *path*. Returns an empty string if there is no extension.

**Parameters**  
- `path`: The path to query.

**Return value**  
The extension string, or empty string.

**Exceptions**  
- `ArgumentNullException` if *path* is `null`.  
- `ArgumentException` if *path* contains invalid characters.

### HasExtension(string path)

**Purpose**  
Determines whether *path* includes a file name extension.

**Parameters**  
- `path`: The path to test.

**Return value**  
`true` if the file name component has an extension; otherwise `false`.

**Exceptions**  
- `ArgumentNullException` if *path* is `null`.  
- `ArgumentException` if *path* contains invalid characters.

## Usage

```csharp
using RoslynGuardAnalyzer; // namespace containing PathNormalizer

string raw = @"C:\Temp\..\..\Folder\\SubDir\\";
string normalized = PathNormalizer.Normalize(raw);
// normalized => "C:\Folder\SubDir"

bool same = PathNormalizer.ArePathsEqual(
    @"C:\Folder\SubDir\file.txt",
    @"c:\folder\subdir\FILE.TXT");
// same => true (case‑insensitive on Windows)

string[] paths = { @"C:\Temp\", @"..\Folder", @"SubDir\\" };
string[] normalizedArray = PathNormalizer.NormalizeMany(paths);
// normalizedArray => ["C:\Temp", "C:\Folder", "C:\Folder\SubDir"]
```

```csharp
string baseDir = @"D:\Projects\MyApp\src";
string targetFile = @"D:\Projects\MyApp\docs\readme.md";

string relative = PathNormalizer.GetRelativePath(baseDir, targetFile);
// relative => @"..\docs\readme.md"

bool isAbs = PathNormalizer.IsAbsolute(relative);
// isAbs => false

string combined = PathNormalizer.Combine(baseDir, "..", "docs", "readme.md");
// combined => @"D:\Projects\MyApp\docs\readme.md" (normalized)
```

## Notes

- All methods are **pure** and stateless; they rely only on their inputs and therefore are thread‑safe for concurrent use.
- Empty strings are tolerated where semantically meaningful (e.g., `Normalize("")` returns `""`, `GetFileName("")` returns `""`). Methods that require a non‑empty argument will throw `ArgumentException` if the string is empty after trimming.
- The implementation treats path separators (`/` and `\`) as equivalent on the current platform, but does **not** perform Unicode normalization or case folding beyond the platform‑specific rules described.
- When dealing with UNC paths (`\\server\share\...`) or device paths (`\\?\C:\...`), the methods preserve the leading separator sequence and apply the same normalization rules to the remainder.
- `GetDirectoryName` returns `null` for paths that consist solely of a file name (no directory component). Callers should check for `null` before using the result.
- `HasExtension` considers a trailing period with no following characters as **no** extension (returns `false`).  
- The methods do **not** validate that a path refers to an existing file or directory; they operate purely on string representation.
