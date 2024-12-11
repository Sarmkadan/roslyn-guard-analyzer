# HelpGenerator

The `HelpGenerator` class provides static utility methods for generating formatted help text, version information, error messages, and usage summaries for analyzer diagnostics. These methods are designed to produce consistent, localized, and machine-readable help content for Roslyn-based diagnostic analyzers.

## API

### `GenerateFullHelp`

Generates a comprehensive help document for a diagnostic analyzer, including title, description, message formats, and usage examples.

- **Parameters**
  - `diagnosticId` (string): The unique identifier of the diagnostic (e.g., "RG0001").
  - `title` (string): A human-readable title for the diagnostic.
  - `description` (string): A detailed explanation of the diagnostic's purpose and behavior.
  - `messageFormat` (string): The format string used to generate the primary diagnostic message.
  - `helpLinkUri` (string, optional): A URI linking to additional documentation.

- **Returns**
  - `string`: A formatted markdown document containing full help text.

- **Exceptions**
  - Throws `ArgumentNullException` if `diagnosticId`, `title`, `description`, or `messageFormat` is `null`.

---

### `GenerateBriefHelp`

Generates a concise help snippet suitable for inline tooltips or IDE quick info.

- **Parameters**
  - `diagnosticId` (string): The unique identifier of the diagnostic.
  - `title` (string): A brief title for the diagnostic.
  - `description` (string): A short explanation of the diagnostic's purpose.

- **Returns**
  - `string`: A compact markdown-formatted help snippet.

- **Exceptions**
  - Throws `ArgumentNullException` if `diagnosticId`, `title`, or `description` is `null`.

---

### `GenerateVersion`

Generates version information for the analyzer, including assembly version and source repository details.

- **Parameters**
  - `assemblyVersion` (string): The version of the analyzer assembly (e.g., "1.2.3").
  - `repositoryUrl` (string, optional): The URL of the source repository.

- **Returns**
  - `string`: A formatted version string with analyzer and runtime information.

- **Exceptions**
  - Throws `ArgumentNullException` if `assemblyVersion` is `null`.

---

### `GenerateErrorMessage`

Generates a user-facing error message for a diagnostic, incorporating the message format and optional arguments.

- **Parameters**
  - `messageFormat` (string): The format string used to construct the message.
  - `args` (params object[]): Optional arguments to format into the message.

- **Returns**
  - `string`: The formatted error message.

- **Exceptions**
  - Throws `ArgumentNullException` if `messageFormat` is `null`.
  - Throws `FormatException` if formatting fails due to invalid or mismatched arguments.

---
### `GenerateUsageSummary`

Generates a concise usage summary for a diagnostic, including code examples and explanations.

- **Parameters**
  - `diagnosticId` (string): The unique identifier of the diagnostic.
  - `title` (string): A brief title for the diagnostic.
  - `exampleCode` (string): A code snippet demonstrating correct usage or the issue.
  - `explanation` (string): A brief explanation of the example.

- **Returns**
  - `string`: A markdown-formatted usage summary.

- **Exceptions**
  - Throws `ArgumentNullException` if `diagnosticId`, `title`, `exampleCode`, or `explanation` is `null`.

## Usage
