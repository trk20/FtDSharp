using BrilliantSkies.Core.Drawing;
using UnityEngine;

namespace FtDSharp
{
    /// <summary>Base class for drawable figures with duration-based lifetime.</summary>
    internal abstract class Figure
    {
        private readonly float _fadePerFrame;
        private float _framesRemaining;
        protected Vector3 Position;
        protected Color Color;
        protected float Width;

        protected Figure(Vector3 position, Color color, float width, float durationFrames, bool fade)
        {
            Position = position;
            Color = color;
            Width = width;
            _framesRemaining = Mathf.Max(1, durationFrames);
            _fadePerFrame = fade && durationFrames > 0 ? color.a / durationFrames : 0f;
        }

        /// <summary>Update expiration state and returns true if figure has expired.</summary>
        public bool UpdateExpiration(bool gameAdvanced)
        {
            if (_framesRemaining < 0) return true;

            if (gameAdvanced)
            {
                _framesRemaining--;
                if (_fadePerFrame > 0f)
                    Color.a = Mathf.Max(0f, Color.a - _fadePerFrame);
            }

            return _framesRemaining < 0;
        }

        public abstract void DrawFigure();

        protected void DrawArrowLine(Vector3 start, Vector3 end)
        {
            VectorLines.i.Current.Line(start, end, Color, Width);

            var direction = (end - start).normalized;
            var arrowSize = (end - start).magnitude / 20f;

            VectorLines.i.Current.Line(end - Quaternion.Euler(0f, 45f, 0f) * direction * arrowSize, end, Color, Width);
            VectorLines.i.Current.Line(end - Quaternion.Euler(0f, -45f, 0f) * direction * arrowSize, end, Color, Width);
        }
    }

    internal sealed class ArrowFigure : Figure
    {
        private readonly Vector3 _end;

        public ArrowFigure(Vector3 start, Vector3 end, Color color, float width, float duration, bool fade)
            : base(start, color, width, duration, fade)
        {
            _end = end;
        }

        public override void DrawFigure() => DrawArrowLine(Position, _end);
    }

    internal sealed class LineFigure : Figure
    {
        private readonly Vector3 _end;

        public LineFigure(Vector3 start, Vector3 end, Color color, float width, float duration, bool fade)
            : base(start, color, width, duration, fade)
        {
            _end = end;
        }

        public override void DrawFigure() => VectorLines.i.Current.Line(Position, _end, Color, Width);
    }

    internal sealed class PointFigure : Figure
    {
        private readonly float _size;

        public PointFigure(Vector3 position, Color color, float size, float duration, bool fade)
            : base(position, color, size, duration, fade)
        {
            _size = size;
        }

        // Draws a point marker as a small cross that scales with distance because 0 length lines are invisible
        public override void DrawFigure()
        {
            var camera = Camera.current ?? Camera.main;
            if (camera == null) return;

            var distance = Vector3.Distance(camera.transform.position, Position);
            var scaledSize = _size * (distance / 100f); // scaling attempt
            var halfSize = scaledSize * 0.05f;

            var camRight = camera.transform.right;
            var camUp = camera.transform.up;
            var diagonal1 = (camRight + camUp).normalized;
            var diagonal2 = (camRight - camUp).normalized;

            VectorLines.i.Current.Line(Position - diagonal1 * halfSize, Position + diagonal1 * halfSize, Color, Width);
            VectorLines.i.Current.Line(Position - diagonal2 * halfSize, Position + diagonal2 * halfSize, Color, Width);
        }
    }

    internal sealed class CrossFigure : Figure
    {
        private readonly float _scale;

        public CrossFigure(Vector3 position, Color color, float size, float scale, float duration, bool fade)
            : base(position, color, size, duration, fade)
        {
            _scale = scale;
        }

        public override void DrawFigure()
        {
            var offset = Width * _scale;
            VectorLines.i.Current.Line(Position - Vector3.forward * offset, Position + Vector3.forward * offset, Color, Width);
            VectorLines.i.Current.Line(Position - Vector3.up * offset, Position + Vector3.up * offset, Color, Width);
            VectorLines.i.Current.Line(Position - Vector3.right * offset, Position + Vector3.right * offset, Color, Width);
        }
    }

    internal sealed class SphereFigure : Figure
    {
        private readonly float _radius;

        public SphereFigure(Vector3 position, float radius, Color color, float width, float duration, bool fade)
            : base(position, color, width, duration, fade)
        {
            _radius = radius;
        }

        public override void DrawFigure()
        {
            VectorLines.i.Current.Circle(Position, _radius, Color, Vector3.forward, Width);
            VectorLines.i.Current.Circle(Position, _radius, Color, Vector3.right, Width);
            VectorLines.i.Current.Circle(Position, _radius, Color, Vector3.up, Width);
        }
    }

    internal sealed class CircleFigure : Figure
    {
        private readonly float _radius;
        private readonly Quaternion _rotation;

        public CircleFigure(Vector3 position, float radius, Color color, Quaternion rotation, float width, float duration, bool fade)
            : base(position, color, width, duration, fade)
        {
            _radius = radius;
            _rotation = rotation;
        }

        public override void DrawFigure() =>
            VectorLines.i.Current.Circle(Position, _radius, Color, _rotation * Vector3.forward, Width);
    }

    internal sealed class GimbalFigure : Figure
    {
        private readonly float _radius;
        private readonly Quaternion _rotation;

        public GimbalFigure(Vector3 position, float radius, Quaternion rotation, float width, float duration)
            : base(position, Color.white, width, duration, false)
        {
            _radius = radius;
            _rotation = rotation;
        }

        public override void DrawFigure()
        {
            VectorLines.i.Current.Circle(Position, _radius, Color.blue, _rotation * Vector3.forward, Width);
            VectorLines.i.Current.Circle(Position, _radius, Color.red, _rotation * Vector3.right, Width);
            VectorLines.i.Current.Circle(Position, _radius, Color.green, _rotation * Vector3.up, Width);
        }
    }
}

