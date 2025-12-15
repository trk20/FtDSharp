using UnityEngine;
using BrilliantSkies.Core.Types;

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

        public IFriendlyConstruct ParentConstruct
        {
            get
            {
                if (_baseBlock.MainConstruct is MainConstruct main)
                {
                    return new MainConstructFacade(main);
                }
                throw new System.InvalidOperationException("Block is not on a valid construct");
            }
        }

        public int UniqueId => (int)_baseBlock.IdSet.UniqueId;

        public string? CustomName => _baseBlock.Name;

        public Vector3 LocalPosition => (Vector3)_baseBlock.LocalPosition;

        public Vector3 LocalForward => (Vector3)_baseBlock.LocalForward;

        public Vector3 LocalUp => (Vector3)_baseBlock.LocalUp;

        public Quaternion LocalRotation => _baseBlock.LocalRotation;

        public float CurrentHealth => _baseBlock.CurrentHealth;

        public float MaximumHealth => _baseBlock.MaximumHealth;
    }
}
