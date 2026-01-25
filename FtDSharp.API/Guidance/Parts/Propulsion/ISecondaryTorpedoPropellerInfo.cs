namespace FtDSharp
{
    /// <summary>
    /// A secondary torpedo propeller for additional underwater propulsion.
    /// </summary>
    public interface ISecondaryTorpedoPropellerInfo : IVariablePropulsionPartInfo
    {
        new MissilePropulsionMedium Medium => MissilePropulsionMedium.WATER;
    }
}
