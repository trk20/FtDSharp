using System;

namespace FtDSharp.Helpers;

/// <summary>
/// Per-frame lazy cache that resets automatically each game tick.
/// The cached value is computed once per frame and reused until the next frame.
/// </summary>
/// <typeparam name="T">The type of value to cache.</typeparam>
internal sealed class FrameCache<T>
{
    private readonly Func<T> factory;
    private long frame = -1;
    private T? value;

    public FrameCache(Func<T> factory) => this.factory = factory;

    /// <summary>
    /// Gets the cached value, recomputing it if this is the first access this frame.
    /// </summary>
    public T Value
    {
        get
        {
            var currentFrame = Game.TicksSinceStart;
            if (frame != currentFrame)
            {
                frame = currentFrame;
                value = factory();
            }
            return value!;
        }
    }
}
