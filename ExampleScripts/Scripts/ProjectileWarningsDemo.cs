/// <summary>
/// Demonstrates the Projectile Warnings API by visualizing incoming threats.
/// </summary>
public class ProjectileWarningsDemo
{
    [OnPhysicsTick]
    public void Update()
    {
        var deltaTime = Game.GameDeltaTime;
        foreach (IProjectileWarning warning in Warnings.IncomingProjectiles)
        {
            Color color = warning.Type switch
            {
                ProjectileType.Missile => Color.red,
                ProjectileType.Cram => Color.yellow,
                ProjectileType.Shell => Color.cyan,
                _ => Color.gray
            };

            // account for 1 frame delay for drawing
            Vector3 warningDisplayPosition = warning.Position + (warning.Velocity * deltaTime);

            Drawing.Sphere(warningDisplayPosition, radius: 2f, color);

            Vector3 velocityEnd = warningDisplayPosition + (warning.Velocity * deltaTime);
            Drawing.Arrow(warningDisplayPosition, velocityEnd, color, width: 1f);

            if (warning.Acceleration.magnitude > 1f)
            {
                Vector3 accelEnd = warningDisplayPosition + (warning.Acceleration * deltaTime);
                Drawing.Arrow(warningDisplayPosition, accelEnd, Color.green, width: 0.5f);
            }
        }
    }
}
