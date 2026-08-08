using FtDSharp.CodeGen.Models;
using Serilog;

namespace FtDSharp.CodeGen.Passes;

public static class HierarchyPass
{
    public static void Run(IReadOnlyList<BlockDefinition> blocks)
    {
        Log.Debug("Linking parent types for {Count} blocks...", blocks.Count);
        var blocksByType = blocks.ToDictionary(b => b.GameType, b => b);

        foreach (BlockDefinition block in blocks)
        {
            Type? current = block.GameType.BaseType;
            while (current != null && current != typeof(Block) && current != typeof(object))
            {
                if (blocksByType.TryGetValue(current, out BlockDefinition? parentBlock))
                {
                    block.Parent = parentBlock;
                    break;
                }
                current = current.BaseType;
            }
        }
    }
}
