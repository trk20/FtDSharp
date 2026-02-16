using UnityEngine;

namespace FtDSharp
{
    /// <summary>
    /// Interface for plasma weapons.
    /// </summary>
    public interface IPlasmaWeapon : IWeapon
    {
        /// <summary>Number of charges ready to fire.</summary>
        int ChargesReady { get; }

        /// <summary>Whether the weapon is at maximum charge capacity.</summary>
        bool IsFullyCharged { get; }

        /// <summary>Current temperature.</summary>
        float Temperature { get; }

        /// <summary>Temperature at which the weapon stalls.</summary>
        float TemperatureLimit { get; }

        /// <summary>Whether the weapon is overheated and stalled.</summary>
        bool IsStalled { get; }

        /// <summary>Number of shots per burst.</summary>
        int BurstSize { get; }

        /// <summary>Number of charges consumed per shot.</summary>
        int ChargePerShot { get; }

        /// <summary>Rate of fire in rounds per minute.</summary>
        float RateOfFire { get; }

        /// <summary>Projectile color.</summary>
        Color Color { get; }
    }
}
