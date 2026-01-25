namespace FtDSharp
{
    /// <summary>
    /// A short-range air thruster with fixed thrust.
    /// </summary>
    public interface IShortRangeThrusterInfo : IMissilePropulsionInfo
    {
        new MissilePropulsionMedium Medium => MissilePropulsionMedium.AIR;
    }
}
