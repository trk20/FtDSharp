namespace FtDSharp
{
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
}
