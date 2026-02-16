using System;
using UnityEngine;

namespace FtDSharp.Facades
{
    internal sealed class TargetFacade : ITarget
    {
        private readonly TargetObject _target;
        private MainConstruct Main => (_target.C as AllConstruct)!.Main;
        private ConstructableSpecialInfo CSI => Main.GetForce().CSI;

        public TargetFacade(TargetObject target)
        {
            _target = target;
        }

        public int UniqueId => _target.C.UniqueId;
        public string Name => _target.C.GetName();
        public Vector3 Position => _target.GetTargetCom();
        public Vector3 Velocity => _target.GetTargetVelocity();
        public Vector3 Acceleration => _target.GetTargetAcceleration();
        public float Volume => _target.C.Volume;

        // Properties usually only exposed via AI target priority
        public float Stability => Main.StabilityFactor;
        public float PositionError => Vector3.Distance(Position, _target.C.CentreOfMass);
        public int BlockCount => Main.AllBasics.GetNumberBlocksIncludingSubConstructables();
        public int AliveBlockCount => Main.AllBasics.GetNumberAliveBlocksIncludingSubConstructables(); // player can determine fraction of construct alive if desired

        public float TotalFirePower => CSI.TotalFirepower;

        public float ApsPower => CSI.ApsPower;

        public float CramPower => CSI.CramPower;

        public float MissilePower => CSI.MissilePower;

        public float LaserPower => CSI.LaserPower;

        public float PacPower => CSI.PacPower;

        public float PlasmaPower => CSI.PlasmaPower;
        public float FlamerPower => CSI.FlamerPower;

        public float SimpleCannonPower => CSI.SimpleCannonPower;

        public float SimpleLaserPower => CSI.SimpleLaserPower;

        public float MeleePower => CSI.MeleePower;
        public float ArmorCostPercent => CSI.ArmorCostPercent;

        public float PropulsionScore => Main.ThrustersRestricted.GetPropulsionScore();

        public float PowerScore => Main.PowerUsageCreationAndFuelRestricted.GetEngineScore();

        public float AICount => Main.Owner.GetCrewScore();
    }
}
