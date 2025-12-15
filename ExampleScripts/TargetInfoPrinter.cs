using FtDSharp;
using static FtDSharp.Logging;

public class TargetInfoPrinter : IFtDSharp
{
    public TargetInfoPrinter()
    {
        Log("TargetInfoPrinter script initialized.");
    }

    public void Update(float deltaTime)
    {
        ClearLogs();

        // Get the highest priority AI mainframe
        var mainframe = AI.HighestPriorityMainframe;
        var target = mainframe.PrimaryTarget;
        if (target == null)
        {
            Log("No primary target.");
            return;
        }        // Get the aimpoint for this target from this mainframe
        var aimpoint = mainframe.GetAimpoint(target);

        Log(
            "Primary Target Info:\n" +
            $"Target {target.Name}\n" +
            $"Id: {target.UniqueId}\n" +
            $"Speed: {target.Velocity.magnitude} m/s\n" +
            $"Distance: {(target.Position - Game.MainConstruct.Position).magnitude} m\n" +
            $"Firepower: {target.TotalFirePower}\n" +
            $"Stability: {target.Stability}\n" +
            $"Total Blocks: {target.BlockCount}\n" +
            $"Alive Blocks: {target.AliveBlockCount}\n" +
            $"Position Error: {target.PositionError}\n" +
            $"Aimpoint: {aimpoint}");
    }
}