using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using BrilliantSkies.Modding;
using BrilliantSkies.Profiling;
using HarmonyLib;

namespace FtDSharp
{
    public class FtDInterface : GamePlugin_PostLoad
    {

        public string name => "FtDSharp";

        public Version version => ModInfo.ModVersion;

        public void OnLoad()
        {
            new Harmony("FtDSharp").PatchAll();
            ModInfo.OnLoad();

            Entry.AddModule(AbstractModule<FtDSharpProfiler>.Instance);

            ScriptCompilationCache.Warmup();
        }

        public bool AfterAllPluginsLoaded() => true;

        public void OnSave() { }

    }

    public static class ModInfo
    {
        public static readonly string ModName, ModPath;
        public static readonly Version ModVersion;

        static ModInfo()
        {
            ModPath = Assembly.GetExecutingAssembly().Location;
            ModName = Path.GetDirectoryName(ModPath);

            while (Path.GetFileName(ModName) != "Mods")
            {
                ModPath = ModName;
                ModName = Path.GetDirectoryName(ModPath);
            }

            ModName = Path.GetFileName(ModPath);
            ModVersion = ReadVersionFromPluginJson(ModPath);
        }

        public static void OnLoad()
        {
            ModProblems.AddModProblem($"{ModName} v{ModVersion} active!", ModPath, string.Empty, false);
        }

        private static Version ReadVersionFromPluginJson(string modPath)
        {
            var path = Path.Combine(modPath, "plugin.json");
            if (!File.Exists(path))
                throw new FileNotFoundException($"Missing plugin.json at {path}");

            Match match = Regex.Match(File.ReadAllText(path), @"""version""\s*:\s*""([^""]+)""");
            return !match.Success || !Version.TryParse(match.Groups[1].Value, out Version? version)
                ? throw new InvalidOperationException($"Could not read a valid \"version\" from {path}")
                : version;
        }
    }


}
