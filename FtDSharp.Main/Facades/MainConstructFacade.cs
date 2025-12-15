using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Core.Help;

namespace FtDSharp.Facades
{
    public class MainConstructFacade : IMainConstruct
    {
        private readonly MainConstruct _construct;

        public MainConstructFacade(MainConstruct construct)
        {
            _construct = construct;
        }

        public int UniqueId => _construct.UniqueId;
        public Vector3 Position => _construct.myTransform.position;
        public Vector3 Velocity => _construct.PartPhysicsRestricted.iVelocities.VelocityVector;
        public string Name => _construct.GetBlueprintName();
        public Quaternion Rotation => _construct.myTransform.rotation;
        public Vector3 Forward => _construct.myTransform.forward;
        public float Yaw => Angles.FixRot180To180(_construct.myTransform.eulerAngles.y);
        public float Pitch => Angles.FixRot180To180(_construct.myTransform.eulerAngles.x);
        public float Roll => Angles.FixRot180To180(_construct.myTransform.eulerAngles.z);
        public float Stability => _construct.StabilityFactor;
        public float Volume => _construct.AllBasics.GetVolumeOfAloveBlocksIncludingSubConstructable();
        public int AliveBlockCount => _construct.AllBasics.GetNumberAliveBlocksIncludingSubConstructables();
        public int BlockCount => _construct.AllBasics.GetNumberBlocksIncludingSubConstructables();

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

        // todo: optimize
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
            // TODO: Implement block iteration
            return new List<T>();
        }

        private PropulsionFacade? _propulsion;

        public IPropulsion Propulsion => _propulsion ??= new PropulsionFacade(_construct);
    }
}
