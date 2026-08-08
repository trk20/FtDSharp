/// <summary>
/// Demonstrates weapon control functionality.
/// Shows tracking with lead calculation and turret control.
/// </summary>
public class SubObjectHierarchy
{
    [OnPhysicsTick]
    public void Update()
    {
        ClearLogs();
        foreach (ISpinBlock sub in Blocks.SpinBlocks)
        {
            Log($"SpinBlock {sub.UniqueId} has parent {sub.Parent?.UniqueId ?? -1}");
        }
    }
}
