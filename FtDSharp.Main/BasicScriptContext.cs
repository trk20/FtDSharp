using System.Diagnostics;
using FtDSharp.Facades;

namespace FtDSharp
{
    internal sealed class BasicScriptContext : IScriptContext
    {
        private readonly Stopwatch _uptime = Stopwatch.StartNew();
        private readonly BasicLogApi _log = new();
        private MainConstruct? _construct;
        private MainConstructFacade? _facade;
        private long _ticks = 0;

        public IMainConstruct Self => _facade!;
        public ILogApi Log => _log;
        public float TimeSinceStart => (float)_uptime.Elapsed.TotalSeconds;
        public long TicksSinceStart => _ticks;
        public IBlockToConstructBlockTypeStorage? BlockTypeStorage => _construct?.iBlockTypeStorage;
        public AllConstruct? RawAllConstruct => _construct;

        internal void IncrementTick() => _ticks++;

        internal void Attach(LuaBox luaBox)
        {
            var newConstruct = luaBox?.MainConstruct as MainConstruct;
            if (_construct != newConstruct)
            {
                _construct = newConstruct;
                _facade = _construct != null ? new MainConstructFacade(_construct) : null;
                FacadeCache.Clear(); // Clear persistent facade cache when switching constructs
                Blocks.InvalidateCache(); // Clear block list cache when switching constructs
            }
            _log.AttachBinding(luaBox?.binding);
        }
    }
}
