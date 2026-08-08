using FtDSharp.CodeGen.Models;
using FtDSharp.CodeGen.Utils;

namespace FtDSharp.CodeGen.Generators;

public static class BlockTemplateModels
{
    public static BlockInterfaceTemplateModel CreateInterfaceModel(BlockDefinition block)
    {
        var baseInterfaces = new List<string>();
        if (block.ParentInterfaceName != null)
            baseInterfaces.Add(block.ParentInterfaceName);
        else if (block.Surface.IsTurret)
            baseInterfaces.Add("ITurret");
        else if (block.Surface.IsWeapon)
            baseInterfaces.Add("IWeapon");
        else
            baseInterfaces.Add("IBlock");
        baseInterfaces.AddRange(block.ImplementedLogicalInterfaces);

        return new BlockInterfaceTemplateModel(
            ClassName: block.ClassName,
            InterfaceName: block.InterfaceName,
            GameTypeName: block.GameType.Name,
            Inheritance: baseInterfaces,
            Properties: block.Surface.InterfaceProperties.Select(ToPropertyModel).ToList());
    }

    public static BlockFacadeTemplateModel CreateFacadeModel(BlockDefinition block)
    {
        var allInterfaces = new List<string> { block.InterfaceName };
        allInterfaces.AddRange(block.ImplementedLogicalInterfaces);

        return new BlockFacadeTemplateModel(
            ClassName: block.ClassName,
            InterfaceName: block.InterfaceName,
            GameTypeFullName: block.GameType.FullName!,
            AllInterfaces: allInterfaces,
            IsWeapon: block.Surface.IsWeapon,
            IsTurret: block.Surface.IsTurret,
            Properties: block.Surface.FacadeProperties.Select(ToFacadePropertyModel).ToList());
    }

    public static LogicalInterfacesTemplateModel CreateLogicalInterfacesModel(
        List<LogicalInterfaceDefinition> definitions,
        List<BlockDefinition> blocks)
    {
        var interfaces = new List<LogicalInterfaceTemplateModel>();

        foreach (LogicalInterfaceDefinition def in definitions)
        {
            BlockDefinition? sampleBlock = blocks.FirstOrDefault(b => b.ImplementedLogicalInterfaces.Contains(def.InterfaceName));
            if (sampleBlock == null)
                continue;

            var propsForInterface = sampleBlock.AllProperties
                .Where(p => def.PropertyNames.Contains(p.Name))
                .ToDictionary(p => p.Name);

            var parentPropNames = def.InheritsFrom
                .SelectMany(parent => LogicalInterfaces.Definitions
                    .Where(d => d.InterfaceName == parent)
                    .SelectMany(d => d.PropertyNames))
                .ToHashSet();

            interfaces.Add(new LogicalInterfaceTemplateModel(
                Name: def.InterfaceName,
                Description: def.Description,
                InheritsFrom: def.InheritsFrom.ToList(),
                Properties: def.PropertyNames
                    .Where(pn => !parentPropNames.Contains(pn))
                    .Where(propsForInterface.ContainsKey)
                    .Select(pn => ToPropertyModel(propsForInterface[pn]))
                    .ToList()));
        }

        return new LogicalInterfacesTemplateModel(interfaces);
    }

    public static BlockFactoryTemplateModel CreateBlockFactoryModel(List<BlockDefinition> blocks) =>
        new(blocks.Select(b => new BlockFactoryEntryTemplateModel(
            ClassName: b.ClassName,
            GameTypeFullName: b.GameType.FullName!,
            IsWeapon: b.Surface.IsWeapon,
            IsTurret: b.Surface.IsTurret)).ToList());

    public static BlocksAccessorTemplateModel CreateBlocksAccessorModel(List<BlockDefinition> blocks) =>
        new(blocks.Select(b =>
        {
            var pluralName = Pluralizer.Pluralize(b.ClassName);
            var fieldName = $"_{char.ToLower(pluralName[0])}{pluralName[1..]}";

            return new BlocksAccessorEntryTemplateModel(
                ClassName: b.ClassName,
                InterfaceName: b.InterfaceName,
                PluralName: pluralName,
                FieldName: fieldName,
                GameTypeFullName: b.GameType.FullName!,
                HasStore: b.StoreBinding != null,
                StorePropertyName: b.StoreBinding?.PropertyName,
                IsInterfaceStore: b.StoreBinding?.IsInterfaceStore ?? false,
                RequiresTypeFilter: b.StoreBinding?.RequiresTypeFilter ?? false,
                IsWeapon: b.Surface.IsWeapon,
                IsTurret: b.Surface.IsTurret);
        }).ToList());

    private static BlockPropertyTemplateModel ToPropertyModel(PropertyDefinition prop) =>
        new(
            Name: prop.Name,
            TypeName: prop.TypeName,
            Description: prop.Description != null ? TypeNameHelper.EscapeXml(prop.Description) : null,
            HasSetter: prop.HasSetter);

    private static BlockFacadePropertyTemplateModel ToFacadePropertyModel(PropertyDefinition prop) =>
        new(
            Name: prop.Name,
            TypeName: prop.TypeName,
            AccessorPath: prop.AccessorPath,
            HasSetter: prop.HasSetter);
}

public sealed record BlockInterfaceTemplateModel(
    string ClassName,
    string InterfaceName,
    string GameTypeName,
    IReadOnlyList<string> Inheritance,
    IReadOnlyList<BlockPropertyTemplateModel> Properties);

public sealed record BlockFacadeTemplateModel(
    string ClassName,
    string InterfaceName,
    string GameTypeFullName,
    IReadOnlyList<string> AllInterfaces,
    bool IsWeapon,
    bool IsTurret,
    IReadOnlyList<BlockFacadePropertyTemplateModel> Properties);

public sealed record BlockPropertyTemplateModel(
    string Name,
    string TypeName,
    string? Description,
    bool HasSetter);

public sealed record BlockFacadePropertyTemplateModel(
    string Name,
    string TypeName,
    string AccessorPath,
    bool HasSetter);

public sealed record LogicalInterfacesTemplateModel(
    IReadOnlyList<LogicalInterfaceTemplateModel> Interfaces);

public sealed record LogicalInterfaceTemplateModel(
    string Name,
    string Description,
    IReadOnlyList<string> InheritsFrom,
    IReadOnlyList<BlockPropertyTemplateModel> Properties);

public sealed record BlockFactoryTemplateModel(
    IReadOnlyList<BlockFactoryEntryTemplateModel> Blocks);

public sealed record BlockFactoryEntryTemplateModel(
    string ClassName,
    string GameTypeFullName,
    bool IsWeapon,
    bool IsTurret);

public sealed record BlocksAccessorTemplateModel(
    IReadOnlyList<BlocksAccessorEntryTemplateModel> Blocks);

public sealed record BlocksAccessorEntryTemplateModel(
    string ClassName,
    string InterfaceName,
    string PluralName,
    string FieldName,
    string GameTypeFullName,
    bool HasStore,
    string? StorePropertyName,
    bool IsInterfaceStore,
    bool RequiresTypeFilter,
    bool IsWeapon,
    bool IsTurret);
