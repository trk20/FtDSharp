using System.Collections.Generic;
using System.Linq;
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
        /// <summary>Launcher that fired this missile, or null if unavailable.</summary>
        IMissileLauncher? Launcher { get; }
        /// <summary>All controllable parts on this missile.</summary>
        List<IMissilePart> Parts { get; }
        /// <summary>Gets all parts of the specified type.</summary>
        IEnumerable<T> GetParts<T>() where T : class, IMissilePart;
        /// <summary>Gets the first part of the specified type, or null if none found.</summary>
        T? GetPart<T>() where T : class, IMissilePart;
        /// <summary>Returns true if the missile has at least one part of the specified type.</summary>
        bool HasPart<T>() where T : class, IMissilePart;
        /// <summary>Detonates the missile.</summary>
        void Detonate();
        /// <summary>Aims the missile at the specified world coordinate point.</summary>
        /// <param name="aimPoint">World coordinate point to aim at.</param>
        void AimAt(Vector3 aimPoint);
        /// <summary>Sets the variable thrust fraction for variable thrusters or torpedo propellers.</summary>
        void SetVariableThrustFraction(float fraction);

        /// <summary>Trail/smoke visual effects. Automatically dispatches based on trail type.</summary>
        IMissileTrail Trail { get; }
        /// <summary>Flame visual effects.</summary>
        IMissileFlame Flame { get; }
        /// <summary>Engine light visual effects.</summary>
        IMissileEngineLight EngineLight { get; }
    }

    /// <summary>Controls trail/smoke visual effects. Automatically dispatches to smoke or ion trail based on variant.</summary>
    public interface IMissileTrail
    {
        /// <summary>Trail type (Smoke or Ion). Read-only, determined at launch.</summary>
        TrailType Variant { get; }
        /// <summary>Trail/smoke color. Writes to smoke or ion trail color based on Variant.</summary>
        Color Color { set; }
        /// <summary>Trail/smoke enabled. Enables/disables smoke or ion trail based on Variant.</summary>
        bool Enabled { set; }
    }

    /// <summary>Controls flame visual effects.</summary>
    public interface IMissileFlame
    {
        /// <summary>Flame color.</summary>
        Color Color { set; }
        /// <summary>Flame enabled.</summary>
        bool Enabled { set; }
    }

    /// <summary>Controls engine light visual effects.</summary>
    public interface IMissileEngineLight
    {
        /// <summary>Engine light color.</summary>
        Color Color { set; }
        /// <summary>Engine light enabled.</summary>
        bool Enabled { set; }
    }
}
