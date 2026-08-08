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
                IAiTargetManager? targetManager = _mainframe.Node?.targetManager;
                if (targetManager == null || !targetManager.TargetExists)
                    return null;

                TargetObject? primaryTarget = targetManager.GetPrimaryTarget();
                if (primaryTarget == null || primaryTarget.IsNull())
                    return null;
                return new TargetFacade(primaryTarget);
            }
        }

        public IReadOnlyList<ITarget> Targets
        {
            get
            {
                IAiTargetManager? targetManager = _mainframe.Node?.targetManager;
                if (targetManager == null)
                    return System.Array.Empty<ITarget>();

                List<TargetObject> prioritized = targetManager.GetPrioritisedTargetList();
                if (prioritized == null)
                    return System.Array.Empty<ITarget>();

                var result = new List<ITarget>(prioritized.Count);
                foreach (TargetObject targetObj in prioritized)
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
            IAiTargetManager? targetManager = _mainframe.Node?.targetManager;
            if (targetManager == null)
                return target.Position; // fallback to target center

            List<TargetObject> prioritized = targetManager.GetPrioritisedTargetList();
            if (prioritized == null)
                return target.Position;

            // Find the TargetObject matching the given target's UniqueId
            TargetObject targetObj = prioritized.FirstOrDefault(t => t?.C?.UniqueId == target.UniqueId);
            if (targetObj == null)
                return target.Position;

            // Use the game's built-in aimpoint calculation which includes error
            return targetObj.GetAimPointPosition();
        }

        public void SetPrimaryTarget(ITarget? target)
        {
            IAiTargetManager? targetManager = _mainframe.Node?.targetManager;
            if (targetManager == null || target == null)
                return;

            foreach (TargetObject? targetObj in targetManager.GetPrioritisedTargetList().Where(t => t != null) ?? new List<TargetObject>())
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
