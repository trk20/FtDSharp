using BrilliantSkies.Blocks.Ai.WeaponControl;
using BrilliantSkies.Blocks.Ai.WeaponControl.Ciws;
using BrilliantSkies.Blocks.Ai.Wireless;
using BrilliantSkies.Blocks.BreadBoards;
using BrilliantSkies.Blocks.MissileBreadboard;
using BrilliantSkies.Blocks.SoundBlocks;
using BrilliantSkies.Blocks.Tutorials;
using BrilliantSkies.Ftd.Constructs.Modules.All.StandardExplosion;
using BrilliantSkies.ResourceGatherer;

namespace FtDSharp.CodeGen;

public static class Overrides
{
    /// <summary>
    /// Renames for class/interface names (applied to block type names).
    /// Used to fix capitalization, clarify technical names, and improve readability.
    /// </summary>
    public static readonly Dictionary<string, string> ClassRenames = new()
    {
        [nameof(AiBreadboard)] = "AIBreadboard",
        [nameof(MissileBreadboardBlock)] = "MissileBreadboard",

        [nameof(AioLwcBlock)] = "AIOLwcBlock",

        [nameof(PIDGP)] = "GeneralPurposePID",
        [nameof(Turrets)] = "TurretBlock",
        [nameof(Spinners)] = "SpinBlock",

        [nameof(AiActiveSonar)] = "ActiveSonar",
        [nameof(AiActiveSonar90)] = "ActiveSonar90",
        [nameof(AiIrCamera)] = "IrCamera",
        [nameof(AiIrCamera360)] = "IrCamera360",
        [nameof(AiIrCamera90)] = "IrCamera90",
        [nameof(AioCiwsControllerBlock)] = "AIOCiwsControllerBlock",
        [nameof(AiPassiveRadar)] = "PassiveRadar",
        [nameof(AiPassiveSonar)] = "PassiveSonar",
        [nameof(AiRadar90)] = "Radar90",
        [nameof(AiRadarGimbal)] = "RadarTracker",
        [nameof(AiWirelessSnooper)] = "WirelessSnooper",
        [nameof(AIWirelessReciever)] = "WirelessReceiver",
        [nameof(AIWirelessTransmitter)] = "WirelessTransmitter",
        [nameof(AISensorScrambler)] = "ECMSignalJammer",

        [nameof(AdvCannonVenting)] = "ApsCooler",
        [nameof(AdvCannonCorner)] = "ApsCoolerCorner",
        [nameof(AdvCannonSplitter)] = "ApsThreeWayCooler",
        [nameof(AdvCannon4way)] = "ApsFourWayCooler",
        [nameof(AdvCannon5way)] = "ApsFiveWayCooler",
        [nameof(AdvCannonAmmoIntake)] = "ApsAmmoIntake",
        [nameof(AdvCannonAutoloader)] = "ApsAutoloader",
        [nameof(AdvCannonBarrel)] = "ApsBarrel",
        [nameof(AdvCannonBeltFeedAutoloader)] = "BeltFedAutoloader",
        [nameof(AdvCannonFiringPiece)] = "ApsFiringPiece",
        [nameof(AdvCannonLaserTargeter)] = "ApsLaserTargeter",
        [nameof(AdvAmmoController)] = "ApsAmmoController",
        [nameof(AdvAmmoCustomiser)] = "ApsAmmoCustomiser",
        [nameof(AdvAmmoEjector)] = "ApsAmmoEjector",
        [nameof(AdvAmmoEjectorBack)] = "ApsRearAmmoEjector",
        [nameof(AdvCannonConnector)] = "Aps6WayConnector",
        [nameof(AdvCannonRailgunAttachPoint)] = "ApsRailAttachmentPoint",
        [nameof(AdvCannonRailgunCharger)] = "ApsRailCharger",
        [nameof(AdvCannonGaugeIncrease)] = "ApsGaugeIncreaser",

        [nameof(CannonAutoLoader)] = "CramAutoLoader",
        [nameof(CannonAutoLoaderManualPlacement)] = "CramManualAutoLoader",
        [nameof(CannonBarrel)] = "CramBarrel",
        [nameof(CannonBarrelCommon)] = "CramBarrelCommon",
        [nameof(CannonFiringPiece)] = "CramFiringPiece",
        [nameof(CannonFusingBox)] = "CramFusingBox",
        [nameof(CannonLaserTargetter)] = "CramLaserTargeter",
        [nameof(CannonPivotBarrel)] = "CramPivotBarrel",
        [nameof(CannonRecoilReducer)] = "CramMuzzleBrake",
        [nameof(CannonSuppresor)] = "CramSuppressor",
        [nameof(CannonConnector)] = "Cram6WayConnector",
        [nameof(CannonEmpWarhead)] = "CramEmpPellets",
        [nameof(CannonFragWarhead)] = "CramFragPellets",
        [nameof(CannonArmourPiercingWarhead)] = "CramHardenerPellets",
        [nameof(CannonExplosiveWarhead)] = "CramHEPellets",
        [nameof(CannonIncendiaryWarhead)] = "CramIncendiaryPellets",
        [nameof(CannonPredictor)] = "CramPredictor",
        [nameof(BombChuteBarrel)] = "CramBombChute",
        [nameof(CannonGaugeIncrease)] = "CramGaugeIncreaser",

        [nameof(FlamerCompressorLarge)] = "LargeFlamerCompressor",
        [nameof(FlamerTankLarge)] = "LargeFlamerTank",

        [nameof(JetIntakePiped)] = "DuctedJetIntake",
        [nameof(JetIntakePipedFront)] = "FrontDuctedJetIntake",
        [nameof(JetIntakePipedSide)] = "SideDuctedJetIntake",
        [nameof(JetIntakePipedTop)] = "TopDuctedJetIntake",
        [nameof(SmallJetIntakePipedFront)] = "SmallFrontDuctedJetIntake",

        [nameof(DriveWheel3m)] = "DriveWheel3M",
        [nameof(DriveWheel5m)] = "DriveWheel5M",
        [nameof(TurnWheel3m)] = "TurnWheel3M",
        [nameof(TurnWheel5m)] = "TurnWheel5M",

        [nameof(FuelEngineTurboChargerBlock)] = "TurboCharger",
        [nameof(FuelEngineTurboChargerBlockInline)] = "InlineTurboCharger",
        [nameof(EngineModelBlock)] = "FuelEngine",
        [nameof(CiwsControllerBlock)] = "CiwsController",
        [nameof(SpinBlockController)] = "SpinnerController",
        [nameof(MissileBlockLaser)] = "MissileLaserDesignator",
        [nameof(MissileBlockStaggeredFire)] = "StaggeredFireAddOn",
        [nameof(OnOffBlock)] = "OnOffSwitch",
        [nameof(SoundBlock)] = "SoundEmitter",
        [nameof(TextBlock)] = "TextDisplay",
        [nameof(VideoBlock)] = "VideoScreen",

        [nameof(LaunchpadHugeHeavy)] = "HugeLauncher",
        [nameof(LaunchpadHugeLight)] = "HugeRailLauncher",
        [nameof(LaunchpadLargeHeavy)] = "LargeLauncher",
        [nameof(LaunchpadLargeLight)] = "LargeRailLauncher",
        [nameof(LaunchpadMediumHeavy)] = "MediumLauncher",
        [nameof(LaunchpadMediumLight)] = "MediumRailLauncher",
        [nameof(LaunchpadSmallHeavy)] = "SmallLauncher",
        [nameof(LaunchpadSmallLight)] = "SmallRailLauncher",
        [nameof(LaunchpadSmallLightSingle)] = "SmallSingleLauncher",
        [nameof(RearLaunchLaunchpad)] = "ReversedMediumLauncher",
        [nameof(RearLaunchLaunchpadLarge)] = "ReversedLargeLauncher",
        [nameof(RearLaunchLaunchpadSmall)] = "ReversedSmallLauncher",

        [nameof(PlasmaCollector1m)] = "PlasmaCollector1M",
        [nameof(PlasmaCollector3m)] = "PlasmaCollector3M",
        [nameof(PlasmaCollector5m)] = "PlasmaCollector5M",

        [nameof(ParticleCannonPipe)] = "PacTube",
        [nameof(ParticleCannonPipeCorner)] = "PacTubeCorner",
        [nameof(ParticleCannonPipeEnd)] = "PacTubeEnd", // not sure if this refers to tube terminator, input port, or both
        [nameof(ParticleCannonCloseRange)] = "ShortRangePacLens",
        [nameof(ParticleCannonMeleeLens)] = "MeleePacLens",
        [nameof(ParticleCannonScatter)] = "ScatterPacLens",
        [nameof(ParticleCannonSniperAsymmetric)] = "LongRangeAsymmetricPacLens",
        [nameof(ParticleCannonSniperAsymmetricMirror)] = "LongRangeAsymmetricMirrorPacLens",
        [nameof(ParticleCannonSniperRearInputs)] = "LongRangeRearInputPacLens",
        [nameof(ParticleCannonSniperSymmetric)] = "LongRangeSymmetricPacLens",
        [nameof(ParticleCannonVertical)] = "VerticalPacLens",
    };

    public static readonly Dictionary<string, string> PropertyRenames = new()
    {
        ["MinPlatformAz"] = "MinimumAzimuth",
        ["MaxPlatformAz"] = "MaximumAzimuth",
        ["MinPlatformEl"] = "MinimumElevation",
        ["MaxPlatformEl"] = "MaximumElevation",
        ["FlipAz"] = "FlipAzimuth",
        ["direction"] = "BarrelAimDirection",
        ["lerpDirection"] = "BarrelCurrentDirection",
        ["SavedReloadTime"] = "ReloadTime",
        ["SpeedReader"] = "ProjectileSpeed",
        ["HasFiredReader"] = "ShotsFiredSinceLastCheck",
    };

    public static readonly Dictionary<string, (string NewName, string RequiredDataPackagePattern)> ConditionalRenames = new()
    {
        ["P0"] = ("WarheadArmDelay", "parameters"),
        ["P1"] = ("GuidanceActivationDelay", "parameters"),
        ["P2"] = ("EjectionElevation", "parameters"),
        ["P3"] = ("EjectionAzimuth", "parameters"),
        ["P4"] = ("FreeSpaceUsage", "parameters"),
        ["P5"] = ("ThrustBeforeLock", "parameters"),
        ["P6"] = ("RailEject", "parameters"),
        ["P7"] = ("DefaultGuidance", "parameters"),
        ["P8"] = ("BreadboardControllerID", "parameters"),
    };

    public static readonly HashSet<string> SkipProperties =
    [
        "obsolete",
    ];

    public static readonly HashSet<string> SkipPatternProperties =
    [
        "obsolete",
        "Endangered",
        "Cache",
        "_ns",
        "_ps",
    ];

    public static readonly HashSet<string> SkipDataPackages =
    [
        "OldData",
        "PriorityData",
        "StorageData",
    ];

    public static readonly HashSet<Type> SkipClasses =
    [
        typeof(Block),
        typeof(HelicopterSpinner),
        typeof(HelicopterBlade),
        typeof(HelicopterBladeUpsideDown),
        typeof(HelicopterPoleExtension),
        typeof(WorldWarCannon),
        typeof(WorldWarCannonCustomShells),
        typeof(AprilFirst),
        typeof(OilDrill),
        typeof(BoomBlock),
        typeof(AntiMissileCannonController),
        typeof(AiCardDepreciated),
    ];

    public static string ApplyClassRename(string className) =>
        ClassRenames.TryGetValue(className, out var renamed) ? renamed : className;

    public static string ApplyRename(string propertyName) =>
        PropertyRenames.TryGetValue(propertyName, out var renamed) ? renamed : propertyName;

    public static string ApplyRename(string propertyName, string? dataPackageName)
    {
        if (PropertyRenames.TryGetValue(propertyName, out var renamed))
            return renamed;

        if (dataPackageName != null && ConditionalRenames.TryGetValue(propertyName, out var conditional))
        {
            if (dataPackageName.Contains(conditional.RequiredDataPackagePattern))
                return conditional.NewName;
        }

        return propertyName;
    }

    public static bool ShouldSkip(string propertyName) =>
        SkipProperties.Contains(propertyName);
}
