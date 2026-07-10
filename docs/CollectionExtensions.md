# CollectionExtensions

A utility class providing extension methods for working with collections, lists, and enumerables in a functional and null-safe manner. These methods simplify common operations such as batching, filtering, partitioning, and null checks while maintaining readability and performance.

## API

### `Batch<T>`
Partitions an enumerable into batches of a specified size.

- **Parameters**:
  - `source` – The enumerable to partition.
  - `batchSize` – The maximum number of items per batch (must be positive).
- **Returns**: An enumerable of lists, each containing up to `batchSize` items from `source`.
- **Throws**: `ArgumentOutOfRangeException` if `batchSize` is less than 1.

### `DistinctBy<T, TKey>`
Returns distinct elements from a sequence based on a key selector function.

- **Parameters**:
  - `source` – The sequence to filter.
  - `keySelector` – A function to extract the key for each element.
- **Returns**: An enumerable containing only the first occurrence of each distinct key.
- **Throws**: `ArgumentNullException` if `source` or `keySelector` is `null`.

### `AddIfNotNull<T>`
Adds an item to a list only if the item is not `null`.

- **Parameters**:
  - `list` – The target list (modified in place).
  - `item` – The item to add.
- **Throws**: `ArgumentNullException` if `list` is `null`.

### `AddRangeIfNotNull<T>`
Adds a range of items to a list, skipping any `null` items.

- **Parameters**:
  - `list` – The target list (modified in place).
  - `items` – The items to add.
- **Throws**: `ArgumentNullException` if `list` or `items` is `null`.

### `IsNullOrEmpty<T>`
Determines whether a collection is `null` or empty.

- **Parameters**:
  - `collection` – The collection to check.
- **Returns**: `true` if `collection` is `null` or has no elements; otherwise, `false`.

### `OrEmpty<T>`
Returns an empty enumerable if the source is `null`; otherwise, returns the source.

- **Parameters**:
  - `source` – The source enumerable.
- **Returns**: An enumerable that is either the source or an empty enumerable.

### `WithIndex<T>`
Enumerates a sequence with the index of each element.

- **Parameters**:
  - `source` – The sequence to enumerate.
- **Returns**: An enumerable of tuples where each tuple contains the index and the corresponding item.
- **Throws**: `ArgumentNullException` if `source` is `null`.

### `FirstOrNull<T>`
Returns the first element of a sequence or `null` if the sequence is empty or `null`.

- **Parameters**:
  - `source` – The sequence to query.
- **Returns**: The first element, or `null` if the sequence is empty or `null`.
- **Throws**: `ArgumentNullException` if `source` is `null`.

### `ForEach<T>`
Applies an action to each element of a sequence.

- **Parameters**:
  - `source` – The sequence to iterate.
  - `action` – The action to apply to each element.
- **Throws**: `ArgumentNullException` if `source` or `action` is `null`.

### `Partition<T>`
Splits a sequence into two lists based on a predicate.

- **Parameters**:
  - `source` – The sequence to partition.
  - `predicate` – The function to test each element.
- **Returns**: A tuple of two lists: the first containing elements where `predicate` is `true`, the second where it is `false`.
- **Throws**: `ArgumentNullException` if `source` or `predicate` is `null`.

### `Flatten<T>`
Flattens a sequence of sequences into a single sequence.

- **Parameters**:
  - `source` – The sequence of sequences to flatten.
- **Returns**: An enumerable containing all elements from all inner sequences in order.
- **Throws**: `ArgumentNullException` if `source` is `null`.

### `TakeWhile<T>`
Returns elements from a sequence until the specified predicate returns `false`.

- **Parameters**:
  - `source` – The sequence to take from.
  - `predicate` – The function to test each element.
- **Returns**: An enumerable containing elements from `source` up to the first element that does not satisfy `predicate`.
- **Throws**: `ArgumentNullException` if `source` or `predicate` is `null`.

### `GetMode<T>`
Returns the mode (most frequently occurring element) of a sequence.

- **Parameters**:
  - `source` – The sequence to analyze.
- **Returns**: The most frequent element, or `null` if the sequence is empty or all elements are equally frequent.
- **Throws**: `ArgumentNullException` if `source` is `null`.

## Usage
