namespace FtDSharp.Facades
{
    // IParticleWeapon properties satisfied by generated facades:
    //   Attenuation, DamageFactor, BeamCount, Color, OverClocking
    // Properties that need new accessors (cleaner names):
    //   IsLoaded, ChargeFraction, IsDamaged, LastReloadTime
    // All PAC lens variants share the same base class (ParticleCannon)
    // so the property access pattern is identical.

    internal partial class ShortRangePacLensFacade : IParticleWeapon
    {
        public bool IsLoaded => ((ParticleCannonCloseRange)Weapon).PCLoaded;
        public float ChargeFraction => ((ParticleCannonCloseRange)Weapon).PCChargeFraction;
        public bool IsDamaged => ((ParticleCannonCloseRange)Weapon).PCArmsbroken;
        public float LastReloadTime => ((ParticleCannonCloseRange)Weapon).PCLastReloadTime;
    }

    internal partial class LongRangeSymmetricPacLensFacade : IParticleWeapon
    {
        public bool IsLoaded => ((ParticleCannonSniperSymmetric)Weapon).PCLoaded;
        public float ChargeFraction => ((ParticleCannonSniperSymmetric)Weapon).PCChargeFraction;
        public bool IsDamaged => ((ParticleCannonSniperSymmetric)Weapon).PCArmsbroken;
        public float LastReloadTime => ((ParticleCannonSniperSymmetric)Weapon).PCLastReloadTime;
    }

    internal partial class LongRangeAsymmetricPacLensFacade : IParticleWeapon
    {
        public bool IsLoaded => ((ParticleCannonSniperAsymmetric)Weapon).PCLoaded;
        public float ChargeFraction => ((ParticleCannonSniperAsymmetric)Weapon).PCChargeFraction;
        public bool IsDamaged => ((ParticleCannonSniperAsymmetric)Weapon).PCArmsbroken;
        public float LastReloadTime => ((ParticleCannonSniperAsymmetric)Weapon).PCLastReloadTime;
    }

    internal partial class LongRangeAsymmetricMirrorPacLensFacade : IParticleWeapon
    {
        public bool IsLoaded => ((ParticleCannonSniperAsymmetricMirror)Weapon).PCLoaded;
        public float ChargeFraction => ((ParticleCannonSniperAsymmetricMirror)Weapon).PCChargeFraction;
        public bool IsDamaged => ((ParticleCannonSniperAsymmetricMirror)Weapon).PCArmsbroken;
        public float LastReloadTime => ((ParticleCannonSniperAsymmetricMirror)Weapon).PCLastReloadTime;
    }

    internal partial class LongRangeRearInputPacLensFacade : IParticleWeapon
    {
        public bool IsLoaded => ((ParticleCannonSniperRearInputs)Weapon).PCLoaded;
        public float ChargeFraction => ((ParticleCannonSniperRearInputs)Weapon).PCChargeFraction;
        public bool IsDamaged => ((ParticleCannonSniperRearInputs)Weapon).PCArmsbroken;
        public float LastReloadTime => ((ParticleCannonSniperRearInputs)Weapon).PCLastReloadTime;
    }

    internal partial class ScatterPacLensFacade : IParticleWeapon
    {
        public bool IsLoaded => ((ParticleCannonScatter)Weapon).PCLoaded;
        public float ChargeFraction => ((ParticleCannonScatter)Weapon).PCChargeFraction;
        public bool IsDamaged => ((ParticleCannonScatter)Weapon).PCArmsbroken;
        public float LastReloadTime => ((ParticleCannonScatter)Weapon).PCLastReloadTime;
    }

    internal partial class VerticalPacLensFacade : IParticleWeapon
    {
        public bool IsLoaded => ((ParticleCannonVertical)Weapon).PCLoaded;
        public float ChargeFraction => ((ParticleCannonVertical)Weapon).PCChargeFraction;
        public bool IsDamaged => ((ParticleCannonVertical)Weapon).PCArmsbroken;
        public float LastReloadTime => ((ParticleCannonVertical)Weapon).PCLastReloadTime;
    }

    internal partial class MeleePacLensFacade : IParticleWeapon
    {
        public bool IsLoaded => ((ParticleCannonMeleeLens)Weapon).PCLoaded;
        public float ChargeFraction => ((ParticleCannonMeleeLens)Weapon).PCChargeFraction;
        public bool IsDamaged => ((ParticleCannonMeleeLens)Weapon).PCArmsbroken;
        public float LastReloadTime => ((ParticleCannonMeleeLens)Weapon).PCLastReloadTime;
    }
}
