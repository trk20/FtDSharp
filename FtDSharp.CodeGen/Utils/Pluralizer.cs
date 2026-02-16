namespace FtDSharp.CodeGen.Utils;

/// <summary>
/// Handles pluralization of block class names for collection property generation.
/// </summary>
public static class Pluralizer
{
    /// <summary>
    /// Manual overrides for names that can't be handled by rules.
    /// Maps singular class name to plural collection name.
    /// </summary>
    private static readonly Dictionary<string, string> PluralOverrides = new()
    {
        // Words ending in "Optics" are already plural (uncountable noun)
        ["LaserFocusOptics"] = "LaserFocusOptics",
        ["LaserOptics"] = "LaserOptics",
        ["LaserSteeringOptics"] = "LaserSteeringOptics",

        // Irregular plurals (foot -> feet)
        ["ClampyFoot"] = "ClampyFeet",
        ["StickyFoot"] = "StickyFeet",
    };

    /// <summary>
    /// Pluralizes a class name for use as a collection property name.
    /// </summary>
    public static string Pluralize(string className)
    {
        // Check manual overrides first
        if (PluralOverrides.TryGetValue(className, out var plural))
            return plural;

        // Apply rules
        return ApplyPluralizationRules(className);
    }

    private static string ApplyPluralizationRules(string name)
    {
        // Handle words ending in consonant + y (Battery -> Batteries)
        if (name.Length > 1 && name.EndsWith("y") && !IsVowel(name[^2]))
        {
            return name[..^1] + "ies";
        }

        // Handle words ending in ch, sh, x, s, z (Box -> Boxes, Winch -> Winches)
        if (name.EndsWith("ch") || name.EndsWith("sh") ||
            name.EndsWith("x") || name.EndsWith("s") || name.EndsWith("z"))
        {
            return name + "es";
        }

        // Default: just add s
        return name + "s";
    }

    private static bool IsVowel(char c) => "aeiouAEIOU".Contains(c);
}
