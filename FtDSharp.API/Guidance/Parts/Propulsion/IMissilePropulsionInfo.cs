namespace FtDSharp
{
    /// <summary>
    /// Medium in which missile propulsion operates.
    /// </summary>
    public enum MissilePropulsionMedium
    {
        /// <summary>Can only produce thrust in air.</summary>
        AIR,
        /// <summary>Can only produce thrust underwater.</summary>
        WATER
    }

    /// <summary>
    /// Base interface for missile propulsion systems.
    /// </summary>
    public interface IMissilePropulsionInfo : IMissilePart
    {
        /// <summary>Delay from launch before thrust starts in ticks.</summary>
        int ThrustDelay { get; }
        /// <summary>Thrust force.</summary>
        int MaxThrust { get; set; }
        /// <summary>Fuel burn rate.</summary>
        int BurnRate { get; }
        /// <summary>Propulsion medium (air or water).</summary>
        MissilePropulsionMedium Medium { get; }
    }
}
