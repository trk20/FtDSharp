using System;
using BrilliantSkies.Core.Ballistics.External.AimingWithoutDrag;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Ftd.Planets;
using Ftd.Blocks.Flamers;
using HarmonyLib;
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

        // State from last aim/track operation (null if none this frame)
        // Reset per-frame via facade cache (new facade instance each frame)
        private TrackResult? _lastResult;

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

        /// <summary>
        /// Checks if this weapon is ready to fire based on weapon-type-specific conditions.
        /// Note: Future improvement - expose the specific reason why a weapon can't fire.
        /// </summary>
        public bool IsReady => CheckIsReady();

        // --- State properties from last Track/AimAt call ---
        public bool OnTarget => _lastResult?.IsOnTarget ?? false;
        public bool CanAim => _lastResult?.CanAim ?? false;
        public bool IsBlocked => _lastResult?.AimResult.IsBlocked ?? false;
        public bool CanFire => _lastResult?.CanFire ?? false;
        public float FlightTime => _lastResult?.FlightTime ?? 0f;
        public Vector3 AimPoint => _lastResult?.AimPoint ?? Vector3.zero;
        public bool BlockedByTerrain => _lastResult?.IsTerrainBlocking ?? false;

        /// <summary>
        /// Stores the tracking result state. Called internally after Track operations.
        /// </summary>
        internal void SetTrackState(TrackResult result)
        {
            _lastResult = result;
        }

        /// <summary>
        /// Stores the aim result state (creates TrackResult with aim data only).
        /// </summary>
        internal void SetAimState(AimResult result)
        {
            _lastResult = new TrackResult(result, 0f, Vector3.zero, false, IsReady);
        }

        public AimResult AimAt(Vector3 worldPosition)
        {
            var result = AimAtInternal(worldPosition);
            SetAimState(result);
            return result;
        }

        /// <summary>
        /// Internal aim method that directly aims the weapon using a position.
        /// Calculates direction from this weapon's position.
        /// Does NOT update state properties - caller must do that.
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
            var statusReturn = new WeaponStatusReturn();
            statusReturn.Setup(enumAimType.direction, direction, Vector3.zero, 0);
            statusReturn.JustTheTopLevel = true; // Setup sets this to false so can't set with constructor
            _weapon.CheckDirection(statusReturn);

            // Read results from weapon status based on weapon type
            var type = DetermineWeaponType();
            if (type == WeaponType.Unknown)
            {
                return new AimResult(false, false, false);
            }
            return new AimResult(
                isOnTarget: (statusReturn.Missile.CanFire + statusReturn.Cannon.CanFire + statusReturn.Laser.CanFire + statusReturn.Plasma.CanFire + statusReturn.ParticleCannon.CanFire + statusReturn.Flamer.CanFire) > 0,
                isBlocked: (statusReturn.Missile.CantFire + statusReturn.Cannon.CantFire + statusReturn.Laser.CantFire + statusReturn.Plasma.CantFire + statusReturn.ParticleCannon.CantFire + statusReturn.Flamer.CantFire) > 0,
                canAim: (statusReturn.Missile.CanAim + statusReturn.Cannon.CanAim + statusReturn.Laser.CanAim + statusReturn.Plasma.CanAim + statusReturn.ParticleCannon.CanAim + statusReturn.Flamer.CanAim) > 0
            );
        }

        public AimResult Track(Vector3 targetPosition)
        {
            return Track(targetPosition, Vector3.zero, Vector3.zero, TrackOptions.Default).AimResult;
        }

        public TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity)
        {
            return Track(targetPosition, targetVelocity, Vector3.zero, TrackOptions.Default);
        }

        public TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetAcceleration)
        {
            return Track(targetPosition, targetVelocity, targetAcceleration, TrackOptions.Default);
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
                bool lowArc = options.ArcPreference == ArcPreference.PreferLow || options.ArcPreference == ArcPreference.OnlyLow;
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
            var trackResult = new TrackResult(aimResult, flightTime, aimPoint, isTerrainBlocking, IsReady);
            SetTrackState(trackResult);
            return trackResult;
        }

        public TrackResult Track(ITargetable targetable)
        {
            if (targetable == null) return new TrackResult(new AimResult(false, false, false), 0f, Vector3.zero, false, false);
            return Track(targetable.Position, targetable.Velocity, targetable.Acceleration, TrackOptions.Default);
        }

        public TrackResult Track(ITargetable targetable, TrackOptions options)
        {
            if (targetable == null) return new TrackResult(new AimResult(false, false, false), 0f, Vector3.zero, false, false);
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
            if (result.IsOnTarget && IsReady)
            {
                return Fire();
            }
            return false;
        }

        private Vector3 GetAimDirection()
        {
            if (_weapon is Turrets turret)
            {
                return turret.GameWorldRotation * turret.FiringArc.LastGoodLocalDirection;
            }
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
                return flamer.GameWorldRotation * flamer.FiringArc.LastGoodLocalDirection; // I can't find a better way for flamers
            }
            return _weapon.GameWorldRotation * Vector3.forward;
        }

        private WeaponType DetermineWeaponType() => _weapon switch
        {
            Turrets => WeaponType.Turret,
            AdvCannonFiringPiece => WeaponType.APS,
            CannonFiringPiece => WeaponType.CRAM,
            MissileControl => WeaponType.Missile,
            LaserWeaponBase => WeaponType.Laser,
            PlasmaMantlet or PlasmaMantletAA => WeaponType.Plasma,
            ParticleCannon => WeaponType.ParticleCannon,
            FlamerMain => WeaponType.Flamer,
            _ => WeaponType.Unknown
        };

        #region IsReady Check Implementation
        // Field accessors for private fields using Harmony's AccessTools
        // These are cached at class level for performance

        // APS fields
        private static readonly AccessTools.FieldRef<AdvCannonFiringPiece, float>? ApsRailEnergy =
            AccessTools.FieldRefAccess<AdvCannonFiringPiece, float>("_railEnergy");

        // Plasma fields  
        private static readonly AccessTools.FieldRef<PlasmaMantlet, int>? PlasmaCurrentBurstShots =
            AccessTools.FieldRefAccess<PlasmaMantlet, int>("_currentBurstShots");
        private static readonly AccessTools.FieldRef<PlasmaMantletAA, int>? PlasmaAACurrentBurstShots =
            AccessTools.FieldRefAccess<PlasmaMantletAA, int>("_currentBurstShots");

        // Flamer fields
        private static readonly AccessTools.FieldRef<FlamerMain, float>? FlamerReloadFuelNeeded =
            AccessTools.FieldRefAccess<FlamerMain, float>("_reloadFuelNeeded");
        private static readonly AccessTools.FieldRef<FlamerMain, float>? FlamerCurrentFuel =
            AccessTools.FieldRefAccess<FlamerMain, float>("_currentFuel");

        /// <summary>
        /// Checks weapon-specific conditions to determine if the weapon is ready to fire.
        /// Based on internal game WeaponFire methods for each weapon type.
        /// Note: Future improvement - expose the specific reason why a weapon can't fire.
        /// </summary>
        private bool CheckIsReady()
        {
            return _weapon switch
            {
                AdvCannonFiringPiece aps => CheckApsReady(aps),
                CannonFiringPiece cram => CheckCramReady(cram),
                LaserWeaponBase laser => CheckLaserReady(laser),
                PlasmaMantletAA plasmaAA => CheckPlasmaAAReady(plasmaAA),
                PlasmaMantlet plasma => CheckPlasmaReady(plasma),
                FlamerMain flamer => CheckFlamerReady(flamer),
                MissileControl => true, // Missile controller checking tbd
                ParticleCannon pac => pac.PCLoaded && pac.WeaponSyncData.SyncFireReady(pac, pac.CheckTimes.LastFireTime),
                Turrets => true, // Turrets delegate to their weapons
                _ => true // Unknown weapons assumed ready
            };
        }

        private bool CheckApsReady(AdvCannonFiringPiece aps)
        {
            try
            {
                // Check barrel exists
                if (aps.BarrelSystem.BarrelLength == 0) return false;

                // Check gauge increases
                if (!aps.Node.HasEnoughGaugeIncreases) return false;

                // Check shell ammo
                if (aps.Node.ShellRacks.ShellCount == 0) return false;

                // Check shell is loaded
                var nextShell = aps.Node.ShellRacks.GetNextShell(false);
                if (nextShell == null) return false;

                // Check barrel is ready
                var nextBarrel = aps.BarrelSystem.GetNextBarrelReady();
                if (nextBarrel == null) return false;

                // Check weapon sync
                if (!aps.WeaponSyncData.SyncFireReady(aps, aps.CheckTimes.LastFireTime)) return false;

                // Check railgun energy (if applicable)
                if (aps.Node.RailgunCapacity > 0f && ApsRailEnergy != null)
                {
                    var energy = _allConstruct.Main.GetForce().Energy;
                    var railEnergy = ApsRailEnergy(aps);
                    var energyNeeded = Math.Min(aps.Data.EnergyToUse, nextShell.Propellant.MaxRailDraw);

                    // Check if semi-charged firing is allowed
                    if (!aps.Data.AllowSemiChargedRailFiring && railEnergy < energyNeeded)
                        return false;

                    // Check minimum energy threshold
                    if (aps.Data.DontFireBelow > energy.GetFraction(0f))
                        return false;
                }

                return true;
            }
            catch
            {
                return true; // Fallback to ready if check fails
            }
        }

        private bool CheckCramReady(CannonFiringPiece cram)
        {
            try
            {
                // Check ammo
                if (cram.Node.Stats.TotalPerSec == 0) return false;

                // Check packing
                if (cram.PackTime < cram.Node.Stats.MaxPackTime * cram.CramData.MinimumPackPercentage / 100f - 0.001f)
                    return false;

                // Check weapon sync
                if (!cram.WeaponSyncData.SyncFireReady(cram, cram.CheckTimes.LastFireTime)) return false;

                return true;
            }
            catch
            {
                return true;
            }
        }

        private bool CheckLaserReady(LaserWeaponBase laser)
        {
            try
            {
                // Check weapon sync
                if (!laser.WeaponSyncData.SyncFireReady(laser, laser.CheckTimes.LastFireTime)) return false;

                // Note: Ammo check would require accessing internal storage methods
                // The weapon's internal CanFire already accounts for basic ammo availability

                return true;
            }
            catch
            {
                return true;
            }
        }

        private bool CheckPlasmaReady(PlasmaMantlet plasma)
        {
            try
            {
                // Check node and chamber
                if (plasma.Node == null || !plasma.IsReady || plasma.Node.ChamberData.ChargeCapacity == 0)
                    return false;

                // Check charges ready
                int projectileCount = plasma.GetProjectileCount();
                int chargeTarget = plasma.GetCurrentChargeTarget(plasma.IsFullyCharged || plasma.IsStalled);
                int currentBurstShots = PlasmaCurrentBurstShots != null ? PlasmaCurrentBurstShots(plasma) : 0;
                int chargesNeeded = projectileCount * chargeTarget * (plasma.CannonData.BurstSize.Us - currentBurstShots);

                if (plasma.ChargesReady < chargesNeeded && !plasma.IsFullyCharged && !plasma.IsStalled)
                    return false;

                // Check temperature
                if (plasma.Temperature > 1000f ||
                    (plasma.Temperature > plasma.CannonData.TemperatureLimit.Us && currentBurstShots >= plasma.CannonData.BurstSize.Us))
                    return false;

                // Check weapon sync
                if (!plasma.WeaponSyncData.SyncFireReady(plasma, plasma.CheckTimes.LastFireTime)) return false;

                // Check energy
                float acceleratorEnergy = plasma.GetAcceleratorEnergyPerCharge();
                float energyMultiplier = (int)plasma.EmitterType == 1 ? 0.5f : 1f; // Destabilizer type
                float energyNeeded = energyMultiplier * projectileCount * chargeTarget * acceleratorEnergy;

                var energy = _allConstruct.Main.GetForce().Energy;
                if (energy.Quantity < energyNeeded) return false;

                return true;
            }
            catch
            {
                return true;
            }
        }

        private bool CheckPlasmaAAReady(PlasmaMantletAA plasma)
        {
            try
            {
                // Check node and chamber
                if (plasma.Node == null || !plasma.IsReady || plasma.Node.ChamberData.ChargeCapacity == 0)
                    return false;

                int projectileCount = plasma.GetProjectileCount();
                int chargeTarget = plasma.GetCurrentChargeTarget(plasma.IsFullyCharged || plasma.IsStalled);
                int currentBurstShots = PlasmaAACurrentBurstShots != null ? PlasmaAACurrentBurstShots(plasma) : 0;
                int chargesNeeded = projectileCount * chargeTarget * (plasma.CannonData.BurstSize.Us - currentBurstShots);

                if (plasma.ChargesReady < chargesNeeded && !plasma.IsFullyCharged && !plasma.IsStalled)
                    return false;

                if (plasma.Temperature > 1000f ||
                    (plasma.Temperature > plasma.CannonData.TemperatureLimit.Us && currentBurstShots >= plasma.CannonData.BurstSize.Us))
                    return false;

                if (!plasma.WeaponSyncData.SyncFireReady(plasma, plasma.CheckTimes.LastFireTime)) return false;

                float acceleratorEnergy = plasma.GetAcceleratorEnergyPerCharge();
                float energyMultiplier = (int)plasma.EmitterType == 1 ? 0.5f : 1f;
                float energyNeeded = energyMultiplier * projectileCount * chargeTarget * acceleratorEnergy;

                var energy = _allConstruct.Main.GetForce().Energy;
                if (energy.Quantity < energyNeeded) return false;

                return true;
            }
            catch
            {
                return true;
            }
        }

        private bool CheckFlamerReady(FlamerMain flamer)
        {
            try
            {
                // Check fuel using private field accessors
                if (FlamerReloadFuelNeeded != null && FlamerCurrentFuel != null)
                {
                    var reloadNeeded = FlamerReloadFuelNeeded(flamer);
                    var currentFuel = FlamerCurrentFuel(flamer);

                    if (reloadNeeded > 0.001f || flamer.Range < 100 || currentFuel == 0f)
                        return false;
                }

                // Check position (not underwater)
                if (flamer.GetFirePoint(0f).y < 0.5f)
                    return false;

                return true;
            }
            catch
            {
                return true;
            }
        }

        #endregion
    }
}
