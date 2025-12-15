using System.Reflection;
using BrilliantSkies.DataManagement.Packages;
using UnityEngine;
using VariableAttribute = BrilliantSkies.DataManagement.Attributes.VariableAttribute;
using UserEditableAttribute = BrilliantSkies.DataManagement.Attributes.UserEditableAttribute;
using BrilliantSkies.Blocks.BreadBoards.GenericGetter;
using BrilliantSkies.Localisation.Widgets;

namespace FtDSharp.CodeGen.Scanner;

public class BlockScanner
{
    public List<RawBlockInfo> Scan(Assembly gameAssembly)
    {
        var results = new List<RawBlockInfo>();

        var blockTypes = gameAssembly.GetTypes()
            .Where(t => typeof(Block).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
            .Where(t => t.IsPublic || t.IsNestedPublic)
            .Where(t => !Overrides.SkipClasses.Contains(t))
            .OrderBy(t => t.Name)
            .ToList();

        foreach (var blockType in blockTypes)
        {
            var properties = DiscoverRawProperties(blockType);
            if (properties.Count == 0)
                continue;

            results.Add(new RawBlockInfo
            {
                GameType = blockType,
                Properties = properties
            });
        }

        return results;
    }

    private List<RawPropertyInfo> DiscoverRawProperties(Type blockType)
    {
        var properties = new List<RawPropertyInfo>();

        foreach (var prop in blockType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var readable = prop.GetCustomAttribute<ReadableAttribute>();
            if (readable == null) continue;
            if (prop.GetCustomAttribute<ObsoleteAttribute>() != null) continue;
            if (!IsSupportedType(prop.PropertyType)) continue;
            if (Overrides.SkipPatternProperties.Any(prop.Name.Contains)) continue;

            properties.Add(new RawPropertyInfo
            {
                Property = prop,
                DataPackageName = null,
                InnerPropertyName = null,
                Description = readable.Description,
                DisplayName = readable.Name,
                HasUserEditable = false,
                VarUnderlyingType = null,
                VarUsHasSetter = false
            });
        }

        var dataPackageProps = blockType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(p => typeof(DataPackage).IsAssignableFrom(p.PropertyType))
            .ToList();

        foreach (var dpProp in dataPackageProps)
        {
            if (Overrides.SkipDataPackages.Contains(dpProp.Name)) continue;

            foreach (var innerProp in dpProp.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var rawPropInfo = DiscoverDataPackageProperty(dpProp, innerProp);
                if (rawPropInfo != null)
                    properties.Add(rawPropInfo);
            }
        }

        return properties;
    }

    private RawPropertyInfo? DiscoverDataPackageProperty(PropertyInfo dpProp, PropertyInfo innerProp)
    {
        var readable = innerProp.GetCustomAttribute<ReadableAttribute>();
        var variable = innerProp.GetCustomAttribute<VariableAttribute>()
            ?? innerProp.GetCustomAttribute<LocalVariableScrapedAttribute>();

        var allAttrs = innerProp.GetCustomAttributes().ToList();
        var hasEditable = allAttrs.Any(a => a.GetType().Name == nameof(UserEditableAttribute));

        if (allAttrs.Any(a => a.GetType().Name == nameof(ObsoleteAttribute))) return null;
        if (readable == null && variable == null) return null;

        var fullPropName = $"{dpProp.Name}_{innerProp.Name}";
        if (Overrides.SkipPatternProperties.Any(fullPropName.Contains)) return null;

        var propType = innerProp.PropertyType;
        var usProp = propType.GetProperty("Us", BindingFlags.Instance | BindingFlags.Public);

        Type? varUnderlyingType = null;
        bool varUsHasSetter = false;

        if (usProp != null && IsSupportedType(usProp.PropertyType))
        {
            varUnderlyingType = usProp.PropertyType;
            varUsHasSetter = usProp.CanWrite;
        }
        else if (!IsSupportedType(propType))
        {
            return null;
        }

        return new RawPropertyInfo
        {
            Property = innerProp,
            DataPackageName = dpProp.Name,
            InnerPropertyName = innerProp.Name,
            Description = readable?.Description ?? variable?.Description,
            DisplayName = readable?.Name ?? variable?.Name,
            HasUserEditable = hasEditable,
            VarUnderlyingType = varUnderlyingType,
            VarUsHasSetter = varUsHasSetter
        };
    }

    private static bool IsSupportedType(Type type)
    {
        if (type == typeof(int) || type == typeof(float) || type == typeof(double) ||
            type == typeof(bool) || type == typeof(string) || type == typeof(uint) ||
            type == typeof(long) || type == typeof(short) || type == typeof(byte))
            return true;

        if (type.FullName?.StartsWith("UnityEngine.") == true)
        {
            var name = type.Name;
            return name == nameof(Vector3) || name == nameof(Vector2) ||
                   name == nameof(Quaternion) || name == nameof(Color);
        }

        if (type.IsEnum) return true;

        if (Nullable.GetUnderlyingType(type) != null)
            return IsSupportedType(Nullable.GetUnderlyingType(type)!);

        return false;
    }
}
