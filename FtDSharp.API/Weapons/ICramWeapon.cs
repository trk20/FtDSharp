namespace FtDSharp
{
    /// <summary>
    /// Interface for CRAM cannons.
    /// </summary>
    public interface ICramWeapon : IWeapon
    {
        /// <summary>Shell gauge in millimeters.</summary>
        float Gauge { get; }

        /// <summary>Packing progress as a fraction (0–1).</summary>
        float PackedFraction { get; }

        /// <summary>Number of packed pellets (ammo count).</summary>
        float AmmoCount { get; }

        /// <summary>Minimum packing fraction before the weapon will fire (0–1).</summary>
        float MinPackFraction { get; }
    }
}
