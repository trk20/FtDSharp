using System.Linq;
using FtDSharp;
using UnityEngine;
using static FtDSharp.Logging;
using static FtDSharp.AI;
using static FtDSharp.Drawing;

/// <summary>
/// Demonstrates weapon control functionality.
/// Shows tracking with lead calculation and turret control.
/// </summary>
public class WeaponControlDemo : IFtDSharp
{
    public void Update(float deltaTime)
    {
        ClearLogs();
        Log($"Main Construct: {Game.MainConstruct.Name}");

        Log($"Weapons: {Weapons.All.Count} (APS: {Weapons.APS.Count}, CRAM: {Weapons.CRAM.Count}, Lasers: {Weapons.Lasers.Count})");
        Log($"Turrets: {Weapons.Turrets.Count}");

        var mainframe = HighestPriorityMainframe;
        if (mainframe == null)
        {
            Log("No mainframe found");
            return;
        }

        var target = mainframe.PrimaryTarget;
        if (target == null)
        {
            Log("No target found");
            return;
        }

        Log($"Target: {target.Name} at {target.Position}");

        Sphere(target.Position, radius: 5f, Color.red);

        // track the target with lead calculation
        foreach (var weapon in Weapons.All)
        {
            Log($"Weapon {weapon.UniqueId}: Parent={weapon.Parent?.UniqueId}");
        }

        foreach (var turret in Weapons.Turrets.Where(t => t.Parent == null))
        {
            Log($"Turret: {turret.UniqueId} at {turret.WorldPosition} - Az: {turret.Azimuth:F1}° El: {turret.Elevation:F1}°");

            Log($"  About to Track...");
            // aim turret + all mounted weapons
            var result = turret.Track(target);
            Log($"  Track returned: IsOnTarget={result.IsOnTarget}, IsReady={result.IsReady}, CanFire={result.CanFire}, CanAim={result.CanAim}");

            var turretColor = result.CanFire ? Color.green : (result.CanAim ? Color.yellow : Color.red);
            Point(turret.WorldPosition, turretColor, size: 3f);

            // fire all weapons on turret if ready (CanFire = IsOnTarget && IsReady)
            if (result.CanFire)
            {
                turret.Fire();
            }
        }

        foreach (var turret in Weapons.Turrets.Where(t => t.Parent != null))
        {
            Log($"Child Turret: {turret.UniqueId} at {turret.WorldPosition} - Az: {turret.Azimuth:F1}° El: {turret.Elevation:F1}° Parent: {turret.Parent!.UniqueId}");
        }

        // also control any standalone weapons (spinal mounts, fixed weapons)
        foreach (var weapon in Weapons.All.Where(w => w.Parent == null))
        {
            // track with lead calculation
            var result = weapon.Track(target);

            var weaponColor = result.CanFire ? Color.green : (result.CanAim ? Color.yellow : Color.red);
            Point(weapon.WorldPosition, weaponColor, size: 2f);
            Arrow(weapon.WorldPosition, weapon.WorldPosition + weapon.AimDirection * 30f, weaponColor, width: 1f);

            if (result.CanFire)
            {
                weapon.Fire();
            }
        }
    }
}
