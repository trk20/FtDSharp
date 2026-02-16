using FtDSharp.CodeGen.Models;
using Serilog;

namespace FtDSharp.CodeGen.Passes;

public class StoreOptimizationPass : IBlockPass
{
    private readonly Dictionary<Type, string> _concreteStores;
    private readonly Dictionary<Type, string> _interfaceStores;

    public StoreOptimizationPass(Dictionary<Type, string> concreteStores, Dictionary<Type, string> interfaceStores)
    {
        _concreteStores = concreteStores;
        _interfaceStores = interfaceStores;
    }

    public void Process(List<BlockDefinition> blocks)
    {
        Log.Debug("Assigning block stores for {Count} blocks...", blocks.Count);
        int exactCount = 0, interfaceCount = 0, parentCount = 0, fallbackCount = 0;

        foreach (var block in blocks)
        {
            // 1. Check for exact concrete type match
            if (_concreteStores.TryGetValue(block.GameType, out var storeName))
            {
                block.StoreBinding = new StoreBinding(storeName, IsInterfaceStore: false);
                exactCount++;
                continue;
            }

            // 2. Check if block implements an interface with a store
            bool found = false;
            foreach (var iface in block.GameType.GetInterfaces())
            {
                if (_interfaceStores.TryGetValue(iface, out storeName))
                {
                    block.StoreBinding = new StoreBinding(storeName, IsInterfaceStore: true);
                    interfaceCount++;
                    found = true;
                    break;
                }
            }
            if (found) continue;

            // 3. Check parent class hierarchy for a store (filter required)
            var parentType = block.GameType.BaseType;
            while (parentType != null && parentType != typeof(object))
            {
                if (_concreteStores.TryGetValue(parentType, out storeName))
                {
                    block.StoreBinding = new StoreBinding(storeName, IsInterfaceStore: false, RequiresTypeFilter: true);
                    parentCount++;
                    found = true;
                    break;
                }
                parentType = parentType.BaseType;
            }
            if (found) continue;

            fallbackCount++;
        }

        Log.Debug("Block stores: {Exact} exact + {Interface} interface + {Parent} parent-filtered, {Fallback} fall back to All.OfType<>()",
            exactCount, interfaceCount, parentCount, fallbackCount);
    }
}
