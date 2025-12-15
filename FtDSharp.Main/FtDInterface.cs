using System;
using System.IO;
using System.Reflection;
using BrilliantSkies.Modding;
using HarmonyLib;

namespace FtDSharp
{
	public class FtDInterface : GamePlugin_PostLoad
	{

		public string name => "FtDSharp";

		public Version version => new(0, 1, 5);

		public void OnLoad()
		{
			new Harmony("FtDSharp").PatchAll();
			ModInfo.ModVersion = version;
			ModInfo.OnLoad();
		}

		public bool AfterAllPluginsLoaded() => true;

		public void OnSave() { }

	}

	public static class ModInfo
	{
		public static readonly string ModName, ModPath;
		public static Version? ModVersion;

		static ModInfo()
		{
			ModPath = Assembly.GetExecutingAssembly().Location;
			ModName = Path.GetDirectoryName(ModPath)!;

			while (Path.GetFileName(ModName) != "Mods")
			{
				ModPath = ModName;
				ModName = Path.GetDirectoryName(ModPath)!;
			}

			ModName = Path.GetFileName(ModPath);
		}

		public static void OnLoad()
		{
			ModProblems.AddModProblem($"{ModName} v{ModVersion} active!", ModPath, string.Empty, false);
		}
	}


}
