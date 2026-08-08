/// <summary>
/// Demonstrates that typed block interfaces extend IWeapon and can be used for control.
/// Blocks.ApsFiringPieces, Blocks.CramFiringPieces, etc. all implement IWeapon,
/// meaning you can call .Track() and .Fire() directly on them.
/// </summary>
public class WeaponTypePropertiesDemo
{
    [OnPhysicsTick]
    public void Update()
    {
        ClearLogs();

        ITarget? target = AI.HighestPriorityMainframe.PrimaryTarget;

        // Blocks.ApsFiringPieces returns IApsFiringPiece which extends IWeapon
        foreach (IApsFiringPiece aps in Blocks.ApsFiringPieces)
        {
            Log($"--- APS: {aps.CustomName ?? $"ID:{aps.UniqueId}"} ---");

            Log($"  Gauge: {aps.APSShellDiameter * 1000:F0}mm");
            Log($"  Barrels: {aps.BarrelCount}");
            Log($"  Loaded: {aps.APSLoaded}");
            Log($"  Inaccuracy: {aps.APSInaccuracy:F2}°");

            Log($"  ProjectileSpeed: {aps.ProjectileSpeed:F0} m/s");
            Log($"  IsReady: {aps.IsReady}");

            if (target != null)
            {
                aps.Track(target);
                Log($"  Track: OnTarget={aps.OnTarget}, CanFire={aps.CanFire}");

                Color color = aps.CanFire ? Color.green : (aps.CanAim ? Color.yellow : Color.red);
                Drawing.Arrow(aps.WorldPosition, aps.WorldPosition + (aps.AimDirection * 30f), color, 0.5f);

                if (aps.CanFire)
                {
                    aps.Fire();
                }
            }
        }

        // Same pattern for CRAM
        foreach (ICramFiringPiece cram in Blocks.CramFiringPieces)
        {
            Log($"\n--- CRAM: {cram.CustomName ?? $"ID:{cram.UniqueId}"} ---");

            Log($"  Gauge: {cram.CRAMdiameter:F0}mm");
            Log($"  Reload: {cram.CRAMreload:F1}s");
            Log($"  Pellets: {cram.CRAMTotalPellets:F0}");

            Log($"  ProjectileSpeed: {cram.ProjectileSpeed:F0} m/s");

            if (target != null)
            {
                cram.Track(target);
                Log($"  Track: OnTarget={cram.OnTarget}, FlightTime={cram.FlightTime:F2}s");

                Color color = cram.CanFire ? Color.green : Color.yellow;
                Drawing.Arrow(cram.WorldPosition, cram.WorldPosition + (cram.AimDirection * 25f), color, 0.5f);
            }
        }

        // Lasers
        foreach (ILaser laser in Blocks.Lasers)
        {
            Log($"\n--- Laser: {laser.CustomName ?? $"ID:{laser.UniqueId}"} ---");

            // Type-specific
            Log($"  Firing: {laser.Firing}");
            Log($"  Beam Color: {laser.Color}");

            if (target != null)
            {
                laser.Track(target, TrackOptions.InstantHit);
                Log($"  Track: OnTarget={laser.OnTarget}");

                Color beamColor = laser.Firing ? Color.red : Color.gray;
                Drawing.Arrow(laser.WorldPosition, laser.WorldPosition + (laser.AimDirection * 50f), beamColor, 0.3f);
            }
        }
    }
}
