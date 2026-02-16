using HarmonyLib;

namespace FtDSharp.Facades
{
    /// <summary>
    /// Manual facade for simple weapons (WWII-era cannons).
    /// No generated facade exists for WorldWarCannonBase, so this is a complete implementation.
    /// </summary>
    internal class SimpleWeaponFacade : WeaponFacade, ISimpleWeapon
    {
        private static readonly AccessTools.FieldRef<WorldWarCannonBase, int>? TotalCapacityRef =
            AccessTools.FieldRefAccess<WorldWarCannonBase, int>("m_TotalShellsCapacity");

        public SimpleWeaponFacade(WorldWarCannonBase weapon, AllConstruct allConstruct)
            : base(weapon, allConstruct)
        {
        }

        public int LoadedShellCount => ((WorldWarCannonBase)Weapon).LoadedShellCount;

        public int ShellCapacity
        {
            get
            {
                if (TotalCapacityRef == null) return 0;
                return TotalCapacityRef((WorldWarCannonBase)Weapon);
            }
        }
    }
}
