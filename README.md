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

...
