using System.Collections.Generic;
using System.Linq;
using System;

namespace FtDSharp
{
    /// <summary>
    /// Static accessor for weapon-related APIs.
    /// </summary>
    public static class Weapons
    {
        private static IReadOnlyList<IWeapon>? _lastAll;
        private static readonly Dictionary<WeaponType, IReadOnlyList<IWeapon>> _typeCache = new();

        /// <summary>
        /// Gets all weapons on the current construct (excluding turrets).
        /// Includes weapons on subconstructs (turrets, spinblocks, etc).
        /// </summary>
        public static IReadOnlyList<IWeapon> All =>
            Game.MainConstruct?.Weapons ?? Array.Empty<IWeapon>();

        /// <summary>
        /// Gets all turrets on the current construct.
        /// </summary>
        public static IReadOnlyList<ITurret> Turrets =>
            Game.MainConstruct?.Turrets ?? Array.Empty<ITurret>();

        /// <summary>
        /// Gets all APS (Advanced Projectile System) weapons.
        /// </summary>
        public static IReadOnlyList<IWeapon> APS => GetByType(WeaponType.APS);

        /// <summary>
        /// Gets all CRAM cannons.
        /// </summary>
        public static IReadOnlyList<IWeapon> CRAM => GetByType(WeaponType.CRAM);

        /// <summary>
        /// Gets all laser weapons.
        /// </summary>
        public static IReadOnlyList<IWeapon> Lasers => GetByType(WeaponType.Laser);

        /// <summary>
        /// Gets all plasma weapons.
        /// </summary>
        public static IReadOnlyList<IWeapon> Plasma => GetByType(WeaponType.Plasma);

        /// <summary>
        /// Gets all particle cannons (PAC).
        /// </summary>
        public static IReadOnlyList<IWeapon> ParticleCannons => GetByType(WeaponType.ParticleCannon);

        /// <summary>
        /// Gets all flamers.
        /// </summary>
        public static IReadOnlyList<IWeapon> Flamers => GetByType(WeaponType.Flamer);

        /// <summary>
        /// Gets all simple weapons (WWII cannons).
        /// </summary>
        public static IReadOnlyList<IWeapon> SimpleWeapons => GetByType(WeaponType.SimpleWeapon);

        /// <summary>
        /// Gets all missile controllers.
        /// </summary>
        public static IReadOnlyList<IWeapon> MissileControllers => GetByType(WeaponType.Missile);

        private static IReadOnlyList<IWeapon> GetByType(WeaponType type)
        {
            var all = All;
            if (!ReferenceEquals(all, _lastAll))
            {
                _lastAll = all;
                _typeCache.Clear();
            }

            if (!_typeCache.TryGetValue(type, out var cached))
            {
                cached = all.Where(w => w.WeaponType == type).ToList();
                _typeCache[type] = cached;
            }

            return cached;
        }
    }
}