namespace FtDSharp.Tests.Mocks;

public class MockDrawingProvider : IDrawingProvider
{
    public List<IDrawFigure> Figures { get; } = new();
    public int ClearCount { get; private set; }

    public void AddFigure(IDrawFigure figure) => Figures.Add(figure);

    public void Clear()
    {
        ClearCount++;
        Figures.Clear();
    }
}
