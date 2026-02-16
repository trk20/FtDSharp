namespace FtDSharp
{
    /// <summary>
    /// Interface for missile controllers.
    /// </summary>
    public interface IMissileWeapon : IWeapon
    {
        /// <summary>Number of loaded (ready to launch) missiles across all tubes.</summary>
        int LoadedMissileCount { get; }

        /// <summary>Total number of missile tubes across all launchpads.</summary>
        int TotalTubeCount { get; }

        /// <summary>Current firing mode. Can be changed at runtime.</summary>
        FiringMode FiringMode { get; set; }

        /// <summary>Game time of the last missile launch.</summary>
        float LastFireTime { get; }
    }
}
