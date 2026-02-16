using UnityEngine;

namespace FtDSharp
{
    /// <summary>
    /// Interface for flamers.
    /// </summary>
    public interface IFlamerWeapon : IWeapon
    {
        /// <summary>Effective range in meters.</summary>
        float Range { get; }

        /// <summary>Current available fuel (after compressor filtering).</summary>
        float CurrentFuel { get; }

        /// <summary>Fuel consumed per shot.</summary>
        float FuelPerShot { get; }

        /// <summary>
        /// Current intensity value. Computed from fuel and catalyst throughput.
        /// Formula: Min(100, 20 + 20 * catalyst / fuel). Returns 1 if fuel is 0.
        /// </summary>
        float Intensity { get; }

        /// <summary>Oxidizer throughput per second.</summary>
        float OxidizerPerSec { get; }

        /// <summary>Fuel throughput per second.</summary>
        float FuelPerSec { get; }

        /// <summary>Flame color.</summary>
        Color FlameColor { get; }
    }
}
