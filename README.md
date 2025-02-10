# Roslyn Guard Analyzer

...

## HelpGenerator

The `HelpGenerator` class is responsible for generating help text for the CLI application. It provides methods for generating full help text, brief help text, version information, error messages, and usage summaries.

### Usage Example
```csharp
var fullHelp = HelpGenerator.GenerateFullHelp();
Console.WriteLine(fullHelp);

var briefHelp = HelpGenerator.GenerateBriefHelp();
Console.WriteLine(briefHelp);

var version = HelpGenerator.GenerateVersion();
Console.WriteLine(version);

var errorMessage = HelpGenerator.GenerateErrorMessage("Invalid option");
Console.WriteLine(errorMessage);

var usageSummary = HelpGenerator.GenerateUsageSummary();
Console.WriteLine(usageSummary);
```

## CliOptions

The `CliOptions` class represents parsed command-line options for the analyzer. It provides a way to validate and access the options before use.

### Usage Example
```csharp
var options = new CliOptions();
options.ProjectPath = "/path/to/project";
options.OutputFormat = "json";
options.Verbose = true;
options.MaxParallelThreads = 4;

if (options.Validate(out var errors))
{
    Console.WriteLine("Options are valid");
    Console.WriteLine(options.ToString());
}
else
{
    Console.WriteLine("Errors:");
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}
```

...
