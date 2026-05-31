using FtDSharp.Tests.Helpers;
using FtDSharp.Tests.Mocks;
using Xunit;

namespace FtDSharp.Tests;

public class ConstructSwitchTests
{
    [Fact]
    public void Script_Reattach_ScopeChangesAreReflected()
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

        var firstScope = new TestProviderScope();
        firstScope.GameProvider.GameTime = 1.0f;

        var secondScope = new TestProviderScope();
        secondScope.GameProvider.GameTime = 2.0f;

        var host = ScriptTestHelper.CompileAndInstantiate(code, firstScope);

        host.Tick(firstScope);
        host.Tick(secondScope);

        Assert.Equal(["GameTime=1"], firstScope.LogProvider.InfoMessages);
        Assert.Equal(["GameTime=2"], secondScope.LogProvider.InfoMessages);
    }
}