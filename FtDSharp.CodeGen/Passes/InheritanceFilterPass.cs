using FtDSharp.CodeGen.Models;
using Serilog;

namespace FtDSharp.CodeGen.Passes;

public class InheritanceFilterPass : IBlockPass
{
    public static readonly HashSet<string> BaseIBlockProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "ParentConstruct", "UniqueId", "CustomName", "LocalPosition", "LocalForward",
        "LocalUp", "LocalRotation", "CurrentHealth", "MaximumHealth",
        "IgnoreFacesRestriction", "Id", "Name"
    };

    /// <summary>
    /// Properties defined in WeaponFacade that should not be re-generated in derived weapon facades.
    /// </summary>
    public static readonly HashSet<string> WeaponFacadeProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "WeaponType", "AimDirection", "SlotMask", "ProjectileSpeed", "IsReady",
        "OnTarget", "CanAim", "IsBlocked", "CanFire", "FlightTime", "AimPoint", "BlockedByTerrain"
    };

    /// <summary>
    /// Properties defined in TurretFacade that should not be re-generated in derived turret facades.
    /// </summary>
    public static readonly HashSet<string> TurretFacadeProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Weapons", "Azimuth", "Elevation",
        "AnyOnTarget", "AllOnTarget", "AnyReady", "AllReady", "AnyCanFire", "AllCanFire"
    };

    public void Process(List<BlockDefinition> blocks)
    {
        Log.Debug("Filtering inherited properties for {Count} blocks...", blocks.Count);
        foreach (var block in blocks)
            FilterProperties(block);
    }

    private void FilterProperties(BlockDefinition block)
    {
        var inheritedNames = HierarchyPass.GetInheritedPropertyNames(block);
        var logicalPropNames = LogicalInterfacePass.GetLogicalInterfacePropertyNames(block);

        // Determine if this is a weapon/turret block
        bool isWeaponOrTurret = block.ImplementedLogicalInterfaces.Contains("IConstructableWeaponBlock")
                               || typeof(Turrets).IsAssignableFrom(block.GameType);
        bool isTurret = typeof(Turrets).IsAssignableFrom(block.GameType);

        block.Properties = [.. block.AllProperties
            .Where(p => !inheritedNames.Contains(p.Name))
            .Where(p => !logicalPropNames.Contains(p.Name))
            .Where(p => !BaseIBlockProperties.Contains(p.Name))
            .Where(p => !isWeaponOrTurret || !WeaponFacadeProperties.Contains(p.Name))
            .Where(p => !isTurret || !TurretFacadeProperties.Contains(p.Name))
            .OrderBy(p => p.Name)];
    }
}
