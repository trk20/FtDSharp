namespace FtDSharp.Tests.Mocks;

public class MockGameProvider : IGameProvider
{
    public IMainConstruct MainConstruct { get; set; } = null!;
    public float GameTime { get; set; }
    public float RealTime { get; set; }
    public float GameDeltaTime { get; set; } = 0.025f;
    public float RealDeltaTime { get; set; } = 0.025f;
    public long TicksSinceStart { get; set; }
}
