using System;
using System.Collections.Generic;
using System.Linq;

namespace FtDSharp.Tests.Mocks;

public class MockBlocksProvider : IBlocksProvider
{
    public IReadOnlyList<IBlock> All { get; set; } = Array.Empty<IBlock>();

    public IEnumerable<T> OfType<T>() where T : IBlock => All.OfType<T>();

    public IBlock? ById(int uniqueId) => All.FirstOrDefault(b => b.UniqueId == uniqueId);

    public T? ById<T>(int uniqueId) where T : class, IBlock => All.OfType<T>().FirstOrDefault(b => b.UniqueId == uniqueId);

    public void InvalidateCache() { }
}
