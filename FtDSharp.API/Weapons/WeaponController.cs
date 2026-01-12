using System;
using System.Collections.Generic;
using System.Linq;
using BrilliantSkies.Ftd.Planets;
using FtDSharp.Facades;
using UnityEngine;

namespace FtDSharp
{
    /// <summary>
    /// Controls one or more weapons/turrets with unified aiming and firing.
    /// Handles hierarchy correctly: turrets aim based on their closest weapons' calculated directions.
    /// </summary>
    public partial class WeaponController : IWeaponControl
    {
        private float? _overrideProjectileSpeed;
        private List<WeaponItem> _weapons = null!;
        private List<TurretItem> _turrets = null!;  // Sorted by depth (root first)
        private ControlledItems _controlled = null!;

        private static readonly AimResult EmptyAimResult = new AimResult(false, false, false);
        private static readonly TrackResult EmptyTrackResult = new TrackResult(EmptyAimResult, 0f, Vector3.zero, false, false);

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

            var result = AimTurrets();
            result = AggregateResults(result, AimWeapons());
            return result;
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

            // Phase 3: Aim turrets root-first
            var aimResult = AimTurrets();

            // Phase 4: Aim weapons
            aimResult = AggregateResults(aimResult, AimWeapons());

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

        #region Aiming Phases

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
                    // Average from closest weapons
                    var avgDir = Vector3.zero;
                    foreach (var w in turret.ClosestWeapons!)
                    {
                        avgDir += w.CalculatedDirection;
                    }
                    turret.CalculatedDirection = (avgDir / turret.ClosestWeapons.Count).normalized;
                }
                else
                {
                    // Turret with no weapons - calculate directly
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
            BrilliantSkies.Core.Ballistics.External.AimingWithoutDrag.AimingModule module,
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
                // Fallback to direct aim
                direction = (ctx.TargetPosition - firePosition).normalized;
                flightTime = 0f;
                aimPoint = ctx.TargetPosition;
                isTerrainBlocking = false;
            }
        }

        private AimResult AimTurrets()
        {
            bool isOnTarget = false, isBlocked = false, canAim = false;
            foreach (var turret in _turrets)
            {
                var result = turret.Facade.AimAtDirectionInternal(turret.CalculatedDirection);
                isOnTarget |= result.IsOnTarget;
                isBlocked |= result.IsBlocked;
                canAim |= result.CanAim;
            }
            return new AimResult(isOnTarget, isBlocked, canAim);
        }

        private AimResult AimWeapons()
        {
            bool isOnTarget = false, isBlocked = false, canAim = false;
            foreach (var weapon in _weapons)
            {
                var result = weapon.Facade.AimAtDirectionInternal(weapon.CalculatedDirection);
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
