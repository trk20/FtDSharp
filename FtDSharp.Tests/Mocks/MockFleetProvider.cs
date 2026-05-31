namespace FtDSharp.Tests.Mocks;

public class MockFleetProvider : IFleetProvider
{
    public IReadOnlyList<IFriendlyConstruct> All { get; set; } = Array.Empty<IFriendlyConstruct>();
    public IReadOnlyList<IFriendlyConstruct> AllExcludingSelf { get; set; } = Array.Empty<IFriendlyConstruct>();
    public IReadOnlyList<IFleet> Fleets { get; set; } = Array.Empty<IFleet>();
    public IFleet MyFleet { get; set; } = null!;
}
