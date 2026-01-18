using System.Collections.Generic;
using UnityEngine;

namespace FtDSharp
{
    /// <summary>
    /// Represents something that can be tracked/targeted - has position, velocity, and acceleration.
    /// </summary>
    public interface ITargetable
    {
        /// <summary>Current world position.</summary>
        Vector3 Position { get; }
        /// <summary>Current velocity vector.</summary>
        Vector3 Velocity { get; }
        /// <summary>Current acceleration vector.</summary>
        Vector3 Acceleration { get; }
    }

    public interface IConstruct : ITargetable
    {
        public int UniqueId { get; }
        public string Name { get; }
        public float Volume { get; }
        public int AliveBlockCount { get; }
        public int BlockCount { get; }
        public float Stability { get; }
    }

    public interface IBlock : System.IEquatable<IBlock>
    {
        /// <summary> The construct this block belongs to. </summary>
        public IFriendlyConstruct ParentConstruct { get; }
        /// <summary>
        /// The parent block if this block is on a subconstruct (turret, spinblock, piston, etc).
        /// Null if the block is on the main construct body.
        /// </summary>
        public IBlock? Parent { get; }
        /// <summary>
        /// True if this block is mounted directly on the main construct (not on a turret/spinblock).
        /// </summary>
        public bool IsOnRoot { get; }
        /// <summary> Unique identifier for the block (unique within block type). </summary>
        public int UniqueId { get; }
        /// <summary> User-assigned custom name of the block (set via Q menu). </summary>
        public string? CustomName { get; }
        /// <summary> The block type name (e.g., "Turret Block One Axis"). </summary>
        public string BlockTypeName { get; }
        /// <summary>Local position of the block relative to its parent construct.</summary>
        public Vector3 LocalPosition { get; }
        /// <summary>World position of the block.</summary>
        public Vector3 WorldPosition { get; }
        /// <summary>Local forward direction of the block.</summary>
        public Vector3 LocalForward { get; }
        /// <summary>Local up direction of the block.</summary>
        public Vector3 LocalUp { get; }
        /// <summary>Local rotation of the block.</summary>
        public Quaternion LocalRotation { get; }
        /// <summary>World rotation of the block.</summary>
        public Quaternion WorldRotation { get; }
        /// <summary>Current health of the block.</summary>
        public float CurrentHealth { get; }
        /// <summary>Maximum health of the block.</summary>
        public float MaximumHealth { get; }
        /// <summary>
        /// The depth of this block in the subobject hierarchy.
        /// 0 = on root construct, 1 = on turret/spinblock, 2 = on turret-on-turret, etc.
        /// </summary>
        public int SubobjectDepth { get; }
    }

    public interface IFriendlyConstruct : IConstruct
    {
        /// <summary> Current rotation of the construct. </summary>
        public Quaternion Rotation { get; }
        /// <summary> Current forward direction of the construct. </summary>
        public Vector3 Forward { get; }
        /// <summary> Current yaw angle in degrees (-180 to 180). </summary>
        public float Yaw { get; }
        /// <summary> Current pitch angle in degrees (-180 to 180). </summary>
        public float Pitch { get; }
        /// <summary> Current roll angle in degrees (-180 to 180). </summary>
        public float Roll { get; }
        /// <summary> The fleet this construct belongs to. </summary>
        public IFleet Fleet { get; }
    }

    public interface IMainConstruct : IFriendlyConstruct
    {
        public bool TryGetBlockById(int id, out IBlock? block);
        public List<T> GetAllBlocksOfType<T>() where T : IBlock;
        /// <summary> Get all blocks on the construct, including subconstructs. </summary>
        public IEnumerable<IBlock> GetAllBlocks();
        public List<IMissile> Missiles { get; }
        /// <summary> Axis-based propulsion control. </summary>
        public IPropulsion Propulsion { get; }
    }

}