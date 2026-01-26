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

    // Missile part templates
    private readonly Template _missilePartInterfaceTemplate;
    private readonly Template _missilePartFacadeTemplate;
    private readonly Template _missilePartEnumsTemplate;
    private readonly Template _missilePartFactoryTemplate;

    public TemplateRenderer()
    {
        _interfaceTemplate = LoadTemplate("Interface.scriban");
        _facadeTemplate = LoadTemplate("Facade.scriban");
        _logicalInterfacesTemplate = LoadTemplate("LogicalInterfaces.scriban");
        _blockFactoryTemplate = LoadTemplate("BlockFactory.scriban");
        _blocksAccessorTemplate = LoadTemplate("BlocksAccessor.scriban");

        // Missile part templates
        _missilePartInterfaceTemplate = LoadTemplate("MissilePartInterface.scriban");
        _missilePartFacadeTemplate = LoadTemplate("MissilePartFacade.scriban");
        _missilePartEnumsTemplate = LoadTemplate("MissilePartEnums.scriban");
        _missilePartFactoryTemplate = LoadTemplate("MissilePartFactory.scriban");
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

        // Determine if this is a weapon block
        bool isWeapon = block.ImplementedLogicalInterfaces.Contains("IConstructableWeaponBlock");

        // Build inheritance list
        var baseInterfaces = new List<string>();
        if (block.ParentInterfaceName != null)
        {
            baseInterfaces.Add(block.ParentInterfaceName);
        }
        else if (isWeapon)
        {
            // Weapon blocks inherit from IWeapon (which includes IBlock and IWeaponControl)
            baseInterfaces.Add("IWeapon");
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

        // Determine if this is a weapon or turret block
        bool isWeapon = block.ImplementedLogicalInterfaces.Contains("IConstructableWeaponBlock");
        bool isTurret = typeof(Turrets).IsAssignableFrom(block.GameType);

        // All interfaces (explicit listing)
        var allInterfaces = new List<string> { block.InterfaceName };
        allInterfaces.AddRange(block.ImplementedLogicalInterfaces);

        // Filter out properties already provided by BlockFacadeBase or base IBlock interface
        var facadeProperties = block.AllProperties
            .Where(p => !Passes.InheritanceFilterPass.BaseIBlockProperties.Contains(p.Name))
            .Where(p => !(isWeapon || isTurret) || !Passes.InheritanceFilterPass.WeaponFacadeProperties.Contains(p.Name))
            .ToList();

        model["class_name"] = block.ClassName;
        model["interface_name"] = block.InterfaceName;
        model["game_type_full_name"] = block.GameType.FullName;
        model["all_interfaces"] = allInterfaces;
        model["is_weapon"] = isWeapon;
        model["is_turret"] = isTurret;
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
        var model = new ScriptObject
        {
            ["blocks"] = blocks.Select(b =>
        {
            bool isWeapon = b.ImplementedLogicalInterfaces.Contains("IConstructableWeaponBlock");
            bool isTurret = typeof(Turrets).IsAssignableFrom(b.GameType);
            return new ScriptObject
            {
                ["class_name"] = b.ClassName,
                ["game_type_full_name"] = b.GameType.FullName,
                ["is_weapon"] = isWeapon,
                ["is_turret"] = isTurret
            };
        }).ToList()
        };

        return model;
    }

    private ScriptObject CreateBlocksAccessorModel(List<BlockDefinition> blocks)
    {
        var model = new ScriptObject
        {
            ["blocks"] = blocks.Select(b =>
        {
            // Pluralize the name using proper English rules
            var pluralName = Pluralizer.Pluralize(b.ClassName);

            var fieldName = $"_{char.ToLower(pluralName[0])}{pluralName.Substring(1)}";

            // Determine if this is a weapon or turret block
            bool isWeapon = b.ImplementedLogicalInterfaces.Contains("IConstructableWeaponBlock");
            bool isTurret = typeof(Turrets).IsAssignableFrom(b.GameType);

            return new ScriptObject
            {
                ["class_name"] = b.ClassName,
                ["interface_name"] = b.InterfaceName,
                ["plural_name"] = pluralName,
                ["field_name"] = fieldName,
                ["game_type_full_name"] = b.GameType.FullName,
                ["has_store"] = b.StoreBinding != null,
                ["store_property_name"] = b.StoreBinding?.PropertyName,
                ["is_interface_store"] = b.StoreBinding?.IsInterfaceStore ?? false,
                ["is_weapon"] = isWeapon,
                ["is_turret"] = isTurret
            };
        }).ToList()
        };

        return model;
    }

    // ============ Missile Part Rendering ============

    public string RenderMissilePartInterface(MissilePartDefinition part)
    {
        var model = CreateMissilePartInterfaceModel(part);
        return _missilePartInterfaceTemplate.Render(model);
    }

    public string RenderMissilePartFacade(MissilePartDefinition part)
    {
        var model = CreateMissilePartFacadeModel(part);
        return _missilePartFacadeTemplate.Render(model);
    }

    public string RenderMissilePartEnums(List<GeneratedEnum> enums)
    {
        var model = CreateMissilePartEnumsModel(enums);
        return _missilePartEnumsTemplate.Render(model);
    }

    public string RenderMissilePartFactory(List<MissilePartDefinition> parts)
    {
        var model = CreateMissilePartFactoryModel(parts);
        return _missilePartFactoryTemplate.Render(model);
    }

    private ScriptObject CreateMissilePartInterfaceModel(MissilePartDefinition part)
    {
        var model = new ScriptObject
        {
            ["interface_name"] = part.InterfaceName,
            ["game_type_name"] = part.GameType.Name,
            ["parameters"] = part.Parameters.Select(p =>
            {
                string typeName = p.IsBool ? "bool" : (p.EnumTypeName ?? "float");
                return new ScriptObject
                {
                    ["name"] = p.PropertyName,
                    ["type_name"] = typeName,
                    ["description"] = !string.IsNullOrEmpty(p.Description) ? TypeNameHelper.EscapeXml(p.Description) : null,
                    ["is_read_only"] = p.IsReadOnly
                };
            }).ToList(),

            ["direct_properties"] = part.DirectProperties.Select(p => new ScriptObject
            {
                ["name"] = p.PropertyName,
                ["type_name"] = p.TypeName,
                ["description"] = !string.IsNullOrEmpty(p.Description) ? TypeNameHelper.EscapeXml(p.Description) : null,
                ["is_read_only"] = p.IsReadOnly
            }).ToList()
        };

        return model;
    }

    private ScriptObject CreateMissilePartFacadeModel(MissilePartDefinition part)
    {
        var model = new ScriptObject();
        var className = part.InterfaceName.StartsWith("I")
            ? part.InterfaceName.Substring(1)
            : part.InterfaceName;

        model["class_name"] = className;
        model["interface_name"] = part.InterfaceName;
        model["game_type_full_name"] = part.GameType.FullName;
        model["parameters"] = part.Parameters.Select(p =>
        {
            string typeName = p.IsBool ? "bool" : (p.EnumTypeName ?? "float");
            return new ScriptObject
            {
                ["index"] = p.Index,
                ["name"] = p.PropertyName,
                ["type_name"] = typeName,
                ["is_read_only"] = p.IsReadOnly,
                ["is_bool"] = p.IsBool,
                ["is_enum"] = p.EnumTypeName != null
            };
        }).ToList();

        model["direct_properties"] = part.DirectProperties.Select(p => new ScriptObject
        {
            ["name"] = p.PropertyName,
            ["type_name"] = p.TypeName,
            ["access_path"] = p.AccessPath,
            ["is_read_only"] = p.IsReadOnly,
            ["is_bool_float"] = p.IsBoolFloat
        }).ToList();

        return model;
    }

    private ScriptObject CreateMissilePartEnumsModel(List<GeneratedEnum> enums)
    {
        var model = new ScriptObject
        {
            ["enums"] = enums.Select(e => new ScriptObject
            {
                ["name"] = e.Name,
                ["description"] = $"Values for {e.Name} parameter.",
                ["values"] = e.Values.Select(v => new ScriptObject
                {
                    ["name"] = v.Value,
                    ["int_value"] = (int)v.Key
                }).ToList()
            }).ToList()
        };

        return model;
    }

    private ScriptObject CreateMissilePartFactoryModel(List<MissilePartDefinition> parts)
    {
        var model = new ScriptObject();

        // Sort by inheritance depth (most derived first) to avoid unreachable pattern errors
        var orderedParts = parts
            .OrderByDescending(p => GetInheritanceDepth(p.GameType))
            .ToList();

        model["parts"] = orderedParts.Select(p =>
        {
            var className = p.InterfaceName.StartsWith('I')
                ? p.InterfaceName[1..]
                : p.InterfaceName;
            return new ScriptObject
            {
                ["class_name"] = className,
                ["game_type_full_name"] = p.GameType.FullName
            };
        }).ToList();

        return model;
    }

    private static int GetInheritanceDepth(Type type)
    {
        int depth = 0;
        var current = type;
        while (current.BaseType != null)
        {
            depth++;
            current = current.BaseType;
        }
        return depth;
    }
}
