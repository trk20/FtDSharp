using System;
using System.Collections.Generic;

namespace FtDSharp;

/// <summary>
/// Static accessor for missile guidance.
/// </summary>
public static class Guidance
{
    /// <summary>
    /// Gets all active script controllable missiles launched by the current construct.
    /// </summary>
    public static IReadOnlyList<IMissile> Missiles =>
        ScriptContext.Current?.Guidance.Missiles ?? Array.Empty<IMissile>();
}
