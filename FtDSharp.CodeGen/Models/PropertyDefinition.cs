using System.Reflection;

namespace FtDSharp.CodeGen.Models;

public class PropertyDefinition
{
    public string OriginalName { get; set; } = "";
    public string Name { get; set; } = "";

    public Type PropertyType { get; set; } = typeof(object);
    public string TypeName { get; set; } = "";

    public string AccessorPath { get; set; } = "";
    public bool HasGetter { get; set; } = true;
    public bool HasSetter { get; set; }

    public string? Description { get; set; }

    public PropertyInfo? SourceProperty { get; set; }
    public Type? DeclaringType { get; set; }
    public string? DataPackageName { get; set; }

    public override string ToString() => $"{Name} ({TypeName}) -> {AccessorPath}";
}
