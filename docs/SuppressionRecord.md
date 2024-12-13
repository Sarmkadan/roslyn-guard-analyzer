# SuppressionRecord

A `SuppressionRecord` represents a single suppression entry that can be used to temporarily disable or exempt specific diagnostics in the Roslyn Guard Analyzer. It captures metadata about the suppression, including its identifier, rule, target scope, justification, validity period, and authoring details.

## API

### `Id`
- **Purpose**: A unique identifier for the suppression record.
- **Type**: `string`
- **Notes**: Must be non-null and unique within the scope of suppression management.

### `RuleId`
- **Purpose**: The diagnostic rule ID that this suppression applies to (e.g., "RG0001").
- **Type**: `string`
- **Notes**: Must be non-null. Used to match against diagnostics during analysis.

### `TargetFile`
- **Purpose**: The file path or identifier to which the suppression applies.
- **Type**: `string?`
- **Notes**: Optional. If `null`, the suppression applies globally or to all files matching `TargetElement`. Must be a valid file path if specified.

### `TargetElement`
- **Purpose**: A symbolic or syntactic element (e.g., method name, type name) to which the suppression applies.
- **Type**: `string?`
- **Notes**: Optional. If `null`, the suppression applies to the entire file specified by `TargetFile` (or globally if `TargetFile` is also `null`). Must be non-empty if specified.

### `Justification`
- **Purpose**: The rationale for applying the suppression.
- **Type**: `string`
- **Notes**: Must be non-null and non-empty. Used for auditing and documentation.

### `CreatedAt`
- **Purpose**: The timestamp when the suppression record was created.
- **Type**: `DateTime`
- **Notes**: Automatically set on creation. Immutable after instantiation.

### `ExpiresAt`
- **Purpose**: The optional expiration date/time for the suppression.
- **Type**: `DateTime?`
- **Notes**: If `null`, the suppression does not expire. Otherwise, the suppression is considered invalid after this point.

### `Author`
- **Purpose**: The identifier or name of the person or system that created the suppression.
- **Type**: `string`
- **Notes**: Must be non-null and non-empty. Used for accountability.

### `IsActive`
- **Purpose**: Indicates whether the suppression is currently active and should be applied during analysis.
- **Type**: `bool`
- **Notes**: Can be toggled to enable or disable the suppression without removing it.

### `Matches`
- **Purpose**: Determines whether the suppression applies to a given diagnostic based on rule ID, target file, and target element.
- **Type**: `bool`
- **Parameters**:
  - `ruleId`: The diagnostic rule ID to match.
  - `targetFile`: The file path where the diagnostic occurred.
  - `targetElement`: The symbolic element where the diagnostic occurred.
- **Return Value**: `true` if the suppression matches the diagnostic context; otherwise, `false`.
- **Notes**: Returns `false` if `IsActive` is `false`. Does not throw.

## Usage

### Example 1: Creating and Applying a Suppression
