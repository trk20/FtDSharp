using BrilliantSkies.Core.Logger;
using BrilliantSkies.Modding;
using BrilliantSkies.Modding.Containers;
using HarmonyLib;

namespace FtDSharp
{
    [HarmonyPatch(typeof(Configured), nameof(Configured.Done))]
    internal static class RenameBlockPatch
    {
        internal const string NewName = "Programmable Block";
        internal const string NewDescription =
            "Allows writing and running of C# code to control aspects of the vehicle. " +
            "Useful for advanced logic, automation, and custom behaviors.\n " +
            "[FtDSharp]";
        internal const string NewTooltipDescription =
            "Allows you to write and execute C# scripts to control a variety of vehicle systems";

        private static void Postfix()
        {
            var container = Configured.i.Get<ModificationComponentContainerItem>();
            var luaBoxItems = container.FindAllItemsCorrespondingToClassName("LuaBox");
            foreach (var item in luaBoxItems)
            {
                item.DisplayName.ScrapableEnglish = NewName;
                item.InventoryNameOverride.ScrapableEnglish = NewName;
                item.Description.ScrapableEnglish = NewDescription;
            }

            AdvLogger.LogInfo("[FtDSharp] Renamed Lua Box → Programmable Block");
        }
    }

    [HarmonyPatch(typeof(LuaBox), nameof(LuaBox.Secondary), new System.Type[0])]
    internal static class RenameBlockTooltipPatch
    {
        private static void Postfix(InteractionReturn __result)
        {
            if (__result == null) return;

            __result.SpecialNameField = RenameBlockPatch.NewName;
            __result.SpecialBasicDescriptionField = RenameBlockPatch.NewTooltipDescription;
        }
    }
}
