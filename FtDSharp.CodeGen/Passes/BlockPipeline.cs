using FtDSharp.CodeGen.Models;
using FtDSharp.CodeGen.Scanner;
using Serilog;

namespace FtDSharp.CodeGen.Passes;

/// <summary>
/// Runs block scanning and transformation passes through render-surface computation.
/// </summary>
public static class BlockPipeline
{
    public static List<BlockDefinition> Run()
    {
        Log.Debug("Fetching block types and block stores...");
        List<RawBlockInfo> rawBlocks = new BlockScanner().Scan(typeof(Block).Assembly);
        (Dictionary<Type, string> concreteStores, Dictionary<Type, string> interfaceStores) = new BlockStoreScanner().Scan();

        Log.Debug("Found {Count} block types", rawBlocks.Count);
        Log.Debug("Discovered {ConcreteCount} concrete + {InterfaceCount} interface BlockStore<T> properties",
            concreteStores.Count, interfaceStores.Count);

        List<BlockDefinition> blocks = BuildInitialModel(rawBlocks);
        Log.Debug("Created {Count} block definitions", blocks.Count);

        Log.Debug("Running transformation stages...");
        HierarchyPass.Run(blocks);
        PropertyFlattenerPass.Run(rawBlocks, blocks);
        NamingPass.Run(blocks);
        LogicalInterfacePass.Run(blocks);
        StoreOptimizationPass.Run(blocks, concreteStores, interfaceStores);
        BlockSurfaceRules.ApplyAll(blocks);

        return blocks;
    }

    private static List<BlockDefinition> BuildInitialModel(List<RawBlockInfo> rawBlocks) =>
        [.. rawBlocks
            .Select(rb => new BlockDefinition
            {
                GameType = rb.GameType,
                ClassName = Overrides.ApplyClassRename(rb.GameType.Name)
            })];
}
