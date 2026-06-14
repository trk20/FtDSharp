namespace FtDSharp.CodeGen.Models;

/// <summary>
/// Immutable render projection for a block, computed once after all prerequisite passes complete.
/// </summary>
public sealed class BlockRenderSurface
{
    public BlockKind Kind { get; init; }
    public bool IsWeapon => Kind == BlockKind.Weapon;
    public bool IsTurret => Kind == BlockKind.Turret;
    public IReadOnlyList<PropertyDefinition> InterfaceProperties { get; init; } = [];
    public IReadOnlyList<PropertyDefinition> FacadeProperties { get; init; } = [];
}
