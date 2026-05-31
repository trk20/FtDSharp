namespace FtDSharp
{
    public static class Game
    {
        public static IMainConstruct MainConstruct => ScriptContext.Current!.Game.MainConstruct;
        public static float Time => GameTime;
        public static float RealTime => ScriptContext.Current?.Game.RealTime ?? 0f;
        public static float GameTime => ScriptContext.Current?.Game.GameTime ?? 0f;
        public static float RealDeltaTime => ScriptContext.Current?.Game.RealDeltaTime ?? 0f;
        public static float GameDeltaTime => ScriptContext.Current?.Game.GameDeltaTime ?? 0f;
        public static long TicksSinceStart => ScriptContext.Current?.Game.TicksSinceStart ?? 0;
    }
}
