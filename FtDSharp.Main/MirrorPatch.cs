using System;
using System.Collections.Generic;
using System.Reflection;
using BrilliantSkies.Core.CSharp;
using BrilliantSkies.Core.Logger;

namespace FtDSharp
{
    /// <summary>
    /// Harmony prefix that replaces <see cref="Mirror.GetAllOfInterface(Type)"/> to skip
    /// Microsoft.CodeAnalysis and System assemblies, preventing TypeLoadExceptions caused
    /// by Roslyn types with unresolvable dependencies.
    /// Applied manually by <see cref="FtDPrePatch"/> during early plugin loading.
    /// </summary>
    internal static class MirrorPatch
    {
        public static bool Prefix_GetAllOfInterface(Type type, ref IEnumerable<Type> __result)
        {
            var results = new List<Type>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string assemblyName = assembly.GetName().Name;

                if (assemblyName.StartsWith("Microsoft.CodeAnalysis") || assemblyName.StartsWith("System"))
                    continue;

                IEnumerable<Type> loadableTypes;
                bool error;
                try
                {
                    loadableTypes = assembly.GetLoadableTypes(out error);
                }
                catch (Exception e)
                {
                    AdvLogger.LogException(
                        $"Exception in assembly {assembly} when looking for interface classes of type {type}",
                        e, LogOptions._AlertDevAndCustomerInGame);
                    continue;
                }

                if (error)
                {
                    AdvLogger.LogError(
                        $"Error returned in assembly {assembly} when looking for interface classes of type {type}",
                        LogOptions.Popup | LogOptions.StackTrace);
                    continue;
                }

                foreach (Type loadableType in loadableTypes)
                {
                    try
                    {
                        if (type.IsAssignableFrom(loadableType) && loadableType.IsClass && !loadableType.IsAbstract)
                            results.Add(loadableType);
                    }
                    catch (Exception e)
                    {
                        AdvLogger.LogException(
                            $"Exception in assembly {assembly} on class {loadableType} when looking for interface classes of type {type}",
                            e, LogOptions._AlertDevAndCustomerInGame);
                    }
                }
            }

            __result = results;
            return false;
        }
    }
}
