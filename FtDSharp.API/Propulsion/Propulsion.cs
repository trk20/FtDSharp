namespace FtDSharp
{
    /// <summary>
    /// Provides intuitive axis-based control over construct propulsion.
    /// All axis values range from -1 to 1.
    /// Positive values: Forward, Right, Up, Clockwise (when viewed from above/behind).
    /// </summary>
    public interface IPropulsion
    {
        /// <summary>
        /// Forward/backward thrust axis. Positive = forward, negative = backward.
        /// Range: -1 to 1.
        /// </summary>
        float Forwards { get; set; }

        /// <summary>
        /// Left/right strafe axis. Positive = right, negative = left.
        /// Range: -1 to 1.
        /// </summary>
        float Strafe { get; set; }

        /// <summary>
        /// Up/down hover axis. Positive = up, negative = down.
        /// Range: -1 to 1.
        /// </summary>
        float Hover { get; set; }

        /// <summary>
        /// Yaw rotation axis. Positive = turn right, negative = turn left.
        /// Range: -1 to 1.
        /// </summary>
        float Yaw { get; set; }

        /// <summary>
        /// Pitch rotation axis. Positive = pitch up, negative = pitch down.
        /// Range: -1 to 1.
        /// </summary>
        float Pitch { get; set; }

        /// <summary>
        /// Roll rotation axis. Positive = roll right, negative = roll left.
        /// Range: -1 to 1.
        /// </summary>
        float Roll { get; set; }

        /// <summary>
        /// Extra propulsion axis A.
        /// </summary>
        float A { get; set; }

        /// <summary>
        /// Extra propulsion axis B.
        /// </summary>
        float B { get; set; }

        /// <summary>
        /// Extra propulsion axis C.
        /// </summary>
        float C { get; set; }

        /// <summary>
        /// Extra propulsion axis D.
        /// </summary>
        float D { get; set; }

        /// <summary>
        /// Extra propulsion axis E.
        /// </summary>
        float E { get; set; }

        /// <summary>
        /// Primary drive direct control. 
        /// </summary>
        float MainDrive { get; set; }

        /// <summary>
        /// Secondary drive direct control.
        /// </summary>
        float SecondaryDrive { get; set; }

        /// <summary>
        /// Tertiary drive direct control.
        /// </summary>
        float TertiaryDrive { get; set; }
    }
}
