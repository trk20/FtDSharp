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
    }
}
