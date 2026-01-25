using System.Collections.Generic;

namespace FtDSharp
{
    /// <summary>
    /// Represents the script's own construct with full control capabilities.
    /// </summary>
    public interface IMainConstruct : IFriendlyConstruct
    {
        /// <summary>Attempts to find a block by its unique ID.</summary>
        bool TryGetBlockById(int id, out IBlock? block);
        /// <summary>Gets all blocks of a specific type.</summary>
        List<T> GetAllBlocksOfType<T>() where T : IBlock;
        /// <summary>Get all blocks on the construct, including subconstructs.</summary>
        IEnumerable<IBlock> GetAllBlocks();
        /// <summary>All active script-controllable missiles launched by this construct.</summary>
        List<IMissile> Missiles { get; }
        /// <summary>Axis-based propulsion control.</summary>
        IPropulsion Propulsion { get; }
        /// <summary>All AI mainframes on this construct, sorted by priority (lower = higher).</summary>
        IReadOnlyList<IMainframe> Mainframes { get; }
        /// <summary>All weapons on this construct (excluding turrets).</summary>
        IReadOnlyList<IWeapon> Weapons { get; }
        /// <summary>All turrets on this construct.</summary>
        IReadOnlyList<ITurret> Turrets { get; }
    }
}
