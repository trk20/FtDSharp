namespace FtDSharp
{
    public static class Game
    {
        public static IMainConstruct MainConstruct => ScriptApi.Context?.Self!;
        public static float Time => GameTime;
        public static float RealTime => ScriptApi.Context?.RealTimeSinceStart ?? 0f;
        public static float GameTime => ScriptApi.Context?.GameTimeSinceStart ?? 0f;
        public static float RealDeltaTime => ScriptApi.Context?.RealDeltaTime ?? 0f;
        public static float GameDeltaTime => ScriptApi.Context?.GameDeltaTime ?? 0f;
        public static long TicksSinceStart => ScriptApi.Context?.TicksSinceStart ?? 0;
    }
}
