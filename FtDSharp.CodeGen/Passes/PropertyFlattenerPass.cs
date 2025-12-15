using FtDSharp.CodeGen.Models;
using FtDSharp.CodeGen.Scanner;
using FtDSharp.CodeGen.Utils;
using Serilog;

namespace FtDSharp.CodeGen.Passes;

public class PropertyFlattenerPass
{
    public void Process(List<RawBlockInfo> rawBlocks, List<BlockDefinition> blocks)
    {

        Log.Debug("Processing raw properties for {Count} blocks...", rawBlocks.Count);
        var blocksByType = blocks.ToDictionary(b => b.GameType, b => b);

        foreach (var rawBlock in rawBlocks)
        {
            if (!blocksByType.TryGetValue(rawBlock.GameType, out var block))
                continue;

            foreach (var rawProp in rawBlock.Properties)
            {
                var propDef = ConvertToPropertyDefinition(rawProp, rawBlock.GameType);
                if (propDef != null)
                    block.AllProperties.Add(propDef);
            }
        }
    }

    private PropertyDefinition? ConvertToPropertyDefinition(RawPropertyInfo raw, Type declaringBlockType)
    {
        bool hasSetter = raw.IsDataPackageProperty &&
            raw.HasUserEditable &&
            (raw.VarUnderlyingType == null || raw.VarUsHasSetter);

        var resolvedType = raw.ResolvedPropertyType;

        return new PropertyDefinition
        {
            OriginalName = raw.RawCombinedName,
            Name = raw.RawCombinedName,
            PropertyType = resolvedType,
            TypeName = TypeNameHelper.GetFriendlyTypeName(resolvedType),
            AccessorPath = raw.BuildAccessorPath(),
            HasGetter = true,
            HasSetter = hasSetter,
            Description = CleanDescription(raw.Description, raw.DisplayName),
            SourceProperty = raw.Property,
            DeclaringType = declaringBlockType,
            DataPackageName = raw.DataPackageName
        };
    }

    private static string? CleanDescription(string? description, string? fallbackName)
    {
        if (string.IsNullOrWhiteSpace(description) || description == "!!!!")
            description = fallbackName;

        if (description == null)
            return null;

        if (description.StartsWith("LP: "))
            description = description[4..].Trim();

        if (description.EndsWith("{0:0.#}°"))
            description = description.Replace("{0:0.#}°", "in degrees");

        if (description == "!!!!" || string.IsNullOrWhiteSpace(description))
            return null;

        return description;
    }
}
