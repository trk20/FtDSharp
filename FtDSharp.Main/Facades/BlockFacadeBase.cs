using UnityEngine;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Core.Logger;
using System;

namespace FtDSharp.Facades
{
    /// <summary>
    /// Base class for all generated block facades.
    /// Provides common IBlock implementation.
    /// </summary>
    internal abstract class BlockFacadeBase : IBlock
    {
        private readonly Block _baseBlock;

        protected BlockFacadeBase(Block block)
        {
            _baseBlock = block;
        }

        /// <summary>
        /// The underlying game Block object. Internal use only.
        /// </summary>
        protected Block BaseBlock => _baseBlock;

        public bool Equals(IBlock? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            // Compare by UniqueId + BlockTypeName to handle different facade instances for same block
            return UniqueId == other.UniqueId && BlockTypeName == other.BlockTypeName;
        }

        public override bool Equals(object? obj) => obj is IBlock block && Equals(block);

        public override int GetHashCode() => HashCode.Combine(_baseBlock.IdSet.UniqueId, _baseBlock.Name);

        public IFriendlyConstruct ParentConstruct
        {
            get
            {
                if (_baseBlock.MainConstruct is MainConstruct main)
                {
                    return new MainConstructFacade(main);
                }
                throw new InvalidOperationException("Block is not on a valid construct");
            }
        }

        public IBlock? Parent
        {
            get
            {
                try
                {
                    if (_baseBlock == null) return null;

                    // Check if this block is part of a subconstruct
                    var subConstruct = _baseBlock.SubConstruct;
                    if (subConstruct != null)
                    {
                        // Get the block that owns this subconstruct (e.g., turret, spinblock)
                        var activeBlock = subConstruct.ActiveBlock;
                        if (activeBlock == _baseBlock)
                        {
                            var subConstruct2 = subConstruct.Parent as SubConstruct;
                            activeBlock = subConstruct2?.ActiveBlock;
                        }

                        if (activeBlock != null)
                        {
                            return BlockFacadeFactory.CreateFacade(activeBlock);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    AdvLogger.LogError($"[FtDSharp] Parent property error: {ex}", LogOptions.None);
                }

                return null;
            }
        }

        public int UniqueId => _baseBlock.IdSet.Id.Us;

        public bool IsOnRoot => Parent == null;

        public string? CustomName
        {
            get
            {
                string name = _baseBlock.IdSet.Name;
                return string.IsNullOrEmpty(name) ? null : name;
            }
        }

        public string BlockTypeName => _baseBlock.Name;

        public Vector3 LocalPosition => (Vector3)_baseBlock.LocalPosition;

        public Vector3 WorldPosition => _baseBlock.GameWorldPosition;

        public Vector3 LocalForward => (Vector3)_baseBlock.LocalForward;

        public Vector3 LocalUp => (Vector3)_baseBlock.LocalUp;

        public Quaternion LocalRotation => _baseBlock.LocalRotation;

        public Quaternion WorldRotation => _baseBlock.GameWorldRotation;

        public float CurrentHealth => _baseBlock.CurrentHealth;

        public float MaximumHealth => _baseBlock.MaximumHealth;
    }
}
