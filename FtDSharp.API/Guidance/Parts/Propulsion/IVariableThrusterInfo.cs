namespace FtDSharp
{
    /// <summary>
    /// A variable thrust air thruster.
    /// </summary>
    public interface IVariableThrusterInfo : IVariablePropulsionPartInfo
    {
        new MissilePropulsionMedium Medium => MissilePropulsionMedium.AIR;
    }
}
