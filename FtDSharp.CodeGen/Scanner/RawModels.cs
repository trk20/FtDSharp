using System.Reflection;

namespace FtDSharp.CodeGen.Scanner;

public class RawBlockInfo
{
    public Type GameType { get; set; } = typeof(object);
    public List<RawPropertyInfo> Properties { get; set; } = [];
}

public class RawPropertyInfo
{
    public PropertyInfo Property { get; set; } = null!;
    public string? DataPackageName { get; set; }
    public string? InnerPropertyName { get; set; }
    public bool IsDataPackageProperty => DataPackageName != null;

    public string? Description { get; set; }
    public string? DisplayName { get; set; }
    public bool HasUserEditable { get; set; }
    public Type? VarUnderlyingType { get; set; }
    public bool VarUsHasSetter { get; set; }

    public Type ResolvedPropertyType => VarUnderlyingType ?? Property.PropertyType;

    public string RawCombinedName => DataPackageName != null
        ? $"{DataPackageName}_{InnerPropertyName}"
        : Property.Name;

    public string BuildAccessorPath()
    {
        if (DataPackageName == null)
            return Property.Name;

        return VarUnderlyingType != null
            ? $"{DataPackageName}.{InnerPropertyName}.Us"
            : $"{DataPackageName}.{InnerPropertyName}";
    }
}
