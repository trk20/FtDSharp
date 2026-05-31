public class TimeAwareness
{
    [OnStart]
    public void Initialize()
    {
        Log("TimeAwareness script initialized.");
    }

    [OnPhysicsTick]
    public void Update()
    {
        ClearLogs();
        Log(
            $"Ticks since start: {Game.TicksSinceStart} ticks\n" +
            $"Time since start: {Game.GameTime} seconds\n" +
            $"Real time since start: {Game.RealTime} seconds\n" +
            $"Delta time: {Game.GameDeltaTime} seconds\n" +
            $"Real delta time: {Game.RealDeltaTime} seconds");
    }
}