using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FtDSharp.Facades
{
    /// <summary>
    /// Facade wrapping a Turret block for script access.
    /// Turrets coordinate multiple weapons mounted on them.
    /// </summary>
    internal class TurretFacade : WeaponFacade, ITurret
    {
        private readonly Turrets _turret;
        private readonly Lazy<IReadOnlyList<IWeapon>> _weapons;
        private readonly Lazy<WeaponController> _turretController;

        public TurretFacade(Turrets turret, AllConstruct allConstruct) : base(turret, allConstruct)
        {
            _turret = turret;
            _weapons = new Lazy<IReadOnlyList<IWeapon>>(DiscoverWeapons);
            // Controller includes this turret so hierarchy is handled correctly
            _turretController = new Lazy<WeaponController>(() => new WeaponController(this));
        }

        /// <summary>
        /// Gets the underlying Turrets block. Internal use only.
        /// </summary>
        internal Turrets TurretBlock => _turret;

        public IReadOnlyList<IWeapon> Weapons => _weapons.Value;

        public float Azimuth
        {
            get
            {
                // Extract azimuth from the turret's local rotation
                var euler = _turret.lastLocalRotation.eulerAngles;
                return euler.y > 180 ? euler.y - 360 : euler.y;
            }
        }

        public float Elevation
        {
            get
            {
                // Extract elevation from the turret's local rotation
                var euler = _turret.lastLocalRotation.eulerAngles;
                return euler.x > 180 ? euler.x - 360 : euler.x;
            }
        }

        // Override Track methods to use the turret controller (coordinate all weapons)
        public override AimResult Track(Vector3 targetPosition)
        {
            return _turretController.Value.Track(targetPosition, Vector3.zero).AimResult;
        }

        public override TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity)
        {
            return _turretController.Value.Track(targetPosition, targetVelocity);
        }

        public override TrackResult Track(ITargetable targetable)
        {
            return _turretController.Value.Track(targetable);
        }

        public override TrackResult Track(ITargetable targetable, TrackOptions options)
        {
            return _turretController.Value.Track(targetable, options);
        }

        public override TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetAcceleration)
        {
            return _turretController.Value.Track(targetPosition, targetVelocity, targetAcceleration);
        }

        public override TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetAcceleration, TrackOptions options)
        {
            return _turretController.Value.Track(targetPosition, targetVelocity, targetAcceleration, options);
        }

        // Override AimAt and TryFireAt to use turret controller
        public override AimResult AimAt(Vector3 worldPosition)
        {
            return _turretController.Value.AimAt(worldPosition);
        }

        public override bool TryFireAt(Vector3 worldPosition)
        {
            return _turretController.Value.TryFireAt(worldPosition);
        }

        public override bool Fire()
        {
            return _turretController.Value.Fire();
        }

        // --- Aggregate state properties from mounted weapons ---

        /// <summary>
        /// True if any mounted weapon is on target (from last Track/AimAt this frame).
        /// </summary>
        public bool AnyOnTarget => Weapons.Count > 0 && Weapons.Any(w => w.OnTarget);

        /// <summary>
        /// True if all mounted weapons are on target (from last Track/AimAt this frame).
        /// </summary>
        public bool AllOnTarget => Weapons.Count > 0 && Weapons.All(w => w.OnTarget);

        /// <summary>
        /// True if any mounted weapon is ready to fire.
        /// </summary>
        public bool AnyReady => Weapons.Count > 0 && Weapons.Any(w => w.IsReady);

        /// <summary>
        /// True if all mounted weapons are ready to fire.
        /// </summary>
        public bool AllReady => Weapons.Count > 0 && Weapons.All(w => w.IsReady);

        /// <summary>
        /// True if any mounted weapon can fire (on target AND ready).
        /// </summary>
        public bool AnyCanFire => Weapons.Count > 0 && Weapons.Any(w => w.CanFire);

        /// <summary>
        /// True if all mounted weapons can fire (on target AND ready).
        /// </summary>
        public bool AllCanFire => Weapons.Count > 0 && Weapons.All(w => w.CanFire);

        /// <summary>
        /// Discovers all weapons mounted on this turret and nested subobjects.
        /// Uses a visited set to prevent duplicates.
        /// Uses the facade cache to ensure consistent instances.
        /// </summary>
        private IReadOnlyList<IWeapon> DiscoverWeapons()
        {
            var weapons = new List<IWeapon>();
            var visited = new HashSet<ConstructableWeapon>();

            // Get weapons directly registered to the turret
            if (_turret.weaponObj != null)
            {
                foreach (var weapon in _turret.weaponObj)
                {
                    if (weapon == null || weapon == _turret || !weapon.IsAlive || visited.Contains(weapon))
                        continue;
                    visited.Add(weapon);

                    weapons.Add(BlockFacadeFactory.GetOrCreateWeaponFacade(weapon, AllConstruct));
                }
            }

            // Get weapons from nested subconstructs (turrets on turrets, spinblocks on turrets, etc.)
            var subConstruct = _turret.SubConstruct;
            if (subConstruct != null)
            {
                var subConstructList = subConstruct.AllBasicsRestricted?.AllSubconstructsBelowUs;
                if (subConstructList != null)
                {
                    foreach (var nestedSub in subConstructList)
                    {
                        var nestedWeapons = nestedSub.WeaponryRestricted?.Weapons;
                        if (nestedWeapons != null)
                        {
                            foreach (var weapon in nestedWeapons)
                            {
                                if (weapon == null || weapon == _turret || !weapon.IsAlive || visited.Contains(weapon))
                                    continue;
                                visited.Add(weapon);

                                var nestedAllConstruct = nestedSub as AllConstruct ?? AllConstruct;
                                weapons.Add(BlockFacadeFactory.GetOrCreateWeaponFacade(weapon, nestedAllConstruct));
                            }
                        }
                    }
                }
            }

            return weapons;
        }
    }
}
