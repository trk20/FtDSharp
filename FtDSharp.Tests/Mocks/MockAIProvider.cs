namespace FtDSharp.Tests.Mocks;

public class MockAIProvider : IAIProvider
{
    public IReadOnlyList<IMainframe> Mainframes { get; set; } = Array.Empty<IMainframe>();
}
