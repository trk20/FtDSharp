using System.Collections.Generic;
using System.Linq;

namespace FtDSharp;

/// <summary>
/// Provides typed access to controlled weapons and turrets.
/// </summary>
public class ControlledItems
{
    internal ControlledItems(IEnumerable<IWeapon> weapons, IEnumerable<ITurret> turrets)
    {
        Weapons = weapons.ToList();
        Turrets = turrets.ToList();
        All = Turrets.Cast<IWeapon>().Concat(Weapons).ToList();
    }

    /// <summary>All weapons (excluding turrets) controlled by this controller.</summary>
    public IReadOnlyList<IWeapon> Weapons { get; }

    /// <summary>All turrets controlled by this controller.</summary>
    public IReadOnlyList<ITurret> Turrets { get; }

    /// <summary>All items (weapons and turrets) controlled by this controller.</summary>
    public IReadOnlyList<IWeapon> All { get; }

    /// <summary>Total count of all controlled items.</summary>
    public int Count => All.Count;
}
