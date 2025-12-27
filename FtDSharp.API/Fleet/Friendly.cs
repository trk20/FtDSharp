using System.Collections.Generic;
using System.Linq;

namespace FtDSharp
{
    /// <summary>
    /// Static accessor for friendly construct and fleet information.
    /// </summary>
    public static class Friendly
    {
        /// <summary>
        /// All friendly constructs, including the current construct.
        /// </summary>
        public static IReadOnlyList<IFriendlyConstruct> All =>
            ScriptApi.Context?.Friendlies ?? System.Array.Empty<IFriendlyConstruct>();

        /// <summary>
        /// All friendly constructs except the current construct.
        /// </summary>
        public static IReadOnlyList<IFriendlyConstruct> AllExcludingSelf =>
            ScriptApi.Context?.FriendliesExcludingSelf ?? System.Array.Empty<IFriendlyConstruct>();

        /// <summary>
        /// All fleets containing friendly constructs.
        /// </summary>
        public static IReadOnlyList<IFleet> Fleets =>
            ScriptApi.Context?.Fleets ?? System.Array.Empty<IFleet>();

        /// <summary>
        /// The fleet that the current construct belongs to.
        /// </summary>
        public static IFleet MyFleet => ScriptApi.Context?.MyFleet!;
    }
}
