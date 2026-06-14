using FtDSharp.CodeGen.Models;
using Serilog;

namespace FtDSharp.CodeGen.Passes;

public static class StoreOptimizationPass
{
    public static void Run(
        IReadOnlyList<BlockDefinition> blocks,
        IReadOnlyDictionary<Type, string> concreteStores,
        IReadOnlyDictionary<Type, string> interfaceStores)
    {
        Log.Debug("Assigning block stores for {Count} blocks...", blocks.Count);
        int exactCount = 0, interfaceCount = 0, parentCount = 0, fallbackCount = 0;

        foreach (var block in blocks)
        {
            if (concreteStores.TryGetValue(block.GameType, out var storeName))
            {
                block.StoreBinding = new StoreBinding(storeName, IsInterfaceStore: false);
                exactCount++;
                continue;
            }

            bool found = false;
            foreach (var iface in block.GameType.GetInterfaces())
            {
                if (interfaceStores.TryGetValue(iface, out storeName))
                {
                    block.StoreBinding = new StoreBinding(storeName, IsInterfaceStore: true);
                    interfaceCount++;
                    found = true;
                    break;
                }
            }
            if (found) continue;

            var parentType = block.GameType.BaseType;
            while (parentType != null && parentType != typeof(object))
            {
                if (concreteStores.TryGetValue(parentType, out storeName))
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
