using FtDSharp.Facades;

namespace FtDSharp
{
    internal sealed class BasicScriptContext : IScriptContext
    {
        private readonly BasicLogApi _log = new();
        private MainConstruct? _construct;
        private MainConstructFacade? _facade;
        private long _ticks = 0;
        private float _realTime;
        private float _gameTime;
        private float _realDeltaTime;
        private float _gameDeltaTime;

        public IMainConstruct Self => _facade!;
        public ILogApi Log => _log;
        public float RealTimeSinceStart => _realTime;
        public float GameTimeSinceStart => _gameTime;
        public float RealDeltaTime => _realDeltaTime;
        public float GameDeltaTime => _gameDeltaTime;
        public long TicksSinceStart => _ticks;
        public IBlockToConstructBlockTypeStorage? BlockTypeStorage => _construct?.iBlockTypeStorage;
        public AllConstruct? RawAllConstruct => _construct;

        internal void IncrementTick(float gameDeltaTime, float realDeltaTime)
        {
            if (gameDeltaTime < 0f) gameDeltaTime = 0f;
            if (realDeltaTime < 0f) realDeltaTime = 0f;

            _ticks++;
            _gameDeltaTime = gameDeltaTime;
            _realDeltaTime = realDeltaTime;
            _gameTime += gameDeltaTime;
            _realTime += realDeltaTime;
        }

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
