using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FtDSharp.Facades;
using FtDSharp.Tests.Mocks;
using UnityEngine;
using Xunit;

namespace FtDSharp.Tests;

public class FacadeTests
{
    [Fact]
    public void Game_GameTime_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            scope.GameProvider.GameTime = 12.5f;

            using (ScriptContext.Push(scope))
            {
                Assert.Equal(12.5f, Game.GameTime);
                Assert.Equal(12.5f, Game.Time);
            }
        });
    }

    [Fact]
    public void Game_RealTime_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            scope.GameProvider.RealTime = 8.75f;

            using (ScriptContext.Push(scope))
            {
                Assert.Equal(8.75f, Game.RealTime);
            }
        });
    }

    [Fact]
    public void Game_MainConstruct_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            var expected = ScriptContextTestHelpers.CreateReference<MainConstructFacade, IMainConstruct>();
            scope.GameProvider.MainConstruct = expected;

            using (ScriptContext.Push(scope))
            {
                Assert.Same(expected, Game.MainConstruct);
            }
        });
    }

    [Fact]
    public void Game_DeltaTime_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            scope.GameProvider.GameDeltaTime = 0.025f;
            scope.GameProvider.RealDeltaTime = 0.05f;

            using (ScriptContext.Push(scope))
            {
                Assert.Equal(0.025f, Game.GameDeltaTime);
                Assert.Equal(0.05f, Game.RealDeltaTime);
            }
        });
    }

    [Fact]
    public void Game_TicksSinceStart_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            scope.GameProvider.TicksSinceStart = 12345;

            using (ScriptContext.Push(scope))
            {
                Assert.Equal(12345, Game.TicksSinceStart);
            }
        });
    }

    [Fact]
    public void Game_WithNoScope_ReturnsDefaults()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            // MainConstruct throws without scope (scripts always have scope at runtime)
            Assert.Throws<NullReferenceException>(() => _ = Game.MainConstruct);
            Assert.Equal(0f, Game.GameTime);
            Assert.Equal(0f, Game.RealTime);
            Assert.Equal(0f, Game.GameDeltaTime);
            Assert.Equal(0f, Game.RealDeltaTime);
            Assert.Equal(0L, Game.TicksSinceStart);
        });
    }

    [Fact]
    public void Log_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();

            using (ScriptContext.Push(scope))
            {
                Logging.Log("info");
            }

            Assert.Equal(new[] { "info" }, scope.LogProvider.InfoMessages);
        });
    }

    [Fact]
    public void LogWarning_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();

            using (ScriptContext.Push(scope))
            {
                Logging.LogWarning("warn");
            }

            Assert.Equal(new[] { "warn" }, scope.LogProvider.WarnMessages);
        });
    }

    [Fact]
    public void LogError_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();

            using (ScriptContext.Push(scope))
            {
                Logging.LogError("error");
            }

            Assert.Equal(new[] { "error" }, scope.LogProvider.ErrorMessages);
        });
    }

    [Fact]
    public void ClearLogs_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            scope.LogProvider.Info("existing");

            using (ScriptContext.Push(scope))
            {
                Logging.ClearLogs();
            }

            Assert.Equal(1, scope.LogProvider.ClearCount);
            Assert.Empty(scope.LogProvider.InfoMessages);
        });
    }

    [Fact]
    public void Logging_WithNoScope_DoesNotThrow()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var exception = Record.Exception(() =>
            {
                Logging.Log("info");
                Logging.LogWarning("warn");
                Logging.LogError("error");
                Logging.ClearLogs();
            });

            Assert.Null(exception);
        });
    }

    [Fact]
    public void AI_Mainframes_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            var expected = new IMainframe[] { new TestMainframe() };
            scope.AIProvider.Mainframes = expected;

            using (ScriptContext.Push(scope))
            {
                Assert.Same(expected, AI.Mainframes);
            }
        });
    }

    [Fact]
    public void AI_HighestPriorityMainframe_Throws_WhenEmpty()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            scope.AIProvider.Mainframes = Array.Empty<IMainframe>();

            using (ScriptContext.Push(scope))
            {
                Assert.Throws<InvalidOperationException>(() => AI.HighestPriorityMainframe);
            }
        });
    }

    [Fact]
    public void AI_HighestPriorityMainframe_ReturnsByPriority()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            var highestPriority = new TestMainframe(priority: 1);
            scope.AIProvider.Mainframes =
            [
                new TestMainframe(priority: 5),
                highestPriority,
                new TestMainframe(priority: 3)
            ];

            using (ScriptContext.Push(scope))
            {
                Assert.Same(highestPriority, AI.HighestPriorityMainframe);
            }
        });
    }

    [Fact]
    public void AI_WithNoScope_Throws()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            Assert.Throws<InvalidOperationException>(() => AI.HighestPriorityMainframe);
        });
    }

    [Fact]
    public void Drawing_Arrow_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            var start = new Vector3(1f, 2f, 3f);
            var end = new Vector3(4f, 5f, 6f);

            using (ScriptContext.Push(scope))
            {
                Drawing.Arrow(start, end, Color.red);
            }

            Assert.Single(scope.DrawingProvider.Figures);
            Assert.IsType<ArrowFigure>(scope.DrawingProvider.Figures[0]);
        });
    }

    [Fact]
    public void Drawing_Line_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();

            using (ScriptContext.Push(scope))
            {
                Drawing.Line(Vector3.zero, Vector3.one, Color.red, 1f, 0f, false);
            }

            Assert.Single(scope.DrawingProvider.Figures);
            Assert.IsType<LineFigure>(scope.DrawingProvider.Figures[0]);
        });
    }

    [Fact]
    public void Drawing_Point_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();

            using (ScriptContext.Push(scope))
            {
                Drawing.Point(Vector3.one, Color.red, 1f, 0f, false);
            }

            Assert.Single(scope.DrawingProvider.Figures);
            Assert.IsType<PointFigure>(scope.DrawingProvider.Figures[0]);
        });
    }

    [Fact]
    public void Drawing_Cross_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();

            using (ScriptContext.Push(scope))
            {
                Drawing.Cross(Vector3.one, Color.red, 1f, 2f, 0f, false);
            }

            Assert.Single(scope.DrawingProvider.Figures);
            Assert.IsType<CrossFigure>(scope.DrawingProvider.Figures[0]);
        });
    }

    [Fact]
    public void Drawing_Sphere_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();

            using (ScriptContext.Push(scope))
            {
                Drawing.Sphere(Vector3.one, 3f, Color.red, 1f, 0f, false);
            }

            Assert.Single(scope.DrawingProvider.Figures);
            Assert.IsType<SphereFigure>(scope.DrawingProvider.Figures[0]);
        });
    }

    [Fact]
    public void Drawing_Clear_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            scope.DrawingProvider.AddFigure(new LineFigure(Vector3.zero, Vector3.one, Color.white, 1f, 1f, false));

            using (ScriptContext.Push(scope))
            {
                Drawing.Clear();
            }

            Assert.Equal(1, scope.DrawingProvider.ClearCount);
            Assert.Empty(scope.DrawingProvider.Figures);
        });
    }

    [Fact]
    public void Drawing_WithNoScope_DoesNotThrow()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var exception = Record.Exception(() =>
            {
                Drawing.Arrow(Vector3.zero, Vector3.one, Color.green);
                Drawing.Clear();
            });

            Assert.Null(exception);
        });
    }

    [Fact]
    public void Weapons_All_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            var expected = new IWeapon[] { null! };
            scope.WeaponsProvider.Weapons = expected;

            using (ScriptContext.Push(scope))
            {
                Assert.Same(expected, Weapons.All);
            }
        });
    }

    [Fact]
    public void Weapons_Turrets_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            var expected = new ITurret[] { null! };
            scope.WeaponsProvider.Turrets = expected;

            using (ScriptContext.Push(scope))
            {
                Assert.Same(expected, Weapons.Turrets);
            }
        });
    }

    [Fact]
    public void Weapons_WithNoScope_ReturnsEmpty()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            Assert.Empty(Weapons.All);
            Assert.Empty(Weapons.Turrets);
        });
    }

    [Fact]
    public void Guidance_Missiles_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            var expected = new List<IMissile> { null! };
            scope.GuidanceProvider.Missiles = expected;

            using (ScriptContext.Push(scope))
            {
                Assert.Same(expected, Guidance.Missiles);
            }
        });
    }

    [Fact]
    public void Guidance_WithNoScope_ReturnsEmptyList()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var missiles = Guidance.Missiles;

            Assert.NotNull(missiles);
            Assert.Empty(missiles);
        });
    }

    [Fact]
    public void Warnings_IncomingProjectiles_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            var expected = new IProjectileWarning[] { new TestProjectileWarning { Type = ProjectileType.Missile } };
            scope.WarningsProvider.IncomingProjectiles = expected;

            using (ScriptContext.Push(scope))
            {
                Assert.Same(expected, Warnings.IncomingProjectiles);
            }
        });
    }

    [Fact]
    public void Warnings_WithNoScope_ReturnsEmpty()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            Assert.Empty(Warnings.IncomingProjectiles);
            Assert.Empty(Warnings.IncomingMissiles);
            Assert.Empty(Warnings.IncomingShells);
        });
    }

    [Fact]
    public void Friendly_All_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            var expected = new IFriendlyConstruct[]
            {
                ScriptContextTestHelpers.CreateReference<FriendlyConstructFacade, IFriendlyConstruct>()
            };
            scope.FleetProvider.All = expected;

            using (ScriptContext.Push(scope))
            {
                Assert.Same(expected, Friendly.All);
            }
        });
    }

    [Fact]
    public void Friendly_MyFleet_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            var expected = ScriptContextTestHelpers.CreateReference<FleetFacade, IFleet>();
            scope.FleetProvider.MyFleet = expected;

            using (ScriptContext.Push(scope))
            {
                Assert.Same(expected, Friendly.MyFleet);
            }
        });
    }

    [Fact]
    public void Friendly_WithNoScope_ReturnsEmpty()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            Assert.Empty(Friendly.All);
            Assert.Empty(Friendly.AllExcludingSelf);
            Assert.Empty(Friendly.Fleets);
            // MyFleet throws without scope (scripts always have scope at runtime)
            Assert.Throws<NullReferenceException>(() => _ = Friendly.MyFleet);
        });
    }

    [Fact]
    public void Warnings_IncomingMissiles_FiltersCorrectly()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            scope.GameProvider.TicksSinceStart = 1000;
            scope.WarningsProvider.IncomingProjectiles = new IProjectileWarning[]
            {
                new TestProjectileWarning { Type = ProjectileType.Missile },
                new TestProjectileWarning { Type = ProjectileType.Shell },
                new TestProjectileWarning { Type = ProjectileType.Missile },
            };

            using (ScriptContext.Push(scope))
            {
                Assert.Equal(2, Warnings.IncomingMissiles.Count);
                Assert.All(Warnings.IncomingMissiles, w => Assert.Equal(ProjectileType.Missile, w.Type));
            }
        });
    }

    [Fact]
    public void Warnings_IncomingShells_FiltersCorrectly()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();
            scope.GameProvider.TicksSinceStart = 2000;
            scope.WarningsProvider.IncomingProjectiles = new IProjectileWarning[]
            {
                new TestProjectileWarning { Type = ProjectileType.Missile },
                new TestProjectileWarning { Type = ProjectileType.Shell },
                new TestProjectileWarning { Type = ProjectileType.Cram },
            };

            using (ScriptContext.Push(scope))
            {
                Assert.Equal(2, Warnings.IncomingShells.Count);
                Assert.All(Warnings.IncomingShells, w =>
                    Assert.True(w.Type == ProjectileType.Shell || w.Type == ProjectileType.Cram));
            }
        });
    }

    [Fact]
    public void Weapons_CreateController_DelegatesToProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();

            using (ScriptContext.Push(scope))
            {
                var controller = Weapons.CreateController(Array.Empty<IWeapon>());

                Assert.NotNull(controller);
                Assert.Single(scope.WeaponsProvider.CreateControllerCalls);
            }
        });
    }
}

internal static class ScriptContextTestHelpers
{
    public static void InIsolatedContext(Action assertion)
    {
        var previous = ScriptContext.Current;
        ScriptContext.Current = null;

        try
        {
            assertion();
        }
        finally
        {
            ScriptContext.Current = previous;
        }
    }

    public static TInterface CreateReference<TConcrete, TInterface>()
        where TConcrete : class
        where TInterface : class
    {
        return (TInterface)RuntimeHelpers.GetUninitializedObject(typeof(TConcrete));
    }
}

internal sealed class TestMainframe : IMainframe
{
    public TestMainframe(int priority = 0)
    {
        Block = new TestAIMainframeBlock(priority);
    }

    public IAIMainframe Block { get; }
    public ITarget? PrimaryTarget => null;
    public IReadOnlyList<ITarget> Targets => Array.Empty<ITarget>();
    public Vector3 GetAimpoint(ITarget target) => Vector3.zero;
    public void SetPrimaryTarget(ITarget? target)
    {
    }
}

internal sealed class TestAIMainframeBlock : IAIMainframe
{
    public TestAIMainframeBlock(int priority)
    {
        Priority = priority;
    }

    public float AIMainframeTotalGPP => 0f;
    public float AIMainframeTotalGPPFree => 0f;
    public float AIMainframeTotalGPPNeeded => 0f;
    public bool AttackSalvage { get; set; }
    public float BearingWeighting { get; set; }
    public BrilliantSkies.Ai.FiringType Firing { get; set; }
    public float MinimumDetectionPoints { get; set; }
    public TrackerAssignmentMode Mode => default;
    public BrilliantSkies.Ai.MovementType Movement { get; set; }
    public int Priority { get; }
    public float RangeWeighting { get; set; }
    public uint SelectedBehaviourId => 0u;
    public uint SelectedManoeuvreId => 0u;
    public float SpeedWeighting { get; set; }
    public IFriendlyConstruct ParentConstruct => null!;
    public IBlock? Parent => null;
    public bool IsOnRoot => true;
    public int UniqueId => 0;
    public string? CustomName => null;
    public string BlockTypeName => "AIMainframe";
    public Vector3 LocalPosition => Vector3.zero;
    public Vector3 WorldPosition => Vector3.zero;
    public Vector3 LocalForward => Vector3.forward;
    public Vector3 LocalUp => Vector3.up;
    public Quaternion LocalRotation => Quaternion.identity;
    public Quaternion WorldRotation => Quaternion.identity;
    public float CurrentHealth => 0f;
    public float MaximumHealth => 0f;
    public bool IsAlive => true;
    public int SubobjectDepth => 0;

    public bool Equals(IBlock? other) => ReferenceEquals(this, other);
}

internal sealed class TestProjectileWarning : IProjectileWarning
{
    public ProjectileType Type { get; init; }
    public float Diameter => 1f;
    public float TimeSinceFiring => 0f;
    public float TimeSinceLastSpotted => 0f;
    public bool IsFake => false;
    public int ShotsFiredAt => 0;
    public int CiwsAimingAt => 0;
    public Vector3 Position => Vector3.zero;
    public Vector3 Velocity => Vector3.zero;
    public Vector3 Acceleration => Vector3.zero;
}