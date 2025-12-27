using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FtDSharp.Facades
{
    /// <summary>
    /// Facade for the script's own construct. Extends FriendlyConstructFacade with control capabilities.
    /// </summary>
    public class MainConstructFacade : FriendlyConstructFacade, IMainConstruct
    {
        public MainConstructFacade(MainConstruct construct) : base(construct)
        {
        }

        /// <summary> Enumerate all blocks on the construct and subconstructs. </summary>
        public IEnumerable<IBlock> GetAllBlocks()
        {
            // Blocks on the main construct
            foreach (var block in _construct.AllBasics.AliveAndDead.Blocks)
            {
                if (block.IsStructural) continue;
                var wrapped = BlockFactory.Wrap(block);
                if (wrapped != null) yield return wrapped;
            }

            // Blocks on subconstructs
            foreach (var sc in _construct.AllBasics.AllSubconstructsBelowUs)
            {
                foreach (var block in sc.AllBasics.AliveAndDead.Blocks)
                {
                    if (block.IsStructural) continue;
                    var wrapped = BlockFactory.Wrap(block);
                    if (wrapped != null) yield return wrapped;
                }
            }
        }

        private Lazy<List<IMissile>> _missiles => new(() =>
        {
            var missiles = new List<IMissile>();
            // iterate over all Lua transceivers
            if (_construct.iBlockTypeStorage?.MissileLuaTransceiverStore?.Blocks != null)
            {
                foreach (var transceiver in _construct.iBlockTypeStorage.MissileLuaTransceiverStore.Blocks)
                {
                    if (transceiver?.Missiles == null) continue;

                    foreach (var missile in transceiver.Missiles)
                    {
                        if (missile != null)
                        {
                            missiles.Add(new MissileFacade(missile));
                        }
                    }
                }
            }
            return missiles;
        });

        public List<IMissile> Missiles
        {
            get => _missiles.Value;
        }

        public bool TryGetBlockById(int id, out IBlock? block)
        {
            if (Blocks.All.FirstOrDefault(b => b.UniqueId == id) is IBlock foundBlock)
            {
                block = foundBlock;
                return true;
            }
            block = null;
            return false;
        }

        public List<T> GetAllBlocksOfType<T>() where T : IBlock
        {
            return Blocks.OfType<T>().ToList();
        }

        private PropulsionFacade? _propulsion;

        public IPropulsion Propulsion => _propulsion ??= new PropulsionFacade(_construct);
    }
}
