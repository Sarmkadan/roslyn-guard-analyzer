# CustomAnalysisRuleExtensions
The `CustomAnalysisRuleExtensions` class provides a set of extension methods for creating and customizing analysis rules in the roslyn-guard-analyzer project. These methods enable developers to define rules with specific attributes, namespaces, and message formats, as well as to target particular code elements and complexity levels.

## API
The following members are part of the `CustomAnalysisRuleExtensions` class:
* `WithAttribute`: Returns a `CustomRuleBuilder` instance with the specified attribute. Parameters: the attribute to apply. Return value: a `CustomRuleBuilder` instance. Throws: no exceptions are specified.
* `WithNamespace`: Returns a `CustomRuleBuilder` instance with the specified namespace. Parameters: the namespace to apply. Return value: a `CustomRuleBuilder` instance. Throws: no exceptions are specified.
* `WithLocationAwareMessage`: Returns a `CustomRuleBuilder` instance with a location-aware message. Parameters: none. Return value: a `CustomRuleBuilder` instance. Throws: no exceptions are specified.
* `ForContainerElements`: Returns a `CustomRuleBuilder` instance targeting container elements. Parameters: none. Return value: a `CustomRuleBuilder` instance. Throws: no exceptions are specified.
* `GetViolationPredicate`: Returns a predicate function to determine if a code element is a violation. Parameters: a `CodeElement` instance. Return value: a boolean indicating whether the element is a violation. Throws: no exceptions are specified.
* `GetMessageFactory`: Returns a factory function to generate a message for a code element. Parameters: a `CodeElement` instance. Return value: a string message. Throws: no exceptions are specified.
* `WithMaxComplexity`: Returns a `CustomRuleBuilder` instance with a maximum complexity level. Parameters: the maximum complexity level. Return value: a `CustomRuleBuilder` instance. Throws: no exceptions are specified.
* `ForPublicNonStaticMembers`: Returns a `CustomRuleBuilder` instance targeting public non-static members. Parameters: none. Return value: a `CustomRuleBuilder` instance. Throws: no exceptions are specified.

## Usage
Here are two examples of using the `CustomAnalysisRuleExtensions` class:
```csharp
// Example 1: Create a rule with a specific attribute and namespace
var rule = CustomAnalysisRuleExtensions.WithAttribute("MyAttribute")
    .WithNamespace("MyNamespace")
    .WithLocationAwareMessage()
    .ForContainerElements()
    .Build();

// Example 2: Create a rule with a maximum complexity level and targeting public non-static members
var rule2 = CustomAnalysisRuleExtensions.WithMaxComplexity(10)
    .ForPublicNonStaticMembers()
    .WithAttribute("MyAttribute2")
    .Build();
```

## Notes
When using the `CustomAnalysisRuleExtensions` class, consider the following edge cases:
* The `WithAttribute` and `WithNamespace` methods can be chained to apply multiple attributes and namespaces to a rule.
* The `GetViolationPredicate` and `GetMessageFactory` methods can be used to customize the behavior of a rule.
* The `WithMaxComplexity` method can be used to limit the complexity of code elements targeted by a rule.
* The `ForContainerElements` and `ForPublicNonStaticMembers` methods can be used to target specific types of code elements.
* The `CustomAnalysisRuleExtensions` class is thread-safe, as it only provides static methods and does not maintain any instance state. However, the `CustomRuleBuilder` instances returned by these methods may not be thread-safe, depending on their implementation.
