namespace FtDSharp.CodeGen.Utils;

public class NameScope
{
    private readonly Dictionary<string, int> _usedNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _collisions = new();

    public IReadOnlyList<string> Collisions => _collisions;
    public bool IsTaken(string name) => _usedNames.ContainsKey(name);

    public string Register(string candidateName, string? dataPackageName, string originalName)
    {
        var finalName = candidateName;

        if (_usedNames.ContainsKey(candidateName))
        {
            if (!string.IsNullOrEmpty(dataPackageName))
                finalName = $"{dataPackageName}_{candidateName}";
            else
            {
                var count = _usedNames.GetValueOrDefault(candidateName, 1) + 1;
                _usedNames[candidateName] = count;
                finalName = $"{candidateName}{count}";
            }

            _collisions.Add($"Collision: '{candidateName}' (from '{originalName}') resolved to '{finalName}'");
        }

        _usedNames[finalName] = 1;
        return finalName;
    }

    public void MarkUsed(string name) => _usedNames.TryAdd(name, 1);
}
