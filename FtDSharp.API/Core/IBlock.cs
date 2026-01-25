using UnityEngine;

namespace FtDSharp
{
    /// <summary>
    /// Represents a block on a construct with position, rotation, health, and hierarchy info.
    /// </summary>
    public interface IBlock : System.IEquatable<IBlock>
    {
        /// <summary>The construct this block belongs to.</summary>
        IFriendlyConstruct ParentConstruct { get; }
        /// <summary>
        /// The parent block if this block is on a subconstruct (turret, spinblock, piston, etc).
        /// Null if the block is on the main construct body.
        /// </summary>
        IBlock? Parent { get; }
        /// <summary>
        /// True if this block is mounted directly on the main construct (not on a turret/spinblock).
        /// </summary>
        bool IsOnRoot { get; }
        /// <summary>Unique identifier for the block (unique within block type).</summary>
        int UniqueId { get; }
        /// <summary>User-assigned custom name of the block (set via Q menu).</summary>
        string? CustomName { get; }
        /// <summary>The block type name (e.g., "Turret Block One Axis").</summary>
        string BlockTypeName { get; }
        /// <summary>Local position of the block relative to its parent construct.</summary>
        Vector3 LocalPosition { get; }
        /// <summary>World position of the block.</summary>
        Vector3 WorldPosition { get; }
        /// <summary>Local forward direction of the block.</summary>
        Vector3 LocalForward { get; }
        /// <summary>Local up direction of the block.</summary>
        Vector3 LocalUp { get; }
        /// <summary>Local rotation of the block.</summary>
        Quaternion LocalRotation { get; }
        /// <summary>World rotation of the block.</summary>
        Quaternion WorldRotation { get; }
        /// <summary>Current health of the block.</summary>
        float CurrentHealth { get; }
        /// <summary>Maximum health of the block.</summary>
        float MaximumHealth { get; }
        /// <summary>
        /// The depth of this block in the subobject hierarchy.
        /// 0 = on root construct, 1 = on turret/spinblock, 2 = on turret-on-turret, etc.
        /// </summary>
        int SubobjectDepth { get; }
    }
}
