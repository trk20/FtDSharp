using System.Text.RegularExpressions;
using FtDSharp.CodeGen.Models;
using Serilog;

namespace FtDSharp.CodeGen.Passes;

public partial class NamingPass
{
    private static readonly Regex PrefixPattern = GetPrefixRegex();

    public static void Run(IReadOnlyList<BlockDefinition> blocks)
    {
        Log.Debug("Applying naming transformations for {Count} blocks...", blocks.Count);
        var collisions = new List<string>();

        foreach (var block in blocks)
            ProcessBlock(block, collisions);

        if (collisions.Count > 0)
        {
            Log.Warning("{Count} naming collisions detected", collisions.Count);
            foreach (var collision in collisions)
                Log.Debug("{Collision}", collision);
        }
    }

    private static void ProcessBlock(BlockDefinition block, List<string> collisions)
    {
        var scope = new Utils.NameScope();

        foreach (var prop in block.AllProperties)
        {
            var candidate = PrefixPattern.Replace(prop.OriginalName, "");
            candidate = Overrides.ApplyRename(candidate, prop.DataPackageName);

            if (Overrides.ShouldSkip(candidate))
            {
                prop.IsExcluded = true;
                continue;
            }

            prop.Name = scope.Register(candidate, prop.DataPackageName, prop.OriginalName);
        }

        foreach (var collision in scope.Collisions)
            collisions.Add($"[{block.ClassName}] {collision}");

        block.AllProperties.RemoveAll(p => p.IsExcluded);
    }

    [GeneratedRegex(@"^(?<prefix>[A-Za-z]+)_", RegexOptions.Compiled)]
    private static partial Regex GetPrefixRegex();
}
