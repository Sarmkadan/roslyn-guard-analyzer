#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace RoslynGuardAnalyzer.Utilities;

/// <summary>
/// Extension methods for collections and enumerables.
/// Provides batching, grouping, and null-safe operations.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Batches a sequence into smaller chunks of specified size.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence to batch.</param>
    /// <param name="batchSize">The maximum number of items per batch.</param>
    /// <returns>An enumerable of batches, each containing up to <paramref name="batchSize"/> items.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="batchSize"/> is less than or equal to 0.</exception>
    public static IEnumerable<IList<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be greater than 0", nameof(batchSize));

        var batch = new List<T>(batchSize);

        foreach (var item in source)
        {
            batch.Add(item);
            if (batch.Count == batchSize)
            {
                yield return batch;
                batch = new List<T>(batchSize);
            }
        }

        if (batch.Count > 0)
            yield return batch;
    }

    /// <summary>
    /// Returns distinct items from a sequence based on a key selector.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key used for distinct comparison.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <returns>An enumerable sequence containing only distinct elements based on the key selector.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is null.</exception>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        return source.GroupBy(keySelector).Select(g => g.First());
    }

    /// <summary>
    /// Safely adds an item to a collection if it's not null.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to add to.</param>
    /// <param name="item">The item to add (can be null).</param>
    public static void AddIfNotNull<T>(this ICollection<T> collection, T? item) where T : class
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (item is not null)
            collection.Add(item);
    }

    /// <summary>
    /// Safely adds all items from an enumerable to a collection, skipping null items.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to add to.</param>
    /// <param name="items">The items to add (can be null).</param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
    public static void AddRangeIfNotNull<T>(this ICollection<T> collection, IEnumerable<T>? items) where T : class
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (items is not null)
        {
            foreach (var item in items.Where(i => i is not null))
                collection.Add(item);
        }
    }

    /// <summary>
    /// Checks if a collection is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to check.</param>
    /// <returns>True if the collection is null or empty; otherwise, false.</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source is null || !source.Any();
    }

    /// <summary>
    /// Returns the source if it's not null or empty, otherwise returns empty enumerable.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence (can be null).</param>
    /// <returns>The original source if not null or empty; otherwise, an empty enumerable.</returns>
    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? source)
    {
        return source ?? [];
    }

    /// <summary>
    /// Iterates over items and their indices in a single operation.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <returns>An enumerable of tuples containing the index and item.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    public static IEnumerable<(int Index, T Item)> WithIndex<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select((item, index) => (index, item));
    }

    /// <summary>
    /// Finds the first element that satisfies a predicate, returning null if not found.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="predicate">The predicate to match elements against.</param>
    /// <returns>The first element that matches the predicate, or null if none found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
    public static T? FirstOrNull<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        return source.FirstOrDefault(predicate);
    }

    /// <summary>
    /// Performs an action on each item in a collection (side effect operation).
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The action to perform on each item.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="action"/> is null.</exception>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
            action(item);
    }

    /// <summary>
    /// Performs an action on each item along with its index.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The action to perform on each item and its index.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="action"/> is null.</exception>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        int index = 0;
        foreach (var item in source)
            action(item, index++);
    }

    /// <summary>
    /// Partitions a sequence into two collections based on a predicate.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence to partition.</param>
    /// <param name="predicate">The predicate to determine partitioning.</param>
    /// <returns>A tuple containing two lists: (True items, False items).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
    public static (List<T> True, List<T> False) Partition<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var trueList = new List<T>();
        var falseList = new List<T>();

        foreach (var item in source)
        {
            if (predicate(item))
                trueList.Add(item);
            else
                falseList.Add(item);
        }

        return (trueList, falseList);
    }

    /// <summary>
    /// Flattens a sequence of sequences into a single sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequences.</typeparam>
    /// <param name="source">The source sequence of sequences.</param>
    /// <returns>A flattened sequence containing all items from all inner sequences.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.SelectMany(x => x);
    }

    /// <summary>
    /// Takes items from a collection until a condition is false.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="predicate">The predicate that determines when to stop taking items.</param>
    /// <returns>An enumerable containing items from the source until the predicate returns false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
    public static IEnumerable<T> TakeWhile<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        foreach (var item in source)
        {
            if (!predicate(item))
                break;

            yield return item;
        }
    }

    /// <summary>
    /// Gets the mode (most common value) from a sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <returns>The most common element in the sequence, or null if the sequence is empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    public static T? GetMode<T>(this IEnumerable<T> source) where T : class
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .GroupBy(x => x)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();
    }
}
