namespace FtDSharp.Facades
{
    internal partial class LaserCombinerFacade : ILaserWeapon
    {
        public float HoldFireEnergyFraction =>
            ((LaserCombiner)Weapon).Data.HoldFireEnergyPercentage.Us / 100f;
    }
}
