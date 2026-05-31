namespace FtDSharp.Tests.Helpers;

public static class ScriptTestHelper
{
    /// <summary>Attempts to compile script source code and returns the result.</summary>
    public static (bool Success, ScriptHost Host) TryCompile(string code)
    {
        var host = new ScriptHost();
        var hash = ScriptHost.ComputeHash(code);
        var (success, _) = host.Compile(code, hash);
        return (success, host);
    }

    /// <summary>Compiles script source code and returns the ScriptHost.</summary>
    public static ScriptHost Compile(string code)
    {
        var host = new ScriptHost();
        var hash = ScriptHost.ComputeHash(code);
        var (success, diagnostics) = host.Compile(code, hash);
        if (!success)
            throw new InvalidOperationException(
                $"Compilation failed:\n{string.Join("\n", diagnostics.Select(d => d.ToString()))}");
        return host;
    }

    /// <summary>Compiles, instantiates, and returns the host ready to tick.</summary>
    public static ScriptHost CompileAndInstantiate(string code, IProviderScope scope)
    {
        var host = Compile(code);
        var hash = ScriptHost.ComputeHash(code);
        if (!host.Instantiate(hash, scope))
            throw new InvalidOperationException($"Instantiation failed: {host.LastError}");
        return host;
    }
}
