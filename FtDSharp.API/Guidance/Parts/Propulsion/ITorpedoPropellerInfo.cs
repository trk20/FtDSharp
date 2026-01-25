namespace FtDSharp
{
    /// <summary>
    /// A torpedo propeller for underwater propulsion.
    /// </summary>
    public interface ITorpedoPropellerInfo : IVariablePropulsionPartInfo
    {
        new MissilePropulsionMedium Medium => MissilePropulsionMedium.WATER;
    }
}
