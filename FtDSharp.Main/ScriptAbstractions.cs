using System;
using System.Collections.Generic;
using BrilliantSkies.Core.Logger;

namespace FtDSharp
{
    public interface IFtDSharp
    {
        void Update(float deltaTime);
    }

    internal interface IScriptContext
    {
        IMainConstruct Self { get; }
        ILogApi Log { get; }
        float TimeSinceStart { get; }
        long TicksSinceStart { get; }
        /// <summary>Raw access to the block type storage.</summary>
        IBlockToConstructBlockTypeStorage? BlockTypeStorage { get; }
        /// <summary>All AI Mainframes on the construct.</summary>
        IReadOnlyList<IMainframe> Mainframes { get; }
        /// <summary>All valid incoming projectile warnings.</summary>
        IReadOnlyList<IProjectileWarning> IncomingProjectiles { get; }
        /// <summary>All friendly constructs including the current construct.</summary>
        IReadOnlyList<IFriendlyConstruct> Friendlies { get; }
        /// <summary>All friendly constructs excluding the current construct.</summary>
        IReadOnlyList<IFriendlyConstruct> FriendliesExcludingSelf { get; }
        /// <summary>All fleets containing friendly constructs.</summary>
        IReadOnlyList<IFleet> Fleets { get; }
        /// <summary>The fleet the current construct belongs to.</summary>
        IFleet MyFleet { get; }
        /// <summary>All weapons on the construct (excluding turrets).</summary>
        IReadOnlyList<IWeapon> Weapons { get; }
        /// <summary>All turrets on the construct.</summary>
        IReadOnlyList<ITurret> Turrets { get; }
    }

    public interface ILogApi
    {

        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void ClearLogs();
    }

    class BasicLogApi : ILogApi
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
