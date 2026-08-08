using System.Collections.Generic;
using System.Linq;
using FtDSharp.Helpers;

namespace FtDSharp.Facades
{
    /// <summary>
    /// Facade for the script's own construct. Extends FriendlyConstructFacade with control capabilities.
    /// </summary>
    internal sealed class MainConstructFacade : FriendlyConstructFacade, IMainConstruct
    {
        private readonly FrameCache<IReadOnlyList<IMainframe>> _mainframesCache;
        private readonly FrameCache<IReadOnlyList<IWeapon>> _weaponsCache;
        private readonly FrameCache<IReadOnlyList<ITurret>> _turretsCache;
        private readonly FrameCache<List<IMissile>> _missilesCache;

        // cache missile facades by UIDs to avoid per-frame allocation
        private static readonly Dictionary<int, MissileFacade> _missileCache = new();

        public MainConstructFacade(MainConstruct construct) : base(construct)
        {
            _mainframesCache = new FrameCache<IReadOnlyList<IMainframe>>(GetMainframes);
            _weaponsCache = new FrameCache<IReadOnlyList<IWeapon>>(GetWeaponsExcludingTurrets);
            _turretsCache = new FrameCache<IReadOnlyList<ITurret>>(GetTurrets);
            _missilesCache = new FrameCache<List<IMissile>>(GetMissiles);
        }

        #region Mainframes, Weapons, Turrets

        public IReadOnlyList<IMainframe> Mainframes => _mainframesCache.Value;
        public IReadOnlyList<IWeapon> Weapons => _weaponsCache.Value;
        public IReadOnlyList<ITurret> Turrets => _turretsCache.Value;

        private IReadOnlyList<IMainframe> GetMainframes()
        {
            return _construct.iBlockTypeStorage?.MainframeStore?.Blocks
                .Where(mainframe => mainframe?.IsAlive ?? false)
                .Select(mainframe => new MainframeFacade(mainframe!))
                .OrderBy(a => a.Block.Priority)
                .Cast<IMainframe>().ToList() ?? new List<IMainframe>();
        }

        private IEnumerable<WeaponFacade> GetAllWeaponFacades()
        {
            var allConstruct = _construct as AllConstruct;
            if (allConstruct == null) yield break;

            var seen = new HashSet<ConstructableWeapon>();

            List<ConstructableWeapon>? mainWeapons = allConstruct.WeaponryRestricted?.Weapons;
            if (mainWeapons != null)
            {
                foreach (ConstructableWeapon weapon in mainWeapons)
                {
                    if (weapon == null || !weapon.IsAlive || !seen.Add(weapon))
                        continue;
                    yield return BlockFacadeFactory.GetOrCreateWeaponFacade(weapon, allConstruct);
                }
            }

            List<SubConstruct>? subConstructs = allConstruct.AllBasicsRestricted?.AllSubconstructsBelowUs;
            if (subConstructs == null)
                yield break;

            foreach (SubConstruct subConstruct in subConstructs)
            {
                if (subConstruct is not AllConstruct subAll)
                    continue;

                // Turret/spin ActiveBlocks are the mount; always include them even if missing from
                // WeaponryRestricted (turrets normally register on ParentConstruct instead).
                if (subConstruct.ActiveBlock is ConstructableWeapon activeWeapon
                    && activeWeapon.IsAlive
                    && seen.Add(activeWeapon))
                {
                    yield return BlockFacadeFactory.GetOrCreateWeaponFacade(activeWeapon, subAll);
                }

                List<ConstructableWeapon>? subWeapons = subAll.WeaponryRestricted?.Weapons;
                if (subWeapons == null)
                    continue;

                foreach (ConstructableWeapon weapon in subWeapons)
                {
                    if (weapon == null || !weapon.IsAlive || !seen.Add(weapon))
                        continue;
                    yield return BlockFacadeFactory.GetOrCreateWeaponFacade(weapon, subAll);
                }
            }
        }

        private IReadOnlyList<IWeapon> GetWeaponsExcludingTurrets()
        {
            return GetAllWeaponFacades()
                .Where(w => w.WeaponType != WeaponType.Turret)
                .Where(w => WeaponControlAuthority.HasControllingLwc(w.Weapon))
                .Cast<IWeapon>()
                .ToList();
        }

        private IReadOnlyList<ITurret> GetTurrets()
        {
            return GetAllWeaponFacades()
                .OfType<ITurret>()
                .Where(t => t is WeaponFacade wf && WeaponControlAuthority.HasControllingLwc(wf.Weapon))
                .ToList();
        }

        #endregion

        /// <summary> Enumerate all alive blocks on the construct and subconstructs. </summary>
        public IEnumerable<IBlock> GetAllBlocks()
        {
            // Blocks on the main construct
            foreach (Block block in _construct.AllBasics.AliveAndDead.Blocks)
            {
                if (block.IsStructural || !block.IsAlive) continue;
                IBlock? wrapped = BlockFactory.Wrap(block);
                if (wrapped != null) yield return wrapped;
            }

            // Blocks on subconstructs
            foreach (SubConstruct sc in _construct.AllBasics.AllSubconstructsBelowUs)
            {
                foreach (Block block in sc.AllBasics.AliveAndDead.Blocks)
                {
                    if (block.IsStructural || !block.IsAlive) continue;
                    IBlock? wrapped = BlockFactory.Wrap(block);
                    if (wrapped != null) yield return wrapped;
                }
            }
        }

        private List<IMissile> GetMissiles()
        {
            var missiles = new List<IMissile>();
            var seenIds = new HashSet<int>();

            if (_construct.iBlockTypeStorage?.MissileLuaTransceiverStore?.Blocks != null)
            {
                foreach (MissileBlockLuaTransceiver? transceiver in _construct.iBlockTypeStorage.MissileLuaTransceiverStore.Blocks)
                {
                    if (transceiver?.Missiles == null) continue;

                    foreach (BrilliantSkies.Ftd.Missiles.Missile missile in transceiver.Missiles)
                    {
                        if (missile != null && missile.IsAlive())
                        {
                            var id = missile.UniqueId;
                            seenIds.Add(id);

                            // Get or create cached facade
                            if (!_missileCache.TryGetValue(id, out MissileFacade? facade))
                            {
                                facade = new MissileFacade(missile);
                                _missileCache[id] = facade;
                            }
                            missiles.Add(facade);
                        }
                    }
                }
            }

            // remove facades for missiles that are no longer alive to prevent unbounded cache growth
            if (_missileCache.Count > seenIds.Count)
            {
                var toRemove = new List<int>();
                foreach (KeyValuePair<int, MissileFacade> kvp in _missileCache)
                {
                    if (!seenIds.Contains(kvp.Key))
                        toRemove.Add(kvp.Key);
                }
                foreach (var id in toRemove)
                    _missileCache.Remove(id);
            }

            return missiles;
        }

        public List<IMissile> Missiles => _missilesCache.Value;

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
