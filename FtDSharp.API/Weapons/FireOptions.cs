namespace FtDSharp;

/// <summary>
/// Per-call policy for <see cref="IWeaponControl.Fire"/>
/// </summary>
public struct FireOptions
{
    /// <summary> 
    /// Use the controlling LWC's attached failsafe when present. Default true.
    /// </summary>
    public bool RespectFailsafe { get; set; }

    /// <summary>
    /// Skip fire when the controlling LWC's mainframe has AI weapon firing Off. Default true.
    /// </summary>
    public bool RespectAiFiring { get; set; }

    /// <summary>Uses failsafe when connected and respects AI firing Off.</summary>
    public static FireOptions Default => new()
    {
        RespectFailsafe = true,
        RespectAiFiring = true,
    };

    /// <summary>
    /// Skips failsafe and AI firing checks.
    /// </summary>
    public static FireOptions Unrestricted => new()
    {
        RespectFailsafe = false,
        RespectAiFiring = false,
    };
}
