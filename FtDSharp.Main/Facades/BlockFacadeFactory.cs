using System;
using System.Collections.Generic;

namespace FtDSharp.Facades
{
    /// <summary>
    /// Persistent cache for facade instances. Ensures the same facade is returned
    /// for the same underlying block, regardless of access path.
    /// Unlike per-frame caching, this persists across frames since block references
    /// are stable until the block is removed from the design.
    /// </summary>
    internal static class FacadeCache
    {
        // Persistent cache keyed by (block type name, unique ID)
        // Facade type is NOT included to ensure consistent instances regardless of access path
        private static readonly Dictionary<(string, int), object> cache = new();

        /// <summary>
        /// Gets a cached facade or creates a new one using the factory.
        /// The cache key is based on the block identity only, not the facade type.
        /// Handles deleted blocks with lazy cleanup.
        /// </summary>
        public static TFacade GetOrCreate<TFacade>(Block block, Func<TFacade> factory)
            where TFacade : class
        {
            if (block == null) return null!;

            // Don't cache facades for deleted blocks
            if (block.IsDeleted) return factory();

            var key = (block.Name, block.IdSet.Id.Us);

            if (cache.TryGetValue(key, out var cached))
            {
                // Lazy cleanup: if the cached facade's underlying block was deleted, remove and recreate
                if (cached is BlockFacadeBase facade && facade.Block.IsDeleted)
                {
                    cache.Remove(key);
                    var newFacade = factory();
                    cache[key] = newFacade;
                    return newFacade;
                }
                return (cached as TFacade)!;
            }

            var createdFacade = factory();
            cache[key] = createdFacade;
            return createdFacade;
        }

        /// <summary>
        /// Clears the entire facade cache. Called when switching constructs or for testing.
        /// </summary>
        public static void Clear() => cache.Clear();
    }

    /// <summary>
    /// Factory for creating block facades from game Block objects.
    /// Uses FacadeCache to ensure consistent instances within a frame.
    /// </summary>
    internal static class BlockFacadeFactory
    {
        /// <summary>
        /// Gets or creates a WeaponFacade for the given weapon.
        /// Returns the same instance for the same weapon within a frame.
        /// Uses BlockFactory.Wrap to create the most derived facade type.
        /// </summary>
        public static WeaponFacade GetOrCreateWeaponFacade(ConstructableWeapon weapon, AllConstruct allConstruct)
        {
            if (weapon == null) return null!;

            // Use FacadeCache with BlockFactory.Wrap to ensure most derived type is created
            return FacadeCache.GetOrCreate<WeaponFacade>(weapon, () =>
            {
                // BlockFactory.Wrap returns the most derived facade type
                var facade = BlockFactory.Wrap(weapon);
                if (facade is WeaponFacade weaponFacade)
                    return weaponFacade;

                // Fallback for weapons not in the generated factory
                if (weapon is Turrets turret)
                    return new TurretFacade(turret, allConstruct);
                return new WeaponFacade(weapon, allConstruct);
            });
        }

        /// <summary>
        /// Gets or creates a TurretFacade for the given turret.
        /// Returns the same instance for the same turret within a frame.
        /// Uses BlockFactory.Wrap to create the most derived facade type.
        /// </summary>
        public static TurretFacade GetOrCreateTurretFacade(Turrets turret, AllConstruct allConstruct)
        {
            if (turret == null) return null!;
            return FacadeCache.GetOrCreate<TurretFacade>(turret, () =>
            {
                // BlockFactory.Wrap returns the most derived facade type
                var facade = BlockFactory.Wrap(turret);
                if (facade is TurretFacade turretFacade)
                    return turretFacade;

                // Fallback
                return new TurretFacade(turret, allConstruct);
            });
        }

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
                var allConstruct = block.MainConstruct as AllConstruct;
                if (allConstruct != null)
                {
                    return GetOrCreateWeaponFacade(weapon, allConstruct);
                }
            }

            return FacadeCache.GetOrCreate<GenericBlockFacade>(block, () => new GenericBlockFacade(block));
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
