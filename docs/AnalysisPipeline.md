# AnalysisPipeline

`AnalysisPipeline` is a builder-style class for constructing and executing a sequence of diagnostic analyzers in Roslyn. It allows chaining analyzer handlers, configuring execution behavior, and retrieving a human-readable description of the configured pipeline.

## API

### `AnalysisPipeline Use(DiagnosticAnalyzer analyzer)`

Adds a Roslyn diagnostic analyzer to the pipeline.

- **Parameters**
  - `analyzer` – The `DiagnosticAnalyzer` instance to include in the pipeline.
- **Return Value**
  - Returns the current `AnalysisPipeline` instance for method chaining.
- **Exceptions**
  - Throws `ArgumentNullException` if `analyzer` is `null`.

---

### `AnalysisPipeline UseHandler(DiagnosticHandler handler)`

Registers a handler to process diagnostics produced by analyzers in the pipeline.

- **Parameters**
  - `handler` – The `DiagnosticHandler` instance responsible for handling diagnostics.
- **Return Value**
  - Returns the current `AnalysisPipeline` instance for method chaining.
- **Exceptions**
  - Throws `ArgumentNullException` if `handler` is `null`.

---

### `async Task ExecuteAsync(Compilation compilation, CancellationToken cancellationToken = default)`

Executes the configured pipeline against the provided compilation.

- **Parameters**
  - `compilation` – The `Compilation` to analyze.
  - `cancellationToken` – Optional token to monitor for cancellation.
- **Return Value**
  - Returns a `Task` representing the asynchronous analysis.
- **Exceptions**
  - Throws `ArgumentNullException` if `compilation` is `null`.
  - Throws `OperationCanceledException` if `cancellationToken` is triggered.

---
### `string GetChainDescription()`

Generates a human-readable description of the current pipeline configuration.

- **Return Value**
  - Returns a `string` describing the sequence of analyzers and handlers.
- **Exceptions**
  - Never throws.

## Usage

### Basic Usage with Default Handler
