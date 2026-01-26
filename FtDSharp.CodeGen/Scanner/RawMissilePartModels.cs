using System.Reflection;

namespace FtDSharp.CodeGen.Scanner;

public class RawMissileComponentInfo
{
    /// <summary>The game type inheriting from MissileComponent.</summary>
    public Type GameType { get; set; } = typeof(object);

    /// <summary>The component category (FuelAndControl, Warhead, Utility, Thruster, Nose, SecondaryThruster).</summary>
    public int Category { get; set; }

    /// <summary>The category name as a string.</summary>
    public string CategoryName { get; set; } = "";

    /// <summary>Parameters discovered from the component.</summary>
    public List<RawMissileParameterInfo> Parameters { get; set; } = [];
}

/// <summary>
/// Raw discovered missile component parameter information.
/// </summary>
public class RawMissileParameterInfo
{
    /// <summary>Index in the UIParameterBag.</summary>
    public int Index { get; set; }

    /// <summary>Name of the parameter from game localization.</summary>
    public string Name { get; set; } = "";

    /// <summary>Description of the parameter.</summary>
    public string Description { get; set; } = "";

    /// <summary>Minimum value.</summary>
    public float Min { get; set; }

    /// <summary>Maximum value.</summary>
    public float Max { get; set; }

    /// <summary>Default value.</summary>
    public float Default { get; set; }

    /// <summary>Whether the parameter has display value mappings (for enums).</summary>
    public bool HasDisplayValues { get; set; }

    /// <summary>Display value mappings if present.</summary>
    public Dictionary<float, string> DisplayValues { get; set; } = [];
}
