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

## EmptyCatchBlockRuleTests

The EmptyCatchBlockRuleTests class contains tests for the EmptyCatchBlockRule. It ensures that empty catch blocks, which silently swallow exceptions, are flagged while catch blocks that handle or rethrow exceptions are left alone.

The following example demonstrates how to use the rule:
```csharp
public async Task EmptyCatchBlockRule_FlagsCatchBlockWithNoStatementsAndNoThrow
public async Task EmptyCatchBlockRule_DoesNotFlagCatchBlockWithThrow
public async Task EmptyCatchBlockRule_DoesNotFlagCatchBlockWithThrowNew
public async Task EmptyCatchBlockRule_DoesNotFlagCatchBlockWithLogging
public async Task EmptyCatchBlockRule_DoesNotFlagCatchBlockWithCommentOnly
public async Task EmptyCatchBlockRule_MessageContainsFixSuggestions
```

These tests verify that the EmptyCatchBlockRule correctly identifies and flags empty catch blocks and suggests appropriate fixes.
