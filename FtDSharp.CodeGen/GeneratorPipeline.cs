using FtDSharp.CodeGen.Generators;
using FtDSharp.CodeGen.Passes;
using FtDSharp.CodeGen.Utils;
using Serilog;

namespace FtDSharp.CodeGen;

public class GeneratorPipeline
{
    public void Run(string apiOutputPath, string facadeOutputPath)
    {
        Log.Debug("API output: {Path}", Path.GetFullPath(apiOutputPath));
        Log.Debug("Facade output: {Path}", Path.GetFullPath(facadeOutputPath));

        GeneratedOutputWriter.CleanGeneratedFiles(apiOutputPath);
        GeneratedOutputWriter.CleanGeneratedFiles(facadeOutputPath);

        List<Models.BlockDefinition> blocks = BlockPipeline.Run();

        var referencedAsParent = new HashSet<Type>();
        foreach (Models.BlockDefinition block in blocks)
        {
            if (block.Parent != null)
                referencedAsParent.Add(block.Parent.GameType);
        }

        var blocksToGenerate = blocks
            .Where(b => b.Surface.InterfaceProperties.Any()
                || b.Parent != null
                || b.ImplementedLogicalInterfaces.Any()
                || referencedAsParent.Contains(b.GameType))
            .OrderBy(b => b.ClassName)
            .ToList();

        Log.Debug("{Count} blocks will have code generated", blocksToGenerate.Count);

        Log.Debug("Generating block code...");
        var renderer = new TemplateRenderer();

        GeneratedOutputWriter.Write(apiOutputPath, "LogicalInterfaces.g.cs",
            renderer.RenderLogicalInterfaces([.. LogicalInterfaces.Definitions], blocksToGenerate));

        int totalProperties = 0;
        foreach (Models.BlockDefinition? block in blocksToGenerate)
        {
            totalProperties += block.Surface.InterfaceProperties.Count;

            GeneratedOutputWriter.Write(apiOutputPath, $"I{block.ClassName}.g.cs",
                renderer.RenderInterface(block));

            GeneratedOutputWriter.Write(facadeOutputPath, $"{block.ClassName}Facade.g.cs",
                renderer.RenderFacade(block));
        }

        GeneratedOutputWriter.Write(facadeOutputPath, "BlockFactory.g.cs",
            renderer.RenderBlockFactory(blocksToGenerate));

        GeneratedOutputWriter.Write(apiOutputPath, "IBlocksProvider.g.cs",
            renderer.RenderBlocksProviderInterface(blocksToGenerate));

        GeneratedOutputWriter.Write(apiOutputPath, "Blocks.g.cs",
            renderer.RenderBlocksApi(blocksToGenerate));

        GeneratedOutputWriter.Write(facadeOutputPath, "BlocksProvider.g.cs",
            renderer.RenderBlocksProviderImpl(blocksToGenerate));

        Log.Information("Generated {BlockCount} interfaces/facades with {PropCount} total properties",
            blocksToGenerate.Count, totalProperties);

        GenerateMissileParts(renderer, apiOutputPath, facadeOutputPath);
    }

    private static void GenerateMissileParts(TemplateRenderer renderer, string apiOutputPath, string facadeOutputPath)
    {
        Log.Debug("Generating missile part interfaces and facades...");
        MissilePartConfig.Validate();

        var partsOutputPath = Path.Combine(apiOutputPath, "MissileParts");
        var partFacadesOutputPath = Path.Combine(facadeOutputPath, "MissileParts");
        GeneratedOutputWriter.CleanGeneratedFiles(partsOutputPath);
        GeneratedOutputWriter.CleanGeneratedFiles(partFacadesOutputPath);

        var partsToGenerate = MissilePartConfig.Definitions
            .Where(d => !MissilePartConfig.SkipComponentNames.Contains(d.GameType.Name))
            .ToList();

        if (MissilePartConfig.Enums.Any())
        {
            GeneratedOutputWriter.Write(partsOutputPath, "MissilePartEnums.g.cs",
                renderer.RenderMissilePartEnums([.. MissilePartConfig.Enums]));
            Log.Debug("Generated {Count} missile part enums", MissilePartConfig.Enums.Count);
        }

        int partCount = 0;
        int paramCount = 0;
        foreach (MissilePartDefinition? part in partsToGenerate)
        {
            partCount++;
            paramCount += part.Parameters.Count;

            GeneratedOutputWriter.Write(partsOutputPath, $"{part.InterfaceName}.g.cs",
                renderer.RenderMissilePartInterface(part));

            var className = MissilePartNaming.FacadeClassName(part.InterfaceName);
            GeneratedOutputWriter.Write(partFacadesOutputPath, $"{className}Facade.g.cs",
                renderer.RenderMissilePartFacade(part));
        }

        GeneratedOutputWriter.Write(partFacadesOutputPath, "MissilePartFactory.g.cs",
            renderer.RenderMissilePartFactory(partsToGenerate));

        Log.Information("Generated {PartCount} missile part interfaces/facades with {ParamCount} total parameters",
            partCount, paramCount);
    }

}
