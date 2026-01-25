namespace FtDSharp
{
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
}
