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
        private static readonly Dictionary<Type, object> _interfaceCache = new();

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
        public static IReadOnlyList<IApsWeapon> APS => GetByInterface<IApsWeapon>();

        /// <summary>
        /// Gets all CRAM cannons.
        /// </summary>
        public static IReadOnlyList<ICramWeapon> CRAM => GetByInterface<ICramWeapon>();

        /// <summary>
        /// Gets all laser weapons.
        /// </summary>
        public static IReadOnlyList<ILaserWeapon> Lasers => GetByInterface<ILaserWeapon>();

        /// <summary>
        /// Gets all plasma weapons.
        /// </summary>
        public static IReadOnlyList<IPlasmaWeapon> Plasma => GetByInterface<IPlasmaWeapon>();

        /// <summary>
        /// Gets all particle cannons (PAC).
        /// </summary>
        public static IReadOnlyList<IParticleWeapon> ParticleCannons => GetByInterface<IParticleWeapon>();

        /// <summary>
        /// Gets all flamers.
        /// </summary>
        public static IReadOnlyList<IFlamerWeapon> Flamers => GetByInterface<IFlamerWeapon>();

        /// <summary>
        /// Gets all simple weapons.
        /// </summary>
        public static IReadOnlyList<ISimpleWeapon> SimpleWeapons => GetByInterface<ISimpleWeapon>();

        /// <summary>
        /// Gets all missile controllers.
        /// </summary>
        public static IReadOnlyList<IMissileController> MissileControllers => GetByInterface<IMissileController>();

        private static IReadOnlyList<T> GetByInterface<T>() where T : class, IWeapon
        {
            var all = All;
            if (!ReferenceEquals(all, _lastAll))
            {
                _lastAll = all;
                _typeCache.Clear();
                _interfaceCache.Clear();
            }

            var key = typeof(T);
            if (_interfaceCache.TryGetValue(key, out var cached))
                return (IReadOnlyList<T>)cached;

            var result = all.OfType<T>().ToList();
            _interfaceCache[key] = result;
            return result;
        }
    }
}