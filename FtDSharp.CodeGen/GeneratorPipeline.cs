using FtDSharp.CodeGen.Generators;
using FtDSharp.CodeGen.Models;
using FtDSharp.CodeGen.Passes;
using FtDSharp.CodeGen.Scanner;
using Serilog;

namespace FtDSharp.CodeGen;

public class GeneratorPipeline
{
    public void Run(string apiOutputPath, string facadeOutputPath)
    {
        Log.Debug("API output: {Path}", Path.GetFullPath(apiOutputPath));
        Log.Debug("Facade output: {Path}", Path.GetFullPath(facadeOutputPath));

        CleanupOutputDirs(apiOutputPath, facadeOutputPath);

        Log.Debug("Fetching block types and block stores...");
        var rawBlocks = new BlockScanner().Scan(typeof(Block).Assembly);
        var (concreteStores, interfaceStores) = new BlockStoreScanner().Scan();

        Log.Debug("Found {Count} block types", rawBlocks.Count);
        Log.Debug("Discovered {ConcreteCount} concrete + {InterfaceCount} interface BlockStore<T> properties",
            concreteStores.Count, interfaceStores.Count);

        Log.Debug("Building block definitions...");
        var blocks = BuildInitialModel(rawBlocks);
        Log.Debug("Created {Count} block definitions", blocks.Count);

        Log.Debug("Running transformation stages...");

        new HierarchyPass().Process(blocks);

        new PropertyFlattenerPass().Process(rawBlocks, blocks);

        new NamingPass().Process(blocks);

        new LogicalInterfacePass().Process(blocks);

        new StoreOptimizationPass(concreteStores, interfaceStores).Process(blocks);

        new InheritanceFilterPass().Process(blocks);

        var referencedAsParent = new HashSet<Type>();
        foreach (var block in blocks)
        {
            if (block.Parent != null)
                referencedAsParent.Add(block.Parent.GameType);
        }

        var blocksToGenerate = blocks
            .Where(b => b.Properties.Any()
                || b.Parent != null
                || b.ImplementedLogicalInterfaces.Any()
                || referencedAsParent.Contains(b.GameType))
            .OrderBy(b => b.ClassName)
            .ToList();

        Log.Debug("{Count} blocks will have code generated", blocksToGenerate.Count);

        Log.Debug("Phase 4: Generating code...");
        var renderer = new TemplateRenderer();

        var logicalInterfacesCode = renderer.RenderLogicalInterfaces([.. LogicalInterfaces.Definitions], blocksToGenerate);
        File.WriteAllText(Path.Combine(apiOutputPath, "LogicalInterfaces.g.cs"), logicalInterfacesCode);

        int totalProperties = 0;
        foreach (var block in blocksToGenerate)
        {
            totalProperties += block.Properties.Count;

            var interfaceCode = renderer.RenderInterface(block);
            File.WriteAllText(Path.Combine(apiOutputPath, $"I{block.ClassName}.g.cs"), interfaceCode);

            var facadeCode = renderer.RenderFacade(block);
            File.WriteAllText(Path.Combine(facadeOutputPath, $"{block.ClassName}Facade.g.cs"), facadeCode);
        }

        var factoryCode = renderer.RenderBlockFactory(blocksToGenerate);
        File.WriteAllText(Path.Combine(facadeOutputPath, "BlockFactory.g.cs"), factoryCode);

        var blocksCode = renderer.RenderBlocksAccessor(blocksToGenerate);
        File.WriteAllText(Path.Combine(apiOutputPath, "Blocks.g.cs"), blocksCode);

        Log.Information("Generated {BlockCount} interfaces/facades with {PropCount} total properties",
            blocksToGenerate.Count, totalProperties);
    }

    private static void CleanupOutputDirs(params string[] paths)
    {
        foreach (var path in paths)
        {
            Directory.CreateDirectory(path);
            foreach (var file in Directory.GetFiles(path, "*.g.cs"))
                File.Delete(file);
        }
    }

    private static List<BlockDefinition> BuildInitialModel(List<RawBlockInfo> rawBlocks)
    {
        return [.. rawBlocks
            .Select(rb => new BlockDefinition
            {
                GameType = rb.GameType,
                ClassName = Overrides.ApplyClassRename(rb.GameType.Name)
            })
        ];
    }
}
