namespace FtDSharp.CodeGen.Models;

public class BlockDefinition
{
    public Type GameType { get; set; } = typeof(object);
    public string ClassName { get; set; } = "";
    public string InterfaceName => $"I{ClassName}";

    public BlockDefinition? Parent { get; set; }
    public string? ParentInterfaceName => Parent?.InterfaceName;

    public List<string> ImplementedLogicalInterfaces { get; set; } = [];
    public List<PropertyDefinition> AllProperties { get; set; } = [];
    public StoreBinding? StoreBinding { get; set; }

    /// <summary>
    /// Render projection populated by <see cref="Passes.BlockSurfaceRules.ApplyAll"/>.
    /// </summary>
    public BlockRenderSurface Surface { get; private set; } = EmptySurface;

    private static readonly BlockRenderSurface EmptySurface = new();

    internal void SetSurface(BlockRenderSurface surface) => Surface = surface;

    public HashSet<string> GetInheritedPropertyNames()
    {
        var inherited = new HashSet<string>();
        BlockDefinition? current = Parent;

        while (current != null)
        {
            foreach (PropertyDefinition prop in current.AllProperties)
                inherited.Add(prop.Name);
            current = current.Parent;
        }

        return inherited;
    }

    public override string ToString() =>
        $"{ClassName} ({Surface.InterfaceProperties.Count} unique, {AllProperties.Count} total props)";
}
