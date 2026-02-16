using HarmonyLib;

namespace FtDSharp.Facades
{
    internal partial class ApsFiringPieceFacade : IApsWeapon
    {
        private static readonly AccessTools.FieldRef<AdvCannonFiringPiece, float>? RailEnergy =
            AccessTools.FieldRefAccess<AdvCannonFiringPiece, float>("_railEnergy");

        public float Gauge => ((AdvCannonFiringPiece)Weapon).APSShellDiameter * 1000f;

        public int ShellCount => ((AdvCannonFiringPiece)Weapon).ShellCountReader;

        public bool IsLoaded
        {
            get
            {
                var aps = (AdvCannonFiringPiece)Weapon;
                return aps.APSLoaded && aps.APSBarrelLoaded;
            }
        }

        public float Inaccuracy => ((AdvCannonFiringPiece)Weapon).APSInaccuracy;

        public float RailgunChargeFraction
        {
            get
            {
                var aps = (AdvCannonFiringPiece)Weapon;
                float capacity = aps.Node.RailgunCapacity;
                if (capacity <= 0f || RailEnergy == null) return 0f;
                return RailEnergy(aps) / capacity;
            }
        }

        public float RailgunCapacity => ((AdvCannonFiringPiece)Weapon).Node.RailgunCapacity;

        public float RecoilCapacity => ((AdvCannonFiringPiece)Weapon).APSHydraulicCapacity;
    }
}
