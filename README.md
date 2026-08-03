## AsyncVoidWarningRuleTests

The AsyncVoidWarningRuleTests class contains tests for the AsyncVoidWarningRule.

The following example demonstrates how to use the rule:
```csharp
public async Task AsyncVoidWarningRule_FlagsAsyncVoidMethod
public async Task AsyncVoidWarningRule_DoesNotFlagNonVoidReturnType
public async Task AsyncVoidWarningRule_DoesNotFlagNonAsyncMethod
public async Task AsyncVoidWarningRule_DoesNotFlagEventHandlerMethod
public async Task AsyncVoidWarningRule_DoesNotFlagNonMethodElement
public async Task AsyncVoidWarningRule_MessageContainsLocation
```

These tests verify that the AsyncVoidWarningRule correctly identifies and flags async void methods.