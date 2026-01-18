using FtDSharp;
using static FtDSharp.Logging;

/// <summary>
/// Demonstrates weapon control functionality.
/// Shows tracking with lead calculation and turret control.
/// </summary>
public class SubObjectHierarchy : IFtDSharp
{
    public void Update(float deltaTime)
    {
        ClearLogs();
        foreach (var sub in Blocks.SpinBlocks)
        {
            Log($"SpinBlock {sub.UniqueId} has parent {sub.Parent?.UniqueId ?? -1}");
        }
    }
}
