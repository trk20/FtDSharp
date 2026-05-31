namespace FtDSharp
{
    public interface IProviderScope : System.IDisposable
    {
        IGameProvider Game { get; }
        ILogProvider Log { get; }
        IDrawingProvider Drawing { get; }
        IAIProvider AI { get; }
        IWeaponsProvider Weapons { get; }
        IGuidanceProvider Guidance { get; }
        IWarningsProvider Warnings { get; }
        IFleetProvider Fleet { get; }
        IBlocksProvider Blocks { get; }
        IPropulsionProvider Propulsion { get; }
    }
}
