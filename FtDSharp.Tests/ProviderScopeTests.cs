using System;
using System.Threading.Tasks;
using FtDSharp.Tests.Mocks;
using Xunit;

namespace FtDSharp.Tests;

public class ProviderScopeTests
{
    [Fact]
    public void Push_SetsScope_Dispose_ClearsScope()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();

            IDisposable guard = ScriptContext.Push(scope);

            Assert.Same(scope, ScriptContext.Current);

            guard.Dispose();

            Assert.Null(ScriptContext.Current);
        });
    }

    [Fact]
    public void NestedPush_RestoresCorrectScope()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var outer = new TestProviderScope();
            var inner = new TestProviderScope();

            using (ScriptContext.Push(outer))
            {
                Assert.Same(outer, ScriptContext.Current);

                using (ScriptContext.Push(inner))
                {
                    Assert.Same(inner, ScriptContext.Current);
                }

                Assert.Same(outer, ScriptContext.Current);
            }

            Assert.Null(ScriptContext.Current);
        });
    }

    [Fact]
    public void Push_ExceptionInUsing_StillRestores()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var outer = new TestProviderScope();
            var inner = new TestProviderScope();

            using (ScriptContext.Push(outer))
            {
                InvalidOperationException? exception = null;

                try
                {
                    using (ScriptContext.Push(inner))
                    {
                        Assert.Same(inner, ScriptContext.Current);
                        throw new InvalidOperationException("boom");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    exception = ex;
                }

                Assert.NotNull(exception);
                Assert.Equal("boom", exception.Message);
                Assert.Same(outer, ScriptContext.Current);
            }

            Assert.Null(ScriptContext.Current);
        });
    }

    [Fact]
    public async Task Scope_IsIsolatedPerAsyncLocal()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
        });

        var outer = new TestProviderScope();
        var inner = new TestProviderScope();
        var innerScopeEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseInnerScope = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        await Task.Run(async () =>
        {
            ScriptContextTestHelpers.InIsolatedContext(() =>
            {
            });

            using (ScriptContext.Push(outer))
            {
                var innerTask = Task.Run(async () =>
                {
                    Assert.Same(outer, ScriptContext.Current);

                    using (ScriptContext.Push(inner))
                    {
                        innerScopeEntered.SetResult(true);
                        await releaseInnerScope.Task;
                        Assert.Same(inner, ScriptContext.Current);
                    }

                    Assert.Same(outer, ScriptContext.Current);
                });

                await innerScopeEntered.Task;
                Assert.Same(outer, ScriptContext.Current);

                releaseInnerScope.SetResult(true);
                await innerTask;
                Assert.Same(outer, ScriptContext.Current);
            }

            Assert.Null(ScriptContext.Current);
        });

        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            Assert.Null(ScriptContext.Current);
        });
    }

    [Fact]
    public void NullScope_FacadesReturnDefaults()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            using (ScriptContext.Push(new TestProviderScope()))
            {
                Assert.NotNull(ScriptContext.Current);
            }

            Assert.Null(ScriptContext.Current);
            Assert.Equal(0f, Game.GameTime);
            Assert.Empty(AI.Mainframes);
            Assert.Empty(Weapons.All);
            Assert.Empty(Weapons.Turrets);
            Assert.Empty(Guidance.Missiles);
            Assert.Empty(Warnings.IncomingProjectiles);
            Assert.Empty(Friendly.All);
            // MainConstruct, MyFleet, and HighestPriorityMainframe throw without scope (scripts always have scope at runtime)
            Assert.Throws<NullReferenceException>(() => _ = Game.MainConstruct);
            Assert.Throws<InvalidOperationException>(() => AI.HighestPriorityMainframe);
            Assert.Throws<NullReferenceException>(() => _ = Friendly.MyFleet);
        });
    }
}