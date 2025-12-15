namespace FtDSharp
{
    public static class Game
    {
        public static IMainConstruct MainConstruct => ScriptApi.Context?.Self!;
        public static float Time => ScriptApi.Context?.TimeSinceStart ?? 0f;
        public static long TicksSinceStart => ScriptApi.Context?.TicksSinceStart ?? 0;
    }
}
