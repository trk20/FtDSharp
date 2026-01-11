using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public static IReadOnlyList<IWeapon> All => ScriptApi.Context?.Weapons ?? System.Array.Empty<IWeapon>();

        /// <summary>
        /// Gets all turrets on the current construct.
        /// </summary>
        public static IReadOnlyList<ITurret> Turrets => ScriptApi.Context?.Turrets ?? System.Array.Empty<ITurret>();

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
        public static IReadOnlyList<IWeapon> MissileLaunchers => All.Where(w => w.WeaponType == WeaponType.Missile).ToList();
    }

    /// <summary>
    /// Represents the result of aiming a weapon.
    /// </summary>
    public readonly struct AimResult
    {
        /// <summary>Whether the weapon can fire at the target.</summary>
        public bool CanFire { get; }
        /// <summary>Whether the weapon cannot fire at the target.</summary>
        public bool CantFire { get; }
        /// <summary>Whether the weapon can aim at the target.</summary>
        public bool CanAim { get; }

        public AimResult(bool canFire, bool cantFire, bool canAim)
        {
            CanFire = canFire;
            CantFire = cantFire;
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
            ArcPreference = ArcPreference.Low
        };

        /// <summary>
        /// Options for instant-hit weapons (no gravity, no arc).
        /// </summary>
        public static TrackOptions InstantHit => new TrackOptions
        {
            ProjectileSpeed = null,
            TargetAcceleration = null,
            UseGravity = false,
            ArcPreference = ArcPreference.Low
        };
    }

    /// <summary>
    /// Arc preference for ballistic trajectory calculations.
    /// </summary>
    public enum ArcPreference
    {
        /// <summary>Prefer lower arc (faster, flatter trajectory).</summary>
        Low,
        /// <summary>Prefer higher arc (slower, more curved trajectory).</summary>
        High
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

        public TrackResult(AimResult aimResult, float flightTime, Vector3 aimPoint, bool isTerrainBlocking)
        {
            AimResult = aimResult;
            FlightTime = flightTime;
            AimPoint = aimPoint;
            IsTerrainBlocking = isTerrainBlocking;
        }

        /// <summary>Whether the weapon can fire at the target.</summary>
        public bool CanFire => AimResult.CanFire;
        /// <summary>Whether the weapon cannot fire at the target.</summary>
        public bool CantFire => AimResult.CantFire;
        /// <summary>Whether the weapon can aim at the target.</summary>
        public bool CanAim => AimResult.CanAim;
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
    /// </summary>
    public interface IWeapon : IBlock
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
        /// Aims the weapon at a world-space position (direct aim, no lead).
        /// </summary>
        /// <param name="worldPosition">The target position in world space.</param>
        /// <returns>Information about the weapon's aim status.</returns>
        AimResult AimAt(Vector3 worldPosition);

        /// <summary>
        /// Tracks a moving target with lead calculation.
        /// </summary>
        /// <param name="targetPosition">Current target position in world space.</param>
        /// <param name="targetVelocity">Target velocity vector.</param>
        /// <returns>Information about the weapon's aim status.</returns>
        AimResult Track(Vector3 targetPosition, Vector3 targetVelocity);

        /// <summary>
        /// Tracks a moving target with lead calculation including acceleration.
        /// </summary>
        /// <param name="targetPosition">Current target position in world space.</param>
        /// <param name="targetVelocity">Target velocity vector.</param>
        /// <param name="targetAcceleration">Target acceleration vector.</param>
        /// <returns>Information about the weapon's aim status.</returns>
        AimResult Track(Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetAcceleration);

        /// <summary>
        /// Tracks any targetable object (ITarget, IProjectileWarning, IConstruct) with lead calculation.
        /// </summary>
        /// <param name="targetable">The object to track.</param>
        /// <returns>Information about the weapon's aim status.</returns>
        AimResult Track(ITargetable targetable);

        /// <summary>
        /// Tracks a targetable object with custom tracking options.
        /// </summary>
        /// <param name="targetable">The object to track.</param>
        /// <param name="options">Tracking options (gravity, arc preference, projectile speed override).</param>
        /// <returns>Extended tracking result with flight time and aim point.</returns>
        TrackResult Track(ITargetable targetable, TrackOptions options);

        /// <summary>
        /// Tracks a position/velocity/acceleration with custom tracking options.
        /// </summary>
        /// <param name="targetPosition">Current target position in world space.</param>
        /// <param name="targetVelocity">Target velocity vector.</param>
        /// <param name="targetAcceleration">Target acceleration vector.</param>
        /// <param name="options">Tracking options (gravity, arc preference, projectile speed override).</param>
        /// <returns>Extended tracking result with flight time and aim point.</returns>
        TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetAcceleration, TrackOptions options);

        /// <summary>
        /// Attempts to fire the weapon.
        /// </summary>
        /// <returns>True if the weapon fired successfully.</returns>
        bool Fire();

        /// <summary>
        /// Attempts to fire the weapon only if it is aimed at the specified position.
        /// </summary>
        /// <param name="worldPosition">The target position to check aim against.</param>
        /// <returns>True if the weapon fired successfully.</returns>
        bool TryFireAt(Vector3 worldPosition);
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
    }
}