using System.Text.RegularExpressions;
using FtDSharp.CodeGen.Models;
using Serilog;

namespace FtDSharp.CodeGen.Passes;

public partial class NamingPass : IBlockPass
{
    private static readonly Regex PrefixPattern = GetPrefixRegex();
    private readonly List<string> _allCollisions = new();

    public IReadOnlyList<string> AllCollisions => _allCollisions;

    public void Process(List<BlockDefinition> blocks)
    {
        Log.Debug("Applying naming transformations for {Count} blocks...", blocks.Count);
        _allCollisions.Clear();

        foreach (var block in blocks)
            ProcessBlock(block);

        if (_allCollisions.Count > 0)
        {
            Log.Warning("{Count} naming collisions detected", _allCollisions.Count);
            foreach (var collision in _allCollisions)
                Log.Debug("{Collision}", collision);
        }
    }

    private void ProcessBlock(BlockDefinition block)
    {
        var scope = new Utils.NameScope();

        foreach (var prop in block.AllProperties)
        {
            var candidate = PrefixPattern.Replace(prop.OriginalName, "");
            candidate = Overrides.ApplyRename(candidate, prop.DataPackageName);

            if (Overrides.ShouldSkip(candidate))
            {
                prop.Name = $"__SKIP__{candidate}";
                continue;
            }

            prop.Name = scope.Register(candidate, prop.DataPackageName, prop.OriginalName);
        }

        foreach (var collision in scope.Collisions)
            _allCollisions.Add($"[{block.ClassName}] {collision}");

        block.AllProperties.RemoveAll(p => p.Name.StartsWith("__SKIP__"));
    }

    [GeneratedRegex(@"^(?<prefix>[A-Za-z]+)_", RegexOptions.Compiled)]
    private static partial Regex GetPrefixRegex();
}
