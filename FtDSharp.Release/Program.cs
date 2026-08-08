using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace FtDSharp.Release;

internal static class Program
{
    private static readonly string[] _managedSearchPaths =
    [
        @"C:\Program Files (x86)\Steam\steamapps\common\From The Depths\From_The_Depths_Data\Managed",
        @"C:\Program Files\Steam\steamapps\common\From The Depths\From_The_Depths_Data\Managed",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "From The Depths", "From_The_Depths_Data", "Managed"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam", "steam", "steamapps", "common", "From The Depths", "From_The_Depths_Data", "Managed"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "Steam", "steamapps", "common", "From The Depths", "From_The_Depths_Data", "Managed"),
        "ftd-managed",
    ];

    private static readonly string[] ReleaseDlls =
    [
        "0Harmony.dll",
        "Microsoft.CodeAnalysis.dll",
        "Microsoft.CodeAnalysis.CSharp.dll",
        "System.Collections.Immutable.dll",
        "System.Reflection.Metadata.dll",
        "System.Text.Encoding.CodePages.dll",
        "System.Memory.dll",
        "System.Runtime.CompilerServices.Unsafe.dll",
        "System.Numerics.Vectors.dll",
        "System.Threading.Tasks.Extensions.dll",
        "Microsoft.CodeAnalysis.BannedApiAnalyzers.dll",
        "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll",
        "FtDSharp.API.dll",
        "FtDSharp.dll",
    ];

    public static int Main(string[] args)
    {
        try
        {
            ReleaseOptions options = ParseArgs(args);
            var root = ResolveWorkspaceRoot();
            Directory.SetCurrentDirectory(root);

            Console.WriteLine("[1/6] Locating game DLLs...");
            var managedPath = LocateManaged(root);
            SyncManaged(root, managedPath);
            EnsureUnityReference(root);
            Console.WriteLine();

            Console.WriteLine("[2/6] Updating versions...");
            var pluginJsonPath = Path.Combine(root, "plugin.json");
            var modVersion = options.Version ?? ReadPluginJsonVersion(pluginJsonPath);
            var gameVersion = options.GameVersion ?? ReadFtdGameVersion(Path.Combine(root, "ftd-managed", "Ftd.dll"));
            UpdatePluginJson(pluginJsonPath, modVersion, gameVersion);
            Console.WriteLine($"  Mod version:  {modVersion}{(options.Version is null ? " (from plugin.json)" : "")}");
            Console.WriteLine($"  Game version: {gameVersion}{(options.GameVersion is null ? " (from FtdVersion)" : "")}");
            Console.WriteLine();

            Console.WriteLine("[3/6] Generating API bindings...");
            RunDotnet(root, "run", "--project", "FtDSharp.CodeGen");
            Console.WriteLine("  Code generation complete");
            Console.WriteLine();

            Console.WriteLine("[4/6] Building project...");
            RunDotnet(root, "build", "FtDSharp.csproj", "-c", "Release");
            Console.WriteLine("  Build complete");
            Console.WriteLine();

            Console.WriteLine("[5/6] Staging release artifacts...");
            var distMod = Path.Combine(root, "dist", "FtDSharp");
            StageRelease(root, distMod);
            Console.WriteLine();

            Console.WriteLine("[6/6] Creating release archive...");
            var zipPath = Path.Combine(root, "FtDSharp.zip");
            if (File.Exists(zipPath))
                File.Delete(zipPath);
            ZipFile.CreateFromDirectory(distMod, zipPath, CompressionLevel.Optimal, includeBaseDirectory: true);
            Directory.Delete(Path.Combine(root, "dist"), recursive: true);
            Console.WriteLine($"  Created {zipPath}");
            Console.WriteLine();

            Console.WriteLine("Release summary");
            Console.WriteLine($"  Version:      {modVersion}");
            Console.WriteLine($"  Game version: {gameVersion}");
            Console.WriteLine($"  Archive:      FtDSharp.zip");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static ReleaseOptions ParseArgs(string[] args)
    {
        string? version = null;
        string? gameVersion = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-v" or "--version":
                    version = RequireValue(args, ref i, args[i]);
                    break;
                case "--gameversion":
                    gameVersion = RequireValue(args, ref i, args[i]);
                    break;
                case "-h" or "--help":
                    PrintUsage();
                    Environment.Exit(0);
                    break;
                default:
                    throw new ArgumentException($"Unknown argument: {args[i]}\n\n{UsageText()}");
            }
        }

        return version is not null && !Regex.IsMatch(version, @"^\d+\.\d+\.\d+$")
            ? throw new ArgumentException($"Version must be major.minor.patch (got '{version}').")
            : gameVersion is not null && !Regex.IsMatch(gameVersion, @"^\d+\.\d+\.\d+$")
            ? throw new ArgumentException($"Game version must be major.minor.patch (got '{gameVersion}').")
            : new ReleaseOptions(version, gameVersion);
    }

    private static string RequireValue(string[] args, ref int i, string flag)
    {
        if (i + 1 >= args.Length)
            throw new ArgumentException($"Missing value for {flag}");
        i++;
        return args[i];
    }

    private static void PrintUsage() => Console.WriteLine(UsageText());

    private static string UsageText() =>
        """
        Usage: dotnet run --project FtDSharp.Release -- [-v VERSION] [--gameversion VERSION]

        Version defaults: mod version from plugin.json, game version from Ftd.dll (FtdVersion).
        Pass -v / --gameversion to override.

        Example: dotnet run --project FtDSharp.Release
                 dotnet run --project FtDSharp.Release -- -v 0.5.0
                 dotnet run --project FtDSharp.Release -- -v 0.5.0 --gameversion 4.3.4
        """;

    private static string ResolveWorkspaceRoot()
    {
        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        if (!string.IsNullOrEmpty(assemblyPath))
        {
            var dir = Path.GetDirectoryName(assemblyPath);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir, "FtDSharp.sln")))
                    return dir;
                dir = Path.GetDirectoryName(dir);
            }
        }

        var current = Directory.GetCurrentDirectory();
        while (current != null)
        {
            if (File.Exists(Path.Combine(current, "FtDSharp.sln")))
                return current;
            current = Path.GetDirectoryName(current);
        }

        throw new InvalidOperationException("Could not find FtDSharp.sln (workspace root).");
    }

    private static string LocateManaged(string root)
    {
        foreach (var candidate in _managedSearchPaths)
        {
            var path = Path.IsPathRooted(candidate) ? candidate : Path.Combine(root, candidate);
            if (!Directory.Exists(path))
                continue;

            if (Directory.EnumerateFiles(path, "*.dll").Any())
            {
                Console.WriteLine($"  Found at: {path}");
                return path;
            }
        }

        Console.Error.WriteLine("  Could not find From The Depths Managed DLLs");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Searched locations:");
        foreach (var candidate in _managedSearchPaths)
        {
            var path = Path.IsPathRooted(candidate) ? candidate : Path.Combine(root, candidate);
            Console.Error.WriteLine($"  - {path}");
        }

        Console.Error.WriteLine();
        Console.Error.WriteLine("Please either:");
        Console.Error.WriteLine("  1. Install From The Depths via Steam");
        Console.Error.WriteLine("  2. Manually copy DLLs to ftd-managed/");
        throw new InvalidOperationException("Managed DLLs not found.");
    }

    private static void SyncManaged(string root, string managedPath)
    {
        var dest = Path.Combine(root, "ftd-managed");
        if (Path.GetFullPath(managedPath) == Path.GetFullPath(dest))
        {
            var count = Directory.EnumerateFiles(dest, "*.dll").Count();
            Console.WriteLine($"  {count} DLLs ready");
            return;
        }

        Console.WriteLine("  Copying DLLs to ftd-managed/...");
        Directory.CreateDirectory(dest);
        foreach (var dll in Directory.EnumerateFiles(managedPath, "*.dll"))
            File.Copy(dll, Path.Combine(dest, Path.GetFileName(dll)), overwrite: true);

        Console.WriteLine($"  {Directory.EnumerateFiles(dest, "*.dll").Count()} DLLs ready");
    }

    private static void EnsureUnityReference(string root)
    {
        Console.WriteLine("  Preparing IDE reference assemblies...");
        var references = Path.Combine(root, "References");
        Directory.CreateDirectory(references);
        var dest = Path.Combine(references, "UnityEngine.CoreModule.dll");
        if (!File.Exists(dest))
            File.Copy(Path.Combine(root, "ftd-managed", "UnityEngine.CoreModule.dll"), dest);
        Console.WriteLine("  References/UnityEngine.CoreModule.dll ready");
    }

    private static string ReadPluginJsonVersion(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Missing {path}; pass -v VERSION to set the mod version.");

        var json = JsonNode.Parse(File.ReadAllText(path))
            ?? throw new InvalidOperationException($"Failed to parse {path}");
        var version = json["version"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(version) || !Regex.IsMatch(version, @"^\d+\.\d+\.\d+$"))
            throw new InvalidOperationException($"plugin.json has no valid \"version\"; pass -v VERSION to set it.");

        return version;
    }

    private static string ReadFtdGameVersion(string ftdDllPath)
    {
        if (!File.Exists(ftdDllPath))
            throw new InvalidOperationException($"Missing {ftdDllPath}; pass --gameversion VERSION or sync Managed DLLs.");

        var assembly = Assembly.LoadFrom(ftdDllPath);
        Type type = assembly.GetType("FtdVersion")
            ?? throw new InvalidOperationException("Type FtdVersion not found in Ftd.dll; pass --gameversion VERSION.");

        int ReadField(string name)
        {
            FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidOperationException($"FtdVersion.{name} not found.");
            return Convert.ToInt32(field.GetValue(null));
        }

        var version = $"{ReadField("Major")}.{ReadField("Minor")}.{ReadField("Subordinate")}";
        Console.WriteLine($"  Detected game version from FtdVersion: {version}");
        return version;
    }

    private static void UpdatePluginJson(string path, string version, string gameVersion)
    {
        JsonNode json = JsonNode.Parse(File.ReadAllText(path))
            ?? throw new InvalidOperationException($"Failed to parse {path}");
        json["version"] = version;
        json["gameversion"] = gameVersion;
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(path, json.ToJsonString(options) + Environment.NewLine);
        Console.WriteLine("  Updated plugin.json");
    }

    private static void RunDotnet(string root, params string[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = root,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            UseShellExecute = false,
        };
        foreach (var arg in args)
            psi.ArgumentList.Add(arg);

        using Process process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start dotnet.");
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"dotnet {string.Join(' ', args)} failed with exit code {process.ExitCode}.");
    }

    private static void StageRelease(string root, string distMod)
    {
        if (Directory.Exists(Path.Combine(root, "dist")))
            Directory.Delete(Path.Combine(root, "dist"), recursive: true);

        Directory.CreateDirectory(Path.Combine(distMod, "TipOfTheDay"));
        Directory.CreateDirectory(Path.Combine(distMod, "ExampleScripts", "Scripts"));
        Directory.CreateDirectory(Path.Combine(distMod, "ScriptProject"));
        Directory.CreateDirectory(Path.Combine(distMod, "References"));

        var buildOut = Path.Combine(root, "bin", "Release", "netstandard2.1");
        Console.WriteLine("  Copying mod DLLs...");
        foreach (var dll in ReleaseDlls)
        {
            var source = dll == "0Harmony.dll"
                ? Path.Combine(root, dll)
                : Path.Combine(buildOut, dll);
            if (!File.Exists(source))
                throw new InvalidOperationException($"Missing: {source}");
            File.Copy(source, Path.Combine(distMod, dll));
            Console.WriteLine($"    {dll}");
        }

        Console.WriteLine("  Copying IDE reference assemblies...");
        File.Copy(
            Path.Combine(root, "References", "UnityEngine.CoreModule.dll"),
            Path.Combine(distMod, "References", "UnityEngine.CoreModule.dll"));
        Console.WriteLine("    References/UnityEngine.CoreModule.dll");

        Console.WriteLine("  Copying metadata...");
        foreach (var name in new[] { "header.header", "plugin.json", "LICENSE.md", "README.md" })
            File.Copy(Path.Combine(root, name), Path.Combine(distMod, name));

        var tipSrc = Path.Combine(root, "TipOfTheDay");
        if (Directory.Exists(tipSrc))
        {
            Console.WriteLine("  Copying TipOfTheDay...");
            CopyDirectoryContents(tipSrc, Path.Combine(distMod, "TipOfTheDay"));
        }

        Console.WriteLine("  Copying ScriptProject...");
        File.Copy(
            Path.Combine(root, "ScriptProject", "FtDSharpScript.csproj"),
            Path.Combine(distMod, "ScriptProject", "FtDSharpScript.csproj"));
        File.Copy(
            Path.Combine(root, "ScriptProject", "MyScript.cs"),
            Path.Combine(distMod, "ScriptProject", "MyScript.cs"));
        File.Copy(
            Path.Combine(root, "ScriptProject", "README.md"),
            Path.Combine(distMod, "ScriptProject", "README.md"));

        Console.WriteLine("  Copying ExampleScripts...");
        File.Copy(
            Path.Combine(root, "ExampleScripts", "ExampleScripts.csproj"),
            Path.Combine(distMod, "ExampleScripts", "ExampleScripts.csproj"));
        File.Copy(
            Path.Combine(root, "ExampleScripts", "README.md"),
            Path.Combine(distMod, "ExampleScripts", "README.md"));

        var exampleScripts = Directory.GetFiles(Path.Combine(root, "ExampleScripts", "Scripts"), "*.cs");
        if (exampleScripts.Length == 0)
            throw new InvalidOperationException("No example scripts found in ExampleScripts/Scripts/");
        foreach (var script in exampleScripts)
            File.Copy(script, Path.Combine(distMod, "ExampleScripts", "Scripts", Path.GetFileName(script)));
        Console.WriteLine($"    {exampleScripts.Length} example scripts");

        Console.WriteLine("  Verifying IDE projects build...");
        RunDotnet(root, "build", Path.Combine(distMod, "ScriptProject", "FtDSharpScript.csproj"), "-v", "q");
        RunDotnet(root, "build", Path.Combine(distMod, "ExampleScripts", "ExampleScripts.csproj"), "-v", "q");
        Console.WriteLine("    ScriptProject and ExampleScripts compile");

        Console.WriteLine("  Creating clone helper...");
        var cloneHelper = Path.Combine(distMod, "clone-source.sh");
        File.WriteAllText(cloneHelper,
            """
            #!/bin/bash
            git clone https://github.com/trk20/FtDSharp.git
            echo "FtDSharp source cloned. See README.md for build instructions."
            """);
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(cloneHelper, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.GroupRead | UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
    }

    private static void CopyDirectoryContents(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);
        foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDir, file);
            var dest = Path.Combine(destDir, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(file, dest, overwrite: true);
        }
    }

    private sealed record ReleaseOptions(string? Version, string? GameVersion);
}