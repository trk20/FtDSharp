/// <summary>
/// Demonstrates how different AI mainframes can have different aimpoints for the same target.
/// Each mainframe may have different aimpoint selection cards or detection error,
/// resulting in different aimpoint positions.
/// </summary>
public class MultiAIAimpointComparison
{
    [OnStart]
    public void Initialize()
    {
        Log("MultiAIAimpointComparison script initialized.");
    }

    [OnPhysicsTick]
    public void Update()
    {
        ClearLogs();

        IReadOnlyList<IMainframe> mainframes = AI.Mainframes;
        Log($"Found {mainframes.Count} AI mainframe(s)");

        // Use the highest priority mainframe's primary target as our reference
        ITarget? target = AI.HighestPriorityMainframe.PrimaryTarget;
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
        foreach (IMainframe mainframe in mainframes)
        {
            IAIMainframe block = mainframe.Block;
            Vector3 aimpoint = mainframe.GetAimpoint(target);
            var distanceFromCenter = Vector3.Distance(aimpoint, target.Position);

            Log($"\nMainframe Priority {block.Priority}:");
            Log($"  Aimpoint: {aimpoint}");
            Log($"  Distance from target center: {distanceFromCenter:F2}m");

            // Show which target this mainframe is actually focused on
            ITarget? thisMainframeTarget = mainframe.PrimaryTarget;
            if (thisMainframeTarget != null && thisMainframeTarget.UniqueId != target.UniqueId)
            {
                Log($"  Note: This mainframe's primary target is {thisMainframeTarget.Name} (different from reference)");
            }

            Log($"  Targets tracked: {mainframe.Targets.Count}");
        }

        // If there are multiple mainframes, show aimpoint spread
        if (mainframes.Count > 1)
        {
            Vector3[] aimpoints = mainframes.Select(m => m.GetAimpoint(target)).ToArray();

            // Calculate max spread between any two aimpoints
            var maxSpread = aimpoints
                .SelectMany((a, i) => aimpoints.Skip(i + 1).Select(b => Vector3.Distance(a, b)))
                .DefaultIfEmpty(0f)
                .Max();

            Log($"\n--- Aimpoint Spread Analysis ---");
            Log($"Max aimpoint spread between mainframes: {maxSpread:F2}m");
        }
    }
}
