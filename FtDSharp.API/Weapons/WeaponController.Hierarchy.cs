using System.Collections.Generic;
using System.Linq;
using BrilliantSkies.Core.Ballistics.External.AimingWithoutDrag;
using FtDSharp.Facades;

namespace FtDSharp
{
    public partial class WeaponController
    {
        /// <summary>
        /// Builds the hierarchy of weapons and turrets from input items.
        /// Only processes items explicitly in inputItems - does not discover additional children.
        /// </summary>
        private void BuildHierarchy(List<IWeapon> inputItems)
        {
            _weapons = new List<WeaponItem>();
            _turrets = new List<TurretItem>();

            var facadeLookup = new Dictionary<ConstructableWeapon, WeaponFacade>();
            foreach (var item in inputItems)
            {
                if (item is WeaponFacade wf)
                    facadeLookup[wf.Weapon] = wf;
            }

            var visited = new HashSet<ConstructableWeapon>();

            foreach (var item in inputItems)
            {
                if (item is TurretFacade turretFacade)
                    DiscoverTurret(turretFacade, depth: 0, visited, facadeLookup);
                else if (item is WeaponFacade weaponFacade)
                    DiscoverWeapon(weaponFacade, depth: 0, visited);
            }

            _turrets.Sort((a, b) => a.Depth.CompareTo(b.Depth));
            LinkClosestWeaponsToTurrets();

            _controlled = new ControlledItems(
                _weapons.Select(w => (IWeapon)w.Facade),
                _turrets.Select(t => t.Facade)
            );
        }

        private void DiscoverWeapon(WeaponFacade facade, int depth, HashSet<ConstructableWeapon> visited)
        {
            if (visited.Contains(facade.Weapon))
                return;
            visited.Add(facade.Weapon);

            _weapons!.Add(new WeaponItem
            {
                Facade = facade,
                Depth = depth,
                AimingModule = new AimingModule(new GroundHitChecker())
            });
        }

        /// <summary>
        /// Discovers a turret and only children explicitly in the facadeLookup.
        /// </summary>
        private void DiscoverTurret(TurretFacade facade, int depth, HashSet<ConstructableWeapon> visited, Dictionary<ConstructableWeapon, WeaponFacade> facadeLookup)
        {
            if (visited.Contains(facade.Weapon))
                return;
            visited.Add(facade.Weapon);

            var turretItem = new TurretItem
            {
                Facade = facade,
                Depth = depth,
                AimingModule = new AimingModule(new GroundHitChecker())
            };
            _turrets!.Add(turretItem);

            foreach (var child in facade.Weapons)
            {
                if (child is WeaponFacade childFacade && facadeLookup.TryGetValue(childFacade.Weapon, out var existingFacade))
                {
                    if (existingFacade is TurretFacade nestedTurret)
                        DiscoverTurret(nestedTurret, depth + 1, visited, facadeLookup);
                    else
                        DiscoverWeapon(existingFacade, depth + 1, visited);
                }
            }
        }

        /// <summary>
        /// Links each turret to its closest weapons (minimum depth descendants).
        /// </summary>
        private void LinkClosestWeaponsToTurrets()
        {
            foreach (var turret in _turrets!)
            {
                var descendants = new List<WeaponItem>();
                FindDescendantWeapons(turret.Facade, descendants);

                if (descendants.Count == 0)
                {
                    turret.ClosestWeapons = null;
                    continue;
                }

                int minDepth = int.MaxValue;
                foreach (var w in descendants)
                    if (w.Depth < minDepth) minDepth = w.Depth;

                var closest = new List<WeaponItem>();
                foreach (var w in descendants)
                    if (w.Depth == minDepth) closest.Add(w);

                turret.ClosestWeapons = closest;
            }
        }

        private void FindDescendantWeapons(TurretFacade turretFacade, List<WeaponItem> result)
        {
            foreach (var child in turretFacade.Weapons)
            {
                if (child is TurretFacade nestedTurret)
                {
                    FindDescendantWeapons(nestedTurret, result);
                }
                else if (child is WeaponFacade weaponFacade)
                {
                    foreach (var item in _weapons!)
                    {
                        if (item.Weapon == weaponFacade.Weapon)
                        {
                            result.Add(item);
                            break;
                        }
                    }
                }
            }
        }
    }
}
