using System;
using System.Collections.Generic;
using System.Linq;
using FtDSharp.Facades;

namespace FtDSharp
{
    /// <summary>
    /// Static accessor for projectile warning information.
    /// </summary>
    public static class Warnings
    {
        private static readonly FrameCache<IReadOnlyList<IProjectileWarning>> _all = new(GetWarnings);

        private static readonly FrameCache<IReadOnlyList<IProjectileWarning>> _missiles = new(
            () => IncomingProjectiles.Where(w => w.Type == ProjectileType.Missile).ToList());

        private static readonly FrameCache<IReadOnlyList<IProjectileWarning>> _shells = new(
            () => IncomingProjectiles.Where(w => w.Type == ProjectileType.Shell || w.Type == ProjectileType.Cram).ToList());

        /// <summary>
        /// All valid incoming projectile warnings.
        /// </summary>
        public static IReadOnlyList<IProjectileWarning> IncomingProjectiles => _all.Value;

        /// <summary>
        /// Incoming missiles only (includes harpoons).
        /// </summary>
        public static IReadOnlyList<IProjectileWarning> IncomingMissiles => _missiles.Value;

        /// <summary>
        /// Incoming shells only (APS shells and CRAM).
        /// </summary>
        public static IReadOnlyList<IProjectileWarning> IncomingShells => _shells.Value;

        /// <summary>
        /// Get warnings filtered by a specific projectile type.
        /// </summary>
        public static IEnumerable<IProjectileWarning> GetByType(ProjectileType type)
        {
            return IncomingProjectiles.Where(w => w.Type == type);
        }

        private static IReadOnlyList<IProjectileWarning> GetWarnings()
        {
            var construct = ScriptApi.Context?.RawAllConstruct as MainConstruct;
            if (construct?.MWM?.Warnings == null) return Array.Empty<IProjectileWarning>();

            return construct.MWM.Warnings
                .Where(warning => warning?.IsValid ?? false)
                .Select(warning => new ProjectileWarningFacade(warning))
                .Cast<IProjectileWarning>()
                .ToList();
        }
    }
}
