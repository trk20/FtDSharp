using FtDSharp.CodeGen.Models;
using Serilog;

namespace FtDSharp.CodeGen.Passes;

public class HierarchyPass : IBlockPass
{
    public void Process(List<BlockDefinition> blocks)
    {
        Log.Debug("Linking parent types for {Count} blocks...", blocks.Count);
        var blocksByType = blocks.ToDictionary(b => b.GameType, b => b);

        foreach (var block in blocks)
        {
            var current = block.GameType.BaseType;
            while (current != null && current != typeof(Block) && current != typeof(object))
            {
                if (blocksByType.TryGetValue(current, out var parentBlock))
                {
                    block.Parent = parentBlock;
                    break;
                }
                current = current.BaseType;
            }
        }
    }

    public static HashSet<string> GetInheritedPropertyNames(BlockDefinition block)
    {
        var inherited = new HashSet<string>();
        var current = block.Parent;

        while (current != null)
        {
            foreach (var prop in current.AllProperties)
                inherited.Add(prop.Name);
            current = current.Parent;
        }

        return inherited;
    }
}
