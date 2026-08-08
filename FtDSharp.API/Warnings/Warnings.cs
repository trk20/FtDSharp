using System;
using System.Collections.Generic;
using System.Linq;

namespace FtDSharp;

/// <summary>
/// Static accessor for projectile warning information.
/// </summary>
public static class Warnings
{
    /// <summary>
    /// All valid incoming projectile warnings.
    /// </summary>
    public static IReadOnlyList<IProjectileWarning> IncomingProjectiles =>
        ScriptContext.Current?.Warnings.IncomingProjectiles ?? Array.Empty<IProjectileWarning>();

    /// <summary>
    /// Incoming missiles only (includes harpoons).
    /// </summary>
    public static IReadOnlyList<IProjectileWarning> IncomingMissiles =>
        ScriptContext.Current?.Warnings.IncomingMissiles ?? Array.Empty<IProjectileWarning>();

    /// <summary>
    /// Incoming shells only (APS shells and CRAM).
    /// </summary>
    public static IReadOnlyList<IProjectileWarning> IncomingShells =>
        ScriptContext.Current?.Warnings.IncomingShells ?? Array.Empty<IProjectileWarning>();

    /// <summary>
    /// Get warnings filtered by a specific projectile type.
    /// </summary>
    public static IEnumerable<IProjectileWarning> GetByType(ProjectileType type)
    {
        return IncomingProjectiles.Where(w => w.Type == type);
    }
}
