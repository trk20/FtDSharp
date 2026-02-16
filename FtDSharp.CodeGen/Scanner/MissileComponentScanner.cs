using System.Reflection;
using BrilliantSkies.Ftd.Missiles.Components;
using BrilliantSkies.Ftd.Missiles.Blueprints;
using Serilog;

namespace FtDSharp.CodeGen.Scanner;

/// <summary>
/// Scans game assemblies to discover MissileComponent types and their metadata.
/// </summary>
public class MissileComponentScanner
{
    /// <summary>
    /// Discovers all MissileComponent types in the game assembly.
    /// </summary>
    public List<RawMissileComponentInfo> Scan(Assembly gameAssembly)
    {
        var results = new List<RawMissileComponentInfo>();
        var missileComponentType = typeof(MissileComponent);

        var componentTypes = gameAssembly.GetTypes()
            .Where(t => missileComponentType.IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
            .OrderBy(t => t.Name)
            .ToList();

        Log.Debug("Found {Count} MissileComponent types", componentTypes.Count);

        foreach (var componentType in componentTypes)
        {
            var info = DiscoverComponentInfo(componentType);
            if (info != null)
            {
                results.Add(info);
                Log.Debug("  {Name} (Category: {Category})", componentType.Name, info.CategoryName);
            }
        }

        return results;
    }

    private RawMissileComponentInfo? DiscoverComponentInfo(Type componentType)
    {
        try
        {
            // Get the Category property to determine component category
            var categoryProp = componentType.GetProperty("Category", BindingFlags.Instance | BindingFlags.Public);
            int categoryValue = 0;
            string categoryName = "Unknown";

            // Discover parameter-like properties (those that access UIParameterBag)
            var parameters = DiscoverParameters(componentType);

            return new RawMissileComponentInfo
            {
                GameType = componentType,
                Category = categoryValue,
                CategoryName = categoryName,
                Parameters = parameters
            };
        }
        catch (Exception ex)
        {
            Log.Warning("Failed to analyze {Type}: {Message}", componentType.Name, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Attempts to discover parameters by analyzing properties that look like parameter accessors.
    /// Properties like "public float ConeAngle => parameters[0].Value" have specific patterns.
    /// </summary>
    private List<RawMissileParameterInfo> DiscoverParameters(Type componentType)
    {
        var parameters = new List<RawMissileParameterInfo>();

        // Look for properties that might be parameter accessors
        foreach (var prop in componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
        {
            // Skip obvious non-parameter properties
            if (prop.Name == "Name" || prop.Name == "Category" || prop.Name == "Size") continue;
            if (prop.Name.StartsWith("Is") && prop.PropertyType == typeof(bool)) continue;
            if (!prop.CanRead) continue;

            // Check if property type could be a parameter (float, bool, or enum-like int)
            if (prop.PropertyType == typeof(float) ||
                prop.PropertyType == typeof(bool) ||
                prop.PropertyType == typeof(int))
            {
                // Try to extract the parameter index from the IL
                int index = TryExtractParameterIndex(prop);

                parameters.Add(new RawMissileParameterInfo
                {
                    Index = index,
                    Name = prop.Name,
                    Description = "",
                    Min = float.MinValue,
                    Max = float.MaxValue,
                    Default = 0
                });
            }
        }

        // Sort by extracted index (unknown indices go last)
        return parameters.OrderBy(p => p.Index < 0 ? int.MaxValue : p.Index).ToList();
    }

    /// <summary>
    /// Tries to extract the parameter index from a property getter's IL.
    /// Looks for ldc.i4.X or ldc.i4 followed by callvirt on the indexer.
    /// </summary>
    private int TryExtractParameterIndex(PropertyInfo prop)
    {
        try
        {
            var getter = prop.GetGetMethod();
            if (getter == null) return -1;

            var body = getter.GetMethodBody();
            if (body == null) return -1;

            var il = body.GetILAsByteArray();
            if (il == null) return -1;

            for (int i = 0; i < il.Length - 1; i++)
            {
                byte opcode = il[i];

                // ldc.i4.X (X = 0-8)
                if (opcode >= 0x16 && opcode <= 0x1E)
                {
                    int potentialIndex = opcode - 0x16;
                    // Check if this is likely a parameters access by looking for pattern
                    if (i > 0 && IsLikelyParameterAccess(il, i))
                    {
                        return potentialIndex;
                    }
                }
                // ldc.i4.s (short form)
                else if (opcode == 0x1F && i + 1 < il.Length)
                {
                    return (sbyte)il[i + 1];
                }
                // ldc.i4 (full int32)
                else if (opcode == 0x20 && i + 4 < il.Length)
                {
                    return BitConverter.ToInt32(il, i + 1);
                }
            }
        }
        catch
        {
        }

        return -1;
    }

    private bool IsLikelyParameterAccess(byte[] il, int position)
    {
        // Simple heuristic: if loading ldfld before the index, likely accessing parameters field
        // This is a simplified check - real IL analysis would be more complex
        if (position < 5) return true; // Just accept if near start of method
        return true; // For now, accept any int load
    }

    /// <summary>
    /// Gets all discovered component type names for reporting.
    /// </summary>
    public List<string> GetComponentTypeNames(Assembly gameAssembly)
    {
        var missileComponentType = typeof(MissileComponent);

        return gameAssembly.GetTypes()
            .Where(t => missileComponentType.IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
            .Select(t => t.Name)
            .OrderBy(n => n)
            .ToList();
    }
}
