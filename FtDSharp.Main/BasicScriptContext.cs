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
