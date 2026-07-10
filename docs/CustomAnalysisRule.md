# CustomAnalysisRule

A builder-style type used to define custom static analysis rules for C# code elements. It allows configuring predicates, violation messages, severity levels, and descriptions through a fluent interface, culminating in a `CustomAnalysisRule` instance that can be registered with an analyzer.

## API

### `public CustomAnalysisRule`
The immutable result of a completed rule configuration via `CustomRuleBuilder`. Instances are produced by `Build` and encapsulate the configured predicate, message factory, severity, and description. Each instance is thread-safe once constructed.

### `public Func<CodeElement, bool> ViolationPredicate`
Gets the predicate that determines whether a given `CodeElement` constitutes a violation. Returning `true` indicates a violation; `false` indicates compliance. This predicate is evaluated during analysis and must be deterministic for consistent results.

### `public Func<CodeElement, string> MessageFactory`
Gets the factory function that generates the diagnostic message for a violation. The function receives the violating `CodeElement` and returns a non-null, non-empty string describing the issue. The returned message is used verbatim in diagnostics.

### `public static CustomRuleBuilder Create()`
Initializes a new `CustomRuleBuilder` for fluent rule configuration. This is the entry point for defining a custom analysis rule. Returns a builder in its initial state, ready for method chaining.

### `public CustomRuleBuilder For(Func<CodeElement, bool> predicate)`
Sets the predicate that identifies violations. The provided function is stored for later evaluation during analysis. The predicate must not capture mutable state that could change between invocations, to ensure deterministic behavior. Returns the builder for chaining.

### `public CustomRuleBuilder WithSeverity(DiagnosticSeverity severity)`
Assigns the severity level for violations detected by this rule. The severity influences how diagnostics are reported in IDEs and build outputs. Returns the builder for chaining.

### `public CustomRuleBuilder WithDescription(string description)`
Provides a human-readable description of the rule’s purpose. This text may be surfaced in documentation or IDE tooltips. The description must be non-null and non-empty. Returns the builder for chaining.

### `public CustomRuleBuilder When(Func<bool> condition)`
Conditionally enables or disables the rule based on a runtime condition. The condition is evaluated once during rule construction; if `false`, the rule is effectively inactive. Returns the builder for chaining.

### `public CustomRuleBuilder WithMessage(Func<CodeElement, string> factory)`
Sets the factory that generates violation messages. The factory receives the violating `CodeElement` and returns the diagnostic message. The message must be non-null and non-empty. Returns the builder for chaining.

### `public CustomAnalysisRule Build()`
Finalizes the configuration and produces an immutable `CustomAnalysisRule` instance. After calling this method, the builder must not be reused. The returned rule is ready for registration with an analyzer and is safe for concurrent access.

## Usage
