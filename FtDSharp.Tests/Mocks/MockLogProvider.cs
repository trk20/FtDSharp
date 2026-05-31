namespace FtDSharp.Tests.Mocks;

public class MockLogProvider : ILogProvider
{
    public List<string> InfoMessages { get; } = new();
    public List<string> WarnMessages { get; } = new();
    public List<string> ErrorMessages { get; } = new();
    public int ClearCount { get; private set; }

    public void Info(string message) => InfoMessages.Add(message);
    public void Warn(string message) => WarnMessages.Add(message);
    public void Error(string message) => ErrorMessages.Add(message);

    public void ClearLogs()
    {
        ClearCount++;
        InfoMessages.Clear();
        WarnMessages.Clear();
        ErrorMessages.Clear();
    }
}
