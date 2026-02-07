using System;

namespace FtDSharp.Helpers;

/// <summary>
/// Cache that rebuilds its value only when the dirty-check function indicates changes.
/// Used for caching block lists that only change during building or separator operations.
/// </summary>
/// <typeparam name="T">The type of value to cache.</typeparam>
internal sealed class DirtyCache<T>
{
    private readonly Func<T> factory;
    private readonly Func<bool> isDirty;
    private T? value;
    private bool initialized;

    public DirtyCache(Func<T> factory, Func<bool> isDirty)
    {
        this.factory = factory;
        this.isDirty = isDirty;
    }

    /// <summary>
    /// Gets the cached value, recomputing it only if the dirty-check indicates changes.
    /// </summary>
    public T Value
    {
        get
        {
            if (!initialized || isDirty())
            {
                value = factory();
                initialized = true;
            }
            return value!;
        }
    }

    /// <summary>
    /// Forces the cache to be invalidated on the next access.
    /// </summary>
    public void Invalidate() => initialized = false;
}
