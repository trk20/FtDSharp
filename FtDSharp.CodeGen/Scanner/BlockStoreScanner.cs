using BrilliantSkies.Blocks.BreadBoards.GenericGetter;

namespace FtDSharp.CodeGen.Scanner;

public class BlockStoreScanner
{
    public (Dictionary<Type, string> ConcreteStores, Dictionary<Type, string> InterfaceStores) Scan()
    {
        var concreteStores = new Dictionary<Type, string>();
        var interfaceStores = new Dictionary<Type, string>();
        Type storageInterface = typeof(IBlockToConstructBlockTypeStorage);

        foreach (System.Reflection.PropertyInfo prop in storageInterface.GetProperties())
        {
            Type propType = prop.PropertyType;

            if (!propType.IsGenericType || propType.GetGenericTypeDefinition() != typeof(BlockStore<>))
                continue;

            Type blockType = propType.GetGenericArguments()[0];

            if (blockType == typeof(Block))
                continue;

            if (blockType.IsInterface)
                interfaceStores.TryAdd(blockType, prop.Name);
            else
                concreteStores.TryAdd(blockType, prop.Name);
        }

        return (concreteStores, interfaceStores);
    }
}
