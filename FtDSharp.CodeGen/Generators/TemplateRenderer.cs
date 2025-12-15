using FtDSharp.CodeGen.Models;
using FtDSharp.CodeGen.Utils;
using Scriban;
using Scriban.Runtime;

namespace FtDSharp.CodeGen.Generators;

public class TemplateRenderer
{
    private readonly Template _interfaceTemplate;
    private readonly Template _facadeTemplate;
    private readonly Template _logicalInterfacesTemplate;
    private readonly Template _blockFactoryTemplate;
    private readonly Template _blocksAccessorTemplate;

    public TemplateRenderer()
    {
        _interfaceTemplate = LoadTemplate("Interface.scriban");
        _facadeTemplate = LoadTemplate("Facade.scriban");
        _logicalInterfacesTemplate = LoadTemplate("LogicalInterfaces.scriban");
        _blockFactoryTemplate = LoadTemplate("BlockFactory.scriban");
        _blocksAccessorTemplate = LoadTemplate("BlocksAccessor.scriban");
    }

    private static Template LoadTemplate(string name)
    {
        var assembly = typeof(TemplateRenderer).Assembly;
        var resourceName = $"FtDSharp.CodeGen.Templates.{name}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Template not found: {resourceName}");
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();

        return Template.Parse(content, resourceName);
    }

    public string RenderInterface(BlockDefinition block)
    {
        var model = CreateInterfaceModel(block);
        return _interfaceTemplate.Render(model);
    }

    public string RenderFacade(BlockDefinition block)
    {
        var model = CreateFacadeModel(block);
        return _facadeTemplate.Render(model);
    }

    public string RenderLogicalInterfaces(List<LogicalInterfaceDefinition> definitions, List<BlockDefinition> blocks)
    {
        var model = CreateLogicalInterfacesModel(definitions, blocks);
        return _logicalInterfacesTemplate.Render(model);
    }

    public string RenderBlockFactory(List<BlockDefinition> blocks)
    {
        var model = CreateBlockFactoryModel(blocks);
        return _blockFactoryTemplate.Render(model);
    }

    public string RenderBlocksAccessor(List<BlockDefinition> blocks)
    {
        var model = CreateBlocksAccessorModel(blocks);
        return _blocksAccessorTemplate.Render(model);
    }

    private ScriptObject CreateInterfaceModel(BlockDefinition block)
    {
        var model = new ScriptObject();

        // Build inheritance list
        var baseInterfaces = new List<string>();
        if (block.ParentInterfaceName != null)
        {
            baseInterfaces.Add(block.ParentInterfaceName);
        }
        else
        {
            baseInterfaces.Add("IBlock");
        }
        baseInterfaces.AddRange(block.ImplementedLogicalInterfaces);

        model["class_name"] = block.ClassName;
        model["interface_name"] = block.InterfaceName;
        model["game_type_name"] = block.GameType.Name;
        model["inheritance"] = baseInterfaces;
        model["properties"] = block.Properties.Select(p => new ScriptObject
        {
            ["name"] = p.Name,
            ["type_name"] = p.TypeName,
            ["description"] = p.Description != null ? TypeNameHelper.EscapeXml(p.Description) : null,
            ["has_setter"] = p.HasSetter
        }).ToList();

        return model;
    }

    private ScriptObject CreateFacadeModel(BlockDefinition block)
    {
        var model = new ScriptObject();

        // All interfaces (explicit listing)
        var allInterfaces = new List<string> { block.InterfaceName };
        allInterfaces.AddRange(block.ImplementedLogicalInterfaces);

        // Filter out properties already provided by BlockFacadeBase or base IBlock interface
        var facadeProperties = block.AllProperties
            .Where(p => !Passes.InheritanceFilterPass.BaseIBlockProperties.Contains(p.Name))
            .ToList();

        model["class_name"] = block.ClassName;
        model["interface_name"] = block.InterfaceName;
        model["game_type_full_name"] = block.GameType.FullName;
        model["all_interfaces"] = allInterfaces;
        model["properties"] = facadeProperties.Select(p => new ScriptObject
        {
            ["name"] = p.Name,
            ["type_name"] = p.TypeName,
            ["accessor_path"] = p.AccessorPath,
            ["has_setter"] = p.HasSetter
        }).ToList();

        return model;
    }

    private ScriptObject CreateLogicalInterfacesModel(List<LogicalInterfaceDefinition> definitions, List<BlockDefinition> blocks)
    {
        var model = new ScriptObject();
        var interfaces = new List<ScriptObject>();

        foreach (var def in definitions)
        {
            // Find a representative block that implements this interface
            var sampleBlock = blocks.FirstOrDefault(b => b.ImplementedLogicalInterfaces.Contains(def.InterfaceName));
            if (sampleBlock == null) continue;

            // Get properties from the sample block
            var propsForInterface = sampleBlock.AllProperties
                .Where(p => def.PropertyNames.Contains(p.Name))
                .ToList();

            // Get parent property names to exclude
            var parentPropNames = def.InheritsFrom
                .SelectMany(parent => LogicalInterfaces.Definitions
                    .Where(d => d.InterfaceName == parent)
                    .SelectMany(d => d.PropertyNames))
                .ToHashSet();

            var interfaceModel = new ScriptObject
            {
                ["name"] = def.InterfaceName,
                ["description"] = def.Description,
                ["inherits_from"] = def.InheritsFrom.ToList(),
                ["properties"] = def.PropertyNames
                    .Where(pn => !parentPropNames.Contains(pn))
                    .Select(pn =>
                    {
                        var prop = propsForInterface.FirstOrDefault(p => p.Name == pn);
                        return prop != null ? new ScriptObject
                        {
                            ["name"] = prop.Name,
                            ["type_name"] = prop.TypeName,
                            ["description"] = prop.Description != null ? TypeNameHelper.EscapeXml(prop.Description) : null,
                            ["has_setter"] = prop.HasSetter
                        } : null;
                    })
                    .Where(p => p != null)
                    .ToList()
            };

            interfaces.Add(interfaceModel);
        }

        model["interfaces"] = interfaces;
        return model;
    }

    private ScriptObject CreateBlockFactoryModel(List<BlockDefinition> blocks)
    {
        var model = new ScriptObject();
        model["blocks"] = blocks.Select(b => new ScriptObject
        {
            ["class_name"] = b.ClassName,
            ["game_type_full_name"] = b.GameType.FullName
        }).ToList();

        return model;
    }

    private ScriptObject CreateBlocksAccessorModel(List<BlockDefinition> blocks)
    {
        var model = new ScriptObject();
        model["blocks"] = blocks.Select(b =>
        {
            // Pluralize the name
            var pluralName = b.ClassName.EndsWith("s")
                ? b.ClassName + "es"
                : b.ClassName + "s";

            var fieldName = $"_{char.ToLower(pluralName[0])}{pluralName.Substring(1)}";

            return new ScriptObject
            {
                ["class_name"] = b.ClassName,
                ["interface_name"] = b.InterfaceName,
                ["plural_name"] = pluralName,
                ["field_name"] = fieldName,
                ["game_type_full_name"] = b.GameType.FullName,
                ["has_store"] = b.StoreBinding != null,
                ["store_property_name"] = b.StoreBinding?.PropertyName,
                ["is_interface_store"] = b.StoreBinding?.IsInterfaceStore ?? false
            };
        }).ToList();

        return model;
    }
}
