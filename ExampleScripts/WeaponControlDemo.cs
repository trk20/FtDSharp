using System.Linq;
using FtDSharp;
using UnityEngine;
using static FtDSharp.Logging;
using static FtDSharp.AI;
using static FtDSharp.Drawing;

/// <summary>
/// Demonstrates weapon control with bespoke weapon-type interfaces.
/// Shows typed accessors, pattern matching, turret tracking, and per-type status display.
/// </summary>
public class WeaponControlDemo : IFtDSharp
{
    public void Update(float deltaTime)
    {
        ClearLogs();

        Log($"=== {Game.MainConstruct.Name} ===");
        Log($"Weapons: {Weapons.All.Count} | Turrets: {Weapons.Turrets.Count}");
        Log($"  APS: {Weapons.APS.Count}  CRAM: {Weapons.CRAM.Count}  Laser: {Weapons.Lasers.Count}  Plasma: {Weapons.Plasma.Count}");
        Log($"  PAC: {Weapons.ParticleCannons.Count}  Flamer: {Weapons.Flamers.Count}  Missile: {Weapons.MissileControllers.Count}  Simple: {Weapons.SimpleWeapons.Count}");

        foreach (var turret in Weapons.Turrets.Where(t => t.Parent == null))
        {
            Log($"Turret {turret.UniqueId}: {turret.Weapons.Count(w => w.IsReady)}/{turret.Weapons.Count} weapons ready");

            foreach (var weapon in turret.Weapons)
                LogWeaponStatus(weapon);
        }

        foreach (var weapon in Weapons.All.Where(w => w.Parent == null))
            LogWeaponStatus(weapon);


        var mainframe = HighestPriorityMainframe;
        if (mainframe == null) { Log("No mainframe"); return; }

        var target = mainframe.PrimaryTarget;
        if (target == null) { Log("No target"); return; }

        Log($"Target: {target.Name}");
        Sphere(target.Position, radius: 5f, Color.red);

        // aim turret + mounted weapons, updates each of their states accordingly (OnTarget, CanAim, CanFire, etc.)
        Log("Turrets:");
        foreach (var turret in Weapons.Turrets.Where(t => t.Parent == null))
        {
            turret.Track(target);
            var turretStatusColor = turret.CanFire ? Color.green : (turret.CanAim ? Color.yellow : Color.red);

            foreach (var weapon in turret.Weapons)
            {
                var weaponStatusColor = weapon.CanFire ? Color.green : (weapon.CanAim ? Color.yellow : Color.red);
                Arrow(weapon.WorldPosition, weapon.WorldPosition + weapon.AimDirection * 30f, weaponStatusColor, width: 1f);
            }
            Point(turret.WorldPosition, turretStatusColor, size: 3f);

            if (turret.CanFire) turret.Fire();
        }

        Log("Standalone weapons:");
        foreach (var weapon in Weapons.All.Where(w => w.Parent == null))
        {
            var result = weapon.Track(target);
            var color = result.CanFire ? Color.green : (result.CanAim ? Color.yellow : Color.red);

            Arrow(weapon.WorldPosition, weapon.WorldPosition + weapon.AimDirection * 30f, color, width: 1f);
            Point(weapon.WorldPosition, color, size: 2f);

            if (result.CanFire) weapon.Fire();
        }
    }

    /// <summary>
    /// Logs type-specific weapon status using pattern matching.
    /// All properties shown here are available without calling Track/AimAt.
    /// </summary>
    static void LogWeaponStatus(IWeapon weapon)
    {
        switch (weapon)
        {
            case IApsWeapon aps:
                Log($"    [{aps.Gauge:F0}mm APS] loaded={aps.IsLoaded} shells={aps.ShellCount}");
                break;
            case ICramWeapon cram:
                Log($"    [{cram.Gauge:F0}mm CRAM] pack={cram.PackedFraction:P0}");
                break;
            case IPlasmaWeapon plasma:
                Log($"    [Plasma] charges={plasma.ChargesReady} charged={plasma.IsFullyCharged}");
                break;
            case IParticleWeapon pac:
                Log($"    [PAC] charge={pac.ChargeFraction:P0} beams={pac.BeamCount}");
                break;
            case IFlamerWeapon flamer:
                Log($"    [Flamer] range={flamer.Range:F0}m fuel={flamer.CurrentFuel:F0}");
                break;
            case IMissileController missiles:
                Log($"    [Missiles] {missiles.LoadedMissileCount}/{missiles.TotalTubeCount} ready");
                break;
            case ILaserWeapon laser:
                Log($"    [Laser] firing={laser.IsFiring}");
                break;
            case ISimpleWeapon simple:
                Log($"    [Simple] {simple.LoadedShellCount}/{simple.ShellCapacity} shells");
                break;
            default:
                Log($"    [{weapon.WeaponType}] ready={weapon.IsReady}");
                break;
        }
    }
}
