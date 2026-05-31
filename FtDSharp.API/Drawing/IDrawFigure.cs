namespace FtDSharp
{
    public interface IDrawFigure
    {
        bool UpdateExpiration(bool gameAdvanced);
        void DrawFigure();
    }
}