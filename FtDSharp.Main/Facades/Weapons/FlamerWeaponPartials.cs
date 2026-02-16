using Ftd.Blocks.Flamers;
using HarmonyLib;
using UnityEngine;

namespace FtDSharp.Facades
{
    // Both FlamerMainForward and FlamerMainSwivel extend FlamerMain,
    // so the bespoke property implementations are identical.
    // Generated facades already provide FlameColor.

    internal partial class FlamerMainForwardFacade : IFlamerWeapon
    {
        private static readonly AccessTools.FieldRef<FlamerMain, float>? CurrentFuelRef =
            AccessTools.FieldRefAccess<FlamerMain, float>("_currentFuel");

        private static readonly AccessTools.FieldRef<FlamerMain, float>? ReloadFuelNeededRef =
            AccessTools.FieldRefAccess<FlamerMain, float>("_reloadFuelNeeded");

        public float Range => ((FlamerMainForward)Weapon).Range;

        public float CurrentFuel
        {
            get
            {
                if (CurrentFuelRef == null) return 0f;
                return CurrentFuelRef((FlamerMainForward)Weapon);
            }
        }

        public float FuelPerShot
        {
            get
            {
                if (ReloadFuelNeededRef == null) return 0f;
                return ReloadFuelNeededRef((FlamerMainForward)Weapon);
            }
        }

        public float Intensity
        {
            get
            {
                var flamer = (FlamerMainForward)Weapon;
                return FlamerConstants.GetIntensity(flamer.Node.FuelPerSec, flamer.Node.CatalystPerSec);
            }
        }

        public float OxidizerPerSec => ((FlamerMainForward)Weapon).Node.OxidizerPerSec;

        public float FuelPerSec => ((FlamerMainForward)Weapon).Node.FuelPerSec;
    }

    internal partial class FlamerMainSwivelFacade : IFlamerWeapon
    {
        private static readonly AccessTools.FieldRef<FlamerMain, float>? SwivelCurrentFuelRef =
            AccessTools.FieldRefAccess<FlamerMain, float>("_currentFuel");

        private static readonly AccessTools.FieldRef<FlamerMain, float>? SwivelReloadFuelNeededRef =
            AccessTools.FieldRefAccess<FlamerMain, float>("_reloadFuelNeeded");

        public float Range => ((FlamerMainSwivel)Weapon).Range;

        public float CurrentFuel
        {
            get
            {
                if (SwivelCurrentFuelRef == null) return 0f;
                return SwivelCurrentFuelRef((FlamerMainSwivel)Weapon);
            }
        }

        public float FuelPerShot
        {
            get
            {
                if (SwivelReloadFuelNeededRef == null) return 0f;
                return SwivelReloadFuelNeededRef((FlamerMainSwivel)Weapon);
            }
        }

        public float Intensity
        {
            get
            {
                var flamer = (FlamerMainSwivel)Weapon;
                return FlamerConstants.GetIntensity(flamer.Node.FuelPerSec, flamer.Node.CatalystPerSec);
            }
        }

        public float OxidizerPerSec => ((FlamerMainSwivel)Weapon).Node.OxidizerPerSec;

        public float FuelPerSec => ((FlamerMainSwivel)Weapon).Node.FuelPerSec;
    }
}
