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
        public new AimResult Track(Vector3 targetPosition, Vector3 targetVelocity)
        {
            return _turretController.Value.Track(targetPosition, targetVelocity);
        }

        public new AimResult Track(ITargetable targetable)
        {
            return _turretController.Value.Track(targetable);
        }

        public new TrackResult Track(ITargetable targetable, TrackOptions options)
        {
            return _turretController.Value.Track(targetable, options);
        }

        public new TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetAcceleration, TrackOptions options)
        {
            return _turretController.Value.Track(targetPosition, targetVelocity, targetAcceleration, options);
        }

        // Override Fire to fire all mounted weapons
        public new bool Fire()
        {
            return _turretController.Value.Fire();
        }

        /// <summary>
        /// Discovers all weapons mounted on this turret and nested subobjects.
        /// Uses a visited set to prevent duplicates.
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

                    if (weapon is Turrets nestedTurret)
                    {
                        weapons.Add(new TurretFacade(nestedTurret, AllConstruct));
                    }
                    else
                    {
                        weapons.Add(new WeaponFacade(weapon, AllConstruct));
                    }
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
                                if (weapon is Turrets nestedTurret)
                                {
                                    weapons.Add(new TurretFacade(nestedTurret, nestedAllConstruct));
                                }
                                else
                                {
                                    weapons.Add(new WeaponFacade(weapon, nestedAllConstruct));
                                }
                            }
                        }
                    }
                }
            }

            return weapons;
        }
    }
}
