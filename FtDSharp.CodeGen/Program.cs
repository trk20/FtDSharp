using System.Reflection;
using Serilog;
using Serilog.Events;

namespace FtDSharp.CodeGen;

class Program
{
    static void Main(string[] args)
    {
        var level = args.Contains("-v") || args.Contains("--verbose")
            ? LogEventLevel.Debug
            : LogEventLevel.Information;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(level)
            .WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        var positionalArgs = args.Where(a => !a.StartsWith("-")).ToArray();

        // Resolve workspace root from assembly location to avoid path issues with different working directories
        var workspaceRoot = ResolveWorkspaceRoot();

        var outputApiPath = positionalArgs.ElementAtOrDefault(0)
            ?? Path.Combine(workspaceRoot, "FtDSharp.API", "Generated");
        var outputFacadePath = positionalArgs.ElementAtOrDefault(1)
            ?? Path.Combine(workspaceRoot, "FtDSharp.Main", "Facades", "Generated");

        Log.Debug("Workspace root: {Root}", workspaceRoot);

        var pipeline = new GeneratorPipeline();
        pipeline.Run(outputApiPath, outputFacadePath);
    }

    /// <summary>
    /// Resolves the workspace root directory by walking up from the assembly location
    /// until we find the FtDSharp.sln file.
    /// </summary>
    private static string ResolveWorkspaceRoot()
    {
        // Try assembly location first
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

        // Fall back to current directory traversal
        var current = Directory.GetCurrentDirectory();
        while (current != null)
        {
            if (File.Exists(Path.Combine(current, "FtDSharp.sln")))
                return current;
            current = Path.GetDirectoryName(current);
        }

        // Last resort: use relative path from current directory
        Log.Warning("Could not find FtDSharp.sln, using current directory as workspace root");
        return Directory.GetCurrentDirectory();
    }
}
