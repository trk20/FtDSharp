using System.Collections.Generic;
using UnityEngine;

namespace FtDSharp
{
    /// <summary>
    /// Represents an AI Mainframe block and provides access to its targeting information.
    /// Each mainframe can have its own primary target and aimpoint selections.
    /// </summary>
    public interface IMainframe
    {
        /// <summary> The underlying AI Mainframe block with configuration properties. </summary>
        IAIMainframe Block { get; }

        /// <summary> This mainframe's primary (highest priority) target, or null if no targets. </summary>
        ITarget? PrimaryTarget { get; }

        /// <summary> All targets tracked by this mainframe, ordered by priority. </summary>
        IReadOnlyList<ITarget> Targets { get; }

        /// <summary>
        /// Gets the aimpoint position for a target as calculated by this mainframe.
        /// </summary>
        /// <param name="target">The target to get an aimpoint for (should be from this mainframe's Targets list).</param>
        /// <returns>World position of the aimpoint with error applied.</returns>
        Vector3 GetAimpoint(ITarget target);

        /// <summary>
        /// Forces this mainframe to set its primary target to the specified target.
        /// Will persist until changed or the target is lost/destroyed.
        /// </summary>
        /// <param name="target">The target to set as primary, or null to clear forced target.</param>
        void SetPrimaryTarget(ITarget? target);
    }
}
