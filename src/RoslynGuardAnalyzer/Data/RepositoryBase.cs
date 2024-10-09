// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace RoslynGuardAnalyzer.Data;

/// <summary>
/// Base repository class providing common CRUD operations.
/// </summary>
/// <typeparam name="T">The entity type managed by this repository.</typeparam>
public abstract class RepositoryBase<T> where T : class
{
    protected readonly Dictionary<string, T> _storage = new();
    protected readonly object _lockObject = new();

    /// <summary>
    /// Adds an entity to the repository.
    /// </summary>
    public virtual void Add(string id, T entity)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be null or empty.", nameof(id));

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        lock (_lockObject)
        {
            if (_storage.ContainsKey(id))
                throw new InvalidOperationException($"Entity with ID '{id}' already exists.");

            _storage[id] = entity;
        }
    }

    /// <summary>
    /// Retrieves an entity by ID.
    /// </summary>
    public virtual T? GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        lock (_lockObject)
        {
            _storage.TryGetValue(id, out var entity);
            return entity;
        }
    }

    /// <summary>
    /// Gets all entities in the repository.
    /// </summary>
    public virtual IReadOnlyList<T> GetAll()
    {
        lock (_lockObject)
        {
            return _storage.Values.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    public virtual void Update(string id, T entity)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be null or empty.", nameof(id));

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        lock (_lockObject)
        {
            if (!_storage.ContainsKey(id))
                throw new KeyNotFoundException($"Entity with ID '{id}' not found.");

            _storage[id] = entity;
        }
    }

    /// <summary>
    /// Removes an entity by ID.
    /// </summary>
    public virtual bool Remove(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        lock (_lockObject)
        {
            return _storage.Remove(id);
        }
    }

    /// <summary>
    /// Checks if an entity exists.
    /// </summary>
    public virtual bool Exists(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        lock (_lockObject)
        {
            return _storage.ContainsKey(id);
        }
    }

    /// <summary>
    /// Gets the count of entities in the repository.
    /// </summary>
    public virtual int Count()
    {
        lock (_lockObject)
        {
            return _storage.Count;
        }
    }

    /// <summary>
    /// Clears all entities from the repository.
    /// </summary>
    public virtual void Clear()
    {
        lock (_lockObject)
        {
            _storage.Clear();
        }
    }

    /// <summary>
    /// Adds multiple entities at once.
    /// </summary>
    public virtual void AddRange(Dictionary<string, T> entities)
    {
        if (entities == null || !entities.Any())
            return;

        lock (_lockObject)
        {
            foreach (var kvp in entities)
            {
                if (_storage.ContainsKey(kvp.Key))
                    throw new InvalidOperationException($"Entity with ID '{kvp.Key}' already exists.");

                _storage[kvp.Key] = kvp.Value;
            }
        }
    }

    /// <summary>
    /// Finds entities matching a predicate.
    /// </summary>
    public virtual IReadOnlyList<T> Find(Func<T, bool> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        lock (_lockObject)
        {
            return _storage.Values.Where(predicate).ToList().AsReadOnly();
        }
    }
}
