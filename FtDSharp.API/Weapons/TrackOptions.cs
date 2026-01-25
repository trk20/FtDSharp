using UnityEngine;

namespace FtDSharp
{
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
}
