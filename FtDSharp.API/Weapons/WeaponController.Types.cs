using System.Collections.Generic;
using System.Linq;
using BrilliantSkies.Core.Ballistics.External.AimingWithoutDrag;
using FtDSharp.Facades;
using UnityEngine;

namespace FtDSharp
{
    /// <summary>
    /// Provides typed access to controlled weapons and turrets.
    /// </summary>
    public class ControlledItems
    {
        private readonly IReadOnlyList<IWeapon> _weapons;
        private readonly IReadOnlyList<ITurret> _turrets;
        private readonly IReadOnlyList<IWeapon> _all;

        internal ControlledItems(IEnumerable<IWeapon> weapons, IEnumerable<ITurret> turrets)
        {
            _weapons = weapons.ToList();
            _turrets = turrets.ToList();
            _all = _turrets.Cast<IWeapon>().Concat(_weapons).ToList();
        }

        /// <summary>All weapons (excluding turrets) controlled by this controller.</summary>
        public IReadOnlyList<IWeapon> Weapons => _weapons;

        /// <summary>All turrets controlled by this controller.</summary>
        public IReadOnlyList<ITurret> Turrets => _turrets;

        /// <summary>All items (weapons and turrets) controlled by this controller.</summary>
        public IReadOnlyList<IWeapon> All => _all;

        /// <summary>Total count of all controlled items.</summary>
        public int Count => _all.Count;
    }

    public partial class WeaponController
    {
        /// <summary>
        /// Represents a weapon in the controller's hierarchy.
        /// </summary>
        private class WeaponItem
        {
            public WeaponFacade Facade = null!;
            public int Depth;
            public Vector3 CalculatedDirection;
            public float FlightTime;
            public Vector3 AimPoint;
            public bool IsTerrainBlocking;
            public AimingModule AimingModule = null!;

            /// <summary>Convenience accessor for the underlying weapon.</summary>
            public ConstructableWeapon Weapon => Facade.Weapon;

            /// <summary>World position of the weapon.</summary>
            public Vector3 WorldPosition => Facade.WorldPosition;

            /// <summary>Projectile speed of this weapon.</summary>
            public float ProjectileSpeed => Facade.ProjectileSpeed;
        }

        /// <summary>
        /// Represents a turret in the controller's hierarchy.
        /// </summary>
        private class TurretItem
        {
            public TurretFacade Facade = null!;
            public int Depth;
            public List<WeaponItem>? ClosestWeapons;
            public Vector3 CalculatedDirection;
            public AimingModule AimingModule = null!;  // Only used if turret has no weapons

            /// <summary>Convenience accessor for the underlying turret.</summary>
            public Turrets Turret => Facade.TurretBlock;

            /// <summary>World position of the turret.</summary>
            public Vector3 WorldPosition => Facade.WorldPosition;

            /// <summary>Whether this turret has weapons to derive aim from.</summary>
            public bool HasWeapons => ClosestWeapons != null && ClosestWeapons.Count > 0;
        }

        /// <summary>
        /// Context for tracking calculations, passed between phases.
        /// </summary>
        private readonly struct TrackContext
        {
            public readonly Vector3 TargetPosition;
            public readonly Vector3 TargetVelocity;
            public readonly Vector3 TargetAcceleration;
            public readonly Vector3 ConstructVelocity;
            public readonly TrackOptions Options;
            public readonly bool LowArc;

            public TrackContext(
                Vector3 targetPosition,
                Vector3 targetVelocity,
                Vector3 targetAcceleration,
                Vector3 constructVelocity,
                TrackOptions options)
            {
                TargetPosition = targetPosition;
                TargetVelocity = targetVelocity;
                TargetAcceleration = targetAcceleration;
                ConstructVelocity = constructVelocity;
                Options = options;
                LowArc = options.ArcPreference == ArcPreference.Low;
            }
        }
    }
}
