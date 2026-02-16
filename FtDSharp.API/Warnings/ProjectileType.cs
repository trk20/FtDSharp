namespace FtDSharp
{
    /// <summary>
    /// Type of incoming projectile.
    /// </summary>
    public enum ProjectileType
    {
        /// <summary>Unknown projectile type.</summary>
        Unknown,
        /// <summary>Includes both regular missiles and harpoons. Use HasHarpoon to distinguish.</summary>
        Missile,
        /// <summary>APS (Advanced Projectile System) shells.</summary>
        Shell,
        /// <summary>CRAM cannon shells.</summary>
        Cram
    }
}
