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

        private readonly Dictionary<object, List<IDrawFigure>> _buckets = new Dictionary<object, List<IDrawFigure>>();
        private readonly object _lock = new();
        private ulong _lastGameFrame;

        private DrawingService()
        {
            GameEvents.PreUpdateEvent.RegWithEvent(DrawFigures);
            GameEvents.FixedUpdateEvent.RegWithEvent(UpdateFigureLifetimes);
        }

        public void AddFigure(object owner, IDrawFigure figure)
        {
            lock (_lock)
            {
                if (!_buckets.TryGetValue(owner, out List<IDrawFigure>? figures))
                {
                    figures = new List<IDrawFigure>();
                    _buckets[owner] = figures;
                }

                figures.Add(figure);
            }
        }

        public void Clear(object owner)
        {
            lock (_lock)
            {
                if (_buckets.TryGetValue(owner, out List<IDrawFigure>? figures))
                {
                    figures.Clear();
                }
            }
        }

        public void RemoveOwner(object owner)
        {
            lock (_lock)
            {
                _buckets.Remove(owner);
            }
        }

        private void DrawFigures(ITimeStep dt)
        {
            lock (_lock)
            {
                foreach (List<IDrawFigure> figures in _buckets.Values)
                {
                    foreach (IDrawFigure figure in figures)
                    {
                        figure.DrawFigure();
                    }
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
                foreach (List<IDrawFigure> figures in _buckets.Values)
                {
                    for (int i = figures.Count - 1; i >= 0; i--)
                    {
                        if (figures[i].UpdateExpiration(gameAdvanced))
                        {
                            figures.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}
