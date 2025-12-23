using FtDSharp;
using UnityEngine;
using static FtDSharp.Warnings;
using static FtDSharp.Drawing;

/// <summary>
/// Demonstrates the Projectile Warnings API by visualizing incoming threats.
/// </summary>
public class ProjectileWarningsDemo : IFtDSharp
{
    public void Update(float deltaTime)
    {
        foreach (var warning in IncomingProjectiles)
        {
            var color = warning.Type switch
            {
                ProjectileType.Missile => Color.red,
                ProjectileType.Cram => Color.yellow,
                ProjectileType.Shell => Color.cyan,
                _ => Color.gray
            };

            // account for 1 frame delay for drawing
            var warningDisplayPosition = warning.Position + warning.Velocity * deltaTime;

            Sphere(warningDisplayPosition, radius: 2f, color);

            var velocityEnd = warningDisplayPosition + warning.Velocity * deltaTime;
            Arrow(warningDisplayPosition, velocityEnd, color, width: 1f);

            if (warning.Acceleration.magnitude > 1f)
            {
                var accelEnd = warningDisplayPosition + warning.Acceleration * deltaTime;
                Arrow(warningDisplayPosition, accelEnd, Color.green, width: 0.5f);
            }
        }
    }
}
