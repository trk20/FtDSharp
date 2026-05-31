/// <summary>
/// Demonstrates the Drawing API by visualizing target information.
/// </summary>
public class DrawingDemo
{
    private const float LoadingRadius = 15f;
    private float AnimationAngle;

    [OnPhysicsTick]
    public void Update()
    {
        var self = Game.MainConstruct;
        var mainframe = AI.HighestPriorityMainframe;

        Drawing.Arrow(self.Position, self.Position + self.Forward * 50f, Color.blue, width: 2f);

        Drawing.Gimbal(self.Position, radius: 10f, self.Rotation, width: 1f);

        AnimationAngle += Game.GameDeltaTime * 180f;
        var offset = Quaternion.Euler(0, AnimationAngle, 0) * Vector3.forward * LoadingRadius;
        Drawing.Point(self.Position + Vector3.up * 30f + offset, Color.cyan, size: 10f, duration: 30f, fade: true);

        if (mainframe == null) return;

        foreach (var target in mainframe.Targets)
        {
            var aimpoint = mainframe.GetAimpoint(target);

            Drawing.Line(self.Position, target.Position, Color.yellow);

            Drawing.Sphere(target.Position, radius: 20f, Color.red, width: 1.5f);

            Drawing.Cross(aimpoint, Color.green, width: 2f);

            Drawing.Arrow(target.Position, target.Position + target.Velocity, Color.cyan);
        }
    }
}
