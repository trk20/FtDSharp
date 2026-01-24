using System.Collections.Generic;
using System.Linq;

namespace FtDSharp.Facades
{
    /// <summary>
    /// Facade for the script's own construct. Extends FriendlyConstructFacade with control capabilities.
    /// </summary>
    public class MainConstructFacade : FriendlyConstructFacade, IMainConstruct
    {
        private readonly FrameCache<IReadOnlyList<IMainframe>> _mainframesCache;
        private readonly FrameCache<IReadOnlyList<IWeapon>> _weaponsCache;
        private readonly FrameCache<IReadOnlyList<ITurret>> _turretsCache;
        private readonly FrameCache<List<IMissile>> _missilesCache;

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

            // Get weapons from main construct
            var mainWeapons = allConstruct.WeaponryRestricted?.Weapons;
            if (mainWeapons != null)
            {
                foreach (var weapon in mainWeapons)
                {
                    if (weapon != null && weapon.IsAlive)
                    {
                        yield return BlockFacadeFactory.GetOrCreateWeaponFacade(weapon, allConstruct);
                    }
                }
            }

            // Get weapons from all subconstructs
            var subConstructs = allConstruct.AllBasicsRestricted?.AllSubconstructsBelowUs;
            if (subConstructs != null)
            {
                foreach (var subConstruct in subConstructs)
                {
                    if (subConstruct is AllConstruct subAll)
                    {
                        var subWeapons = subAll.WeaponryRestricted?.Weapons;
                        if (subWeapons != null)
                        {
                            foreach (var weapon in subWeapons)
                            {
                                if (weapon != null && weapon.IsAlive)
                                {
                                    yield return BlockFacadeFactory.GetOrCreateWeaponFacade(weapon, subAll);
                                }
                            }
                        }
                    }
                }
            }
        }

        private IReadOnlyList<IWeapon> GetWeaponsExcludingTurrets()
        {
            return GetAllWeaponFacades()
                .Where(w => w.WeaponType != WeaponType.Turret)
                .Cast<IWeapon>()
                .ToList();
        }

        private IReadOnlyList<ITurret> GetTurrets()
        {
            var allConstruct = _construct as AllConstruct;
            if (allConstruct == null) return new List<ITurret>();

            var turrets = new List<ITurret>();

            // Get turrets from main construct
            var mainWeapons = allConstruct.WeaponryRestricted?.Weapons;
            if (mainWeapons != null)
            {
                foreach (var weapon in mainWeapons)
                {
                    if (weapon is Turrets turret && turret.IsAlive)
                    {
                        turrets.Add(BlockFacadeFactory.GetOrCreateTurretFacade(turret, allConstruct));
                    }
                }
            }

            // Get turrets from subconstructs
            var subConstructs = allConstruct.AllBasicsRestricted?.SubConstructList;
            if (subConstructs != null)
            {
                foreach (var subConstruct in subConstructs)
                {
                    if (subConstruct is AllConstruct subAll)
                    {
                        var subWeapons = subAll.WeaponryRestricted?.Weapons;
                        if (subWeapons != null)
                        {
                            foreach (var weapon in subWeapons)
                            {
                                if (weapon is Turrets turret && turret.IsAlive)
                                {
                                    turrets.Add(BlockFacadeFactory.GetOrCreateTurretFacade(turret, subAll));
                                }
                            }
                        }
                    }
                }
            }

            return turrets;
        }

        #endregion

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

        private List<IMissile> GetMissiles()
        {
            var missiles = new List<IMissile>();
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
