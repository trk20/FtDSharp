using FtDSharp;
using UnityEngine;
using static FtDSharp.Logging;
using static FtDSharp.Guidance;
using static FtDSharp.AI;

/// <summary>
/// Demonstrates typed access to missile part parameters and visual control.
/// Shows how to read/modify missile component settings and control propulsion effects.
/// </summary>
public class MissilePartsDemo : IFtDSharp
{
    public void Update(float deltaTime)
    {
        ClearLogs();
        var target = HighestPriorityMainframe.PrimaryTarget;
        if (target == null)
        {
            Log("No target");
            return;
        }

        foreach (var missileController in Weapons.MissileControllers)
        {
            missileController.Fire();
        }
        foreach (var missile in Missiles)
        {
            missile.AimAt(target.Position);

            // Control propulsion visual effects based on distance
            float distanceToTarget = Vector3.Distance(missile.Position, target.Position);

            // Color flame and trail based on distance (green when close, red when far)
            var color = Color.Lerp(Color.green, Color.red, Mathf.InverseLerp(0, 1000, distanceToTarget));
            missile.Flame.Color = color;
            missile.Trail.Color = color;  // Automatically uses smoke or ion based on Trail.Variant
            missile.EngineLight.Color = color;

            // Log thrust from variable thruster if present
            foreach (var thruster in missile.GetParts<IVariableThruster>())
            {
                Log($"  Thruster: {thruster.Thrust}, Distance: {distanceToTarget:F0}m");
            }

            // Log ballast tank depth if present (for torpedoes)
            foreach (var ballast in missile.GetParts<IBallastTank>())
            {
                Log($"  Ballast: FloatHeight={ballast.FloatHeightDecrease}, Buoyancy={ballast.BuoyancyModifier}");
            }
        }
    }
}
