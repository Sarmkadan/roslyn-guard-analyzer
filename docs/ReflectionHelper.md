# ReflectionHelper
ReflectionHelper is a static utility that provides convenient reflection‑based queries for types, members, and attributes without requiring boilerplate code.

## API
### GetPublicMethods
- **Purpose**: Returns an enumeration of `MethodInfo` objects representing the public methods available on the reflected target.
- **Parameters**: None.
- **Return Value**: `IEnumerable<MethodInfo>` containing the public methods; empty if none exist.
- **Throws**: May throw a `NullReferenceException` if the internal target type cannot be resolved; may throw an `InvalidOperationException` if reflection fails for the target is not accessible.

### GetPublicProperties
- **Purpose**: Returns an enumeration of `PropertyInfo` objects representing the public properties available on the reflected target.
- **Parameters**: None.
- **Return Value**: `IEnumerable<PropertyInfo>` containing the public properties; empty if none exist.
- **Throws**: May throw a `NullReferenceException` if the internal target type cannot be resolved; may throw an `InvalidOperationException` if reflection fails.

### GetPublicFields
- **Purpose**: Returns an enumeration of `FieldInfo` objects representing the public fields available on the reflected target.
- **Parameters**: None.
- **Return Value**: `IEnumerable<FieldInfo>` containing the public fields; empty if none exist.
- **Throws**: May throw a `NullReferenceException` if the internal target type cannot be resolved; may throw an `InvalidOperationException` if reflection fails.

### ImplementsInterface
- **Purpose**: Indicates whether the reflected target type implements a specific interface (the interface is determined by the helper’s internal configuration).
- **Parameters**: None.
- **Return Value**: `true` if the target implements the interface; otherwise `false`.
- **Throws**: May throw a `NullReferenceException` if the internal target type cannot be resolved.

### IsSubclassOf
- **Purpose**: Indicates whether the reflected target type is a subclass of a specific base type (the base type is determined by the helper’s internal configuration).
- **Parameters**: None.
- **Return Value**: `true` if the target inherits from the base type; otherwise `false`.
- **Throws**: May throw a `NullReferenceException` if the internal target type cannot be resolved.

### GetAttributes<T>
- **Purpose**: Returns an enumeration of attributes of type `T` applied to the reflected target.
- **Parameters**: None.
- **Return Value**: `IEnumerable<T>` containing the matching attributes; empty if none are present.
- **Throws**: May throw an `ArgumentException` if `T` is not an attribute type; may throw a `NullReferenceException` if the internal target type cannot be resolved.

### IsAsync
- **Purpose**: Indicates whether a specific method (determined by the helper’s internal configuration) is marked with the `async` modifier.
- **Parameters**: None.
- **Return Value**: `true` if the method is asynchronous; otherwise `false`.
- **Throws**: May throw a `NullReferenceException` if the internal target method cannot be resolved.

### IsVirtual
- **Purpose**: Indicates whether a specific method (determined by the helper’s internal configuration) is declared as `virtual`.
- **Parameters**: None.
- **Return Value**: `true` if the method is virtual; otherwise `false`.
- **Throws**: May throw a `NullReferenceException` if the internal target method cannot be resolved.

### GetParameterCount
- **Purpose**: Returns the number of parameters of a specific method (determined by the helper’s internal configuration).
- **Parameters**: None.
- **Return Value**: An `int` representing the parameter count.
- **Throws**: May throw a `NullReferenceException` if the internal target method cannot be resolved.

### GetParameterNames
- **Purpose**: Returns the names of the parameters of a specific method (determined by the helper’s internal configuration).
- **Parameters**: None.
- **Return Value**: `IEnumerable<string>` containing the parameter names; empty if the method has no parameters.
- **Throws**: May throw a `NullReferenceException` if the internal target method cannot be resolved.

### GetImplementationsOfInterface
- **Purpose**: Returns an enumeration of `Type` objects that implement a specific interface (the interface is determined by the helper’s internal configuration).
- **Parameters**: None.
- **Return Value**: `IEnumerable<Type>` containing the implementing types; empty if none exist.
- **Throws**: May throw a `NullReferenceException` if the internal interface type cannot be resolved.

### GetTypesWithAttribute<T>
- **Purpose**: Returns an enumeration of `Type` objects that are decorated with a specific attribute type `T`.
- **Parameters**: None.
- **Return Value**: `IEnumerable<Type>` containing the types bearing the attribute; empty if none exist.
- **Throws**: May throw an `ArgumentException` if `T` is not an attribute type; may throw a `NullReferenceException` if the internal assembly scan fails.

### GetFullName
- **Purpose**: Returns the fully qualified name of the reflected target type.
- **Parameters**: None.
- **Return Value**: A `string` containing the full name; `null` if the target has no name.
- **Throws**: May throw a `NullReferenceException` if the internal target type cannot be resolved.

### IsValueType
- **Purpose**: Indicates whether the reflected target type is a value type.
- **Parameters**: None.
- **Return Value**: `true` if the target is a value type; otherwise `false`.
- **Throws**: May throw a `NullReferenceException` if the internal target type cannot be resolved.

### IsAbstract
- **Purpose**: Indicates whether the reflected target type is abstract.
- **Parameters**: None.
- **Return Value**: `true` if the target is abstract; otherwise `false`.
- **Throws**: May throw a `NullReferenceException` if the internal target type cannot be resolved.

### IsSealed
- **Purpose**: Indicates whether the reflected target type is sealed.
- **Parameters**: None.
- **Return Value**: `true` if the target is sealed; otherwise `false`.
- **Throws**: May throw a `NullReferenceException` if the internal target type cannot be resolved.

### GetBaseType
- **Purpose**: Returns the direct base type of the reflected target type.
- **Parameters**: None.
- **Return Value**: The base `Type`, or `null` if the target has no base (e.g., `object` or an interface).
- **Throws**: May throw a `NullReferenceException` if the internal target type cannot be resolved.

### GetInheritanceHierarchy
- **Purpose**: Returns an enumeration of `Type` objects representing the inheritance chain of the reflected target, starting from the target itself and proceeding up through base types.
- **Parameters**: None.
- **Return Value**: `IEnumerable<Type>` containing the hierarchy; empty if the target has no inheritance chain.
- **Throws**: May throw a `NullReferenceException` if the internal target type cannot be resolved.

## Usage
```csharp
// Example 1: Enumerate all public methods of the type reflected by ReflectionHelper.
foreach (var method in ReflectionHelper.GetPublicMethods)
{
    Console.WriteLine(method.Name);
}

// Example 2: Check whether the reflected method is asynchronous and virtual.
if (ReflectionHelper.IsAsync && ReflectionHelper.IsVirtual)
{
    Console.WriteLine("The method is async and can be overridden.");
}
```

## Notes
- All members are static and do not modify internal state; therefore they are thread‑safe for concurrent invocation.
- If the helper’s internal target type or member cannot be resolved (e.g., due to a `null` reference), most members will throw a `NullReferenceException`. Consumers should ensure the helper is properly initialized before use.
- Enumerating members (`GetPublicMethods`, `GetPublicProperties`, `GetPublicFields`, `GetImplementationsOfInterface`, `GetTypesWithAttribute<T>`, `GetInheritanceHierarchy`) will return an empty sequence when no matching items exist rather than throwing.
- Generic members (`GetAttributes<T>`, `GetTypesWithAttribute<T>`) constrain `T` to attribute types; supplying a non‑attribute type results in an `ArgumentException`.
- The boolean members (`ImplementsInterface`, `IsSubclassOf`, `IsAsync`, `IsVirtual`, `IsValueType`, `IsAbstract`, `IsSealed`) reflect a predetermined configuration within the helper; their meaning depends on how the helper was set up, which is not visible from the signatures alone.
