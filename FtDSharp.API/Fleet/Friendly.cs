using System;
using System.Collections.Generic;

namespace FtDSharp;

/// <summary>
/// Static accessor for friendly construct and fleet information.
/// </summary>
public static class Friendly
{
    /// <summary>
    /// All friendly constructs, including the current construct.
    /// </summary>
    public static IReadOnlyList<IFriendlyConstruct> All =>
        ScriptContext.Current?.Fleet.All ?? Array.Empty<IFriendlyConstruct>();

    /// <summary>
    /// All friendly constructs except the current construct.
    /// </summary>
    public static IReadOnlyList<IFriendlyConstruct> AllExcludingSelf =>
        ScriptContext.Current?.Fleet.AllExcludingSelf ?? Array.Empty<IFriendlyConstruct>();

    /// <summary>
    /// All fleets containing friendly constructs.
    /// </summary>
    public static IReadOnlyList<IFleet> Fleets =>
        ScriptContext.Current?.Fleet.Fleets ?? Array.Empty<IFleet>();

    /// <summary>
    /// The fleet that the current construct belongs to.
    /// </summary>
    public static IFleet MyFleet => ScriptContext.Current!.Fleet.MyFleet;
}
