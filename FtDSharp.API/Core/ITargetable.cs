using UnityEngine;

namespace FtDSharp
{
    /// <summary>
    /// Represents something that can be tracked/targeted - has position, velocity, and acceleration.
    /// </summary>
    public interface ITargetable
    {
        /// <summary>Current world position.</summary>
        Vector3 Position { get; }
        /// <summary>Current velocity vector.</summary>
        Vector3 Velocity { get; }
        /// <summary>Current acceleration vector.</summary>
        Vector3 Acceleration { get; }
    }
}
