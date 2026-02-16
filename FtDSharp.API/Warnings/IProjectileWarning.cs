namespace FtDSharp
{
    /// <summary>
    /// Represents a detected incoming projectile warning from munition warners, radar, etc.
    /// </summary>
    public interface IProjectileWarning : ITargetable
    {
        /// <summary>Type of the incoming projectile.</summary>
        ProjectileType Type { get; }
        /// <summary>Diameter of the projectile in meters.</summary>
        float Diameter { get; }
        /// <summary>Time since the projectile was fired in seconds.</summary>
        float TimeSinceFiring { get; }
        /// <summary>Time since this projectile was last spotted in seconds.</summary>
        float TimeSinceLastSpotted { get; }
        /// <summary>Whether this is a fake projectile (created via projectile avoidance GUI).</summary>
        bool IsFake { get; }
        /// <summary>Number of shots that have been fired at this projectile.</summary>
        int ShotsFiredAt { get; }
        /// <summary>Number of CIWS turrets currently aiming at this projectile.</summary>
        int CiwsAimingAt { get; }
    }
}
