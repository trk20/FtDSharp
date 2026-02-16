namespace FtDSharp.Facades
{
    internal partial class CramFiringPieceFacade : ICramWeapon
    {
        public float Gauge => ((CannonFiringPiece)Weapon).CRAMdiameter * 1000f;

        public float AmmoCount => ((CannonFiringPiece)Weapon).CramData.Ammo.Us;

        public float MinPackFraction => ((CannonFiringPiece)Weapon).CramData.MinimumPackPercentage.Us;
    }
}
