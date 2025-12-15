using System;
using System.Collections.Generic;
using UnityEngine;
using BrilliantSkies.Ftd.Missiles;
using GameMissileSize = BrilliantSkies.Ftd.Missiles.MissileSize;
using BrilliantSkies.Core.Returns.Positions;
using BrilliantSkies.Ftd.Missiles.Blueprints;
using System.Linq;
using BrilliantSkies.Ftd.Missiles.Components;

namespace FtDSharp.Facades
{
    public class MissileFacade : IMissile
    {
        private readonly Missile _missile;

        public MissileFacade(Missile missile)
        {
            _missile = missile;
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
            .Select(part => (IMissilePart)new MissilePartFacade(part))
            .ToList();

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

    }

    public class MissilePartFacade : IMissilePart
    {
        private readonly MissileComponent _component;
        public string PartType => _component.Name;

        public MissilePartFacade(MissileComponent component)
        {
            _component = component;
        }
    }
}
