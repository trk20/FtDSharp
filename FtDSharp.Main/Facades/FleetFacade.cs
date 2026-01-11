using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FtDSharp.Facades
{
    /// <summary>
    /// Facade wrapping a game Fleet object.
    /// </summary>
    internal class FleetFacade : IFleet
    {
        private readonly Fleet _fleet;
        private readonly Lazy<IReadOnlyList<IFriendlyConstruct>> _members;

        public FleetFacade(Fleet fleet)
        {
            _fleet = fleet ?? throw new ArgumentNullException(nameof(fleet));
            _members = new Lazy<IReadOnlyList<IFriendlyConstruct>>(GetMembers);
        }

        public int Id => _fleet.Id.ObjectId.Id;
        public string Name => _fleet.Name ?? string.Empty;
        public Vector3 Position => _fleet.CurrentPosition;
        public Quaternion Rotation => _fleet.CurrentRotation;

        public IFriendlyConstruct Flagship => new FriendlyConstructFacade(_fleet.flagShip.C);

        public IReadOnlyList<IFriendlyConstruct> Members => _members.Value;

        private IReadOnlyList<IFriendlyConstruct> GetMembers()
        {
            if (_fleet.forces == null)
                return Array.Empty<IFriendlyConstruct>();

            return _fleet.forces
                .Where(f => f?.C != null && f.C.Exists)
                .Select(f => new FriendlyConstructFacade(f.C))
                .Cast<IFriendlyConstruct>()
                .ToList();
        }
    }
}
