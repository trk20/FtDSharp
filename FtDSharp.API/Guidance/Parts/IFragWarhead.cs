namespace FtDSharp
{
    /// <summary>
    /// A fragment warhead with controllable cone angle and elevation.
    /// </summary>
    public interface IFragWarhead : IMissilePart
    {
        /// <summary>Angle spread of the fragments in degrees, from 0 to 180.</summary>
        float ConeAngle { get; set; }
        /// <summary>Elevation offset of the fragment cone in degrees, from -90 to 90.</summary>
        float ElevationOffset { get; set; }
    }
}
