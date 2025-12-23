using UnityEngine;

namespace FtDSharp.Facades
{
    internal class ProjectileWarningFacade : IProjectileWarning
    {
        private readonly MissileWarning _warning;

        public ProjectileWarningFacade(MissileWarning warning)
        {
            _warning = warning;
        }

        public ProjectileType Type => ConvertProjectileType(_warning.P.ProjectileType);

        public Vector3 Position => _warning.P.Position;

        public Vector3 Velocity => _warning.P.Velocity;

        public Vector3 Acceleration => _warning.Acceleration;

        public float Diameter => _warning.P.Diameter;

        // public float Length => _warning.P.Length;

        // public float Health => _warning.P.Health;

        public float TimeSinceFiring => _warning.P.TimeOfFiring.Since;

        public float TimeSinceLastSpotted => _warning.LastSpotted.Since;

        public bool IsFake => _warning.IsFake;

        public int ShotsFiredAt => _warning.ShotsFiredAt;

        public int CiwsAimingAt => _warning.NumberOfCiwsAimingAt;

        private static ProjectileType ConvertProjectileType(enumProjectileType gameType)
        {
            return gameType switch
            {
                enumProjectileType.missile => ProjectileType.Missile,
                enumProjectileType.cram => ProjectileType.Cram,
                // APS shells appear as NA for some reason
                enumProjectileType.NA => ProjectileType.Shell,
                _ => ProjectileType.Unknown
            };
        }
    }
}
