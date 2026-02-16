namespace FtDSharp.CodeGen.Models;

/// <summary>
/// Represents the binding of a block type to a BlockStore for optimized access.
/// </summary>
/// <param name="PropertyName">The property name on IBlockToConstructBlockTypeStorage.</param>
/// <param name="IsInterfaceStore">Whether the store holds an interface rather than a concrete type.</param>
/// <param name="RequiresTypeFilter">Whether filtering by specific type is needed (when using parent's store).</param>
public record StoreBinding(string PropertyName, bool IsInterfaceStore, bool RequiresTypeFilter = false);
