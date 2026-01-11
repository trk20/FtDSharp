// Here to allow adding specialized facades in the future as needed
namespace FtDSharp.Facades
{
    /// <summary>
    /// Factory for creating block facades from game Block objects.
    /// </summary>
    internal static class BlockFacadeFactory
    {
        /// <summary>
        /// Creates a facade for a Block. Returns a specialized facade for known types,
        /// or a generic BlockFacade for blocks that don't have a specialized facade.
        /// </summary>
        public static IBlock CreateFacade(Block block)
        {
            if (block == null) return null!;

            // Create WeaponFacade for ConstructableWeapons (including turrets)
            if (block is ConstructableWeapon weapon)
            {
                // Get the AllConstruct from the block's main construct
                var allConstruct = block.MainConstruct as AllConstruct;
                if (allConstruct != null)
                {
                    return new WeaponFacade(weapon, allConstruct);
                }
            }

            return new GenericBlockFacade(block);
        }
    }

    /// <summary>
    /// Generic facade for blocks that don't have a specialized wrapper.
    /// </summary>
    internal class GenericBlockFacade : BlockFacadeBase
    {
        public GenericBlockFacade(Block block) : base(block)
        {
        }
    }
}
