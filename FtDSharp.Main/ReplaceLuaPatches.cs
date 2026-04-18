using System;
using System.Runtime.CompilerServices;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Logger;
using HarmonyLib;
using Microsoft.CodeAnalysis;
using UnityEngine;

namespace FtDSharp
{
    [HarmonyPatch]
    public static class ReplaceLuaPatches
    {
        private const string LegacyLuaTemplate = "function Update(I)\n-- put your code here \nend";

        internal const string DefaultCSharpTemplate =
@"using FtDSharp;
using static FtDSharp.Logging;
using static FtDSharp.Game;

public class SampleScript : IFtDSharp
{
    public SampleScript()
    {
        Log(""FtDSharp script template running."");
        Log($""I am {MainConstruct.Name} at {MainConstruct.Position}"");
    }

    public void Update()
    {
        // TODO: implement your logic here.
    }
}
";

        private static readonly AccessTools.FieldRef<LuaBox, string> CodeFieldRef =
            AccessTools.FieldRefAccess<LuaBox, string>("_code");

        private static readonly ConditionalWeakTable<LuaBox, LuaRuntimeState> RuntimeTable = new();

        private static LuaRuntimeState GetRuntime(LuaBox luaBox) =>
            RuntimeTable.GetValue(luaBox, _ => new LuaRuntimeState());

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LuaBox), nameof(LuaBox.BlockStart))]
        private static void LuaBox_BlockStart_Postfix(LuaBox __instance)
        {
            var runtime = GetRuntime(__instance);
            __instance.binding = new LuaBinding((__instance.MainConstruct as MainConstruct)!);
            runtime.Attach(__instance);
            runtime.EnsureCompiled(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LuaBox), "InitialiseLua")] // private method
        private static bool LuaBox_InitialiseLua_Prefix(LuaBox __instance)
        {
            GetRuntime(__instance).Attach(__instance);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LuaBox), "SetLuaCode")] // private method
        private static bool LuaBox_SetLuaCode_Prefix(LuaBox __instance)
        {
            var runtime = GetRuntime(__instance);
            runtime.Attach(__instance);
            __instance.Running = runtime.EnsureCompiled(__instance); // note: lags first time - maybe precompile default script
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LuaBox), "RunLuaUpdate")] // private method
        private static bool LuaBox_RunLuaUpdate_Prefix(LuaBox __instance)
        {
            if (Net.IsClient)
            {
                __instance.Running = false;
                return false;
            }

            if (RuntimeTable.TryGetValue(__instance, out var runtime))
            {
                runtime.Tick(__instance);
                __instance.Running = runtime.IsActive;
            }
            else
            {
                __instance.Running = false;
            }

            return false;
        }

        // todo: investigate multiplayer (probably broken)

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameSpeedManager), "SetTimeScale")]
        private static void GameSpeedManager_SetTimeScale_Prefix(float newTimeScale)
        {
            bool wasPaused = Time.timeScale <= 0f;
            bool willPause = newTimeScale <= 0f;

            if (!wasPaused && willPause)
                PauseClock.OnPaused();
            else if (wasPaused && !willPause)
                PauseClock.OnUnpaused();
        }

        private sealed class LuaRuntimeState
        {
            private readonly ScriptHost _host = new();
            private readonly BasicScriptContext _context = new();
            private string _cachedSource = string.Empty;
            private string? _cachedHash;
            private float _lastAdjustedRealtime;

            internal bool IsActive => _host.Active;

            internal void Attach(LuaBox luaBox)
            {
                _context.Attach(luaBox);
                if (_lastAdjustedRealtime <= 0f)
                {
                    _lastAdjustedRealtime = PauseClock.AdjustedRealtime;
                }
            }

            internal bool EnsureCompiled(LuaBox luaBox)
            {
                if (Net.IsClient)
                {
                    _host.Deactivate();
                    return false;
                }

                var code = NormalizeInitialCode(luaBox, luaBox.GetText());
                if (string.IsNullOrWhiteSpace(code))
                {
                    _host.Deactivate();
                    _cachedSource = string.Empty;
                    _cachedHash = null;
                    return false;
                }

                var hash = ScriptHost.ComputeHash(code);

                // Already active with the same code — no-op
                if (_host.Active && string.Equals(hash, _cachedHash, StringComparison.Ordinal))
                {
                    return true;
                }

                if (_host.Active)
                {
                    _host.Deactivate();
                }

                _cachedSource = code;
                _cachedHash = hash;

                var (compiled, diagnostics) = _host.Compile(code, hash);
                if (!compiled)
                {
                    ReportDiagnostics(diagnostics, _host.LastError);
                    return false;
                }

                var instantiated = _host.Instantiate(hash, _context);
                if (!instantiated)
                {
                    ReportDiagnostics(Array.Empty<Diagnostic>(), _host.LastError);
                    return false;
                }

                _lastAdjustedRealtime = PauseClock.AdjustedRealtime;
                if (_host.LastCompileTime.TotalMilliseconds > 0)
                {
                    AdvLogger.LogInfo($"[FtDSharp] Compiled C# script for LuaBox #{luaBox.GetHashCode()} in {_host.LastCompileTime.TotalMilliseconds:F2} ms");
                }

                return true;
            }

            internal void Tick(LuaBox luaBox)
            {
                if (!luaBox.Running || !_host.Active)
                {
                    _host.Deactivate();
                    _lastAdjustedRealtime = PauseClock.AdjustedRealtime;
                    return;
                }

                var adjustedRealtime = PauseClock.AdjustedRealtime;
                var realDelta = adjustedRealtime - _lastAdjustedRealtime;
                _lastAdjustedRealtime = adjustedRealtime;

                if (realDelta < 0f)
                {
                    realDelta = 0f;
                }

                var gameDelta = Time.fixedDeltaTime;

                _context.Attach(luaBox);
                _context.IncrementTick(gameDelta, realDelta);
                _host.Tick(_context);
            }

            private void ReportDiagnostics(Diagnostic[] diagnostics, string? errorMessage)
            {
                // Show the human-readable error message if available
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    _context.Log.Error(errorMessage);
                    return;
                }

                // Fall back to Roslyn diagnostics
                if (diagnostics == null || diagnostics.Length == 0)
                {
                    _context.Log.Error("Failed to compile LuaBox script (no diagnostics available).");
                    return;
                }

                foreach (var diagnostic in diagnostics)
                {
                    _context.Log.Error(diagnostic.ToString());
                }
            }
        }

        private static string NormalizeInitialCode(LuaBox luaBox, string? code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return string.Empty;
            }

            if (string.Equals(code, LegacyLuaTemplate, StringComparison.Ordinal))
            {
                CodeFieldRef(luaBox) = DefaultCSharpTemplate;
                return DefaultCSharpTemplate;
            }

            return code;
        }
    }
}
