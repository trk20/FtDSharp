using System;
using HarmonyLib;

namespace FtDSharp.Facades
{
    /// <summary>
    /// Manual facade for simple weapons (WWII-era cannons).
    /// No generated facade exists for WorldWarCannonBase, so this is a complete implementation.
    /// </summary>
    internal class SimpleWeaponFacade : WeaponFacade, ISimpleWeapon
    {
        private static readonly Func<WorldWarCannonBase, int>? TotalCapacityGetter =
            AccessTools.MethodDelegate<Func<WorldWarCannonBase, int>>(
                AccessTools.PropertyGetter(typeof(WorldWarCannonBase), "TotalShellsCapacity"));

        public SimpleWeaponFacade(WorldWarCannonBase weapon, AllConstruct allConstruct)
            : base(weapon, allConstruct)
        {
        }

        public int LoadedShellCount => ((WorldWarCannonBase)Weapon).LoadedShellCount;

        public int ShellCapacity
        {
            get
            {
                if (TotalCapacityGetter == null) return 0;
                return TotalCapacityGetter((WorldWarCannonBase)Weapon);
            }
        }
    }
}
