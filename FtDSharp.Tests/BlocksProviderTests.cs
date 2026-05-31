using FtDSharp.Tests.Mocks;
using Xunit;

namespace FtDSharp.Tests;

public class BlocksProviderTests
{
    [Fact]
    public void Blocks_ScopeWiring_ExposesMockBlocksProvider()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();

            using (ScriptContext.Push(scope))
            {
                Assert.Same(scope.BlocksProvider, ScriptContext.Current!.Blocks);
            }
        });
    }

    [Fact]
    public void Blocks_WithEmptyProvider_ReturnsEmptyAll()
    {
        ScriptContextTestHelpers.InIsolatedContext(() =>
        {
            var scope = new TestProviderScope();

            using (ScriptContext.Push(scope))
            {
                Assert.Empty(Blocks.All);
            }
        });
    }
}
