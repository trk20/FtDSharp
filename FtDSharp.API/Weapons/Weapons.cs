using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace FtDSharp
{
    /// <summary>
    /// Static accessor for weapon-related APIs.
    /// </summary>
    public static class Weapons
    {
        /// <summary>
        /// Gets all weapons on the current construct (excluding turrets).
        /// Includes weapons on subconstructs (turrets, spinblocks, etc).
        /// </summary>
        public static IReadOnlyList<IWeapon> All =>
            Game.MainConstruct?.Weapons ?? Array.Empty<IWeapon>();

        /// <summary>
        /// Gets all turrets on the current construct.
        /// </summary>
        public static IReadOnlyList<ITurret> Turrets =>
            Game.MainConstruct?.Turrets ?? Array.Empty<ITurret>();

        /// <summary>
        /// Gets all APS (Advanced Projectile System) weapons.
        /// </summary>
        public static IReadOnlyList<IWeapon> APS => All.Where(w => w.WeaponType == WeaponType.APS).ToList();

        /// <summary>
        /// Gets all CRAM cannons.
        /// </summary>
        public static IReadOnlyList<IWeapon> CRAM => All.Where(w => w.WeaponType == WeaponType.CRAM).ToList();

        /// <summary>
        /// Gets all laser weapons.
        /// </summary>
        public static IReadOnlyList<IWeapon> Lasers => All.Where(w => w.WeaponType == WeaponType.Laser).ToList();

        /// <summary>
        /// Gets all plasma weapons.
        /// </summary>
        public static IReadOnlyList<IWeapon> Plasma => All.Where(w => w.WeaponType == WeaponType.Plasma).ToList();

        /// <summary>
        /// Gets all particle cannons (PAC).
        /// </summary>
        public static IReadOnlyList<IWeapon> ParticleCannons => All.Where(w => w.WeaponType == WeaponType.ParticleCannon).ToList();

        /// <summary>
        /// Gets all flamers.
        /// </summary>
        public static IReadOnlyList<IWeapon> Flamers => All.Where(w => w.WeaponType == WeaponType.Flamer).ToList();

        /// <summary>
        /// Gets all missile launchers.
        /// </summary>
        public static IReadOnlyList<IWeapon> MissileControllers => All.Where(w => w.WeaponType == WeaponType.Missile).ToList();
    }

    /// <summary>
    /// Represents the result of aiming a weapon.
    /// </summary>
    public readonly struct AimResult
    {
        /// <summary>
        /// Whether the weapon is on target and within traversal limits.
        /// Note: This does NOT mean the weapon can actually fire - check IsReady for that.
        /// </summary>
        public bool IsOnTarget { get; }
        /// <summary>Whether the weapon is blocked from aiming (e.g., cannot traverse to target).</summary>
        public bool IsBlocked { get; }
        /// <summary>Whether the weapon can physically aim at the target position.</summary>
        public bool CanAim { get; }

        public AimResult(bool isOnTarget, bool isBlocked, bool canAim)
        {
            IsOnTarget = isOnTarget;
            IsBlocked = isBlocked;
            CanAim = canAim;
        }
    }

    /// <summary>
    /// Options for tracking/lead calculation.
    /// </summary>
    public struct TrackOptions
    {
        /// <summary>
        /// Override projectile speed for lead calculation.
        /// If null, uses the weapon's actual projectile speed.
        /// </summary>
        public float? ProjectileSpeed { get; set; }

        /// <summary>
        /// Override target acceleration for lead calculation.
        /// If null, uses the target's actual acceleration.
        /// Set to Vector3.zero to ignore target acceleration.
        /// </summary>
        public Vector3? TargetAcceleration { get; set; }

        /// <summary>
        /// Whether to account for gravity in the calculation.
        /// Default is true.
        /// </summary>
        public bool UseGravity { get; set; }

        /// <summary>
        /// Arc preference for ballistic solutions.
        /// </summary>
        public ArcPreference ArcPreference { get; set; }

        /// <summary>
        /// Default tracking options (gravity enabled, low arc preferred, uses target acceleration).
        /// </summary>
        public static TrackOptions Default => new TrackOptions
        {
            ProjectileSpeed = null,
            TargetAcceleration = null,
            UseGravity = true,
            ArcPreference = ArcPreference.PreferLow
        };

        /// <summary>
        /// Options for instant-hit weapons (no gravity, no arc).
        /// </summary>
        public static TrackOptions InstantHit => new TrackOptions
        {
            ProjectileSpeed = null,
            TargetAcceleration = null,
            UseGravity = false,
            ArcPreference = ArcPreference.PreferLow
        };
    }

    /// <summary>
    /// Arc preference for ballistic trajectory calculations.
    /// </summary>
    public enum ArcPreference
    {
        /// <summary>Prefer lower arc (faster, flatter trajectory), but accept higher if needed.</summary>
        PreferLow,
        /// <summary>Prefer higher arc (slower, more curved trajectory), but accept lower if needed.</summary>
        PreferHigh,
        /// <summary>Only use low arc solutions. CanAim = false if no low arc solution exists.</summary>
        OnlyLow,
        /// <summary>Only use high arc solutions. CanAim = false if no high arc solution exists.</summary>
        OnlyHigh
    }

    /// <summary>
    /// Extended result from tracking that includes calculated values.
    /// </summary>
    public readonly struct TrackResult
    {
        /// <summary>The basic aim result.</summary>
        public AimResult AimResult { get; }
        /// <summary>Calculated flight time to target in seconds.</summary>
        public float FlightTime { get; }
        /// <summary>The calculated aim point position.</summary>
        public Vector3 AimPoint { get; }
        /// <summary>Whether terrain would block the shot.</summary>
        public bool IsTerrainBlocking { get; }
        /// <summary>
        /// Whether the weapon(s) are ready to fire (ammo, reload, energy, etc.).
        /// Combined with IsOnTarget to determine if weapon will actually fire.
        /// </summary>
        public bool IsReady { get; }

        public TrackResult(AimResult aimResult, float flightTime, Vector3 aimPoint, bool isTerrainBlocking, bool isReady)
        {
            AimResult = aimResult;
            FlightTime = flightTime;
            AimPoint = aimPoint;
            IsTerrainBlocking = isTerrainBlocking;
            IsReady = isReady;
        }

        /// <summary>
        /// Whether the weapon is on target and within traversal limits.
        /// Combined with IsReady to determine if weapon will actually fire.
        /// </summary>
        public bool IsOnTarget => AimResult.IsOnTarget;
        /// <summary>Whether the weapon can physically aim at the target.</summary>
        public bool CanAim => AimResult.CanAim;
        /// <summary>
        /// Whether the weapon can actually fire right now.
        /// True only if both IsOnTarget AND IsReady are true.
        /// </summary>
        public bool CanFire => AimResult.IsOnTarget && IsReady;
    }

    /// <summary>
    /// Interface for weapon control operations (aiming, tracking, firing).
    /// Implemented by both individual weapons and weapon controllers.
    /// </summary>
    public interface IWeaponControl
    {
        /// <summary>
        /// Aims at a world-space position (direct aim, no lead calculation).
        /// </summary>
        /// <param name="worldPosition">The target position in world space.</param>
        /// <returns>Information about the aim status.</returns>
        AimResult AimAt(Vector3 worldPosition);

        /// <summary>
        /// Tracks a moving target with lead calculation.
        /// </summary>
        /// <param name="targetPosition">Current target position in world space.</param>
        /// <param name="targetVelocity">Target velocity vector.</param>
        /// <returns>Tracking result with aim status and calculated values.</returns>
        TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity);

        /// <summary>
        /// Tracks a moving target with lead calculation including acceleration.
        /// </summary>
        /// <param name="targetPosition">Current target position in world space.</param>
        /// <param name="targetVelocity">Target velocity vector.</param>
        /// <param name="targetAcceleration">Target acceleration vector.</param>
        /// <returns>Tracking result with aim status and calculated values.</returns>
        TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetAcceleration);

        /// <summary>
        /// Tracks any targetable object (ITarget, IProjectileWarning, IConstruct) with lead calculation.
        /// </summary>
        /// <param name="targetable">The object to track.</param>
        /// <returns>Tracking result with aim status and calculated values.</returns>
        TrackResult Track(ITargetable targetable);

        /// <summary>
        /// Tracks a targetable object with custom tracking options.
        /// </summary>
        /// <param name="targetable">The object to track.</param>
        /// <param name="options">Tracking options (gravity, arc preference, projectile speed override).</param>
        /// <returns>Tracking result with aim status and calculated values.</returns>
        TrackResult Track(ITargetable targetable, TrackOptions options);

        /// <summary>
        /// Tracks a position/velocity/acceleration with custom tracking options.
        /// </summary>
        /// <param name="targetPosition">Current target position in world space.</param>
        /// <param name="targetVelocity">Target velocity vector.</param>
        /// <param name="targetAcceleration">Target acceleration vector.</param>
        /// <param name="options">Tracking options (gravity, arc preference, projectile speed override).</param>
        /// <returns>Tracking result with aim status and calculated values.</returns>
        TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetAcceleration, TrackOptions options);

        /// <summary>
        /// Attempts to fire the weapon(s).
        /// </summary>
        /// <returns>True if any weapon fired successfully.</returns>
        bool Fire();

        /// <summary>
        /// Aims at a position and fires if on target.
        /// </summary>
        /// <param name="worldPosition">The target position to aim at.</param>
        /// <returns>True if any weapon fired successfully.</returns>
        bool TryFireAt(Vector3 worldPosition);
    }

    /// <summary>
    /// The type of weapon system.
    /// </summary>
    public enum WeaponType
    {
        Unknown,
        APS,            // Advanced Projectile System
        CRAM,           // CRAM cannons
        Missile,        // Missile launchers
        Torpedo,        // Torpedo launchers
        Laser,          // Laser systems
        Plasma,         // Plasma cannons
        ParticleCannon,
        Flamer,
        Turret          // Turret (coordinates weapons, not a weapon itself)
    }

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