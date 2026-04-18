using System.Reflection;

namespace FtDSharp.CodeGen.Utils;

/// <summary>
/// Helpers for getting types from game assemblies that may have unresolvable dependencies
/// </summary>
public static class AssemblyTypes
{
    public static Type[] GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null).ToArray()!;
        }
    }
}
