using System.Collections.Generic;

namespace FtDSharp
{
    /// <summary>
    /// Interface for turrets that can coordinate multiple weapons.
    /// Turrets are weapons that also have weapons mounted on them.
    /// </summary>
    public interface ITurret : IWeapon
    {
        /// <summary>
        /// Weapons mounted on this turret and any nested subobjects.
        /// </summary>
        IReadOnlyList<IWeapon> Weapons { get; }

        /// <summary>
        /// Current azimuth (horizontal) angle in degrees.
        /// </summary>
        float Azimuth { get; }

        /// <summary>
        /// Current elevation (vertical) angle in degrees.
        /// </summary>
        float Elevation { get; }

        // --- Aggregate state properties from mounted weapons ---

        /// <summary>
        /// True if any mounted weapon is on target (from last Track/AimAt this frame).
        /// </summary>
        bool AnyOnTarget { get; }

        /// <summary>
        /// True if all mounted weapons are on target (from last Track/AimAt this frame).
        /// </summary>
        bool AllOnTarget { get; }

        /// <summary>
        /// True if any mounted weapon is ready to fire.
        /// </summary>
        bool AnyReady { get; }

        /// <summary>
        /// True if all mounted weapons are ready to fire.
        /// </summary>
        bool AllReady { get; }

        /// <summary>
        /// True if any mounted weapon can fire (on target AND ready).
        /// </summary>
        bool AnyCanFire { get; }

        /// <summary>
        /// True if all mounted weapons can fire (on target AND ready).
        /// </summary>
        bool AllCanFire { get; }
    }
}
