using UnityEngine;

namespace FtDSharp
{
    /// <summary>
    /// Represents a friendly construct with orientation information and fleet membership.
    /// </summary>
    public interface IFriendlyConstruct : IConstruct
    {
        /// <summary>Current rotation of the construct.</summary>
        Quaternion Rotation { get; }
        /// <summary>Current forward direction of the construct.</summary>
        Vector3 Forward { get; }
        /// <summary>Current yaw angle in degrees (-180 to 180).</summary>
        float Yaw { get; }
        /// <summary>Current pitch angle in degrees (-180 to 180).</summary>
        float Pitch { get; }
        /// <summary>Current roll angle in degrees (-180 to 180).</summary>
        float Roll { get; }
        /// <summary>The fleet this construct belongs to.</summary>
        IFleet Fleet { get; }
    }
}
