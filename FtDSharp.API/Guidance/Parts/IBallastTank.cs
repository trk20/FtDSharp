namespace FtDSharp
{
    /// <summary>
    /// A ballast tank for torpedo depth control.
    /// </summary>
    public interface IBallastTank : IMissilePart
    {
        /// <summary>Current buoyancy level (0.0 = empty, 1.0 = full).</summary>
        float BuoyancyLevel { get; set; }
    }
}
