using System;
using System.Collections.Generic;
using System.Linq;
using FtDSharp.Facades;
using FtDSharp.Helpers;

namespace FtDSharp
{
    /// <summary>
    /// Static accessor for friendly construct and fleet information.
    /// </summary>
    public static class Friendly
    {
        private static readonly FrameCache<IReadOnlyList<IFriendlyConstruct>> _allCache =
            new(GetFriendlies);
        private static readonly FrameCache<IReadOnlyList<IFriendlyConstruct>> _excludingSelfCache =
            new(GetFriendliesExcludingSelf);
        private static readonly FrameCache<IReadOnlyList<IFleet>> _fleetsCache =
            new(GetFleets);

        /// <summary>
        /// All friendly constructs, including the current construct.
        /// </summary>
        public static IReadOnlyList<IFriendlyConstruct> All => _allCache.Value;

        /// <summary>
        /// All friendly constructs except the current construct.
        /// </summary>
        public static IReadOnlyList<IFriendlyConstruct> AllExcludingSelf => _excludingSelfCache.Value;

        /// <summary>
        /// All fleets containing friendly constructs.
        /// </summary>
        public static IReadOnlyList<IFleet> Fleets => _fleetsCache.Value;

        /// <summary>
        /// The fleet that the current construct belongs to.
        /// </summary>
        public static IFleet MyFleet => Game.MainConstruct?.Fleet!;

        private static IReadOnlyList<IFriendlyConstruct> GetFriendlies()
        {
            var construct = ScriptApi.Context?.RawAllConstruct;
            if (construct == null) return Array.Empty<IFriendlyConstruct>();

            var myTeam = construct.GetTeam();
            return StaticConstructablesManager.Constructables
                .Where(c => c != null && !c.Destroyed && c.GetTeam() == myTeam)
                .Select(c => new FriendlyConstructFacade(c))
                .Cast<IFriendlyConstruct>()
                .ToList();
        }

        private static IReadOnlyList<IFriendlyConstruct> GetFriendliesExcludingSelf()
        {
            var construct = ScriptApi.Context?.RawAllConstruct;
            if (construct == null) return Array.Empty<IFriendlyConstruct>();

            var myTeam = construct.GetTeam();
            return StaticConstructablesManager.Constructables
                .Where(c => c != null && !c.Destroyed && c != construct && c.GetTeam() == myTeam)
                .Select(c => new FriendlyConstructFacade(c))
                .Cast<IFriendlyConstruct>()
                .ToList();
        }

        private static IReadOnlyList<IFleet> GetFleets()
        {
            return All
                .Select(f => f.Fleet)
                .Where(fleet => fleet != null)
                .GroupBy(fleet => fleet.Id)
                .Select(g => g.First())
                .ToList();
        }
    }
}
