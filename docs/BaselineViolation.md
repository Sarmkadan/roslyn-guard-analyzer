# BaselineViolation

Represents a recorded diagnostic violation that has been baseline-approved, used to track and validate suppressions in the Roslyn Guard Analyzer system.

## API

### Properties

- **`string Id`**  
  Unique identifier for the baseline violation entry.

- **`string RuleId`**  
  Identifier of the associated analysis rule that triggered the original violation.

- **`string FilePath`**  
  Full file path where the violation was detected.

- **`int LineNumber`**  
  Source code line number where the violation occurred.

- **`string ContentHash`**  
  Cryptographic hash of the violation's content for integrity verification.

- **`DateTime CreatedAt`**  
  Timestamp when the baseline violation was recorded.

- **`string? Description`**  
  Optional human-readable description of the violation context.

- **`string Version`**  
  Version string of the baseline entry format or rule configuration.

- **`string ProjectName`**  
  Name of the project containing the violating file.

- **`DateTime BaselineCreatedAt`**  
  Timestamp when the baseline file was initially created.

### Constructors

- **`public BaselineViolation`**  
  Parameterless constructor for creating empty instances.

- **`public BaselineViolation`**  
  Copy constructor for duplicating existing baseline violation entries.

### Methods

- **`public static BaselineViolation FromRuleViolation(RuleViolation violation)`**  
  Creates a `BaselineViolation` instance from a `RuleViolation` object.  
  *Parameters:*  
  - `violation` (RuleViolation): The source violation to convert.  
  *Returns:*  
  - A new `BaselineViolation` populated with data from the input violation.

- **`public static string ComputeContentHash(string content)`**  
  Generates a cryptographic hash string for the provided content.  
  *Parameters:*  
  - `content` (string): The text content to hash.  
  *Returns:*  
  - A SHA-256 hash string representing the input content.

- **`public bool Matches(BaselineViolation other)`**  
  Determines whether this violation matches another based on file path, line number, and rule ID.  
  *Parameters:*  
  - `other` (BaselineViolation): The violation to compare against.  
  *Returns:*  
  - `true` if all matching criteria are satisfied; otherwise `false`.

- **`public bool IsValid`**  
  Indicates whether the violation has valid required fields (`Id`, `RuleId`, `FilePath`, `LineNumber`).  
  *Returns:*  
  - `true` if all required properties are non-null and properly formatted; otherwise `false`.

- **`public bool Equals(BaselineViolation other)`**  
  Compares this violation to another for equality using all property values.  
  *Parameters:*  
  - `other` (BaselineViolation): The violation to compare.  
  *Returns:*  
  - `true` if all properties match; otherwise `false`.

- **`public override bool Equals(object obj)`**  
  Standard equality comparison override.  
  *Parameters:*  
  - `obj` (object): The object to compare.  
  *Returns:*  
  - `true` if the object is a `BaselineViolation` with matching property values.

- **`public override int GetHashCode()`**  
  Hash code implementation for use in hash-based collections.  
  *Returns:*  
  - Integer hash code derived from property values.

- **`public override string ToString()`**  
  String representation of the violation including key identifying fields.  
  *Returns:*  
  - Formatted string containing `Id`, `RuleId`, and `FilePath`.

## Usage

### Example 1: Creating BaselineViolation from RuleViolation

```csharp
var ruleViolation = new RuleViolation(
    ruleId: "RG1001",
    filePath: @"C:\Projects\App\Program.cs",
    lineNumber: 42,
    description: "Possible null reference"
);

var baselineEntry = BaselineViolation.FromRuleViolation(ruleViolation);
baselineEntry.Id = Guid.NewGuid().ToString();
baselineEntry.CreatedAt = DateTime.UtcNow;

Console.WriteLine($"Baseline entry created: {baselineEntry.Id}");
```

### Example 2: Validating and Matching Violations

```csharp
var storedViolation = new BaselineViolation {
    RuleId = "RG1002",
    FilePath = "Service.cs",
    LineNumber = 15,
    ContentHash = BaselineViolation.ComputeContentHash("var x = null;")
};

var currentViolation = new BaselineViolation {
    RuleId = "RG1002",
    FilePath = "Service.cs",
    LineNumber = 15,
    ContentHash = BaselineViolation.ComputeContentHash("var x = null;")
};

if (storedViolation.Matches(currentViolation) && storedViolation.IsValid) {
    Console.WriteLine("Violation matches baseline and is valid");
}
```

## Notes

- The `Description` property is nullable and may be omitted in serialized forms.
- `ContentHash` uses SHA-256 hashing; changes to source content will invalidate matches.
- Instances should be treated as immutable after creation to ensure consistent behavior in hash-based collections.
- Thread-safety is not guaranteed for shared instances; external synchronization is required for concurrent access.
- `FromRuleViolation` does not automatically populate `Id` or `CreatedAt`; these must be set explicitly.
- `Matches` performs strict comparison and does not account for whitespace or formatting differences in content.
