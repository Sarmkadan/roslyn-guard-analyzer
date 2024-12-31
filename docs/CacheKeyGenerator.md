# CacheKeyGenerator

Provides deterministic string keys used for caching analysis results, rule executions, and other artifacts within the Roslyn‑guard analyzer. The methods combine input values into stable identifiers and optionally compute hash‑based representations to ensure uniform length and collision resistance.

## API

### `public static string GenerateProjectAnalysisKey(string projectFilePath)`
Generates a cache key that uniquely identifies an analysis of a whole project.  
- **Parameters**  
  - `projectFilePath`: Full path to the project file (`.csproj`).  
- **Return value**  
  - A string key derived from the project file path.  
- **Exceptions**  
  - `ArgumentNullException` if `projectFilePath` is `null`.  
  - `ArgumentException` if `projectFilePath` is empty or whitespace.  

### `public static string GenerateFileAnalysisKey(string filePath)`
Generates a cache key for the analysis of a single source file.  
- **Parameters**  
  - `filePath`: Full path to the source file.  
- **Return value**  
  - A string key based on the file path.  
- **Exceptions**  
  - `ArgumentNullException` if `filePath` is `null`.  
  - `ArgumentException` if `filePath` is empty or whitespace.  

### `public static string GenerateResultKey(string analyzerId, string ruleId, string locationIdentifier)`
Creates a key that identifies a specific diagnostic result produced by a rule.  
- **Parameters**  
  - `analyzerId`: The identifier of the analyzer that produced the result.  
  - `ruleId`: The identifier of the rule within the analyzer.  
  - `locationIdentifier`: A string representing the source location (e.g., file path plus line/column).  
- **Return value**  
  - A composite key encoding the analyzer, rule, and location.  
- **Exceptions**  
  - `ArgumentNullException` if any parameter is `null`.  
  - `ArgumentException` if any parameter is empty or whitespace.  

### `public static string GenerateRuleExecutionKey(string analyzerId, string ruleId)`
Produces a key used to cache the execution state of a particular rule.  
- **Parameters**  
  - `analyzerId`: Identifier of the analyzer.  
  - `ruleId`: Identifier of the rule.  
- **Return value**  
  - A string key representing the rule execution context.  
- **Exceptions**  
  - `ArgumentNullException` if either parameter is `null`.  
  - `ArgumentException` if either parameter is empty or whitespace.  

### `public static string GenerateCodeElementKey(string symbolName, string containingNamespace)`
Generates a key for a code element (type, method, property, etc.) based on its symbolic name.  
- **Parameters**  
  - `symbolName`: The name of the symbol (e.g., method name).  
  - `containingNamespace`: The namespace that contains the symbol.  
- **Return value**  
  - A key uniquely identifying the code element within its namespace.  
- **Exceptions**  
  - `ArgumentNullException` if either parameter is `null`.  
  - `ArgumentException` if either parameter is empty or whitespace.  

### `public static string ComputeHash(string input)`
Computes a stable hash (e.g., SHA‑256) of the supplied string and returns it as a hexadecimal string.  
- **Parameters**  
  - `input`: The string to hash.  
- **Return value**  
  - A lowercase hexadecimal string representing the hash.  
- **Exceptions**  
  - `ArgumentNullException` if `input` is `null`.  

### `public static string ComputeFileHash(string filePath)`
Computes a hash of the file’s contents and returns it as a hexadecimal string.  
- **Parameters**  
  - `filePath`: Path to the file to be hashed.  
- **Return value**  
  - A lowercase hexadecimal string representing the file’s content hash.  
- **Exceptions**  
  - `ArgumentNullException` if `filePath` is `null`.  
  - `ArgumentException` if `filePath` is empty or whitespace.  
  - `FileNotFoundException` if the file does not exist.  
  - `IOException` for other I/O errors (e.g., insufficient permissions).  

### `public static string CreateCompositeKey(params string[] parts)`
Combines an arbitrary number of string fragments into a single cache key using a deterministic separator.  
- **Parameters**  
  - `parts`: One or more string fragments to be combined.  
- **Return value**  
  - A string where each fragment is separated by a reserved delimiter (e.g., `|`).  
- **Exceptions**  
  - `ArgumentNullException` if `parts` is `null` or any element within `parts` is `null`.  
  - `ArgumentException` if any element is empty or whitespace.  

### `public static string GeneratePatternKey(string pattern)`
Creates a key that represents a specific analysis pattern (e.g., a regex or syntactic pattern) for caching pattern‑matching results.  
- **Parameters**  
  - `pattern`: The pattern definition string.  
- **Return value**  
  - A hashed representation of the pattern, suitable for use as a cache key.  
- **Exceptions**  
  - `ArgumentNullException` if `pattern` is `null`.  
  - `ArgumentException` if `pattern` is empty or whitespace.  

## Usage

```csharp
using RoslynGuard.Analyzer.Caching;

// Example 1: Creating a key for a file‑level analysis and storing a result.
string filePath = @"C:\Projects\MyApp\Program.cs";
string fileKey = CacheKeyGenerator.GenerateFileAnalysisKey(filePath);

// Suppose we have already computed some analysis result for this file.
object analysisResult = GetAnalysisResult(filePath);

// Store in a dictionary‑based cache.
var cache = new Dictionary<string, object>();
cache[fileKey] = analysisResult;
```

```csharp
using RoslynGuard.Analyzer.Caching;

// Example 2: Generating a rule‑execution key and using a hash for large inputs.
string analyzerId = "RoslynGuard.Design";
string ruleId     = "RG001";
string location   = $"filepath:{filePath}|line:42|col:10";

string ruleExecKey = CacheKeyGenerator.GenerateRuleExecutionKey(analyzerId, ruleId);
string resultKey   = CacheKeyGenerator.GenerateResultKey(analyzerId, ruleId, location);

// For a large source snippet, compute a hash to keep the key size manageable.
string largeSnippet = File.ReadAllText(filePath);
string snippetHash  = CacheKeyGenerator.ComputeHash(largeSnippet);
string composite    = CacheKeyGenerator.CreateCompositeKey(ruleExecKey, resultKey, snippetHash);

// Use `composite` as the cache entry key for the rule’s output on this location.
```

## Notes

- All methods are **static** and rely solely on their input parameters; they contain no mutable state and are therefore thread‑safe for concurrent calls.  
- The hash‑based methods (`ComputeHash`, `ComputeFileHash`) produce deterministic output for identical inputs, but changing the underlying file after `ComputeFileHash` has been called will invalidate any previously cached keys that depend on that hash.  
- `CreateCompositeKey` uses a reserved separator that is guaranteed not to appear in properly escaped input fragments; supplying a fragment that contains the separator may lead to ambiguous keys. Callers should ensure fragments are sanitized or avoid using the separator character.  
- Passing `null`, empty, or whitespace‑only strings to any method will result in an `ArgumentException` or `ArgumentNullException` as documented; defensive validation is recommended when keys are constructed from external sources.  
- The generated keys are intended for use as dictionary keys or in similar lookup structures; they are not guaranteed to be secure cryptographic identifiers, only collision‑resistant enough for caching purposes within the analyzer’s scope.
