using System;
using BrilliantSkies.Core.Ballistics.External.AimingWithoutDrag;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Ftd.Planets;
using Ftd.Blocks.Flamers;
using UnityEngine;

namespace FtDSharp.Facades
{
    /// <summary>
    /// Facade wrapping a ConstructableWeapon for script access.
    /// </summary>
    internal class WeaponFacade : BlockFacadeBase, IWeapon
    {
        private readonly ConstructableWeapon _weapon;
        private readonly FiredMunitionReturn _fireReturn;
        private readonly AllConstruct _allConstruct;
        private WeaponType? _cachedWeaponType;
        private readonly AimingModule _aimingModule;

        public WeaponFacade(ConstructableWeapon weapon, AllConstruct allConstruct) : base(weapon)
        {
            _weapon = weapon;
            _allConstruct = allConstruct;
            _fireReturn = new FiredMunitionReturn();
            _aimingModule = new AimingModule(new GroundHitChecker());
        }

        /// <summary>
        /// Gets the underlying ConstructableWeapon. Internal use only.
        /// </summary>
        internal ConstructableWeapon Weapon => _weapon;

        /// <summary>
        /// Gets the AllConstruct this weapon belongs to. Internal use only.
        /// </summary>
        internal AllConstruct AllConstruct => _allConstruct;

        // IEquatable<IWeapon> - delegates to base IBlock equality
        public bool Equals(IWeapon? other) => base.Equals(other);

        public WeaponType WeaponType
        {
            get
            {
                if (_cachedWeaponType == null)
                {
                    _cachedWeaponType = DetermineWeaponType();
                }
                return _cachedWeaponType.Value;
            }
        }

        public Vector3 AimDirection => GetAimDirection();

        public int SlotMask => _weapon.WeaponSlotMask;

        public float ProjectileSpeed => _weapon.SpeedReader;

        public AimResult AimAt(Vector3 worldPosition)
        {
            return AimAtInternal(worldPosition);
        }

        /// <summary>
        /// Internal aim method that directly aims the weapon using a position.
        /// Calculates direction from this weapon's position.
        /// </summary>
        internal AimResult AimAtInternal(Vector3 worldPosition)
        {
            var direction = (worldPosition - _weapon.GameWorldPosition).normalized;
            return AimAtDirectionInternal(direction);
        }

        /// <summary>
        /// Internal aim method that directly aims the weapon using a direction.
        /// Used when direction has been pre-calculated (e.g., for nested turrets).
        /// Aims only this specific weapon/turret, not children.
        /// </summary>
        internal AimResult AimAtDirectionInternal(Vector3 direction)
        {
            var statusReturn = new WeaponStatusReturn()
            {
                JustTheTopLevel = true
            };
            statusReturn.Setup(enumAimType.direction, direction, Vector3.zero, 0);
            _weapon.CheckDirection(statusReturn);

            // Read results from weapon status based on weapon type
            var type = DetermineWeaponType();
            if (type == WeaponType.Unknown)
            {
                return new AimResult(false, false, false);
            }

            return new AimResult(
                canFire: (statusReturn.Missile.CanFire + statusReturn.Cannon.CanFire + statusReturn.Laser.CanFire + statusReturn.Plasma.CanFire + statusReturn.ParticleCannon.CanFire + statusReturn.Flamer.CanFire) > 0,
                cantFire: (statusReturn.Missile.CantFire + statusReturn.Cannon.CantFire + statusReturn.Laser.CantFire + statusReturn.Plasma.CantFire + statusReturn.ParticleCannon.CantFire + statusReturn.Flamer.CantFire) > 0,
                canAim: (statusReturn.Missile.CanAim + statusReturn.Cannon.CanAim + statusReturn.Laser.CanAim + statusReturn.Plasma.CanAim + statusReturn.ParticleCannon.CanAim + statusReturn.Flamer.CanAim) > 0
            );
        }

        public AimResult Track(Vector3 targetPosition)
        {
            return Track(targetPosition, Vector3.zero, Vector3.zero, TrackOptions.Default).AimResult;
        }

        public AimResult Track(Vector3 targetPosition, Vector3 targetVelocity)
        {
            return Track(targetPosition, targetVelocity, Vector3.zero, TrackOptions.Default).AimResult;
        }

        public AimResult Track(Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetAcceleration)
        {
            return Track(targetPosition, targetVelocity, targetAcceleration, TrackOptions.Default).AimResult;
        }

        public TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetAcceleration, TrackOptions options)
        {
            // Calculate lead for THIS weapon only, aim only this weapon (no turret control)
            var weaponPos = _weapon.GameWorldPosition;
            var projectileSpeed = options.ProjectileSpeed ?? ProjectileSpeed;
            if (projectileSpeed <= 0) projectileSpeed = 500f;

            var constructVelocity = ParentConstruct?.Velocity ?? Vector3.zero;

            // Configure aiming module
            _aimingModule.TargetPosition = targetPosition;
            _aimingModule.TargetVelocity = targetVelocity;
            _aimingModule.TargetAcceleration = options.TargetAcceleration ?? targetAcceleration;
            _aimingModule.FirePostPosition = weaponPos;
            _aimingModule.FirePostVelocity = constructVelocity;
            _aimingModule.ProjectileVelocity = projectileSpeed;
            _aimingModule.UseGravity = options.UseGravity;

            Vector3 aimDirection;
            float flightTime = 0f;
            Vector3 aimPoint = targetPosition;
            bool isTerrainBlocking = false;

            try
            {
                bool lowArc = options.ArcPreference == ArcPreference.Low;
                aimDirection = _aimingModule.CalculateAimDirection(
                    Planet.i.World.Physics,
                    recalculateFlightTimes: true,
                    lowArcSolution: lowArc
                );
                flightTime = _aimingModule.FlightTimeSolved;
                aimPoint = _aimingModule.TargetPositionSolved;
                isTerrainBlocking = _aimingModule.IsTerrainCollisionBeforeHittingTarget;
            }
            catch
            {
                // Fallback to direct aim
                aimDirection = (targetPosition - weaponPos).normalized;
            }

            var aimResult = AimAtDirectionInternal(aimDirection);
            return new TrackResult(aimResult, flightTime, aimPoint, isTerrainBlocking);
        }

        public AimResult Track(ITargetable targetable)
        {
            if (targetable == null) return new AimResult(false, false, false);
            return Track(targetable.Position, targetable.Velocity, targetable.Acceleration, TrackOptions.Default).AimResult;
        }

        public TrackResult Track(ITargetable targetable, TrackOptions options)
        {
            if (targetable == null) return new TrackResult(new AimResult(false, false, false), 0f, Vector3.zero, false);
            return Track(targetable.Position, targetable.Velocity, targetable.Acceleration, options);
        }


        public bool Fire()
        {
            return FireInternal();
        }

        internal bool FireInternal()
        {
            _fireReturn.Setup(0, _allConstruct.GetGunnerReward());
            _fireReturn.InteractWithMissiles = true;
            _weapon.Fire(_fireReturn);
            return _fireReturn.GetFiredAny();
        }

        public bool TryFireAt(Vector3 worldPosition)
        {
            var result = AimAt(worldPosition);
            if (result.CanFire)
            {
                return Fire();
            }
            return false;
        }

        private Vector3 GetAimDirection()
        {
            if (_weapon is CannonFiringPiece cannon)
            {
                return cannon.direction;
            }
            if (_weapon is AdvCannonFiringPiece advCannon)
            {
                return advCannon.direction;
            }
            if (_weapon is PlasmaMantlet plasma)
            {
                return plasma._lerpDirection;
            }
            if (_weapon is PlasmaMantletAA plasmaAA)
            {
                return plasmaAA._lerpDirection;
            }
            if (_weapon is FlamerMain flamer)
            {
                // verify this works
                return flamer.GetRotationForFofm(true, false).Rotation * Vector3.forward;
            }
            return _weapon.GameWorldRotation * Vector3.forward;
        }

        private WeaponType DetermineWeaponType()
        {
            // First check for turrets explicitly
            if (_weapon is Turrets)
            {
                return WeaponType.Turret;
            }

            return _weapon switch
            {
                AdvCannonFiringPiece => WeaponType.APS,
                CannonFiringPiece => WeaponType.CRAM,
                MissileControl => WeaponType.Missile,
                LaserWeaponBase => WeaponType.Laser,
                PlasmaMantlet or PlasmaMantletAA => WeaponType.Plasma,
                ParticleCannon => WeaponType.ParticleCannon,
                FlamerMain => WeaponType.Flamer,
                _ => WeaponType.Unknown
            };
        }
    }
}
