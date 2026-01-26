using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BrilliantSkies.Ftd.Missiles;
using GameMissileSize = BrilliantSkies.Ftd.Missiles.MissileSize;
using BrilliantSkies.Core.Returns.Positions;
using BrilliantSkies.Ftd.Missiles.Blueprints;
using System.Linq;
using BrilliantSkies.Ftd.Missiles.Components;
using BrilliantSkies.Core.Particles;
using BrilliantSkies.Core.Logger;

namespace FtDSharp.Facades
{
    public class MissileFacade : IMissile
    {
        private readonly Missile _missile;

        public MissileFacade(Missile missile)
        {
            _missile = missile;
            Trail = new MissileTrailImpl(this);
            Flame = new MissileFlameImpl(this);
            EngineLight = new MissileEngineLightImpl(this);
        }

        public int Id => _missile.UniqueId;
        public bool Valid => _missile != null && _missile.IsAlive();
        public MissileSize Size => _missile.Blueprint.Size switch
        {
            var size when size == GameMissileSize.S => MissileSize.SMALL,
            var size when size == GameMissileSize.M => MissileSize.MEDIUM,
            var size when size == GameMissileSize.L => MissileSize.LARGE,
            var size when size == GameMissileSize.H => MissileSize.HUGE,
            _ => throw new ArgumentOutOfRangeException()
        };
        public float Length => _missile.Length;
        public float TimeSinceLaunch => _missile.TimeSinceLaunch;
        public float Fuel => _missile.Fuel;
        public float Thrust => GetThrust();
        public float BurnRate => GetBurnRate();
        public Vector3 Position => _missile.Position;
        public Vector3 Velocity => _missile.Velocity;
        public Quaternion Rotation => Quaternion.LookRotation(_missile.Forward, Vector3.up); // approx
        public Vector3 Forward => _missile.Forward;
        public IMissileLauncher Launcher => null!; // todo: reference launcher

        public List<IMissilePart> Parts => _missile.Blueprint.Components
            .Select(part => MissilePartFactory.CreateFacade(part)!)
            .ToList();

        /// <inheritdoc />
        public IEnumerable<T> GetParts<T>() where T : class, IMissilePart
            => Parts.OfType<T>();

        /// <inheritdoc />
        public T? GetPart<T>() where T : class, IMissilePart
            => Parts.OfType<T>().FirstOrDefault();

        /// <inheritdoc />
        public bool HasPart<T>() where T : class, IMissilePart
            => Parts.OfType<T>().Any();

        public void Detonate()
        {
            _missile.ExplosionHandler.ExplodeNow();
        }

        public void AimAt(Vector3 aimPoint)
        {
            var receiver = _missile.Blueprint.LuaReceiver;
            var error = receiver.ErrorSeed;
            // todo: figure out how to apply error without being easily avoided
            // stability/detection error already applied to target position, need something for ECM/GPP error?
            var posReturn = new PositionReturnPosition(() => aimPoint + error);
            var target = new MissileTarget("LuaScript", posReturn, MissileTargetPriority.GuidancePoint);
            _missile.TargetManager.SetTarget(target, MissileTargetPriorityUse.ApplyIfHigherOrSamePriority);
        }

        private float GetThrust()
        {
            if (_missile.Blueprint.IsMissile && !_missile.Blueprint.Thruster.IsUnderWater)
            {
                return _missile.Blueprint.Thruster.ScaledThrust * _missile.GetHealthDependency(HealthDependency.Thrust);
            }
            if (_missile.Blueprint.IsTorpedo && _missile.Blueprint.Propeller.IsUnderWater)
            {
                return _missile.Blueprint.Propeller.ScaledThrust * _missile.GetHealthDependency(HealthDependency.Thrust);
            }
            return 0;
        }

        private float GetBurnRate()
        {
            if (_missile.Blueprint.IsMissile && !_missile.Blueprint.Thruster.IsUnderWater)
            {
                return _missile.Blueprint.Thruster.ScaledThrust * _missile.Blueprint.Thruster.FuelPerThrust * _missile.GetHealthDependency(HealthDependency.Thrust);
            }
            if (_missile.Blueprint.IsTorpedo && _missile.Blueprint.Propeller.IsUnderWater)
            {
                return _missile.Blueprint.Propeller.ScaledThrust * _missile.Blueprint.Propeller.FuelPerThrust * _missile.GetHealthDependency(HealthDependency.Thrust);
            }
            return 0;
        }

        public void SetVariableThrustFraction(float fraction)
        {
            if (_missile.Blueprint.IsMissile && _missile.Blueprint.Thruster is MissileVariableThrustThruster thruster)
            {
                _missile.Blueprint.Thruster.SetParameterValue(1, thruster.MaxThrust * fraction);
            }
            if (_missile.Blueprint.IsTorpedo)
            {
                _missile.Blueprint.Propeller.SetParameterValue(1, _missile.Blueprint.Propeller.MaxThrust * fraction);
            }
        }

        // ===== Propulsion Visual Control =====
        // Uses reflection to access the protected _effectSystem field on MissilePropulsion

        private static readonly FieldInfo? _effectSystemField = typeof(MissilePropulsion)
            .GetField("_effectSystem", BindingFlags.NonPublic | BindingFlags.Instance);

        internal MissilePropulsion? GetActivePropulsion()
        {
            if (_missile.Blueprint.IsMissile && !_missile.Blueprint.Thruster.IsUnderWater)
                return _missile.Blueprint.Thruster;
            if (_missile.Blueprint.IsTorpedo && _missile.Blueprint.Propeller.IsUnderWater)
                return _missile.Blueprint.Propeller;
            return null;
        }

        internal AdvancedJetEffects? GetAdvancedJetEffects()
        {
            var propulsion = GetActivePropulsion();
            if (propulsion == null || _effectSystemField == null)
                return null;
            return _effectSystemField.GetValue(propulsion) as AdvancedJetEffects;
        }

        /// <inheritdoc />
        public IMissileTrail Trail { get; }
        /// <inheritdoc />
        public IMissileFlame Flame { get; }
        /// <inheritdoc />
        public IMissileEngineLight EngineLight { get; }

        internal class MissileTrailImpl : IMissileTrail
        {
            private readonly MissileFacade _parent;
            public MissileTrailImpl(MissileFacade parent) => _parent = parent;

            public TrailType Variant => _parent.GetActivePropulsion()?.IsIonParameter.Us > 0.5f
                ? TrailType.Ion : TrailType.Smoke;

            public Color Color
            {
                set
                {
                    if (Variant == TrailType.Ion)
                    {
                        _parent.GetAdvancedJetEffects()?.SetTrailColor(value);
                    }
                    else
                    {
                        var propulsion = _parent.GetActivePropulsion();
                        if (propulsion != null)
                        {
                            propulsion.parameters.SmokeColor.Locked = false;
                            propulsion.parameters.SmokeColor.Us = value;
                        }
                    }
                }
            }

            public bool Enabled
            {
                set
                {
                    if (Variant == TrailType.Ion)
                        _parent.GetAdvancedJetEffects()?.EnableTrail(value);
                    else
                        _parent.GetAdvancedJetEffects()?.EnableSmoke(value);
                }
            }
        }

        internal class MissileFlameImpl : IMissileFlame
        {
            private readonly MissileFacade _parent;
            public MissileFlameImpl(MissileFacade parent) => _parent = parent;

            public Color Color
            {
                set => _parent.GetAdvancedJetEffects()?.SetFlameColor(value);
            }

            public bool Enabled
            {
                set => _parent.GetAdvancedJetEffects()?.EnableFlame(value);
            }
        }

        internal class MissileEngineLightImpl : IMissileEngineLight
        {
            private readonly MissileFacade _parent;
            public MissileEngineLightImpl(MissileFacade parent) => _parent = parent;

            public Color Color
            {
                set => _parent.GetAdvancedJetEffects()?.SetLightColorOnly(value);
            }

            public bool Enabled
            {
                set => _parent.GetAdvancedJetEffects()?.EnableLight(value);
            }
        }

    }
}
