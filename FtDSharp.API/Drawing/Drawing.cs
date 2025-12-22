using UnityEngine;

namespace FtDSharp
{
    /// <summary>
    /// Provides 3D drawing capabilities for scripts.
    /// Shapes are drawn in world space and persist for a specified duration.
    /// Duration of 0 or 1 = persist for 1 frame. Duration > 1 = persist for that many game frames (at 40fps).
    /// </summary>
    public static class Drawing
    {
        private const float DefaultWidth = 2f;
        private const float DefaultDuration = 1f;

        /// <summary>Draws an arrow from start to end position.</summary>
        public static void Arrow(Vector3 start, Vector3 end, Color color, float width = DefaultWidth, float duration = DefaultDuration, bool fade = false)
            => DrawingService.Instance.AddFigure(new ArrowFigure(start, end, color, width, duration, fade));

        /// <summary>Draws a line from start to end position.</summary>
        public static void Line(Vector3 start, Vector3 end, Color color, float width = DefaultWidth, float duration = DefaultDuration, bool fade = false)
            => DrawingService.Instance.AddFigure(new LineFigure(start, end, color, width, duration, fade));

        /// <summary>Draws a point marker at the specified position. (Note: can be a bit weird with zoom)</summary>
        public static void Point(Vector3 position, Color color, float size = DefaultWidth, float duration = DefaultDuration, bool fade = false)
            => DrawingService.Instance.AddFigure(new PointFigure(position, color, size, duration, fade));

        /// <summary>Draws a 3D cross at the specified position.</summary>
        public static void Cross(Vector3 position, Color color, float width = DefaultWidth, float scale = 1f, float duration = DefaultDuration, bool fade = false)
            => DrawingService.Instance.AddFigure(new CrossFigure(position, color, width, scale, duration, fade));

        /// <summary>Draws a sphere (3 perpendicular circles) at the specified position.</summary>
        public static void Sphere(Vector3 position, float radius, Color color, float width = DefaultWidth, float duration = DefaultDuration, bool fade = false)
            => DrawingService.Instance.AddFigure(new SphereFigure(position, radius, color, width, duration, fade));

        /// <summary>Draws a circle at the specified position with the given orientation.</summary>
        public static void Circle(Vector3 position, float radius, Color color, Vector3 normal, float width = DefaultWidth, float duration = DefaultDuration, bool fade = false)
            => DrawingService.Instance.AddFigure(new CircleFigure(position, radius, color, Quaternion.LookRotation(normal), width, duration, fade));

        /// <summary>Draws a circle at the specified position with the given rotation.</summary>
        public static void Circle(Vector3 position, float radius, Color color, Quaternion rotation, float width = DefaultWidth, float duration = DefaultDuration, bool fade = false)
            => DrawingService.Instance.AddFigure(new CircleFigure(position, radius, color, rotation, width, duration, fade));

        /// <summary>Draws a gimbal with 3 circles (one colored circle for each axis) at the specified position.</summary>
        public static void Gimbal(Vector3 position, float radius, Quaternion rotation, float width = DefaultWidth, float duration = DefaultDuration)
            => DrawingService.Instance.AddFigure(new GimbalFigure(position, radius, rotation, width, duration));

        /// <summary>Clears all currently drawn figures.</summary>
        public static void Clear() => DrawingService.Instance.Clear();
    }
}
