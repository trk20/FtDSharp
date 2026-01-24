using System;
using System.Collections.Generic;
using System.Linq;
using BrilliantSkies.Core.Ballistics.External.AimingWithoutDrag;
using BrilliantSkies.Ftd.Planets;
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

    /// <summary>
    /// Controls one or more weapons/turrets with unified aiming and firing.
    /// Handles hierarchy correctly: turrets aim based on their closest weapons' calculated directions.
    /// </summary>
    public class WeaponController : IWeaponControl
    {
        private float? _overrideProjectileSpeed;
        private List<WeaponItem> _weapons = null!;
        private List<TurretItem> _turrets = null!;
        private ControlledItems _controlled = null!;

        private static readonly AimResult EmptyAimResult = new AimResult(false, false, false);
        private static readonly TrackResult EmptyTrackResult = new TrackResult(EmptyAimResult, 0f, Vector3.zero, false, false);

        #region Nested Types

        private class WeaponItem
        {
            public WeaponFacade Facade = null!;
            public int Depth;
            public Vector3 CalculatedDirection;
            public float FlightTime;
            public Vector3 AimPoint;
            public bool IsTerrainBlocking;
            public AimingModule AimingModule = null!;

            public ConstructableWeapon Weapon => Facade.Weapon;
            public Vector3 WorldPosition => Facade.WorldPosition;
            public float ProjectileSpeed => Facade.ProjectileSpeed;
        }

        private class TurretItem
        {
            public TurretFacade Facade = null!;
            public int Depth;
            public List<WeaponItem>? ClosestWeapons;
            public Vector3 CalculatedDirection;
            public AimingModule AimingModule = null!;

            public Turrets Turret => Facade.TurretBlock;
            public Vector3 WorldPosition => Facade.WorldPosition;
            public bool HasWeapons => ClosestWeapons != null && ClosestWeapons.Count > 0;
        }

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
                LowArc = options.ArcPreference == ArcPreference.PreferLow || options.ArcPreference == ArcPreference.OnlyLow;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a controller for a single weapon.
        /// </summary>
        public WeaponController(IWeapon weapon)
        {
            if (weapon == null) throw new ArgumentNullException(nameof(weapon));
            BuildHierarchy(new List<IWeapon> { weapon });
        }

        /// <summary>
        /// Creates a controller for a turret and all its mounted weapons.
        /// </summary>
        public WeaponController(ITurret turret)
        {
            if (turret == null) throw new ArgumentNullException(nameof(turret));
            var inputItems = turret is TurretFacade tf ? tf.Weapons.Append(turret).ToList() : new List<IWeapon> { turret };
            BuildHierarchy(inputItems);
        }

        /// <summary>
        /// Creates a controller for multiple weapons/turrets.
        /// </summary>
        public WeaponController(IEnumerable<IWeapon> weapons)
        {
            var inputItems = weapons?.ToList() ?? throw new ArgumentNullException(nameof(weapons));
            if (inputItems.Count == 0)
                throw new ArgumentException("At least one weapon is required", nameof(weapons));
            BuildHierarchy(inputItems);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Provides access to the weapons and turrets controlled by this controller.
        /// </summary>
        public ControlledItems Controlled => _controlled;

        /// <summary>
        /// Whether all weapons are known types that can fire.
        /// </summary>
        public bool AllKnownTypes => _weapons.All(w => w.Facade.WeaponType != WeaponType.Unknown);

        #endregion

        #region Configuration

        /// <summary>
        /// Sets an override projectile speed for lead calculations.
        /// </summary>
        public WeaponController WithProjectileSpeed(float? speed)
        {
            _overrideProjectileSpeed = speed;
            return this;
        }

        /// <summary>
        /// Rebuilds the hierarchy. Call if weapons are added/removed from turrets.
        /// </summary>
        public void RebuildHierarchy()
        {
            BuildHierarchy(_controlled.All.ToList());
        }

        #endregion

        #region AimAt

        /// <summary>
        /// Aims all weapons directly at a world position (no lead).
        /// </summary>
        public AimResult AimAt(Vector3 worldPosition)
        {
            if (!HasItems) return EmptyAimResult;

            foreach (var weapon in _weapons)
                weapon.CalculatedDirection = (worldPosition - weapon.WorldPosition).normalized;

            foreach (var turret in _turrets)
            {
                turret.CalculatedDirection = turret.HasWeapons
                    ? AverageDirection(turret.ClosestWeapons!)
                    : (worldPosition - turret.WorldPosition).normalized;
            }

            return AimAllAt(worldPosition);
        }

        #endregion

        #region Track

        /// <summary>
        /// Tracks a moving target with lead calculation.
        /// </summary>
        public TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity)
        {
            return Track(targetPosition, targetVelocity, Vector3.zero, TrackOptions.Default);
        }

        /// <summary>
        /// Tracks a moving target with lead calculation including acceleration.
        /// </summary>
        public TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetAcceleration)
        {
            return Track(targetPosition, targetVelocity, targetAcceleration, TrackOptions.Default);
        }

        /// <summary>
        /// Tracks a targetable object with lead calculation.
        /// </summary>
        public TrackResult Track(ITargetable targetable)
        {
            if (targetable == null) return EmptyTrackResult;
            return Track(targetable.Position, targetable.Velocity, targetable.Acceleration, TrackOptions.Default);
        }

        /// <summary>
        /// Tracks a targetable object with custom tracking options.
        /// </summary>
        public TrackResult Track(ITargetable targetable, TrackOptions options)
        {
            if (targetable == null) return EmptyTrackResult;
            return Track(targetable.Position, targetable.Velocity, targetable.Acceleration, options);
        }

        /// <summary>
        /// Tracks a moving target with lead calculation and custom options.
        /// </summary>
        public TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetAcceleration, TrackOptions options)
        {
            if (!HasItems) return EmptyTrackResult;

            var context = new TrackContext(
                targetPosition,
                targetVelocity,
                options.TargetAcceleration ?? targetAcceleration,
                GetConstructVelocity(),
                options
            );

            // Phase 1: Each weapon calculates its own aim direction
            CalculateWeaponDirections(context);

            // Phase 2: Turrets derive their direction from closest weapons
            CalculateTurretDirections(context);

            // Phase 3: Aim all turrets and weapons
            var aimResult = AimAll();

            // Aggregate tracking results from weapons
            return AggregateTrackResults(aimResult);
        }

        #endregion

        #region Fire

        /// <summary>
        /// Fires all weapons that can fire (excludes turrets to prevent recursion).
        /// </summary>
        public bool Fire()
        {
            bool anyFired = false;
            foreach (var weapon in _weapons)
            {
                anyFired |= weapon.Facade.Fire();
            }
            return anyFired;
        }

        /// <summary>
        /// Aims and fires all weapons at a position.
        /// </summary>
        public bool TryFireAt(Vector3 worldPosition)
        {
            var result = AimAt(worldPosition);
            return result.IsOnTarget && Fire();
        }

        #endregion

        #region Hierarchy Building

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

            // No depth sorting needed - JustTheTopLevel ensures each item is aimed independently
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

        #endregion

        #region Private Helpers

        private bool HasItems => _weapons.Count > 0 || _turrets.Count > 0;

        private Vector3 GetConstructVelocity()
        {
            if (_weapons.Count > 0)
                return _weapons[0].Facade.ParentConstruct?.Velocity ?? Vector3.zero;
            if (_turrets.Count > 0)
                return _turrets[0].Facade.ParentConstruct?.Velocity ?? Vector3.zero;
            return Vector3.zero;
        }

        private static Vector3 AverageDirection(List<WeaponItem> weapons)
        {
            var sum = Vector3.zero;
            foreach (var w in weapons)
            {
                sum += w.CalculatedDirection;
            }
            return (sum / weapons.Count).normalized;
        }

        private static AimResult AggregateResults(AimResult a, AimResult b)
        {
            return new AimResult(
                a.IsOnTarget || b.IsOnTarget,
                a.IsBlocked || b.IsBlocked,
                a.CanAim || b.CanAim
            );
        }

        #endregion

        #region Direction Calculation

        private void CalculateWeaponDirections(TrackContext ctx)
        {
            foreach (var weapon in _weapons!)
            {
                var projectileSpeed = ctx.Options.ProjectileSpeed ?? _overrideProjectileSpeed ?? weapon.ProjectileSpeed;
                if (projectileSpeed <= 0) projectileSpeed = 500f;

                CalculateAimDirection(
                    weapon.AimingModule,
                    weapon.WorldPosition,
                    projectileSpeed,
                    ctx,
                    out weapon.CalculatedDirection,
                    out weapon.FlightTime,
                    out weapon.AimPoint,
                    out weapon.IsTerrainBlocking
                );
            }
        }

        private void CalculateTurretDirections(TrackContext ctx)
        {
            foreach (var turret in _turrets!)
            {
                if (turret.HasWeapons)
                {
                    var avgDir = Vector3.zero;
                    foreach (var w in turret.ClosestWeapons!)
                    {
                        avgDir += w.CalculatedDirection;
                    }
                    turret.CalculatedDirection = (avgDir / turret.ClosestWeapons.Count).normalized;
                }
                else
                {
                    var projectileSpeed = ctx.Options.ProjectileSpeed ?? _overrideProjectileSpeed ?? 500f;
                    CalculateAimDirection(
                        turret.AimingModule,
                        turret.WorldPosition,
                        projectileSpeed,
                        ctx,
                        out turret.CalculatedDirection,
                        out _,
                        out _,
                        out _
                    );
                }
            }
        }

        private void CalculateAimDirection(
            AimingModule module,
            Vector3 firePosition,
            float projectileSpeed,
            TrackContext ctx,
            out Vector3 direction,
            out float flightTime,
            out Vector3 aimPoint,
            out bool isTerrainBlocking)
        {
            module.TargetPosition = ctx.TargetPosition;
            module.TargetVelocity = ctx.TargetVelocity;
            module.TargetAcceleration = ctx.TargetAcceleration;
            module.FirePostPosition = firePosition;
            module.FirePostVelocity = ctx.ConstructVelocity;
            module.ProjectileVelocity = projectileSpeed;
            module.UseGravity = ctx.Options.UseGravity;

            try
            {
                direction = module.CalculateAimDirection(
                    Planet.i.World.Physics,
                    recalculateFlightTimes: true,
                    lowArcSolution: ctx.LowArc
                );
                flightTime = module.FlightTimeSolved;
                aimPoint = module.TargetPositionSolved;
                isTerrainBlocking = module.IsTerrainCollisionBeforeHittingTarget;
            }
            catch
            {
                direction = (ctx.TargetPosition - firePosition).normalized;
                flightTime = 0f;
                aimPoint = ctx.TargetPosition;
                isTerrainBlocking = false;
            }
        }

        #endregion

        #region Aiming

        /// <summary>
        /// Aims all turrets and weapons in a single pass.
        /// JustTheTopLevel ensures each item is aimed independently without affecting children.
        /// </summary>
        private AimResult AimAll()
        {
            bool isOnTarget = false, isBlocked = false, canAim = false;

            // Aim turrets
            foreach (var turret in _turrets)
            {
                var result = turret.Facade.AimAtDirectionInternal(turret.CalculatedDirection);

                float flightTime = 0f;
                Vector3 aimPoint = Vector3.zero;
                bool isTerrainBlocking = false;
                if (turret.HasWeapons)
                {
                    foreach (var weapon in turret.ClosestWeapons!)
                    {
                        flightTime += weapon.FlightTime;
                        aimPoint = weapon.AimPoint;
                        isTerrainBlocking |= weapon.IsTerrainBlocking;
                    }
                    flightTime /= turret.ClosestWeapons.Count;
                }
                turret.Facade.SetTrackState(new TrackResult(result, flightTime, aimPoint, isTerrainBlocking, turret.Facade.IsReady));

                isOnTarget |= result.IsOnTarget;
                isBlocked |= result.IsBlocked;
                canAim |= result.CanAim;
            }

            // Aim weapons
            foreach (var weapon in _weapons)
            {
                var result = weapon.Facade.AimAtDirectionInternal(weapon.CalculatedDirection);
                isOnTarget |= result.IsOnTarget;
                isBlocked |= result.IsBlocked;
                canAim |= result.CanAim;

                var trackResult = new TrackResult(result, weapon.FlightTime, weapon.AimPoint, weapon.IsTerrainBlocking, weapon.Facade.IsReady);
                weapon.Facade.SetTrackState(trackResult);
            }

            return new AimResult(isOnTarget, isBlocked, canAim);
        }

        /// <summary>
        /// Aims all items and propagates AimResult state (for AimAt calls without lead calculation).
        /// </summary>
        private AimResult AimAllAt(Vector3 worldPosition)
        {
            bool isOnTarget = false, isBlocked = false, canAim = false;

            // Aim turrets
            foreach (var turret in _turrets)
            {
                var result = turret.Facade.AimAtDirectionInternal(turret.CalculatedDirection);
                turret.Facade.SetAimState(result);

                isOnTarget |= result.IsOnTarget;
                isBlocked |= result.IsBlocked;
                canAim |= result.CanAim;
            }

            // Aim weapons
            foreach (var weapon in _weapons)
            {
                var result = weapon.Facade.AimAtDirectionInternal(weapon.CalculatedDirection);
                weapon.Facade.SetAimState(result);

                isOnTarget |= result.IsOnTarget;
                isBlocked |= result.IsBlocked;
                canAim |= result.CanAim;
            }

            return new AimResult(isOnTarget, isBlocked, canAim);
        }

        private TrackResult AggregateTrackResults(AimResult aimResult)
        {
            if (_weapons.Count == 0)
                return new TrackResult(aimResult, 0f, Vector3.zero, false, false);

            float totalFlightTime = 0f;
            Vector3 lastAimPoint = Vector3.zero;
            bool anyTerrainBlocking = false;
            bool allReady = true;

            foreach (var weapon in _weapons)
            {
                totalFlightTime += weapon.FlightTime;
                lastAimPoint = weapon.AimPoint;
                anyTerrainBlocking |= weapon.IsTerrainBlocking;
                allReady &= weapon.Facade.IsReady;
            }

            return new TrackResult(
                aimResult,
                totalFlightTime / _weapons.Count,
                lastAimPoint,
                anyTerrainBlocking,
                allReady
            );
        }

        #endregion
    }
}
