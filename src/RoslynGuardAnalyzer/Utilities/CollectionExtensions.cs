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
    public static IEnumerable<IList<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

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
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        return source.GroupBy(keySelector).Select(g => g.First());
    }

    /// <summary>
    /// Safely adds an item to a collection if it's not null.
    /// </summary>
    public static void AddIfNotNull<T>(this ICollection<T> collection, T? item) where T : class
    {
        if (item != null)
            collection.Add(item);
    }

    /// <summary>
    /// Safely adds all items from an enumerable to a collection, skipping null items.
    /// </summary>
    public static void AddRangeIfNotNull<T>(this ICollection<T> collection, IEnumerable<T>? items) where T : class
    {
        if (items != null)
        {
            foreach (var item in items.Where(i => i != null))
                collection.Add(item);
        }
    }

    /// <summary>
    /// Checks if a collection is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }

    /// <summary>
    /// Returns the source if it's not null or empty, otherwise returns empty enumerable.
    /// </summary>
    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? source)
    {
        return source ?? [];
    }

    /// <summary>
    /// Iterates over items and their indices in a single operation.
    /// </summary>
    public static IEnumerable<(int Index, T Item)> WithIndex<T>(this IEnumerable<T> source)
    {
        return source.Select((item, index) => (index, item));
    }

    /// <summary>
    /// Finds the first element that satisfies a predicate, returning null if not found.
    /// </summary>
    public static T? FirstOrNull<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : class
    {
        return source.FirstOrDefault(predicate);
    }

    /// <summary>
    /// Performs an action on each item in a collection (side effect operation).
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
            action(item);
    }

    /// <summary>
    /// Performs an action on each item along with its index.
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        int index = 0;
        foreach (var item in source)
            action(item, index++);
    }

    /// <summary>
    /// Partitions a sequence into two collections based on a predicate.
    /// </summary>
    public static (List<T> True, List<T> False) Partition<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
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
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
    {
        return source.SelectMany(x => x);
    }

    /// <summary>
    /// Takes items from a collection until a condition is false.
    /// </summary>
    public static IEnumerable<T> TakeWhile<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
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
    public static T? GetMode<T>(this IEnumerable<T> source) where T : class
    {
        return source
            .GroupBy(x => x)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();
    }
}
