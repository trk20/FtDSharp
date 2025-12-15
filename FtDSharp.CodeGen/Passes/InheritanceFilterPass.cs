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

        block.Properties = [.. block.AllProperties
            .Where(p => !inheritedNames.Contains(p.Name))
            .Where(p => !logicalPropNames.Contains(p.Name))
            .Where(p => !BaseIBlockProperties.Contains(p.Name))
            .OrderBy(p => p.Name)];
    }
}
