using UnityEngine;

namespace FtDSharp
{
    /// <summary>
    /// Base interface for all controllable weapons.
    /// Extends IWeaponControl for unified aiming, tracking, and firing operations.
    /// </summary>
    public interface IWeapon : IBlock, IWeaponControl
    {
        /// <summary>The type of this weapon.</summary>
        WeaponType WeaponType { get; }

        /// <summary>The world-space direction the weapon is currently aimed.</summary>
        Vector3 AimDirection { get; }

        /// <summary>The weapon slot mask for this weapon.</summary>
        int SlotMask { get; }

        /// <summary>The projectile speed of this weapon, if applicable.</summary>
        float ProjectileSpeed { get; }

        /// <summary>
        /// Whether the weapon is ready to fire (has ammo, is reloaded, has energy, not cooling down, etc.).
        /// </summary>
        bool IsReady { get; } // Note: Future improvement - expose the specific reason why a weapon can't fire.

        // --- State properties from last Track/AimAt call this frame ---

        /// <summary>
        /// Whether the weapon is on target (from last Track/AimAt call this frame).
        /// Returns false if no aim/track has been performed this frame.
        /// </summary>
        bool OnTarget { get; }

        /// <summary>
        /// Whether the weapon can physically aim at the target (from last Track/AimAt call this frame).
        /// Returns false if no aim/track has been performed this frame.
        /// </summary>
        bool CanAim { get; }

        /// <summary>
        /// Whether the shot would be blocked (from last Track/AimAt call this frame).
        /// Returns false if no aim/track has been performed this frame.
        /// </summary>
        bool IsBlocked { get; }

        /// <summary>
        /// Whether the weapon can fire (OnTarget AND IsReady) (from last Track/AimAt call this frame).
        /// Returns false if no aim/track has been performed this frame.
        /// </summary>
        bool CanFire { get; }

        /// <summary>
        /// Flight time to target in seconds (from last Track call this frame).
        /// Returns 0 if no track has been performed this frame.
        /// </summary>
        float FlightTime { get; }

        /// <summary>
        /// Calculated aim point position (from last Track call this frame).
        /// Returns Vector3.zero if no track has been performed this frame.
        /// </summary>
        Vector3 AimPoint { get; }

        /// <summary>
        /// Whether terrain would block the shot (from last Track call this frame).
        /// Returns false if no track has been performed this frame.
        /// </summary>
        bool BlockedByTerrain { get; }
    }
}
