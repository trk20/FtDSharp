using System;
using System.Reflection;
using BrilliantSkies.Core.CSharp;
using BrilliantSkies.Modding;
using HarmonyLib;

namespace FtDSharp
{
    /// <summary>
    /// Early-loading plugin that patches <see cref="Mirror.GetAllOfInterface(Type)"/> before
    /// the game's UI initialization triggers assembly scanning. This prevents
    /// TypeLoadExceptions from Roslyn assemblies bundled with FtDSharp.
    /// </summary>
    public class FtDPrePatch : GamePlugin
    {
        public string name => "FtDSharp.PrePatch";

        public Version version => new(0, 1, 0);

        public void OnLoad()
        {
            var harmony = new Harmony("FtDSharp.PrePatch");

            var original = typeof(Mirror).GetMethod(
                nameof(Mirror.GetAllOfInterface),
                new[] { typeof(Type) });

            var prefix = typeof(MirrorPatch).GetMethod(
                nameof(MirrorPatch.Prefix_GetAllOfInterface),
                BindingFlags.Static | BindingFlags.Public);

            harmony.Patch(original, prefix: new HarmonyMethod(prefix));
        }

        public bool AfterAllPluginsLoaded() => true;

        public void OnSave() { }
    }
}
