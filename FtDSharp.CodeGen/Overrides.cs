using BrilliantSkies.ResourceGatherer;

namespace FtDSharp.CodeGen;

public static class Overrides
{
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

    public static readonly HashSet<string> SkipProperties = new()
    {
        "obsolete",
    };

    public static readonly HashSet<string> SkipPatternProperties = new()
    {
        "obsolete",
        "Endangered",
        "Cache",
        "_ns",
        "_ps",
    };

    public static readonly HashSet<string> SkipDataPackages = new()
    {
        "OldData",
        "PriorityData",
        "StorageData",
    };

    public static readonly HashSet<Type> SkipClasses = new()
    {
        typeof(Block),
        typeof(HelicopterSpinner),
        typeof(HelicopterBlade),
        typeof(HelicopterBladeUpsideDown),
        typeof(HelicopterPoleExtension),
        typeof(WorldWarCannon),
        typeof(WorldWarCannonCustomShells),
        typeof(AprilFirst),
        typeof(OilDrill),
    };

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
