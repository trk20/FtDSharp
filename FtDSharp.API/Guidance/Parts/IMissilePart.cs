namespace FtDSharp
{
    /// <summary>
    /// Base interface for controllable missile parts.
    /// </summary>
    public interface IMissilePart
    {
        /// <summary>The type name of this missile part.</summary>
        string PartType { get; }
    }
}
