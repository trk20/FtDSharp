using UnityEngine;

namespace FtDSharp
{
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
}
