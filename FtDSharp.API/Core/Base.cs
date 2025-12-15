using System.Collections.Generic;
using UnityEngine;

namespace FtDSharp
{
    public interface IConstruct
    {
        public int UniqueId { get; }
        public string Name { get; }
        public Vector3 Position { get; }
        public Vector3 Velocity { get; }
        public float Volume { get; }
        public int AliveBlockCount { get; }
        public int BlockCount { get; }
        public float Stability { get; }
    }

    public interface IBlock
    {
        /// <summary> The construct this block belongs to. </summary>
        public IFriendlyConstruct ParentConstruct { get; }
        /// <summary> Unique identifier for the block. </summary>
        public int UniqueId { get; }
        /// <summary> Custom name of the block, if any. </summary>
        public string? CustomName { get; }
        /// <summary>Local position of the block relative to its parent construct.</summary>
        public Vector3 LocalPosition { get; }
        /// <summary>Local forward direction of the block.</summary>
        public Vector3 LocalForward { get; }
        /// <summary>Local up direction of the block.</summary>
        public Vector3 LocalUp { get; }
        /// <summary>Local rotation of the block.</summary>
        public Quaternion LocalRotation { get; }
        /// <summary>Current health of the block.</summary>
        public float CurrentHealth { get; }
        /// <summary>Maximum health of the block.</summary>
        public float MaximumHealth { get; }
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