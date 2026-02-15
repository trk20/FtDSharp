using System.Collections.Generic;
using System.Linq;

namespace FtDSharp
{
    /// <summary>
    /// Provides typed access to controlled weapons and turrets.
    /// </summary>
    public class ControlledItems
    {
        private readonly IReadOnlyList<IWeapon> _weapons;
        private readonly IReadOnlyList<ITurret> _turrets;
        private readonly IReadOnlyList<IWeapon> _all;

        internal ControlledItems(IEnumerable<IWeapon> weapons, IEnumerable<ITurret> turrets)
        {
            _weapons = weapons.ToList();
            _turrets = turrets.ToList();
            _all = _turrets.Cast<IWeapon>().Concat(_weapons).ToList();
        }

        /// <summary>All weapons (excluding turrets) controlled by this controller.</summary>
        public IReadOnlyList<IWeapon> Weapons => _weapons;

        /// <summary>All turrets controlled by this controller.</summary>
        public IReadOnlyList<ITurret> Turrets => _turrets;

        /// <summary>All items (weapons and turrets) controlled by this controller.</summary>
        public IReadOnlyList<IWeapon> All => _all;

        /// <summary>Total count of all controlled items.</summary>
        public int Count => _all.Count;
    }
}
