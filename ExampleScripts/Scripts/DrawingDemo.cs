/// <summary>
/// Demonstrates the Drawing API by visualizing target information.
/// </summary>
public class DrawingDemo
{
    private const float _loadingRadius = 15f;
    private float _animationAngle;

    [OnPhysicsTick]
    public void Update()
    {
        IMainConstruct self = Game.MainConstruct;
        IMainframe mainframe = AI.HighestPriorityMainframe;

        Drawing.Arrow(self.Position, self.Position + (self.Forward * 50f), Color.blue, width: 2f);

        Drawing.Gimbal(self.Position, radius: 10f, self.Rotation, width: 1f);

        _animationAngle += Game.GameDeltaTime * 180f;
        Vector3 offset = Quaternion.Euler(0, _animationAngle, 0) * Vector3.forward * _loadingRadius;
        Drawing.Point(self.Position + (Vector3.up * 30f) + offset, Color.cyan, size: 10f, duration: 30f, fade: true);

        if (mainframe == null) return;

        foreach (ITarget target in mainframe.Targets)
        {
            Vector3 aimpoint = mainframe.GetAimpoint(target);

            Drawing.Line(self.Position, target.Position, Color.yellow);

            Drawing.Sphere(target.Position, radius: 20f, Color.red, width: 1.5f);

            Drawing.Cross(aimpoint, Color.green, width: 2f);

            Drawing.Arrow(target.Position, target.Position + target.Velocity, Color.cyan);
        }
    }
}
