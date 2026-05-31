using System;
using System.Collections.Generic;
using System.Linq;

namespace FtDSharp
{
    /// <summary>
    /// Static accessor for AI-related functionality on the current construct.
    /// </summary>
    public static class AI
    {
        /// <summary>
        /// All AI Mainframes on the construct, sorted by priority (lower = higher priority).
        /// </summary>
        public static IReadOnlyList<IMainframe> Mainframes =>
            ScriptContext.Current?.AI.Mainframes ?? Array.Empty<IMainframe>();

        /// <summary> 
        /// The highest priority AI Mainframe on the construct.
        /// Cannot be null unless there are no Mainframes at all, in which case the construct is dead anyway.
        /// </summary>
        public static IMainframe HighestPriorityMainframe => Mainframes.OrderBy(m => m.Block.Priority).First();
    }
}
