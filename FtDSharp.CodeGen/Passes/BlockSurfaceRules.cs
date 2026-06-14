using FtDSharp.CodeGen.Models;
using Serilog;

namespace FtDSharp.CodeGen.Passes;

/// <summary>
/// Computes per-block render projections used by Scriban templates.
/// </summary>
public static class BlockSurfaceRules
{
    public const string ConstructableWeaponBlockInterface = "IConstructableWeaponBlock";

    public static readonly HashSet<string> BaseIBlockProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "ParentConstruct", "UniqueId", "CustomName", "LocalPosition", "LocalForward",
        "LocalUp", "LocalRotation", "CurrentHealth", "MaximumHealth",
        "IgnoreFacesRestriction", "Id", "Name"
    };

    /// <summary>
    /// Properties implemented by <c>WeaponFacade</c> and inherited by derived weapon facades.
    /// </summary>
    public static readonly HashSet<string> WeaponFacadeProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "WeaponType", "AimDirection", "SlotMask", "ProjectileSpeed", "IsReady",
        "OnTarget", "CanAim", "IsBlocked", "CanFire", "FlightTime", "AimPoint", "BlockedByTerrain"
    };

    /// <summary>
    /// Properties implemented by <c>TurretFacade</c> and inherited by derived turret facades.
    /// </summary>
    public static readonly HashSet<string> TurretFacadeProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Weapons", "Azimuth", "Elevation",
        "AnyOnTarget", "AllOnTarget", "AnyReady", "AllReady", "AnyCanFire", "AllCanFire"
    };

    public static void ApplyAll(IReadOnlyList<BlockDefinition> blocks)
    {
        Log.Debug("Computing block render surfaces for {Count} blocks...", blocks.Count);
        foreach (var block in blocks)
            block.SetSurface(Compute(block));
    }

    public static BlockRenderSurface Compute(BlockDefinition block)
    {
        var kind = Classify(block);
        var inheritedNames = block.GetInheritedPropertyNames();
        var logicalPropNames = LogicalInterfaces.GetLogicalInterfacePropertyNames(block);

        return new BlockRenderSurface
        {
            Kind = kind,
            InterfaceProperties = FilterInterfaceProperties(block, kind, inheritedNames, logicalPropNames),
            FacadeProperties = FilterFacadeProperties(block, kind),
        };
    }

    /// <summary>
    /// Properties declared on this block's generated interface.
    /// Excludes anything already provided by a parent block interface, logical interface,
    /// <c>IBlock</c>, or a weapon/turret base facade.
    /// </summary>
    private static List<PropertyDefinition> FilterInterfaceProperties(
        BlockDefinition block,
        BlockKind kind,
        HashSet<string> inheritedNames,
        HashSet<string> logicalPropNames)
    {
        var isWeaponOrTurret = kind is BlockKind.Weapon or BlockKind.Turret;

        return block.AllProperties
            .Where(p => !inheritedNames.Contains(p.Name))
            .Where(p => !logicalPropNames.Contains(p.Name))
            .Where(p => !BaseIBlockProperties.Contains(p.Name))
            .Where(p => !isWeaponOrTurret || !WeaponFacadeProperties.Contains(p.Name))
            .Where(p => kind != BlockKind.Turret || !TurretFacadeProperties.Contains(p.Name))
            .OrderBy(p => p.Name)
            .ToList();
    }

    /// <summary>
    /// Properties emitted on this block's generated facade class.
    /// Facades inherit weapon/turret members from <c>WeaponFacade</c>/<c>TurretFacade</c>, so those
    /// base sets are still excluded, but parent-block and logical-interface properties are not:
    /// the concrete facade must wire accessors even when the block interface does not redeclare them.
    /// </summary>
    private static List<PropertyDefinition> FilterFacadeProperties(BlockDefinition block, BlockKind kind)
    {
        var isWeaponOrTurret = kind is BlockKind.Weapon or BlockKind.Turret;

        return block.AllProperties
            .Where(p => !BaseIBlockProperties.Contains(p.Name))
            .Where(p => !isWeaponOrTurret || !WeaponFacadeProperties.Contains(p.Name))
            .ToList();
    }

    private static BlockKind Classify(BlockDefinition block)
    {
        if (typeof(Turrets).IsAssignableFrom(block.GameType))
            return BlockKind.Turret;

        if (block.ImplementedLogicalInterfaces.Contains(ConstructableWeaponBlockInterface))
            return BlockKind.Weapon;

        return BlockKind.Standard;
    }
}
