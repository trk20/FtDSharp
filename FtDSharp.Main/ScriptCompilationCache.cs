using System.Collections.Concurrent;
using System.Reflection;

namespace FtDSharp
{
    internal static class ScriptCompilationCache
    {
        private static readonly ConcurrentDictionary<string, Assembly> Cache = new();

        internal static bool TryGet(string hash, out Assembly? assembly)
        {
            return Cache.TryGetValue(hash, out assembly);
        }

        internal static void Store(string hash, Assembly assembly)
        {
            Cache.TryAdd(hash, assembly);
        }

        internal static void Warmup()
        {
            var host = new ScriptHost();
            var hash = ScriptHost.ComputeHash(ReplaceLuaPatches.DefaultCSharpTemplate);
            host.Compile(ReplaceLuaPatches.DefaultCSharpTemplate, hash);
        }
    }
}
