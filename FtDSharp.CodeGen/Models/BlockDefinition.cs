namespace FtDSharp.CodeGen.Models;

public class BlockDefinition
{
    public Type GameType { get; set; } = typeof(object);
    public string ClassName { get; set; } = "";
    public string InterfaceName => $"I{ClassName}";

    public BlockDefinition? Parent { get; set; }
    public string? ParentInterfaceName => Parent?.InterfaceName;

    public List<string> ImplementedLogicalInterfaces { get; set; } = [];
    public List<PropertyDefinition> Properties { get; set; } = [];
    public List<PropertyDefinition> AllProperties { get; set; } = [];
    public StoreBinding? StoreBinding { get; set; }

    public override string ToString() => $"{ClassName} ({Properties.Count} unique, {AllProperties.Count} total props)";
}
