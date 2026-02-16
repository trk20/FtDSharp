namespace FtDSharp.Facades
{
    internal partial class MissileControlFacade : IMissileWeapon
    {
        public int LoadedMissileCount
        {
            get
            {
                var mc = (MissileControl)Weapon;
                var node = mc.Node;
                if (node == null) return 0;

                int count = 0;
                foreach (var pad in node.firingOrder)
                {
                    if (pad == null) continue;
                    foreach (var tube in pad.MissileTubes)
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
                var node = mc.Node;
                if (node == null) return 0;

                int count = 0;
                for (int i = 0; i < node.launchPads.Count; i++)
                {
                    var pad = node.launchPads[i];
                    if (pad?.BlueprintBuilder?.GantryCount > 0)
                        count += pad.MissileTubes.Count;
                }
                return count;
            }
        }

        public float LastFireTime => ((MissileControl)Weapon).lastFireTime;
    }
}
