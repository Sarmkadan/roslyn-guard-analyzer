using System;
using System.Collections.Generic;
using System.Linq;
using RoslynGuardAnalyzer.Data;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class RepositoryBaseTests
{
    // Simple entity used for testing.
    private sealed class Dummy
    {
        public string? Name { get; set; }
    }

    // Concrete repository implementation for the abstract base class.
    private sealed class DummyRepository : RepositoryBase<Dummy>
    {
        // No extra behaviour – inherits everything from RepositoryBase<T>.
    }

    private DummyRepository CreateRepository() => new DummyRepository();

    [Fact]
    public void Add_HappyPath_IncreasesCountAndStoresEntity()
    {
        var repo = CreateRepository();
        var entity = new Dummy { Name = "first" };

        repo.Add("id1", entity);

        Assert.Equal(1, repo.Count());
        Assert.True(repo.Exists("id1"));
        Assert.Same(entity, repo.GetById("id1"));
    }

    [Fact]
    public void Add_DuplicateId_ThrowsInvalidOperationException()
    {
        var repo = CreateRepository();
        repo.Add("dup", new Dummy());

        var ex = Assert.Throws<InvalidOperationException>(() => repo.Add("dup", new Dummy()));
        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public void Update_ExistingId_UpdatesEntity()
    {
        var repo = CreateRepository();
        repo.Add("u1", new Dummy { Name = "old" });

        var newEntity = new Dummy { Name = "new" };
        repo.Update("u1", newEntity);

        Assert.Same(newEntity, repo.GetById("u1"));
    }

    [Fact]
    public void Update_NonExistingId_ThrowsKeyNotFoundException()
    {
        var repo = CreateRepository();

        var ex = Assert.Throws<KeyNotFoundException>(() => repo.Update("missing", new Dummy()));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void Remove_ExistingId_ReturnsTrueAndRemoves()
    {
        var repo = CreateRepository();
        repo.Add("r1", new Dummy());

        var removed = repo.Remove("r1");

        Assert.True(removed);
        Assert.False(repo.Exists("r1"));
        Assert.Equal(0, repo.Count());
    }

    [Fact]
    public void Remove_NonExistingId_ReturnsFalse()
    {
        var repo = CreateRepository();

        var removed = repo.Remove("does-not-exist");

        Assert.False(removed);
    }

    [Fact]
    public void AddRange_HappyPath_AddsAllAndCountMatches()
    {
        var repo = CreateRepository();
        var batch = new Dictionary<string, Dummy>
        {
            ["a"] = new Dummy { Name = "A" },
            ["b"] = new Dummy { Name = "B" },
            ["c"] = new Dummy { Name = "C" }
        };

        repo.AddRange(batch);

        Assert.Equal(3, repo.Count());
        Assert.True(repo.Exists("a"));
        Assert.True(repo.Exists("b"));
        Assert.True(repo.Exists("c"));
    }

    [Fact]
    public void Find_Predicate_ReturnsMatchingEntities()
    {
        var repo = CreateRepository();
        repo.Add("1", new Dummy { Name = "alpha" });
        repo.Add("2", new Dummy { Name = "beta" });
        repo.Add("3", new Dummy { Name = "alphabet" });

        // Find all names containing "alpha"
        var results = repo.Find(d => d.Name != null && d.Name.Contains("alpha")).ToList();

        Assert.Equal(2, results.Count);
        Assert.All(results, d => Assert.Contains("alpha", d.Name!));
    }

    [Fact]
    public void GetById_NullOrWhiteSpace_ReturnsNull()
    {
        var repo = CreateRepository();
        repo.Add("valid", new Dummy());

        Assert.Null(repo.GetById(null!));
        Assert.Null(repo.GetById(string.Empty));
        Assert.Null(repo.GetById("   "));
    }

    [Fact]
    public void Clear_RemovesAllEntities()
    {
        var repo = CreateRepository();
        repo.Add("x", new Dummy());
        repo.Add("y", new Dummy());

        repo.Clear();

        Assert.Equal(0, repo.Count());
        Assert.Empty(repo.GetAll());
    }

    [Fact]
    public void Add_NullId_ThrowsArgumentException()
    {
        var repo = CreateRepository();
        var ex = Assert.Throws<ArgumentException>(() => repo.Add(null!, new Dummy()));
        Assert.Equal("id", ex.ParamName);
    }

    [Fact]
    public void Add_NullEntity_ThrowsArgumentNullException()
    {
        var repo = CreateRepository();
        Assert.Throws<ArgumentNullException>(() => repo.Add("id", null!));
    }

    [Fact]
    public void Find_NullPredicate_ThrowsArgumentNullException()
    {
        var repo = CreateRepository();
        Assert.Throws<ArgumentNullException>(() => repo.Find(null!));
    }
}
