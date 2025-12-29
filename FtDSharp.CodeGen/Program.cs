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
        var outputApiPath = positionalArgs.ElementAtOrDefault(0) ?? Path.Combine("..", "FtDSharp.API", "Generated");
        var outputFacadePath = positionalArgs.ElementAtOrDefault(1) ?? Path.Combine("..", "FtDSharp.Main", "Facades", "Generated");

        var pipeline = new GeneratorPipeline();
        pipeline.Run(outputApiPath, outputFacadePath);
    }
}
