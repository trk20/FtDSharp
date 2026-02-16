using UnityEngine;

namespace FtDSharp
{
    /// <summary>
    /// Interface for particle cannons (PAC).
    /// </summary>
    public interface IParticleWeapon : IWeapon
    {
        /// <summary>Whether the weapon is fully charged and ready to fire.</summary>
        bool IsLoaded { get; }

        /// <summary>Charge level as a fraction (0–1).</summary>
        float ChargeFraction { get; }

        /// <summary>Whether arms/segments are broken.</summary>
        bool IsDamaged { get; }

        /// <summary>Time since last fire in seconds (effectively last reload duration).</summary>
        float LastReloadTime { get; }

        /// <summary>Beam damage attenuation factor.</summary>
        float Attenuation { get; }

        /// <summary>Damage multiplier.</summary>
        float DamageFactor { get; }

        /// <summary>Number of beams per shot.</summary>
        int BeamCount { get; }

        /// <summary>Beam color.</summary>
        Color Color { get; }

        /// <summary>Overclocking multiplier (1.0–2.0).</summary>
        float OverClocking { get; }
    }
}
