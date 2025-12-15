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
        int exactCount = 0, interfaceCount = 0, fallbackCount = 0;

        foreach (var block in blocks)
        {
            if (_concreteStores.TryGetValue(block.GameType, out var storeName))
            {
                block.StoreBinding = new StoreBinding(storeName, IsInterfaceStore: false);
                exactCount++;
                continue;
            }

            bool foundInterface = false;
            foreach (var iface in block.GameType.GetInterfaces())
            {
                if (_interfaceStores.TryGetValue(iface, out storeName))
                {
                    block.StoreBinding = new StoreBinding(storeName, IsInterfaceStore: true);
                    interfaceCount++;
                    foundInterface = true;
                    break;
                }
            }

            if (!foundInterface)
                fallbackCount++;
        }

        Log.Debug("Block stores: {Exact} exact + {Interface} interface-filtered, {Fallback} fall back to All.OfType<>()",
            exactCount, interfaceCount, fallbackCount);
    }
}
