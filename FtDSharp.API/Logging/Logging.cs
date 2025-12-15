using System;

namespace FtDSharp
{
    public static class Logging
    {
        public static void Log(string message) => ScriptApi.Context?.Log.Info(message);
        public static void ClearLogs() => ScriptApi.Context?.Log.ClearLogs();
        public static void LogWarning(string message) => ScriptApi.Context?.Log.Warn(message);
        public static void LogError(string message) => ScriptApi.Context?.Log.Error(message);
    }
}