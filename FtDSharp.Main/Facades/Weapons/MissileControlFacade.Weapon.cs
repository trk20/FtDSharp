namespace FtDSharp.Facades
{
    internal partial class MissileControlFacade : IMissileController
    {
        public override bool CanFire => IsReady;

        public override bool Fire()
        {
            UnityEngine.Vector3 forward = Weapon.GameWorldRotation * UnityEngine.Vector3.forward;
            SetAimState(AimAtDirectionInternal(forward));
            return FireInternal();
        }

        public int LoadedMissileCount
        {
            get
            {
                var mc = (MissileControl)Weapon;
                BrilliantSkies.Blocks.MissileComponents.MissileNode node = mc.Node;
                if (node == null) return 0;

                int count = 0;
                foreach (LaunchpadAbstract pad in node.firingOrder)
                {
                    if (pad == null) continue;
                    foreach (MissileTube tube in pad.MissileTubes)
                    {
                        if (tube != null && tube.Loaded) count++;
                    }
                }
                return count;
            }
        }

        public int TotalTubeCount
        {
            get
            {
                var mc = (MissileControl)Weapon;
                BrilliantSkies.Blocks.MissileComponents.MissileNode node = mc.Node;
                if (node == null) return 0;

                int count = 0;
                for (int i = 0; i < node.launchPads.Count; i++)
                {
                    LaunchpadAbstract pad = node.launchPads[i];
                    if (pad?.BlueprintBuilder?.GantryCount > 0)
                        count += pad.MissileTubes.Count;
                }
                return count;
            }
        }

        public float LastFireTime => ((MissileControl)Weapon).lastFireTime;
    }
}
