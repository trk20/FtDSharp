using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FtDSharp.Facades;

namespace FtDSharp
{
    internal sealed class BasicScriptContext : IScriptContext
    {
        private readonly Stopwatch _uptime = Stopwatch.StartNew();
        private readonly BasicLogApi _log = new();
        private readonly FrameCache<IReadOnlyList<IMainframe>> _mainframesCache;
        private readonly FrameCache<IReadOnlyList<IProjectileWarning>> _warningsCache;
        private readonly FrameCache<IReadOnlyList<IFriendlyConstruct>> _friendliesCache;
        private readonly FrameCache<IReadOnlyList<IFriendlyConstruct>> _friendliesExcludingSelfCache;
        private readonly FrameCache<IReadOnlyList<IFleet>> _fleetsCache;
        private readonly FrameCache<IFleet> _myFleetCache;
        private readonly FrameCache<IReadOnlyList<IWeapon>> _weaponsCache;
        private readonly FrameCache<IReadOnlyList<ITurret>> _turretsCache;
        private MainConstruct? _construct;
        private MainConstructFacade? _facade;
        private long _ticks = 0;

        public BasicScriptContext()
        {
            _mainframesCache = new FrameCache<IReadOnlyList<IMainframe>>(GetMainframes);
            _warningsCache = new FrameCache<IReadOnlyList<IProjectileWarning>>(GetWarnings);
            _friendliesCache = new FrameCache<IReadOnlyList<IFriendlyConstruct>>(GetFriendlies);
            _friendliesExcludingSelfCache = new FrameCache<IReadOnlyList<IFriendlyConstruct>>(GetFriendliesExcludingSelf);
            _fleetsCache = new FrameCache<IReadOnlyList<IFleet>>(GetFleets);
            _myFleetCache = new FrameCache<IFleet>(GetMyFleet!);
            _weaponsCache = new FrameCache<IReadOnlyList<IWeapon>>(GetWeaponsExcludingTurrets);
            _turretsCache = new FrameCache<IReadOnlyList<ITurret>>(GetTurrets);
        }

        public IMainConstruct Self => _facade!;
        public ILogApi Log => _log;
        public float TimeSinceStart => (float)_uptime.Elapsed.TotalSeconds;
        public long TicksSinceStart => _ticks;
        public IBlockToConstructBlockTypeStorage? BlockTypeStorage => _construct?.iBlockTypeStorage;

        public IReadOnlyList<IMainframe> Mainframes => _mainframesCache.Value;
        public IReadOnlyList<IProjectileWarning> IncomingProjectiles => _warningsCache.Value;
        public IReadOnlyList<IFriendlyConstruct> Friendlies => _friendliesCache.Value;
        public IReadOnlyList<IFriendlyConstruct> FriendliesExcludingSelf => _friendliesExcludingSelfCache.Value;
        public IReadOnlyList<IFleet> Fleets => _fleetsCache.Value;
        public IFleet MyFleet => _myFleetCache.Value;
        public IReadOnlyList<IWeapon> Weapons => _weaponsCache.Value;
        public IReadOnlyList<ITurret> Turrets => _turretsCache.Value;

        private IReadOnlyList<IMainframe> GetMainframes()
        {
            return _construct?.iBlockTypeStorage?.MainframeStore?.Blocks
                .Where(mainframe => mainframe?.IsAlive ?? false)
                .Select(mainframe => new MainframeFacade(mainframe!))
                .OrderBy(a => a.Block.Priority)
                .Cast<IMainframe>().ToList() ?? new List<IMainframe>();
        }

        private IReadOnlyList<IProjectileWarning> GetWarnings()
        {
            return _construct?.MWM?.Warnings
                .Where(warning => warning?.IsValid ?? false)
                .Select(warning => new ProjectileWarningFacade(warning))
                .Cast<IProjectileWarning>().ToList() ?? new List<IProjectileWarning>();
        }

        private IReadOnlyList<IFriendlyConstruct> GetFriendlies()
        {
            if (_construct == null) return new List<IFriendlyConstruct>();

            var myTeam = _construct.GetTeam();
            return StaticConstructablesManager.Constructables
                .Where(c => c != null && !c.Destroyed && c.GetTeam() == myTeam)
                .Select(c => new FriendlyConstructFacade(c))
                .Cast<IFriendlyConstruct>()
                .ToList();
        }

        private IReadOnlyList<IFriendlyConstruct> GetFriendliesExcludingSelf()
        {
            if (_construct == null) return new List<IFriendlyConstruct>();

            var myTeam = _construct.GetTeam();
            return StaticConstructablesManager.Constructables
                .Where(c => c != null && !c.Destroyed && c != _construct && c.GetTeam() == myTeam)
                .Select(c => new FriendlyConstructFacade(c))
                .Cast<IFriendlyConstruct>()
                .ToList();
        }

        private IReadOnlyList<IFleet> GetFleets()
        {
            // Group all friendlies by their fleet and return unique fleets
            return Friendlies
                .Select(f => f.Fleet)
                .Where(fleet => fleet != null)
                .GroupBy(fleet => fleet.Id)
                .Select(g => g.First())
                .ToList();
        }

        private IFleet? GetMyFleet()
        {
            var force = _construct?.GetForce();
            if (force?.Fleet == null) return null;
            return new FleetFacade(force.Fleet);
        }

        /// <summary>
        /// Collects all weapons from the main construct and all subconstructs.
        /// </summary>
        private IEnumerable<WeaponFacade> GetAllWeaponFacades()
        {
            if (_construct == null) yield break;

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
                        yield return new WeaponFacade(weapon, allConstruct);
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
                                    yield return new WeaponFacade(weapon, subAll);
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
            if (_construct == null) return new List<ITurret>();

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
                        turrets.Add(new TurretFacade(turret, allConstruct));
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
                                    turrets.Add(new TurretFacade(turret, subAll));
                                }
                            }
                        }
                    }
                }
            }

            return turrets;
        }

        internal void IncrementTick() => _ticks++;

        internal void Attach(LuaBox luaBox)
        {
            var newConstruct = luaBox?.MainConstruct as MainConstruct;
            if (_construct != newConstruct)
            {
                _construct = newConstruct;
                _facade = _construct != null ? new MainConstructFacade(_construct) : null;
            }
            _log.AttachBinding(luaBox?.binding);
        }
    }
}
