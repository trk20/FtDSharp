using System.Collections.Generic;
using UnityEngine;

namespace FtDSharp
{
    public enum MissileSize
    {
        SMALL,
        MEDIUM,
        LARGE,
        HUGE
    }

    public interface IMissileLauncher : IWeapon
    {
        /// <summary> Size category of the missile launcher. </summary>
        public MissileSize Size { get; }
        /// <summary> Number of missiles currently loaded in the launcher. Small launcher can hold up to 4 missiles, medium/large/huge only 1. </summary>
        public int LoadedMissiles { get; }
    }

    public interface IMissilePart
    {
        public string PartType { get; }
    }


    public interface IFragWarhead : IMissilePart
    {
        /// <summary> Angle spread of the fragments in degrees, from 0 to 180. </summary>
        public float ConeAngle { get; set; }
        /// <summary> Elevation offset of the fragment cone in degrees, from -90 to 90. </summary>
        public float ElevationOffset { get; set; }
    }

    public enum MissilePropulsionMedium
    {
        /// <summary> Can only produce thrust in air. </summary>
        AIR,
        /// <summary> Can only produce thrust underwater. </summary>
        WATER
    }

    public interface IMissilePropulsionInfo : IMissilePart
    {
        /// <summary> Delay from launch before thrust starts in ticks. </summary>
        public int ThrustDelay { get; }
        /// <summary> Thrust force. </summary>
        public int MaxThrust { get; set; }
        /// <summary> Fuel burn rate. </summary>
        public int BurnRate { get; }
        /// <summary> Propulsion medium (air or water). </summary>
        public MissilePropulsionMedium Medium { get; }
    }

    public interface IShortRangeThrusterInfo : IMissilePropulsionInfo
    {
        public new MissilePropulsionMedium Medium => MissilePropulsionMedium.AIR;
    }

    public interface IVariablePropulsionPartInfo : IMissilePropulsionInfo
    {
        /// <summary> Current thrust fraction (0.0 to 1.0). </summary>
        public float ThrustFraction { get; set; }
    }

    public interface IVariableThrusterInfo : IVariablePropulsionPartInfo
    {
        public new MissilePropulsionMedium Medium => MissilePropulsionMedium.AIR;
    }

    public interface ITorpedoPropellerInfo : IVariablePropulsionPartInfo
    {
        public new MissilePropulsionMedium Medium => MissilePropulsionMedium.WATER;
    }

    public interface ISecondaryTorpedoPropellerInfo : IVariablePropulsionPartInfo
    {
        public new MissilePropulsionMedium Medium => MissilePropulsionMedium.WATER;
    }

    public interface IBallastTank : IMissilePart
    {
        /// <summary> Current buoyancy level (0.0 = empty, 1.0 = full). </summary>
        public float BuoyancyLevel { get; set; }
    }

    public interface IMissile
    {
        /// <summary> Unique missile ID. </summary>
        public int Id { get; }
        /// <summary> Whether the missile is still valid (not detonated or destroyed). </summary>
        public bool Valid { get; }
        /// <summary> Missile size. </summary>
        public MissileSize Size { get; }
        /// <summary> Missile length in meters. </summary>
        public float Length { get; }
        /// <summary> Time since launch in ticks. </summary>
        public float TimeSinceLaunch { get; }
        /// <summary> Total fuel amount. </summary>
        public float Fuel { get; }
        /// <summary> Fuel burn rate. </summary>
        public float BurnRate { get; }
        /// <summary> Missile position in world coordinates. </summary>
        public Vector3 Position { get; }
        /// <summary> Missile velocity in meters per second. </summary>
        public Vector3 Velocity { get; }
        /// <summary> Missile thrust </summary>
        public float Thrust { get; }
        /// <summary> Missile rotation in world coordinates. </summary>
        public Quaternion Rotation { get; }
        /// <summary> Missile forward direction in world coordinates. </summary>
        public Vector3 Forward { get; }
        /// <summary> Launcher that fired this missile. </summary>
        public IMissileLauncher Launcher { get; }
        /// <summary> All controllable parts on this missile. </summary>
        public List<IMissilePart> Parts { get; }
        /// <summary> Detonates the missile. </summary>
        public void Detonate();
        /// <summary> Aims the missile at the specified world coordinate point. </summary>
        /// <param name="aimPoint">World coordinate point to aim at.</param>
        public void AimAt(Vector3 aimPoint);
        /// <summary> Sets the variable thrust fraction for variable thrusters or torpedo propellers. </summary>
        public void SetVariableThrustFraction(float fraction);

    }

}