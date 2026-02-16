namespace FtDSharp
{
    /// <summary>
    /// A missile launcher weapon that can fire missiles.
    /// </summary>
    public interface IMissileLauncher : IWeapon
    {
        /// <summary>Size category of the missile launcher.</summary>
        MissileSize Size { get; }
        /// <summary>Number of missiles currently loaded. Small launchers hold up to 4, medium/large/huge only 1.</summary>
        int LoadedMissiles { get; }
    }
}
