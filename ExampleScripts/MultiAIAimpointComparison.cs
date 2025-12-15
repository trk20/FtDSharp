using FtDSharp;
using static FtDSharp.Logging;
using System.Linq;
using UnityEngine;

/// <summary>
/// Demonstrates how different AI mainframes can have different aimpoints for the same target.
/// Each mainframe may have different aimpoint selection cards or detection error,
/// resulting in different aimpoint positions.
/// </summary>
public class MultiAIAimpointComparison : IFtDSharp
{
    public MultiAIAimpointComparison()
    {
        Log("MultiAIAimpointComparison script initialized.");
    }

    public void Update(float deltaTime)
    {
        ClearLogs();

        var mainframes = AI.Mainframes;
        Log($"Found {mainframes.Count} AI mainframe(s)");

        // Use the highest priority mainframe's primary target as our reference
        var target = AI.HighestPriorityMainframe.PrimaryTarget;
        if (target == null)
        {
            Log("No target detected.");
            return;
        }

        Log($"\nTarget: {target.Name} (ID: {target.UniqueId})");
        Log($"Target Position: {target.Position}");
        Log($"Target Position Error: {target.PositionError:F2}m");
        Log("---");

        // Compare aimpoints from each mainframe for the same target
        foreach (var mainframe in mainframes)
        {
            var block = mainframe.Block;
            var aimpoint = mainframe.GetAimpoint(target);
            float distanceFromCenter = Vector3.Distance(aimpoint, target.Position);

            Log($"\nMainframe Priority {block.Priority}:");
            Log($"  Aimpoint: {aimpoint}");
            Log($"  Distance from target center: {distanceFromCenter:F2}m");

            // Show which target this mainframe is actually focused on
            var thisMainframeTarget = mainframe.PrimaryTarget;
            if (thisMainframeTarget != null && thisMainframeTarget.UniqueId != target.UniqueId)
            {
                Log($"  Note: This mainframe's primary target is {thisMainframeTarget.Name} (different from reference)");
            }

            Log($"  Targets tracked: {mainframe.Targets.Count}");
        }

        // If there are multiple mainframes, show aimpoint spread
        if (mainframes.Count > 1)
        {
            var aimpoints = mainframes.Select(m => m.GetAimpoint(target)).ToArray();

            // Calculate max spread between any two aimpoints
            float maxSpread = aimpoints
                .SelectMany((a, i) => aimpoints.Skip(i + 1).Select(b => Vector3.Distance(a, b)))
                .DefaultIfEmpty(0f)
                .Max();

            Log($"\n--- Aimpoint Spread Analysis ---");
            Log($"Max aimpoint spread between mainframes: {maxSpread:F2}m");
        }
    }
}
