namespace FtDSharp;

public static class Logging
{
    public static void Log(string message) => ScriptContext.Current?.Log.Info(message);
    public static void ClearLogs() => ScriptContext.Current?.Log.ClearLogs();
    public static void LogWarning(string message) => ScriptContext.Current?.Log.Warn(message);
    public static void LogError(string message) => ScriptContext.Current?.Log.Error(message);
}