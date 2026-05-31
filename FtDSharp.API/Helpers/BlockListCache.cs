using System;
using System.Collections.Generic;

namespace FtDSharp.Helpers;

/// <summary>
/// Cached block list that rebuilds when the source count or owning construct changes.
/// Reuses its internal list allocation to minimize GC pressure.
/// </summary>
internal sealed class BlockListCache<T>
{
    private readonly Func<int> getCount;
    private readonly Func<object?> getOwner;
    private readonly Action<List<T>> populate;
    private List<T>? list;
    private int lastCount = -1;
    private object? lastOwner;

    public BlockListCache(Func<int> getCount, Func<object?> getOwner, Action<List<T>> populate)
    {
        this.getCount = getCount;
        this.getOwner = getOwner;
        this.populate = populate;
    }

    public IReadOnlyList<T> Value
    {
        get
        {
            var count = getCount();
            var owner = getOwner();
            if (count < 0 || owner == null)
            {
                list?.Clear();
                lastCount = -1;
                lastOwner = null;
                return Array.Empty<T>();
            }

            if (list != null && count == lastCount && ReferenceEquals(owner, lastOwner))
                return list;

            lastCount = count;
            lastOwner = owner;
            if (list == null)
                list = new List<T>(count);
            else
                list.Clear();

            populate(list);
            return list;
        }
    }

    public void Invalidate()
    {
        lastCount = -1;
        lastOwner = null;
    }
}