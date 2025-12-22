using FtDSharp;
using UnityEngine;
using static FtDSharp.Drawing;

/// <summary>
/// Demonstrates the Drawing API by visualizing target information.
/// </summary>
public class DrawingDemo : IFtDSharp
{
    private const float LoadingRadius = 15f;
    private float AnimationAngle;

    public void Update(float deltaTime)
    {
        var self = Game.MainConstruct;
        var mainframe = AI.HighestPriorityMainframe;

        Arrow(self.Position, self.Position + self.Forward * 50f, Color.blue, width: 2f);

        Gimbal(self.Position, radius: 10f, self.Rotation, width: 1f);

        AnimationAngle += deltaTime * 180f;
        var offset = Quaternion.Euler(0, AnimationAngle, 0) * Vector3.forward * LoadingRadius;
        Point(self.Position + Vector3.up * 30f + offset, Color.cyan, size: 10f, duration: 30f, fade: true);

        if (mainframe == null) return;

        foreach (var target in mainframe.Targets)
        {
            var aimpoint = mainframe.GetAimpoint(target);

            Line(self.Position, target.Position, Color.yellow);

            Sphere(target.Position, radius: 20f, Color.red, width: 1.5f);

            Cross(aimpoint, Color.green, width: 2f);

            Arrow(target.Position, target.Position + target.Velocity, Color.cyan);
        }
    }
}
