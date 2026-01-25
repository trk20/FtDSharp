using System.Collections.Generic;
using UnityEngine;

namespace FtDSharp
{
    /// <summary>
    /// Represents a script-controllable missile in flight.
    /// </summary>
    public interface IMissile
    {
        /// <summary>Unique missile ID.</summary>
        int Id { get; }
        /// <summary>Whether the missile is still valid (not detonated or destroyed).</summary>
        bool Valid { get; }
        /// <summary>Missile size.</summary>
        MissileSize Size { get; }
        /// <summary>Missile length in meters.</summary>
        float Length { get; }
        /// <summary>Time since launch in ticks.</summary>
        float TimeSinceLaunch { get; }
        /// <summary>Total fuel amount.</summary>
        float Fuel { get; }
        /// <summary>Fuel burn rate.</summary>
        float BurnRate { get; }
        /// <summary>Missile position in world coordinates.</summary>
        Vector3 Position { get; }
        /// <summary>Missile velocity in meters per second.</summary>
        Vector3 Velocity { get; }
        /// <summary>Missile thrust.</summary>
        float Thrust { get; }
        /// <summary>Missile rotation in world coordinates.</summary>
        Quaternion Rotation { get; }
        /// <summary>Missile forward direction in world coordinates.</summary>
        Vector3 Forward { get; }
        /// <summary>Launcher that fired this missile.</summary>
        IMissileLauncher Launcher { get; }
        /// <summary>All controllable parts on this missile.</summary>
        List<IMissilePart> Parts { get; }
        /// <summary>Detonates the missile.</summary>
        void Detonate();
        /// <summary>Aims the missile at the specified world coordinate point.</summary>
        /// <param name="aimPoint">World coordinate point to aim at.</param>
        void AimAt(Vector3 aimPoint);
        /// <summary>Sets the variable thrust fraction for variable thrusters or torpedo propellers.</summary>
        void SetVariableThrustFraction(float fraction);
    }
}
