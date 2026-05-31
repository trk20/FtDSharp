namespace FtDSharp.Tests.Mocks;

public class MockGuidanceProvider : IGuidanceProvider
{
    public IReadOnlyList<IMissile> Missiles { get; set; } = Array.Empty<IMissile>();
}
