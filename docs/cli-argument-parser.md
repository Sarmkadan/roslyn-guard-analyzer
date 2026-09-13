# CliArgumentParser

Parses command-line arguments into a `CliOptions` object using a state machine approach. Handles flags, options, positional arguments, and response file expansion with security limits.

## API

### `public CliArgumentParser(string[] args)`

Initializes a new instance of the argument parser.

**Parameters**
- `args`: The command-line arguments to parse. If `null`, treated as empty array.

### `public CliOptions Parse()`

Parses the command-line arguments and returns a `CliOptions` object.

**Returns**
- A `CliOptions` instance populated with parsed values.

**Exceptions**
- `ArgumentException`: Thrown when argument processing fails due to security limits being exceeded (response file recursion, total argument length, or expanded argument count).

### `public static CliOptions ParseSafe(string[] args)`

Parses arguments with exception handling, useful for CLI entry points.

**Parameters**
- `args`: The command-line arguments to parse.

**Returns**
- Parsed `CliOptions`, or default options with help shown on error.

**Remarks**
- On parsing errors, writes error message to standard error and returns options with `ShowHelp = true`.

## Supported Arguments

### Help and Version
| Flag | Description |
|------|-------------|
| `-h`, `--help` | Shows help information and exits |
| `-v`, `--version` | Shows version information and exits |

### Target Selection
| Flag | Format | Description |
|------|--------|-------------|
| `--project` | `--project=<path>` or `--project <path>` | Path to project file (.csproj) or solution file (.sln) to analyze |
| `--file` | `--file=<path>` or `--file <path>` | Path to single source file (.cs) to analyze |
| *(positional)* | <path> | Treated as project path if not starting with `-` and no project/file specified |

### Output Configuration
| Flag | Format | Description |
|------|--------|-------------|
| `--output` | `--output=<path>` or `--output <path>` | File path for analysis results (defaults to stdout) |
| `--format` | `--format=<format>` or `--format <format>` | Output format: `text`, `json`, `csv`, `html`, `xml`, `sarif` (default: `text`) |
| `--report-type` | `--report-type=<type>` or `--report-type <type>` | Report type for generated reports: `text`, `json`, `html`, `md` (default: `summary`) |
| `--no-report` | (flag) | Disables report generation |
| `--verbose` | (flag) | Enables verbose logging |

### Analysis Behavior
| Flag | Format | Description |
|------|--------|-------------|
| `--skip-cache` | (flag) | Bypasses cached analysis results |
| `--timeout` | `--timeout=<seconds>` or `--timeout <seconds>` | Analysis timeout in seconds (default: 300, 0 = no timeout) |
| `--threads` | `--threads=<count>` or `--threads <count>` | Maximum parallel threads (default: processor count) |
| `--log-level` | `--log-level=<level>` or `--log-level <level>` | Log level from 0 (silent) to 4 (debug, default: 2) |
| `--config` | `--config=<path>` or `--config <path>` | Path to configuration file (.editorconfig, JSON, or XML) |
| `--no-fail-on-violations` | (flag) | Exits with code 0 even if violations are found |
| `--rule-filter` | `--rule-filter=<filters>` or `--rule-filter <filters>` | Comma-separated list of rule IDs/categories to include |

### Baseline Options
| Flag | Format | Description |
|------|--------|-------------|
| *(in CliOptions but not parsed here)* | | `BaselineFile` and `CreateBaseline` are set via other mechanisms |

## Response File Support

The parser supports response files using the `@filename` syntax:

- `@response.txt` - Expands contents of response.txt as additional arguments
- Response files can contain additional `@file` references (recursive expansion)
- One argument per line in response files
- Lines starting with `#` or `//` are treated as comments and ignored
- Empty lines are ignored

### Security Limits

To prevent denial-of-service attacks, the following limits are enforced:

| Limit | Value | Purpose |
|-------|-------|---------|
| Max response file recursion depth | 50 | Prevents infinite loops via circular `@file` references |
| Max response file size | 1,000,000 bytes (1MB) | Prevents memory exhaustion via large files |
| Max total argument length | 1,000,000 bytes (1MB) | Prevents excessive memory usage after expansion |
| Max expanded arguments | 10,000 | Prevents excessive processing from glob expansion |

## Examples

### Basic Usage
```bash
# Analyze a project
dotnet RoslynGuardAnalyzer.dll --project ./MyProject.sln

# Analyze a single file
dotnet RoslynGuardAnalyzer.dll --file ./src/Program.cs

# Show help
dotnet RoslynGuardAnalyzer.dll --help
```

### With Output Options
```bash
# Write JSON report to file
dotnet RoslynGuardAnalyzer.dll --project ./MyProject.sln --format json --output ./results.json

# Generate HTML report
dotnet RoslynGuardAnalyzer.dll --project ./MyProject.sln --report-type html --output ./report.html

# Enable verbose logging
dotnet RoslynGuardAnalyzer.dll --project ./MyProject.sln --verbose
```

### Response Files
```bash
# Create response file args.txt containing:
# --project=./MyProject.sln
# --format=json
# --output=results.txt

# Use response file
dotnet RoslynGuardAnalyzer.dll @args.txt

# Nested response files supported (within depth limit)
```

### Filtering and Configuration
```bash
# Analyze with specific rules only
dotnet RoslynGuardAnalyzer.dll --project ./MyProject.sln --rule-filter RG0001,RG0002,Security

# Use custom configuration
dotnet RoslynGuardAnalyzer.dll --project ./MyProject.sln --config ./custom.ruleset

# Increase timeout for large projects
dotnet RoslynGuardAnalyzer.dll --project ./LargeSolution.sln --timeout 600

# Limit parallel threads on CI agents
dotnet RoslynGuardAnalyzer.dll --project ./MyProject.sln --threads 2
```