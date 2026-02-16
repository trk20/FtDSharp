using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FtDSharp.Facades
{
    /// <summary>
    /// Facade wrapping an AIMainframe block, providing access to its targeting information.
    /// </summary>
    internal class MainframeFacade : IMainframe
    {
        private readonly AIMainframe _mainframe;
        private readonly AIMainframeFacade _blockFacade;

        public MainframeFacade(AIMainframe mainframe)
        {
            _mainframe = mainframe;
            _blockFacade = new AIMainframeFacade(mainframe);
        }

        public IAIMainframe Block => _blockFacade;

        public ITarget? PrimaryTarget
        {
            get
            {
                var targetManager = _mainframe.Node?.targetManager;
                if (targetManager == null || !targetManager.TargetExists)
                    return null;

                var primaryTarget = targetManager.GetPrimaryTarget();
                if (primaryTarget == null || primaryTarget.IsNull())
                    return null;
                return new TargetFacade(primaryTarget);
            }
        }

        public IReadOnlyList<ITarget> Targets
        {
            get
            {
                var targetManager = _mainframe.Node?.targetManager;
                if (targetManager == null)
                    return System.Array.Empty<ITarget>();

                var prioritized = targetManager.GetPrioritisedTargetList();
                if (prioritized == null)
                    return System.Array.Empty<ITarget>();

                var result = new List<ITarget>(prioritized.Count);
                foreach (var targetObj in prioritized)
                {
                    if (targetObj != null && !targetObj.IsNull())
                    {
                        result.Add(new TargetFacade(targetObj));
                    }
                }
                return result;
            }
        }

        public Vector3 GetAimpoint(ITarget target)
        {
            var targetManager = _mainframe.Node?.targetManager;
            if (targetManager == null)
                return target.Position; // fallback to target center

            var prioritized = targetManager.GetPrioritisedTargetList();
            if (prioritized == null)
                return target.Position;

            // Find the TargetObject matching the given target's UniqueId
            var targetObj = prioritized.FirstOrDefault(t => t?.C?.UniqueId == target.UniqueId);
            if (targetObj == null)
                return target.Position;

            // Use the game's built-in aimpoint calculation which includes error
            return targetObj.GetAimPointPosition();
        }

        public void SetPrimaryTarget(ITarget? target)
        {
            var targetManager = _mainframe.Node?.targetManager;
            if (targetManager == null || target == null)
                return;

            foreach (var targetObj in targetManager.GetPrioritisedTargetList().Where(t => t != null) ?? new List<TargetObject>())
            {
                if (target.UniqueId == targetObj?.C?.UniqueId)
                {
                    targetObj.PlayerChoice = true;
                }
                else
                {
                    targetObj!.PlayerChoice = false;
                }
            }
        }
    }
}
