using FtDSharp.CodeGen.Models;
using Serilog;

namespace FtDSharp.CodeGen.Passes;

public class LogicalInterfacePass : IBlockPass
{
    public void Process(List<BlockDefinition> blocks)
    {
        Log.Debug("Determining interface implementations for {Count} blocks...", blocks.Count);
        foreach (var block in blocks)
            DetermineLogicalInterfaces(block);
    }

    private void DetermineLogicalInterfaces(BlockDefinition block)
    {
        var allPropNames = new HashSet<string>(block.AllProperties.Select(p => p.Name));
        var directInterfaces = new List<string>();

        foreach (var logicalDef in LogicalInterfaces.Definitions)
        {
            var matchCount = logicalDef.PropertyNames.Count(allPropNames.Contains);
            if (matchCount != logicalDef.PropertyNames.Length)
                continue;

            if (!string.IsNullOrEmpty(logicalDef.RequiredDataPackagePattern))
            {
                var matchingFromCorrectPackage = block.AllProperties
                    .Where(p => logicalDef.PropertyNames.Contains(p.Name))
                    .Any(p => p.DataPackageName?.Contains(logicalDef.RequiredDataPackagePattern) == true);

                if (!matchingFromCorrectPackage)
                    continue;
            }

            directInterfaces.Add(logicalDef.InterfaceName);
        }

        block.ImplementedLogicalInterfaces = LogicalInterfaces.ExpandWithParentInterfaces(directInterfaces);
    }

    public static HashSet<string> GetLogicalInterfacePropertyNames(BlockDefinition block)
    {
        var result = new HashSet<string>();

        foreach (var logicalName in block.ImplementedLogicalInterfaces)
        {
            var def = LogicalInterfaces.Definitions.FirstOrDefault(d => d.InterfaceName == logicalName);
            if (def == null) continue;

            foreach (var propName in def.PropertyNames)
                result.Add(propName);
        }

        return result;
    }
}
