using BrilliantSkies.Ftd.Missiles.Components;

namespace FtDSharp.CodeGen;

/// <summary>
/// Configuration for missile part code generation.
/// Defines parameter mappings, type overrides, and naming.
/// </summary>
public static class MissilePartConfig
{
    /// <summary>
    /// Component types to skip (don't generate interfaces for these).
    /// These names are matched against Type.Name.
    /// </summary>
    public static readonly HashSet<string> SkipComponentNames =
    [
        "MissileDummyCone",
        "MissileBreadboardReceiver",
    ];

    /// <summary>
    /// Definitions for each missile component type.
    /// Only components with definitions here will have parameters exposed.
    /// </summary>
    public static readonly List<MissilePartDefinition> Definitions =
    [
        // ===== WARHEADS =====
        new()
        {
            GameType = typeof(MissileFragWarhead),
            InterfaceName = "IFragWarhead",
            Parameters =
            [
                new(0, "ConeAngle", "Frag cone angle in degrees (1-180)", min: 1, max: 180),
                new(1, "ElevationOffset", "Frag elevation offset in degrees (-90 to 90)", min: -90, max: 90),
            ]
        },
        new()
        {
            GameType = typeof(MissileExplosiveWarhead),
            InterfaceName = "IExplosiveWarhead",
            Parameters = [] // No controllable parameters
        },
        new()
        {
            GameType = typeof(MissileEMPWarhead),
            InterfaceName = "IEmpWarhead",
            Parameters = [] // No controllable parameters
        },
        new()
        {
            GameType = typeof(MissileIncendiaryWarhead),
            InterfaceName = "IIncendiaryWarhead",
            Parameters =
            [
                new(0, "Intensity", "Fire intensity"),
                new(1, "Oxidizer", "Oxidizer amount"),
            ]
        },
        new()
        {
            GameType = typeof(MissileThumperHead),
            InterfaceName = "IThumperHead",
            Parameters = [] // No controllable parameters
        },
        new()
        {
            GameType = typeof(MissileShapedChargeHead),
            InterfaceName = "IShapedChargeHead",
            Parameters =
            [
                new(0, "PenetrationFactor", "Penetration factor multiplier"),
            ]
        },

        // ===== PROPULSION =====
        new()
        {
            GameType = typeof(MissileVariableThrustThruster),
            InterfaceName = "IVariableThruster",
            Parameters =
            [
                new(1, "Thrust", "Current thrust setting"),
            ]
        },
        new()
        {
            GameType = typeof(MissileShortRangeThruster),
            InterfaceName = "IShortRangeThruster",
            Parameters =
            [
                new(1, "BurnTime", "Burn time in seconds"),
            ]
        },
        new()
        {
            GameType = typeof(MissilePropeller),
            InterfaceName = "ITorpedoPropeller",
            Parameters =
            [
                new(1, "Thrust", "Current thrust setting"),
            ]
        },
        new()
        {
            GameType = typeof(MissileSecondaryPropeller),
            InterfaceName = "ISecondaryPropeller",
            Parameters =
            [
                new(1, "Thrust", "Current thrust setting"),
            ]
        },
        new()
        {
            GameType = typeof(MissileTurningThruster),
            InterfaceName = "ITurningThruster",
            Parameters =
            [
                new(2, "MaxFuelPercentage", "Maximum fuel percentage to use"),
            ]
        },

        // ===== GUIDANCE =====
        new()
        {
            GameType = typeof(MissileBeamRider),
            InterfaceName = "IBeamRider",
            Parameters =
            [
                new(0, "OursOnly", "Target our lasers only", isBool: true),
            ]
        },
        new()
        {
            GameType = typeof(MissileAPNGuidance),
            InterfaceName = "IApnGuidance",
            Parameters = [] // Indices not reliably extracted
        },
        new()
        {
            GameType = typeof(MissileIRSeeker),
            InterfaceName = "IIrSeeker",
            Parameters =
            [
                new(0, "MaximumAngleOffNose", "Maximum off-nose angle in degrees"),
            ]
        },
        new()
        {
            GameType = typeof(MissileRadarSeeker),
            InterfaceName = "IRadarSeeker",
            Parameters =
            [
                new(0, "MaximumAngleOffNose", "Maximum off-nose angle in degrees"),
            ]
        },
        new()
        {
            GameType = typeof(MissileSonar),
            InterfaceName = "ISonarSeeker",
            Parameters = [] // MaximumAngleOffNose index unknown
        },
        new()
        {
            GameType = typeof(MissileSimpleIRSeeker),
            InterfaceName = "ISimpleIrSeeker",
            Parameters =
            [
                new(3, "MinDecoyStrengthMultiplier", "Minimum decoy strength multiplier"),
            ]
        },
        new()
        {
            GameType = typeof(MissileOneTurn),
            InterfaceName = "IOneTurn",
            Parameters =
            [
                new(0, "DownRangeAimDistance", "Down-range aim distance (meters)"),
                new(1, "StartDelay", "Delay before turn in seconds"),
            ]
        },
        new()
        {
            GameType = typeof(MissileSignalProcessor),
            InterfaceName = "ISignalProcessor",
            Parameters = [] // Indices not reliably extracted
        },

        // ===== UTILITY =====
        new()
        {
            GameType = typeof(MissileBallastTank),
            InterfaceName = "IBallastTank",
            Parameters =
            [
                new(0, "FloatHeightDecrease", "Float height decrease"),
                new(1, "BuoyancyModifier", "Current buoyancy modifier (0-1)"),
            ]
        },
        new()
        {
            GameType = typeof(MissileCableDrum),
            InterfaceName = "ICableDrum",
            Parameters =
            [
                new(0, "CableLength", "Cable length in meters", isReadOnly: true),
            ]
        },
        new()
        {
            GameType = typeof(MissileHarpoon),
            InterfaceName = "IHarpoon",
            Parameters =
            [
                new(0, "CableLength", "Cable length in meters", isReadOnly: true),
            ]
        },
        new()
        {
            GameType = typeof(MissileMagnet),
            InterfaceName = "IMagnet",
            Parameters =
            [
                new(0, "Range", "Magnet activation range"),
                new(1, "StartDelay", "Delay before activation"),
            ]
        },
        new()
        {
            GameType = typeof(MissileInterceptor),
            InterfaceName = "IInterceptor",
            Parameters = [] // Indices not reliably extracted
        },
        new()
        {
            GameType = typeof(MissileStickyFlare),
            InterfaceName = "IStickyFlare",
            Parameters =
            [
                new(0, "IgniteTime", "Time to ignite"),
                new(1, "DropDelay", "Delay before drop"),
            ]
        },

        // ===== FUSES =====
        new()
        {
            GameType = typeof(MissileAltitudeFuse),
            InterfaceName = "IAltitudeFuse",
            Parameters =
            [
                new(0, "MinimumAltitude", "Minimum altitude threshold"),
            ]
        },

        // ===== MIRV =====
        new()
        {
            GameType = typeof(MissileMirvHolder),
            InterfaceName = "IMirvHolder",
            Parameters =
            [
                new(0, "DistanceToTarget", "Distance threshold for release"),
                new(1, "DistanceToTargetSmart", "Smart distance threshold"),
                new(2, "ElapsedTime", "Begin release after this flight time elapsed (seconds)"),
                new(3, "DropStagger", "Stagger between drops (seconds)"),
                new(4, "SizeDifferenceToAllow", "Allowed size difference"),
                new(5, "DropAboveThisAltitude", "Drop only above this altitude"),
                new(6, "DropBelowThisAltitude", "Drop only below this altitude"),
                new(7, "EjectionVelocity", "Ejection velocity"),
                new(8, "EjectionSpread", "Ejection spread angle"),
            ]
        },
        new()
        {
            GameType = typeof(MissileMirvEjector),
            InterfaceName = "IMirvEjector",
            Parameters =
            [
                new(8, "SpeedInheritance", "Speed inheritance factor (0-1)"),
            ]
        },
    ];

    /// <summary>
    /// Enums to generate for parameter type overrides.
    /// </summary>
    public static readonly List<GeneratedEnum> Enums =
    [
        new("BeamRiderMode", new Dictionary<float, string>
        {
            { 0, "OurLasers" },
            { 1, "OurVehiclesLasers" },
        }),
    ];
}

/// <summary>
/// Definition for a missile part interface.
/// </summary>
public class MissilePartDefinition
{
    public Type GameType { get; set; } = typeof(object);
    public string InterfaceName { get; set; } = "";
    public string? InheritFrom { get; set; }
    public List<MissileParameterDefinition> Parameters { get; set; } = [];
    public List<DirectPropertyDefinition> DirectProperties { get; set; } = [];
}

/// <summary>
/// Definition for a missile part parameter (indexed via parameters[index]).
/// </summary>
public class MissileParameterDefinition
{
    public int Index { get; set; }
    public string PropertyName { get; set; }
    public string Description { get; set; }
    public float? Min { get; set; }
    public float? Max { get; set; }
    public bool IsReadOnly { get; set; }
    public string? EnumTypeName { get; set; }
    public bool IsBool { get; set; }

    public MissileParameterDefinition(int index, string propertyName, string description = "",
        float? min = null, float? max = null, bool isReadOnly = false, string? enumTypeName = null, bool isBool = false)
    {
        Index = index;
        PropertyName = propertyName;
        Description = description;
        Min = min;
        Max = max;
        IsReadOnly = isReadOnly;
        EnumTypeName = enumTypeName;
        IsBool = isBool;
    }
}

/// <summary>
/// Definition for a non-indexed property (accessed via direct property path).
/// </summary>
public class DirectPropertyDefinition
{
    public string PropertyName { get; set; }
    public string AccessPath { get; set; }
    public string TypeName { get; set; }
    public string Description { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsBoolFloat { get; set; }

    /// <param name="propertyName">Property name on the generated interface</param>
    /// <param name="accessPath">Path to access the property (e.g., "UseFlameParameter.Us")</param>
    /// <param name="typeName">C# type name (e.g., "bool", "Color")</param>
    /// <param name="description">Property description</param>
    /// <param name="isReadOnly">Whether the property is read-only</param>
    /// <param name="isBoolFloat">If true, treat as bool via "> 0.9f" comparison</param>
    public DirectPropertyDefinition(string propertyName, string accessPath, string typeName,
        string description = "", bool isReadOnly = false, bool isBoolFloat = false)
    {
        PropertyName = propertyName;
        AccessPath = accessPath;
        TypeName = typeName;
        Description = description;
        IsReadOnly = isReadOnly;
        IsBoolFloat = isBoolFloat;
    }
}

/// <summary>
/// Definition for a generated enum type.
/// </summary>
public class GeneratedEnum
{
    public string Name { get; set; }
    public Dictionary<float, string> Values { get; set; }

    public GeneratedEnum(string name, Dictionary<float, string> values)
    {
        Name = name;
        Values = values;
    }
}

