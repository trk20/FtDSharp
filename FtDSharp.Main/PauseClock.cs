using UnityEngine;

namespace FtDSharp
{
    internal static class PauseClock
    {
        private static float _totalPauseDuration;
        private static float _pauseStartedAt;

        internal static float AdjustedRealtime => Time.realtimeSinceStartup - _totalPauseDuration;

        internal static void OnPaused() => _pauseStartedAt = Time.realtimeSinceStartup;

        internal static void OnUnpaused() => _totalPauseDuration += Time.realtimeSinceStartup - _pauseStartedAt;
    }
}
