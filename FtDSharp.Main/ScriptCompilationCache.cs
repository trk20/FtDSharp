using System.Collections.Concurrent;

namespace FtDSharp
{
    internal static class ScriptCompilationCache
    {
        private static readonly ConcurrentDictionary<string, byte[]> Cache = new();

        internal static bool TryGet(string hash, out byte[]? ilBytes)
        {
            return Cache.TryGetValue(hash, out ilBytes);
        }

        internal static void Store(string hash, byte[] ilBytes)
        {
            Cache.TryAdd(hash, ilBytes);
        }

        internal static void Warmup()
        {
            var host = new ScriptHost();
            host.TryCompileAndActivate(ReplaceLuaPatches.DefaultCSharpTemplate, ctx: null, out _);
        }
    }
}
