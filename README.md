# Roslyn Guard Analyzer

...

## CommandLineProcessor

The `CommandLineProcessor` class is a high-level CLI command processor that orchestrates parsing and validation of command-line arguments. It acts as a facade for CLI argument parsing and help text generation.

### Usage Example
```csharp
var processor = new CommandLineProcessor(new[] { "--help" });
var (success, options, errors) = processor.Process();
if (success)
{
    processor.PrintOptionsSummary();
}
else
{
    foreach (var error in errors)
    {
        Console.Error.WriteLine($"  - {error}");
    }
}
```

...
