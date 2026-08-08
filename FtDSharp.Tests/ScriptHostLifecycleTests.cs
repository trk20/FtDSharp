using FtDSharp.Tests.Helpers;
using FtDSharp.Tests.Mocks;
using Xunit;

namespace FtDSharp.Tests;

public class ScriptHostLifecycleTests
{
    private const string EntryPointValidationError = "must be a public instance parameterless void method.";

    [Fact]
    public void Instantiate_BeforeCompile_ReturnsFalseWithError()
    {
        const string code = """
            public class TestScript
            {
                [OnPhysicsTick]
                public void Tick() { }
            }
            """;

        var host = new ScriptHost();
        var hash = ScriptHost.ComputeHash(code);

        var instantiated = host.Instantiate(hash, new TestProviderScope());

        Assert.False(instantiated);
        Assert.False(host.Active);
        Assert.Equal("No compiled assembly found for the given hash. Compile first.", host.LastError);
    }

    [Fact]
    public void Instantiate_WhenConstructorThrows_ReturnsFalseAndLeavesHostInactive()
    {
        const string code = """
            public class TestScript
            {
                public TestScript()
                {
                    throw new InvalidOperationException("constructor failed");
                }

                [OnPhysicsTick]
                public void Tick() { }
            }
            """;

        ScriptHost host = ScriptTestHelper.Compile(code);
        var hash = ScriptHost.ComputeHash(code);

        var instantiated = host.Instantiate(hash, new TestProviderScope());

        Assert.False(instantiated);
        Assert.False(host.Active);
        Assert.Contains("constructor failed", host.LastError);
        Assert.Null(host.CurrentHash);
    }

    [Fact]
    public void Instantiate_WhenOnStartThrows_ReturnsFalseAndCleansUpState()
    {
        const string code = """
            public class TestScript
            {
                [OnStart]
                public void Start()
                {
                    throw new InvalidOperationException("start failed");
                }

                [OnPhysicsTick]
                public void Tick()
                {
                    Log("tick");
                }
            }
            """;

        var scope = new TestProviderScope();
        ScriptHost host = ScriptTestHelper.Compile(code);
        var hash = ScriptHost.ComputeHash(code);

        var instantiated = host.Instantiate(hash, scope);

        Assert.False(instantiated);
        Assert.False(host.Active);
        Assert.Contains("start failed", host.LastError);
        Assert.Null(host.CurrentHash);

        host.Tick(scope);

        Assert.Empty(scope.LogProvider.InfoMessages);
    }

    [Fact]
    public void Deactivate_CanBeCalledRepeatedly()
    {
        const string code = """
            public class TestScript
            {
                [OnPhysicsTick]
                public void Tick() { }
            }
            """;

        var scope = new TestProviderScope();
        ScriptHost host = ScriptTestHelper.CompileAndInstantiate(code, scope);

        Exception firstException = Record.Exception(host.Deactivate);
        Exception secondException = Record.Exception(host.Deactivate);

        Assert.Null(firstException);
        Assert.Null(secondException);
        Assert.False(host.Active);
    }

    [Fact]
    public void Tick_AfterDeactivate_IsNoOp()
    {
        const string code = """
            public class TestScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    Log("tick");
                }
            }
            """;

        var scope = new TestProviderScope();
        ScriptHost host = ScriptTestHelper.CompileAndInstantiate(code, scope);

        host.Tick(scope);
        host.Deactivate();
        host.Tick(scope);

        Assert.Equal(new[] { "tick" }, scope.LogProvider.InfoMessages);
        Assert.False(host.Active);
    }

    [Fact]
    public void Deactivate_DisposesDisposableScript()
    {
        const string code = """
            public class TestScript : IDisposable
            {
                public static bool Disposed { get; private set; }

                [OnPhysicsTick]
                public void Tick() { }

                public void Dispose()
                {
                    Disposed = true;
                }
            }
            """;

        var scope = new TestProviderScope();
        ScriptHost host = ScriptTestHelper.CompileAndInstantiate(code, scope);
        var hash = ScriptHost.ComputeHash(code);

        host.Deactivate();

        Assert.True(ScriptCompilationCache.TryGet(hash, out System.Reflection.Assembly? assembly));
        Type scriptType = Assert.Single(assembly!.GetTypes(), type => type.Name == "TestScript");
        System.Reflection.PropertyInfo? disposedProperty = scriptType.GetProperty("Disposed");

        Assert.NotNull(disposedProperty);
        Assert.Equal(true, disposedProperty!.GetValue(null));
        Assert.False(host.Active);
    }

    [Fact]
    public void OnStopOnlyScript_InstantiatesAndRunsStopOnDeactivate()
    {
        const string code = """
            public class TestScript
            {
                [OnStop]
                public void Stop()
                {
                    Log("stop");
                }
            }
            """;

        var scope = new TestProviderScope();
        ScriptHost host = ScriptTestHelper.CompileAndInstantiate(code, scope);

        host.Tick(scope);
        host.Deactivate();

        Assert.Equal(new[] { "stop" }, scope.LogProvider.InfoMessages);
        Assert.False(host.Active);
    }

    [Fact]
    public void OnStartOnlyScript_InstantiatesAndTickIsNoOp()
    {
        const string code = """
            public class TestScript
            {
                [OnStart]
                public void Start()
                {
                    Log("start");
                }
            }
            """;

        var scope = new TestProviderScope();
        ScriptHost host = ScriptTestHelper.CompileAndInstantiate(code, scope);

        host.Tick(scope);

        Assert.Equal(new[] { "start" }, scope.LogProvider.InfoMessages);
        Assert.True(host.Active);
    }

    [Fact]
    public void StaticEntryPointMethod_IsRejected()
    {
        const string code = """
            public class TestScript
            {
                [OnPhysicsTick]
                public static void Tick()
                {
                }
            }
            """;

        ScriptHost host = ScriptTestHelper.Compile(code);
        var hash = ScriptHost.ComputeHash(code);

        var instantiated = host.Instantiate(hash, new TestProviderScope());

        Assert.False(instantiated);
        Assert.False(host.Active);
        Assert.Contains(EntryPointValidationError, host.LastError);
    }
}