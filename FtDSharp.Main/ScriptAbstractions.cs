using System;
using BrilliantSkies.Core.Logger;

namespace FtDSharp
{
    class BasicLogApi
    {
        private LuaBinding? _binding;

        public void AttachBinding(LuaBinding? binding)
        {
            _binding = binding;
        }

        public void ClearLogs()
        {
            if (_binding != null)
            {
                try
                {
                    _binding.ClearLogs();
                    return;
                }
                catch (Exception)
                {
                }
            }
            AdvLogger.LogInfo("[FtDSharp] Logs cleared.");
        }
        public void Info(string message) => LogToLuaOrFallback("INFO", message, () => AdvLogger.LogInfo("[FtDSharp] " + message));

        public void Warn(string message) => LogToLuaOrFallback("WARN", message, () => AdvLogger.LogWarning("[FtDSharp] " + message, LogOptions.PopupDev));

        public void Error(string message) => LogToLuaOrFallback("ERROR", message, () => AdvLogger.LogError("[FtDSharp] " + message, LogOptions.PopupDev));

        private void LogToLuaOrFallback(string level, string message, Action fallback)
        {
            if (_binding != null)
            {
                try
                {
                    _binding.Log($"[{level}] {message}");
                    return;
                }
                catch (Exception)
                {
                    // Fall back below
                }
            }

            fallback();
        }
    }
}
