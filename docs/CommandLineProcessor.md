# CommandLineProcessor
The `CommandLineProcessor` type is designed to handle command-line arguments and options, providing a structured way to process and validate input. It offers methods to process command-line input, retrieve options, validate paths, and print a summary of available options.

## API
* `public CommandLineProcessor`: The constructor for the `CommandLineProcessor` type, initializing a new instance.
* `public (bool Success, CliOptions Options, List<string> Errors) Process`: Processes the command-line input, returning a tuple containing a success indicator, the parsed options, and a list of errors encountered during processing.
* `public CliOptions GetOptions`: Retrieves the parsed command-line options.
* `public (bool Valid, List<string> Errors) ValidatePaths`: Validates the paths provided in the command-line input, returning a tuple containing a validity indicator and a list of errors encountered during validation.
* `public void PrintOptionsSummary`: Prints a summary of the available command-line options.

## Usage
The following examples demonstrate how to use the `CommandLineProcessor` type:
```csharp
// Example 1: Basic usage
var processor = new CommandLineProcessor();
var (success, options, errors) = processor.Process();
if (success)
{
    Console.WriteLine("Options:");
    foreach (var option in options)
    {
        Console.WriteLine(option);
    }
}
else
{
    Console.WriteLine("Errors:");
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}

// Example 2: Validating paths
var processor2 = new CommandLineProcessor();
var (valid, errors) = processor2.ValidatePaths();
if (valid)
{
    Console.WriteLine("Paths are valid.");
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

## Notes
When using the `CommandLineProcessor` type, consider the following edge cases and thread-safety remarks:
* The `Process` method may throw exceptions if the command-line input is malformed or cannot be parsed.
* The `ValidatePaths` method may return false if the provided paths are invalid or do not exist.
* The `PrintOptionsSummary` method is thread-safe, but the `Process` and `ValidatePaths` methods are not, as they rely on instance state. Therefore, it is recommended to create a new instance of `CommandLineProcessor` for each thread or use synchronization mechanisms to ensure thread safety.
