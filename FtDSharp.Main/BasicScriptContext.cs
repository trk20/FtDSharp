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
        private MainConstruct? _construct;
        private MainConstructFacade? _facade;
        private long _ticks = 0;

        public BasicScriptContext()
        {
            _mainframesCache = new FrameCache<IReadOnlyList<IMainframe>>(GetMainframes);
            _warningsCache = new FrameCache<IReadOnlyList<IProjectileWarning>>(GetWarnings);
        }

        public IMainConstruct Self => _facade!;
        public ILogApi Log => _log;
        public float TimeSinceStart => (float)_uptime.Elapsed.TotalSeconds;
        public long TicksSinceStart => _ticks;
        public IBlockToConstructBlockTypeStorage? BlockTypeStorage => _construct?.iBlockTypeStorage;

        public IReadOnlyList<IMainframe> Mainframes => _mainframesCache.Value;

        public IReadOnlyList<IProjectileWarning> IncomingProjectiles => _warningsCache.Value;

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
