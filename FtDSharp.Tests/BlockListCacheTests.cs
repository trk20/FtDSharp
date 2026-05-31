using System;
using System.Collections.Generic;
using System.Linq;
using FtDSharp.Helpers;
using Xunit;

namespace FtDSharp.Tests;

public class BlockListCacheTests
{
    [Fact]
    public void Value_FirstAccess_PopulatesAndReturnsItems()
    {
        var state = new CacheState<int>
        {
            Count = 3,
            Owner = new object(),
            Items = { 1, 2, 3 }
        };

        var cache = state.CreateCache();

        var value = cache.Value;

        Assert.Equal(1, state.PopulateCalls);
        Assert.Equal(new[] { 1, 2, 3 }, value);
    }

    [Fact]
    public void Value_SameCountAndOwner_ReusesCachedListWithoutRepopulating()
    {
        var state = new CacheState<int>
        {
            Count = 2,
            Owner = new object(),
            Items = { 4, 5 }
        };

        var cache = state.CreateCache();

        var first = cache.Value;
        var second = cache.Value;

        Assert.Equal(1, state.PopulateCalls);
        Assert.Same(first, second);
        Assert.Equal(new[] { 4, 5 }, second);
    }

    [Fact]
    public void Value_CountChange_RebuildsList()
    {
        var state = new CacheState<int>
        {
            Count = 2,
            Owner = new object(),
            Items = { 1, 2 }
        };

        var cache = state.CreateCache();

        var first = cache.Value;

        state.Count = 3;
        state.Items.Clear();
        state.Items.AddRange(new[] { 7, 8, 9 });

        var second = cache.Value;

        Assert.Equal(2, state.PopulateCalls);
        Assert.Same(first, second);
        Assert.Equal(new[] { 7, 8, 9 }, second);
    }

    [Fact]
    public void Value_OwnerReferenceChange_RebuildsList()
    {
        var state = new CacheState<int>
        {
            Count = 1,
            Owner = new object(),
            Items = { 10 }
        };

        var cache = state.CreateCache();

        var first = cache.Value;

        state.Owner = new object();
        state.Items.Clear();
        state.Items.Add(20);

        var second = cache.Value;

        Assert.Equal(2, state.PopulateCalls);
        Assert.Same(first, second);
        Assert.Equal(new[] { 20 }, second);
    }

    [Fact]
    public void Value_NegativeCount_ReturnsEmptyArray()
    {
        var state = new CacheState<int>
        {
            Count = -1,
            Owner = new object(),
            Items = { 1, 2, 3 }
        };

        var cache = state.CreateCache();

        var first = cache.Value;
        var second = cache.Value;

        Assert.Empty(first);
        Assert.Same(Array.Empty<int>(), first);
        Assert.Same(first, second);
        Assert.Equal(0, state.PopulateCalls);
    }

    [Fact]
    public void Value_NullOwner_ReturnsEmptyArray()
    {
        var state = new CacheState<int>
        {
            Count = 2,
            Owner = null,
            Items = { 1, 2 }
        };

        var cache = state.CreateCache();

        var first = cache.Value;
        var second = cache.Value;

        Assert.Empty(first);
        Assert.Same(Array.Empty<int>(), first);
        Assert.Same(first, second);
        Assert.Equal(0, state.PopulateCalls);
    }

    [Fact]
    public void Value_AfterNullOwnerThenValidState_Repopulates()
    {
        var state = new CacheState<int>
        {
            Count = 2,
            Owner = new object(),
            Items = { 1, 2 }
        };

        var cache = state.CreateCache();

        var initial = cache.Value;
        var initialSnapshot = initial.ToArray();

        state.Owner = null;
        var empty = cache.Value;

        state.Owner = new object();
        state.Items.Clear();
        state.Items.AddRange(new[] { 8, 9 });

        var repopulated = cache.Value;

        Assert.Equal(2, state.PopulateCalls);
    Assert.Equal(new[] { 1, 2 }, initialSnapshot);
        Assert.Empty(empty);
        Assert.Same(Array.Empty<int>(), empty);
        Assert.Same(initial, repopulated);
        Assert.Equal(new[] { 8, 9 }, repopulated);
    }

    [Fact]
    public void Invalidate_ForcesNextAccessToRebuild()
    {
        var state = new CacheState<int>
        {
            Count = 2,
            Owner = new object(),
            Items = { 3, 4 }
        };

        var cache = state.CreateCache();

        var first = cache.Value;

        state.Items.Clear();
        state.Items.AddRange(new[] { 30, 40 });
        cache.Invalidate();

        var second = cache.Value;

        Assert.Equal(2, state.PopulateCalls);
        Assert.Same(first, second);
        Assert.Equal(new[] { 30, 40 }, second);
    }

    [Fact]
    public void Invalidate_ReusesListInstanceOnRebuild()
    {
        var state = new CacheState<int>
        {
            Count = 1,
            Owner = new object(),
            Items = { 5 }
        };

        var cache = state.CreateCache();

        var first = cache.Value;

        state.Items.Clear();
        state.Items.Add(6);
        cache.Invalidate();

        var second = cache.Value;

        Assert.Equal(2, state.PopulateCalls);
        Assert.Same(first, second);
        Assert.Equal(new[] { 6 }, second);
    }

    private sealed class CacheState<T>
    {
        public int Count { get; set; }
        public object? Owner { get; set; }
        public List<T> Items { get; } = new();
        public int PopulateCalls { get; private set; }

        public BlockListCache<T> CreateCache()
        {
            return new BlockListCache<T>(
                () => Count,
                () => Owner,
                list =>
                {
                    PopulateCalls++;
                    list.AddRange(Items);
                });
        }
    }
}