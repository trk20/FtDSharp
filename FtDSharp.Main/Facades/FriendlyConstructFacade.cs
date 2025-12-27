using System;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Core.Help;
using UnityEngine;

namespace FtDSharp.Facades
{
    /// <summary>
    /// Facade wrapping a MainConstruct. Provides read-only access to basic construct information.
    /// Serves as base class for MainConstructFacade which adds control capabilities.
    /// </summary>
    public class FriendlyConstructFacade : IFriendlyConstruct
    {
        protected readonly MainConstruct _construct;
        private readonly Lazy<IFleet> _fleet;

        public FriendlyConstructFacade(MainConstruct construct)
        {
            _construct = construct ?? throw new ArgumentNullException(nameof(construct));
            _fleet = new Lazy<IFleet>(GetFleet);
        }

        public int UniqueId => _construct.UniqueId;
        public string Name => _construct.GetBlueprintName();
        public Vector3 Position => _construct.myTransform.position;
        public Vector3 Velocity => _construct.PartPhysicsRestricted.iVelocities.VelocityVector;
        public float Volume => _construct.AllBasics.GetVolumeOfAloveBlocksIncludingSubConstructable();
        public int AliveBlockCount => _construct.AllBasics.GetNumberAliveBlocksIncludingSubConstructables();
        public int BlockCount => _construct.AllBasics.GetNumberBlocksIncludingSubConstructables();
        public float Stability => _construct.StabilityFactor;

        public Quaternion Rotation => _construct.myTransform.rotation;
        public Vector3 Forward => _construct.myTransform.forward;
        public float Yaw => Angles.FixRot180To180(_construct.myTransform.eulerAngles.y);
        public float Pitch => Angles.FixRot180To180(_construct.myTransform.eulerAngles.x);
        public float Roll => Angles.FixRot180To180(_construct.myTransform.eulerAngles.z);

        public IFleet Fleet => _fleet.Value;

        private IFleet GetFleet()
        {
            var force = _construct.GetForce();
            return new FleetFacade(force.Fleet);
        }
    }
}
