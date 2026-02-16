namespace FtDSharp.Facades
{
    // All IPlasmaWeapon properties are already implemented by the generated facades
    // (ChargesReady, IsFullyCharged, Temperature, TemperatureLimit, IsStalled,
    //  BurstSize, ChargePerShot, RateOfFire, Color).
    // These partial declarations only add the interface marker.

    internal partial class PlasmaMantletAAFacade : IPlasmaWeapon { }

    internal partial class PlasmaMantletForwardFacade : IPlasmaWeapon { }
}
