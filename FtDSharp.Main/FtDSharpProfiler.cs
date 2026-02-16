using BrilliantSkies.Profiling;
using BrilliantSkies.Profiling.ProfileTypes;

namespace FtDSharp
{
    /// <summary>
    /// Profiler module that hooks into the game's profiling system.
    /// Tracks overall script execution time visible in the game's profiler UI.
    /// </summary>
    internal sealed class FtDSharpProfiler : AbstractModule<FtDSharpProfiler>
    {
        /// <summary>Profile for overall script Update() execution time.</summary>
        public IProfile ScriptExecution { get; private set; } = null!;

        public override void OnAdd(IProfileCollection collection)
        {
            var grouping = new Grouping(collection, "FtDSharp");
            ScriptExecution = new BasicProfileInDefault(collection, grouping, "Script Update()");
        }
    }
}
