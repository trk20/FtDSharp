namespace FtDSharp.Tests.Mocks;

public class TestProviderScope : IProviderScope
{
    public MockGameProvider GameProvider { get; } = new();
    public MockLogProvider LogProvider { get; } = new();
    public MockDrawingProvider DrawingProvider { get; } = new();
    public MockAIProvider AIProvider { get; } = new();
    public MockWeaponsProvider WeaponsProvider { get; } = new();
    public MockGuidanceProvider GuidanceProvider { get; } = new();
    public MockWarningsProvider WarningsProvider { get; } = new();
    public MockFleetProvider FleetProvider { get; } = new();
    public MockBlocksProvider BlocksProvider { get; } = new();
    public MockPropulsionProvider PropulsionProvider { get; } = new();

    IGameProvider IProviderScope.Game => GameProvider;
    ILogProvider IProviderScope.Log => LogProvider;
    IDrawingProvider IProviderScope.Drawing => DrawingProvider;
    IAIProvider IProviderScope.AI => AIProvider;
    IWeaponsProvider IProviderScope.Weapons => WeaponsProvider;
    IGuidanceProvider IProviderScope.Guidance => GuidanceProvider;
    IWarningsProvider IProviderScope.Warnings => WarningsProvider;
    IFleetProvider IProviderScope.Fleet => FleetProvider;
    IBlocksProvider IProviderScope.Blocks => BlocksProvider;
    IPropulsionProvider IProviderScope.Propulsion => PropulsionProvider;

    public void Dispose()
    {
    }
}
