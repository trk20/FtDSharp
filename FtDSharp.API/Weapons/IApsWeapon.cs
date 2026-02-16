namespace FtDSharp
{
    /// <summary>
    /// Interface for APS.
    /// </summary>
    public interface IApsWeapon : IWeapon
    {
        /// <summary>Shell gauge in millimeters.</summary>
        float Gauge { get; }

        /// <summary>Number of barrels on this weapon.</summary>
        int BarrelCount { get; }

        /// <summary>Number of shells currently loaded in the clip.</summary>
        int ShellCount { get; }

        /// <summary>Whether a shell is chambered and the barrel is ready to fire.</summary>
        bool IsLoaded { get; }

        /// <summary>Current angular inaccuracy in degrees.</summary>
        float Inaccuracy { get; }

        /// <summary>Railgun charge as a fraction (0–1). Returns 0 if no railgun is present.</summary>
        float RailgunChargeFraction { get; }

        /// <summary>Maximum railgun energy capacity. Returns 0 if no railgun is present.</summary>
        float RailgunCapacity { get; }

        /// <summary>Recoil absorption capacity from hydraulic components.</summary>
        float RecoilCapacity { get; }
    }
}
