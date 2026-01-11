using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FtDSharp
{
    /// <summary>
    /// Type of incoming projectile.
    /// </summary>
    public enum ProjectileType
    {
        /// <summary> Unknown projectile type. </summary>
        Unknown,
        /// <summary> Includes both regular missiles and harpoons. Use HasHarpoon to distinguish. </summary>
        Missile,
        /// <summary> APS (Advanced Projectile System) shells. </summary>
        Shell,
        /// <summary> CRAM cannon shells. </summary>
        Cram
    }

    /// <summary>
    /// Represents a detected incoming projectile warning from munition warners, radar, etc.
    /// </summary>
    public interface IProjectileWarning : ITargetable
    {
        /// <summary> Type of the incoming projectile. </summary>
        ProjectileType Type { get; }
        /// <summary> Diameter of the projectile in meters. </summary>
        float Diameter { get; }
        /// <summary> Current health of the projectile. </summary>
        // float Health { get; } // TBD if unfair to expose
        /// <summary> Length of the projectile in meters. </summary>
        // float Length { get; } // TBD if unfair to expose
        /// <summary> Time since the projectile was fired in seconds. </summary>
        float TimeSinceFiring { get; }
        /// <summary> Time since this projectile was last spotted in seconds. </summary>
        float TimeSinceLastSpotted { get; }
        /// <summary> Whether this is a fake projectile (created via projectile avoidance GUI). </summary>
        bool IsFake { get; }
        /// <summary> Number of shots that have been fired at this projectile. </summary>
        int ShotsFiredAt { get; }
        /// <summary> Number of CIWS turrets currently aiming at this projectile. </summary>
        int CiwsAimingAt { get; }
    }

    /// <summary>
    /// Static accessor for projectile warning information.
    /// </summary>
    public static class Warnings
    {
        private static readonly FrameCache<IReadOnlyList<IProjectileWarning>> _all = new(
            () => ScriptApi.Context?.IncomingProjectiles ?? Array.Empty<IProjectileWarning>());

        private static readonly FrameCache<IReadOnlyList<IProjectileWarning>> _missiles = new(
            () => IncomingProjectiles.Where(w => w.Type == ProjectileType.Missile).ToList());

        private static readonly FrameCache<IReadOnlyList<IProjectileWarning>> _shells = new(
            () => IncomingProjectiles.Where(w => w.Type == ProjectileType.Shell || w.Type == ProjectileType.Cram).ToList());

        /// <summary>
        /// All valid incoming projectile warnings.
        /// </summary>
        public static IReadOnlyList<IProjectileWarning> IncomingProjectiles => _all.Value;

        /// <summary>
        /// Incoming missiles only (includes harpoons).
        /// </summary>
        public static IReadOnlyList<IProjectileWarning> IncomingMissiles => _missiles.Value;

        /// <summary>
        /// Incoming shells only (APS shells and CRAM).
        /// </summary>
        public static IReadOnlyList<IProjectileWarning> IncomingShells => _shells.Value;

        /// <summary>
        /// Get warnings filtered by a specific projectile type.
        /// </summary>
        public static IEnumerable<IProjectileWarning> GetByType(ProjectileType type)
        {
            return IncomingProjectiles.Where(w => w.Type == type);
        }
    }
}
