using System;
using FtDSharp.Tests.Helpers;
using FtDSharp.Tests.Mocks;
using Microsoft.CodeAnalysis;
using UnityEngine;
using Xunit;

namespace FtDSharp.Tests;

public class IntegrationTests
{
    [Fact]
    public void Script_CompilesAndExecutes_FullLifecycle()
    {
        const string code = """
            public class TestScript
            {
                private int _ticks;

                [OnStart]
                public void Start()
                {
                    Log("start");
                }

                [OnPhysicsTick]
                public void Tick()
                {
                    _ticks++;
                    Log($"tick:{_ticks}");
                }

                [OnStop]
                public void Stop()
                {
                    Log("stop");
                }
            }
            """;

        var scope = new TestProviderScope();
        (ScriptHost host, var hash, Diagnostic[] diagnostics) = Compile(code);

        Assert.True(host.Compile(code, hash).Success);
        Assert.Empty(diagnostics);
        Assert.True(host.Instantiate(hash, scope));
        Assert.True(host.Active);
        Assert.Equal(new[] { "start" }, scope.LogProvider.InfoMessages);

        host.Tick(scope);
        host.Tick(scope);
        host.Tick(scope);

        Assert.Equal(new[] { "start", "tick:1", "tick:2", "tick:3" }, scope.LogProvider.InfoMessages);

        host.Deactivate();

        Assert.False(host.Active);
        Assert.Equal(new[] { "start", "tick:1", "tick:2", "tick:3", "stop" }, scope.LogProvider.InfoMessages);
    }

    [Fact]
    public void Script_OnStart_RunsOnceAtInstantiation()
    {
        const string code = """
            public class TestScript
            {
                [OnStart]
                public void Start()
                {
                    Log("start");
                }

                [OnPhysicsTick]
                public void Tick()
                {
                    Log("tick");
                }
            }
            """;

        var scope = new TestProviderScope();
        ScriptHost host = ScriptTestHelper.CompileAndInstantiate(code, scope);

        Assert.Equal(new[] { "start" }, scope.LogProvider.InfoMessages);

        host.Tick(scope);
        host.Tick(scope);

        Assert.Equal(new[] { "start", "tick", "tick" }, scope.LogProvider.InfoMessages);
    }

    [Fact]
    public void Script_OnStop_CalledOnDeactivate()
    {
        const string code = """
            public class TestScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                }

                [OnStop]
                public void Stop()
                {
                    Log("stop");
                }
            }
            """;

        var scope = new TestProviderScope();
        ScriptHost host = ScriptTestHelper.CompileAndInstantiate(code, scope);

        host.Deactivate();

        Assert.Equal(new[] { "stop" }, scope.LogProvider.InfoMessages);
        Assert.False(host.Active);
    }

    [Fact]
    public void Script_CanAccessGameProperties()
    {
        const string code = """
            public class TestScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    Log($"GameTime={Game.GameTime}");
                }
            }
            """;

        var scope = new TestProviderScope();
        scope.GameProvider.GameTime = 123.25f;
        ScriptHost host = ScriptTestHelper.CompileAndInstantiate(code, scope);

        host.Tick(scope);

        Assert.Equal(new[] { "GameTime=123.25" }, scope.LogProvider.InfoMessages);
    }

    [Fact]
    public void Script_CanAccessAI()
    {
        const string code = """
            public class TestScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    Log($"Mainframes={AI.Mainframes.Count}");
                }
            }
            """;

        var scope = new TestProviderScope();
        scope.AIProvider.Mainframes =
        [
            new TestMainframe(),
            new TestMainframe(),
            new TestMainframe()
        ];
        ScriptHost host = ScriptTestHelper.CompileAndInstantiate(code, scope);

        host.Tick(scope);

        Assert.Equal(new[] { "Mainframes=3" }, scope.LogProvider.InfoMessages);
    }

    [Fact]
    public void Script_CanUsePropulsion()
    {
        const string code = """
            public class TestScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    Game.MainConstruct.Propulsion.Forwards = 0.75f;
                    Game.MainConstruct.Propulsion.Yaw = -0.25f;
                    Game.MainConstruct.Propulsion.MainDrive = 1f;
                }
            }
            """;

        var scope = new TestProviderScope();
        scope.GameProvider.MainConstruct = new TestMainConstruct(scope.PropulsionProvider.Propulsion);
        ScriptHost host = ScriptTestHelper.CompileAndInstantiate(code, scope);

        host.Tick(scope);

        MockPropulsion propulsion = Assert.IsType<MockPropulsion>(scope.PropulsionProvider.Propulsion);
        Assert.Equal(0.75f, propulsion.Forwards);
        Assert.Equal(-0.25f, propulsion.Yaw);
        Assert.Equal(1f, propulsion.MainDrive);
    }

    [Fact]
    public void TwoScripts_IndependentScopes_NoInterference()
    {
        const string codeA = """
            public class TestScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    Log("A");
                }
            }
            """;

        const string codeB = """
            public class TestScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    Log("B");
                }
            }
            """;

        var scopeA = new TestProviderScope();
        var scopeB = new TestProviderScope();
        ScriptHost hostA = ScriptTestHelper.CompileAndInstantiate(codeA, scopeA);
        ScriptHost hostB = ScriptTestHelper.CompileAndInstantiate(codeB, scopeB);

        hostA.Tick(scopeA);
        hostB.Tick(scopeB);

        Assert.Equal(new[] { "A" }, scopeA.LogProvider.InfoMessages);
        Assert.Equal(new[] { "B" }, scopeB.LogProvider.InfoMessages);
        Assert.Empty(scopeA.LogProvider.ErrorMessages);
        Assert.Empty(scopeB.LogProvider.ErrorMessages);
    }

    [Fact]
    public void TwoScripts_DrawingClear_OnlyAffectsOwn()
    {
        const string drawScript = """
            public class DrawScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    Drawing.Line(Vector3.zero, Vector3.one, Color.white);
                }
            }
            """;

        const string clearScript = """
            public class ClearScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    Drawing.Clear();
                }
            }
            """;

        var scopeA = new TestProviderScope();
        var scopeB = new TestProviderScope();
        ScriptHost drawHostA = ScriptTestHelper.CompileAndInstantiate(drawScript, scopeA);
        ScriptHost drawHostB = ScriptTestHelper.CompileAndInstantiate(drawScript, scopeB);
        ScriptHost clearHostA = ScriptTestHelper.CompileAndInstantiate(clearScript, scopeA);

        drawHostA.Tick(scopeA);
        drawHostB.Tick(scopeB);

        Assert.Single(scopeA.DrawingProvider.Figures);
        Assert.Single(scopeB.DrawingProvider.Figures);

        clearHostA.Tick(scopeA);

        Assert.Empty(scopeA.DrawingProvider.Figures);
        Assert.Single(scopeB.DrawingProvider.Figures);
        Assert.Equal(1, scopeA.DrawingProvider.ClearCount);
        Assert.Equal(0, scopeB.DrawingProvider.ClearCount);
    }

    [Fact]
    public void Script_Deactivated_NoFurtherDrawingOccursOnTick()
    {
        const string code = """
            public class DrawScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    Drawing.Line(Vector3.zero, Vector3.one, Color.white);
                }
            }
            """;

        var scope = new TestProviderScope();
        ScriptHost host = ScriptTestHelper.CompileAndInstantiate(code, scope);

        host.Tick(scope);

        Assert.Single(scope.DrawingProvider.Figures);

        host.Deactivate();

        Assert.False(host.Active);

        host.Tick(scope);

        Assert.Single(scope.DrawingProvider.Figures);
    }

    [Fact]
    public void Script_ThrowsDuringTick_Deactivates()
    {
        const string code = """
            public class TestScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    throw new InvalidOperationException("tick failed");
                }
            }
            """;

        var scope = new TestProviderScope();
        ScriptHost host = ScriptTestHelper.CompileAndInstantiate(code, scope);

        host.Tick(scope);

        Assert.False(host.Active);
        Assert.Single(scope.LogProvider.ErrorMessages);
        Assert.Contains("Error during script execution: tick failed", scope.LogProvider.ErrorMessages[0]);
    }

    [Fact]
    public void Script_ThrowsDuringTick_OnStopStillCalled()
    {
        const string code = """
            public class TestScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    throw new InvalidOperationException("tick failed");
                }

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

        Assert.False(host.Active);
        Assert.Equal(new[] { "stop" }, scope.LogProvider.InfoMessages);
        Assert.Single(scope.LogProvider.ErrorMessages);
    }

    [Fact]
    public void Script_ThrowsInOnStop_DoesNotCrash()
    {
        const string code = """
            public class TestScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                }

                [OnStop]
                public void Stop()
                {
                    throw new InvalidOperationException("stop failed");
                }
            }
            """;

        var scope = new TestProviderScope();
        ScriptHost host = ScriptTestHelper.CompileAndInstantiate(code, scope);

        Exception exception = Record.Exception(host.Deactivate);

        Assert.Null(exception);
        Assert.False(host.Active);
    }

    [Fact]
    public void Script_Recompile_NewVersionRuns()
    {
        const string codeV1 = """
            public class TestScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    Log("v1");
                }
            }
            """;

        const string codeV2 = """
            public class TestScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    Log("v2");
                }
            }
            """;

        var scope = new TestProviderScope();
        var host = new ScriptHost();
        var hashV1 = ScriptHost.ComputeHash(codeV1);
        var hashV2 = ScriptHost.ComputeHash(codeV2);

        (bool Success, Diagnostic[] Diagnostics) compileV1 = host.Compile(codeV1, hashV1);

        Assert.True(compileV1.Success);
        Assert.Empty(compileV1.Diagnostics);
        Assert.True(host.Instantiate(hashV1, scope));

        host.Tick(scope);
        host.Deactivate();

        (bool Success, Diagnostic[] Diagnostics) compileV2 = host.Compile(codeV2, hashV2);

        Assert.True(compileV2.Success);
        Assert.Empty(compileV2.Diagnostics);
        Assert.True(host.Instantiate(hashV2, scope));

        host.Tick(scope);

        Assert.Equal(new[] { "v1", "v2" }, scope.LogProvider.InfoMessages);
    }

    private static (ScriptHost Host, string Hash, Diagnostic[] Diagnostics) Compile(string code)
    {
        var host = new ScriptHost();
        var hash = ScriptHost.ComputeHash(code);
        (var success, Diagnostic[] diagnostics) = host.Compile(code, hash);

        Assert.True(success, host.LastError);

        return (host, hash, diagnostics);
    }
}

internal sealed class TestMainConstruct : IMainConstruct
{
    public TestMainConstruct(IPropulsion propulsion)
    {
        Propulsion = propulsion;
    }

    public int UniqueId => 0;
    public string Name => "Test";
    public float Volume => 1f;
    public int AliveBlockCount => 1;
    public int BlockCount => 1;
    public float Stability => 1f;
    public Vector3 Position => Vector3.zero;
    public Vector3 Velocity => Vector3.zero;
    public Vector3 Acceleration => Vector3.zero;
    public Quaternion Rotation => Quaternion.identity;
    public Vector3 Forward => Vector3.forward;
    public float Yaw => 0f;
    public float Pitch => 0f;
    public float Roll => 0f;
    public IFleet Fleet { get; } = new TestFleet();
    public List<IMissile> Missiles { get; } = new();
    public IPropulsion Propulsion { get; }
    public IReadOnlyList<IMainframe> Mainframes => Array.Empty<IMainframe>();
    public IReadOnlyList<IWeapon> Weapons => Array.Empty<IWeapon>();
    public IReadOnlyList<ITurret> Turrets => Array.Empty<ITurret>();

    public bool TryGetBlockById(int id, out IBlock? block)
    {
        block = null;
        return false;
    }

    public List<T> GetAllBlocksOfType<T>() where T : IBlock => new();

    public IEnumerable<IBlock> GetAllBlocks() => Array.Empty<IBlock>();
}

internal sealed class TestFleet : IFleet
{
    public int Id => 0;
    public string Name => "TestFleet";
    public Vector3 Position => Vector3.zero;
    public Quaternion Rotation => Quaternion.identity;
    public IFriendlyConstruct Flagship => null!;
    public IReadOnlyList<IFriendlyConstruct> Members => Array.Empty<IFriendlyConstruct>();
}