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
        public static IReadOnlyList<IWeapon> APS => All.Where(w => w.WeaponType == WeaponType.APS).ToList();

        /// <summary>
        /// Gets all CRAM cannons.
        /// </summary>
        public static IReadOnlyList<IWeapon> CRAM => All.Where(w => w.WeaponType == WeaponType.CRAM).ToList();

        /// <summary>
        /// Gets all laser weapons.
        /// </summary>
        public static IReadOnlyList<IWeapon> Lasers => All.Where(w => w.WeaponType == WeaponType.Laser).ToList();

        /// <summary>
        /// Gets all plasma weapons.
        /// </summary>
        public static IReadOnlyList<IWeapon> Plasma => All.Where(w => w.WeaponType == WeaponType.Plasma).ToList();

        /// <summary>
        /// Gets all particle cannons (PAC).
        /// </summary>
        public static IReadOnlyList<IWeapon> ParticleCannons => All.Where(w => w.WeaponType == WeaponType.ParticleCannon).ToList();

        /// <summary>
        /// Gets all flamers.
        /// </summary>
        public static IReadOnlyList<IWeapon> Flamers => All.Where(w => w.WeaponType == WeaponType.Flamer).ToList();

        /// <summary>
        /// Gets all missile launchers.
        /// </summary>
        public static IReadOnlyList<IWeapon> MissileControllers => All.Where(w => w.WeaponType == WeaponType.Missile).ToList();
    }
}