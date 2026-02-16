using UnityEngine;

namespace FtDSharp
{
    /// <summary>
    /// Interface for lasers.
    /// </summary>
    public interface ILaserWeapon : IWeapon
    {
        /// <summary>Whether the laser beam is currently firing.</summary>
        bool IsFiring { get; }

        /// <summary>Beam color.</summary>
        Color Color { get; }

        /// <summary>
        /// Energy fraction below which the weapon holds fire (0–1).
        /// </summary>
        float HoldFireEnergyFraction { get; }
    }
}
