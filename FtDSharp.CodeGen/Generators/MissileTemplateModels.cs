using FtDSharp.CodeGen.Utils;

namespace FtDSharp.CodeGen.Generators;

public static class MissileTemplateModels
{
    public static MissilePartInterfaceTemplateModel CreateInterfaceModel(MissilePartDefinition part) =>
        new(
            InterfaceName: part.InterfaceName,
            GameTypeName: part.GameType.Name,
            Parameters: part.Parameters.Select(ToParameterModel).ToList(),
            DirectProperties: part.DirectProperties.Select(ToDirectPropertyModel).ToList());

    public static MissilePartFacadeTemplateModel CreateFacadeModel(MissilePartDefinition part) =>
        new(
            ClassName: MissilePartNaming.FacadeClassName(part.InterfaceName),
            InterfaceName: part.InterfaceName,
            GameTypeFullName: part.GameType.FullName!,
            Parameters: part.Parameters.Select(ToFacadeParameterModel).ToList(),
            DirectProperties: part.DirectProperties.Select(ToFacadeDirectPropertyModel).ToList());

    public static MissilePartEnumsTemplateModel CreateEnumsModel(List<GeneratedEnum> enums) =>
        new(enums.Select(e => new MissileEnumTemplateModel(
            Name: e.Name,
            Description: $"Values for {e.Name} parameter.",
            Values: e.Values.Select(v => new MissileEnumValueTemplateModel(
                Name: v.Value,
                IntValue: (int)v.Key)).ToList())).ToList());

    public static MissilePartFactoryTemplateModel CreateFactoryModel(List<MissilePartDefinition> parts)
    {
        var orderedParts = parts
            .OrderByDescending(p => GetInheritanceDepth(p.GameType))
            .ToList();

        return new MissilePartFactoryTemplateModel(
            orderedParts.Select(p => new MissilePartFactoryEntryTemplateModel(
                ClassName: MissilePartNaming.FacadeClassName(p.InterfaceName),
                GameTypeFullName: p.GameType.FullName!)).ToList());
    }

    private static MissileParameterTemplateModel ToParameterModel(MissileParameterDefinition param) =>
        new(
            Name: param.PropertyName,
            TypeName: param.TypeName,
            Description: !string.IsNullOrEmpty(param.Description) ? TypeNameHelper.EscapeXml(param.Description) : null,
            IsReadOnly: param.IsReadOnly);

    private static MissileFacadeParameterTemplateModel ToFacadeParameterModel(MissileParameterDefinition param) =>
        new(
            Index: param.Index,
            Name: param.PropertyName,
            TypeName: param.TypeName,
            IsReadOnly: param.IsReadOnly,
            IsBool: param.IsBool,
            IsEnum: param.EnumTypeName != null);

    private static MissileDirectPropertyTemplateModel ToDirectPropertyModel(DirectPropertyDefinition prop) =>
        new(
            Name: prop.PropertyName,
            TypeName: prop.TypeName,
            Description: !string.IsNullOrEmpty(prop.Description) ? TypeNameHelper.EscapeXml(prop.Description) : null,
            IsReadOnly: prop.IsReadOnly);

    private static MissileFacadeDirectPropertyTemplateModel ToFacadeDirectPropertyModel(DirectPropertyDefinition prop) =>
        new(
            Name: prop.PropertyName,
            TypeName: prop.TypeName,
            AccessPath: prop.AccessPath,
            IsReadOnly: prop.IsReadOnly,
            IsBoolFloat: prop.IsBoolFloat);

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

public sealed record MissilePartInterfaceTemplateModel(
    string InterfaceName,
    string GameTypeName,
    IReadOnlyList<MissileParameterTemplateModel> Parameters,
    IReadOnlyList<MissileDirectPropertyTemplateModel> DirectProperties);

public sealed record MissilePartFacadeTemplateModel(
    string ClassName,
    string InterfaceName,
    string GameTypeFullName,
    IReadOnlyList<MissileFacadeParameterTemplateModel> Parameters,
    IReadOnlyList<MissileFacadeDirectPropertyTemplateModel> DirectProperties);

public sealed record MissileParameterTemplateModel(
    string Name,
    string TypeName,
    string? Description,
    bool IsReadOnly);

public sealed record MissileFacadeParameterTemplateModel(
    int Index,
    string Name,
    string TypeName,
    bool IsReadOnly,
    bool IsBool,
    bool IsEnum);

public sealed record MissileDirectPropertyTemplateModel(
    string Name,
    string TypeName,
    string? Description,
    bool IsReadOnly);

public sealed record MissileFacadeDirectPropertyTemplateModel(
    string Name,
    string TypeName,
    string AccessPath,
    bool IsReadOnly,
    bool IsBoolFloat);

public sealed record MissilePartEnumsTemplateModel(
    IReadOnlyList<MissileEnumTemplateModel> Enums);

public sealed record MissileEnumTemplateModel(
    string Name,
    string Description,
    IReadOnlyList<MissileEnumValueTemplateModel> Values);

public sealed record MissileEnumValueTemplateModel(
    string Name,
    int IntValue);

public sealed record MissilePartFactoryTemplateModel(
    IReadOnlyList<MissilePartFactoryEntryTemplateModel> Parts);

public sealed record MissilePartFactoryEntryTemplateModel(
    string ClassName,
    string GameTypeFullName);
