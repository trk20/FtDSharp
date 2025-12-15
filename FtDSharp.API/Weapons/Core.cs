using System.Collections.Generic;
using UnityEngine;
using FtDSharp;

namespace FtDSharp
{
    /// <summary>
    /// Static accessor for weapon-related APIs.
    /// Use with: using static FtDSharp.Weapons;
    /// </summary>
    public static class Weapons
    {
        /// <summary>
        /// Gets all missiles belonging to the current construct.
        /// </summary>
        public static List<IMissile> Missiles => Game.MainConstruct?.Missiles ?? new List<IMissile>();
    }

    public interface IWeapon : IBlock
    {
        /// <summary> Attempts to fire the weapon. Returns true if the weapon fired successfully. </summary>
        public bool Fire();
    }

    public interface IAimableWeapon : IWeapon
    {
        /// <summary> Attempts to aim the weapon at the specified world position. Returns true if the weapon is aimed at the target. </summary>
        public bool AimAt(Vector3 worldPosition);
        public Vector3 AimDirection { get; }
    }

}