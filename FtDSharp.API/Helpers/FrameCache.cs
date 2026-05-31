using System;

namespace FtDSharp.Helpers;

/// <summary>
/// Per-frame lazy cache that resets automatically each game tick or owner change.
/// The cached value is computed once per frame and reused until the next frame.
/// </summary>
/// <typeparam name="T">The type of value to cache.</typeparam>
internal sealed class FrameCache<T>
{
    private readonly Func<T> factory;
    private readonly Func<object?>? getOwner;
    private long frame = -1;
    private object? owner;
    private T? value;

    public FrameCache(Func<T> factory) => this.factory = factory;

    public FrameCache(Func<T> factory, Func<object?> getOwner)
    {
        this.factory = factory;
        this.getOwner = getOwner;
    }

    /// <summary>
    /// Gets the cached value, recomputing it if this is the first access this frame.
    /// </summary>
    public T Value
    {
        get
        {
            var currentFrame = Game.TicksSinceStart;
            var currentOwner = getOwner?.Invoke();
            if (frame != currentFrame || !ReferenceEquals(owner, currentOwner))
            {
                frame = currentFrame;
                owner = currentOwner;
                value = factory();
            }
            return value!;
        }
    }
}
