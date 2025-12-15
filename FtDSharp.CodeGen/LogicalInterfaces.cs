using BrilliantSkies.Blocks.Ai.WeaponControl;
using BrilliantSkies.Constructs.Blocks.Sets.Unsorted.ModulesForPropulsionAndControl;
using BrilliantSkies.Ftd.Missiles.Ui;

namespace FtDSharp.CodeGen;

public static class LogicalInterfaces
{
    public static readonly LogicalInterfaceDefinition[] Definitions =
    [
        // Propulsion response - all the *ResponseType and *Scale properties
        new LogicalInterfaceDefinition
        {
            InterfaceName = "IPropulsionResponder",
            Description = "Common propulsion response and scale settings.",
            PropertyNames =
            [
                nameof(StandardPropulsionModule.GovernedValue),
                nameof(StandardPropulsionModule.YawResponseType), nameof(StandardPropulsionModule.YawScale),
                nameof(StandardPropulsionModule.RollResponseType), nameof(StandardPropulsionModule.RollScale),
                nameof(StandardPropulsionModule.PitchResponseType), nameof(StandardPropulsionModule.PitchScale),
                nameof(StandardPropulsionModule.ForwardResponseType), nameof(StandardPropulsionModule.ForwardScale),
                nameof(StandardPropulsionModule.RightResponseType), nameof(StandardPropulsionModule.RightScale),
                nameof(StandardPropulsionModule.UpResponseType), nameof(StandardPropulsionModule.UpScale),
                nameof(StandardPropulsionModule.MainResponseType), nameof(StandardPropulsionModule.MainScale),
                nameof(StandardPropulsionModule.SecondaryResponseType), nameof(StandardPropulsionModule.SecondaryScale),
                nameof(StandardPropulsionModule.TertiaryResponseType), nameof(StandardPropulsionModule.TertiaryScale),
                nameof(StandardPropulsionModule.AResponseType), nameof(StandardPropulsionModule.AScale),
                nameof(StandardPropulsionModule.BResponseType), nameof(StandardPropulsionModule.BScale),
                nameof(StandardPropulsionModule.CResponseType), nameof(StandardPropulsionModule.CScale),
                nameof(StandardPropulsionModule.DResponseType), nameof(StandardPropulsionModule.DScale),
                nameof(StandardPropulsionModule.EResponseType), nameof(StandardPropulsionModule.EScale),
                nameof(StandardPropulsionModule.PowerScale),
                nameof(StandardPropulsionModule.ResponderSettings),
                nameof(StandardPropulsionModule.AutomaticControlType),
            ]
        },
        
        // Vectorable propulsion - yaw/pitch angle control
        new LogicalInterfaceDefinition
        {
            InterfaceName = "IVectorable",
            Description = "Propulsion systems that can adjust thrust direction via yaw/pitch angles.",
            InheritsFrom = ["IPropulsionResponder"],
            PropertyNames =
            [
                nameof(EffectToggle.YawAngle),
                nameof(EffectToggle.PitchAngle),
                nameof(EffectToggle.PointingMethod),
            ]
        },
        
        // Visual effects toggle
        new LogicalInterfaceDefinition
        {
            InterfaceName = "IHasVisualEffects",
            Description = "Blocks with toggleable visual effects.",
            PropertyNames =
            [
                nameof(EffectToggle.AllowEffects),
                nameof(EffectToggle.DisplayFlame),
                nameof(EffectToggle.DisplaySmoke),
                nameof(EffectToggle.DisplayLight),
                nameof(EffectToggle.DisplayTrail),
                nameof(EffectToggle.DisplayWake),
                nameof(EffectToggle.DisplayBubbles),
            ]
        },
        
        // Power user
        new LogicalInterfaceDefinition
        {
            InterfaceName = "IPowerUser",
            Description = "Blocks that consume power and have priority settings.",
            RequiredDataPackagePattern = "PowerUserData",
            PropertyNames =
            [
                nameof(PowerUserData.Priority),
            ]
        },
        
        // Weapon base properties - named IWeaponProperties to avoid conflict with manual IWeapon interface
        new LogicalInterfaceDefinition
        {
            InterfaceName = "IWeaponProperties",
            Description = "Common weapon properties.",
            PropertyNames =
            [
                nameof(WeaponData.RequiredAccuracy),
                nameof(WeaponData.FiringDelay),
                nameof(WeaponData.SavedReloadTime),
                nameof(WeaponData.MinResourcePercentBeforeFiring),
                nameof(WeaponData.FireNow),
                nameof(WeaponFiringArc.LimitationsEnabled),
                nameof(WeaponFiringArc.IsRelativeToParent),
                nameof(WeaponFiringArc.FlipAz),
                nameof(WeaponFiringArc.MinPlatformAz),
                nameof(WeaponFiringArc.MaxPlatformAz),
                nameof(WeaponFiringArc.MinPlatformEl),
                nameof(WeaponFiringArc.MaxPlatformEl),
                nameof(WeaponSyncData.SyncedWeaponId),
                nameof(WeaponSyncData.MinDelay),
                nameof(WeaponSyncData.MaxDelay),
                nameof(WeaponSyncData.MaxWaitTime),
            ]
        },

        new LogicalInterfaceDefinition
        {
            InterfaceName = "IDamageLogger",
            Description = "Blocks that log damage statistics.",
            PropertyNames =[
                nameof(DamageLog.MaterialsDestroyed),
                nameof(DamageLog.AmmoExpended),
                nameof(DamageLog.EnergyExpended),
                nameof(DamageLog.MaterialLost),
            ]
        },

        new LogicalInterfaceDefinition
        {
            InterfaceName = "IDetectionComponent",
            Description = "On/Off for detection components.",
            PropertyNames =[
                nameof(DetectionParametersAndMaths.OnOff),
            ]
        },

        new LogicalInterfaceDefinition
        {
            InterfaceName = "IMissileLaunchpad",
            Description = "Missile launchpads settings.",
            RequiredDataPackagePattern = "parameters",
            PropertyNames =[
                nameof(UIParameterBag.FlameColor),
                nameof(UIParameterBag.ModuleType),
                nameof(UIParameterBag.MunitionDecoratorId),
                nameof(UIParameterBag.P0),
                nameof(UIParameterBag.P1),
                nameof(UIParameterBag.P2),
                nameof(UIParameterBag.P3),
                nameof(UIParameterBag.P4),
                nameof(UIParameterBag.P5),
                nameof(UIParameterBag.P6),
                nameof(UIParameterBag.P7),
                nameof(UIParameterBag.P8),
                nameof(UIParameterBag.SmokeColor)
            ]
        }
    ];

    public static HashSet<string> GetAllLogicalPropertyNames()
    {
        var result = new HashSet<string>();
        foreach (var def in Definitions)
        {
            foreach (var prop in def.PropertyNames)
                result.Add(prop);
        }
        return result;
    }

    public static List<string> ExpandWithParentInterfaces(IEnumerable<string> directInterfaces)
    {
        var result = new HashSet<string>(directInterfaces);
        var toProcess = new Queue<string>(directInterfaces);

        while (toProcess.Count > 0)
        {
            var current = toProcess.Dequeue();
            var def = Definitions.FirstOrDefault(d => d.InterfaceName == current);
            if (def == null) continue;

            foreach (var parent in def.InheritsFrom)
            {
                if (result.Add(parent))
                    toProcess.Enqueue(parent);
            }
        }

        return [.. result.OrderByDescending(i =>
            Definitions.FirstOrDefault(d => d.InterfaceName == i)?.InheritsFrom.Length ?? 0)];
    }
}

public class LogicalInterfaceDefinition
{
    public string InterfaceName { get; set; } = "";
    public string Description { get; set; } = "";
    public string[] InheritsFrom { get; set; } = [];
    public string? RequiredDataPackagePattern { get; set; }

    private string[] _propertyNames = [];
    public string[] PropertyNames
    {
        get => [.. _propertyNames.Select(p => Overrides.ApplyRename(p, RequiredDataPackagePattern))];
        set => _propertyNames = value;
    }
}
