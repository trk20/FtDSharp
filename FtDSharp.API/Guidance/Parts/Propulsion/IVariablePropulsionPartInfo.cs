namespace FtDSharp
{
    /// <summary>
    /// Base interface for propulsion parts with variable thrust.
    /// </summary>
    public interface IVariablePropulsionPartInfo : IMissilePropulsionInfo
    {
        /// <summary>Current thrust fraction (0.0 to 1.0).</summary>
        float ThrustFraction { get; set; }
    }
}
