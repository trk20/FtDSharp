using System.Collections.Generic;
using UnityEngine;

namespace FtDSharp
{
    /// <summary>
    /// Represents a fleet in the game - a group of allied constructs under shared command.
    /// </summary>
    public interface IFleet
    {
        /// <summary> Unique identifier for this fleet. </summary>
        int Id { get; }

        /// <summary> The name of this fleet. </summary>
        string Name { get; }

        /// <summary> Position of the fleet. </summary>
        Vector3 Position { get; }

        /// <summary> Current rotation of the fleet. </summary>
        Quaternion Rotation { get; }

        /// <summary> The flagship of this fleet. </summary>
        IFriendlyConstruct Flagship { get; }

        /// <summary> All constructs in this fleet. </summary>
        IReadOnlyList<IFriendlyConstruct> Members { get; }
    }
}
