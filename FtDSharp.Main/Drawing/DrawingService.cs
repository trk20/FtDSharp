using System.Collections.Generic;
using BrilliantSkies.Core.Timing;

/* 
 Adapted from jalansia's (in AtsuLuaEditor) adaptation of CornHolio's utility (in BreadThing) for drawing debug figures

 If you're reading this and haven't yet, check out their work too :)

 AtsuLuaEditor: https://git.sr.ht/~alisa/FtD-AtsuLuaEditor https://steamcommunity.com/sharedfiles/filedetails/?id=3405611847

 BreadThing: https://github.com/CornHollioFTD/BreadThing https://steamcommunity.com/sharedfiles/filedetails/?id=3540650411

*/

namespace FtDSharp
{
    internal sealed class DrawingService
    {
        private static DrawingService? _instance;
        public static DrawingService Instance => _instance ??= new DrawingService();

        private readonly List<Figure> _figures = new List<Figure>();
        private readonly object _lock = new();
        private ulong _lastGameFrame;

        private DrawingService()
        {
            GameEvents.PreUpdateEvent.RegWithEvent(DrawFigures);
            GameEvents.FixedUpdateEvent.RegWithEvent(UpdateFigureLifetimes);
        }

        public void AddFigure(Figure figure)
        {
            lock (_lock)
            {
                _figures.Add(figure);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _figures.Clear();
            }
        }

        private void DrawFigures(ITimeStep dt)
        {
            lock (_lock)
            {
                foreach (var figure in _figures)
                {
                    figure.DrawFigure();
                }
            }
        }

        private void UpdateFigureLifetimes(ITimeStep dt)
        {
            var currentGameFrame = GameTimer.Instance.FrameCounter;
            var gameAdvanced = currentGameFrame != _lastGameFrame;
            _lastGameFrame = currentGameFrame;

            lock (_lock)
            {
                for (int i = _figures.Count - 1; i >= 0; i--)
                {
                    if (_figures[i].UpdateExpiration(gameAdvanced))
                    {
                        _figures.RemoveAt(i);
                    }
                }
            }
        }
    }
}
