using System.Collections.Generic;
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

        public IReadOnlyList<IMainframe> Mainframes
        {
            get
            {
                var mainframeStore = _construct?.iBlockTypeStorage?.MainframeStore?.Blocks;
                if (mainframeStore == null)
                    return System.Array.Empty<IMainframe>();

                var result = new List<IMainframe>(mainframeStore.Count);
                foreach (var mainframe in mainframeStore)
                {
                    if (mainframe?.IsAlive ?? false)
                    {
                        result.Add(new MainframeFacade(mainframe));
                    }
                }
                // Sort by priority
                result.Sort((a, b) => a.Block.Priority.CompareTo(b.Block.Priority));
                return result;
            }
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
