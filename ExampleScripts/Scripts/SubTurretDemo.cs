/// <summary>
/// Demonstrates fine-grained turret and weapon control.
/// Shows independent target tracking for sub-turrets.
/// Requires a turret with CustomName set to "sub".
/// </summary>
public class SubTurretDemo
{
    IWeaponController? subTurretController;
    IWeaponController? rootTurretController;

    [OnStart]
    public void Initialize()
    {
        var subTurret = Weapons.Turrets.FirstOrDefault(t => t.CustomName == "sub");
        if (subTurret == null)
        {
            LogError("No turret named 'sub' found. Set CustomName to 'sub' on a turret.");
            return;
        }
        subTurretController = Weapons.CreateController(subTurret);

        var rootWeapons = Weapons.Turrets.Concat(Weapons.All).Except(subTurretController.Controlled.All).ToList();
        if (rootWeapons.Count > 0)
            rootTurretController = Weapons.CreateController(rootWeapons);
    }

    [OnPhysicsTick]
    public void Update()
    {
        ClearLogs();

        var mainframe = AI.HighestPriorityMainframe;

        if (subTurretController == null && rootTurretController == null)
        {
            Log("No turrets available for control.");
            return;
        }

        if (subTurretController != null && rootTurretController != null)
        {
            Log($"Subturret controlling {subTurretController.Controlled.Weapons.Count} weapons.");
            Log($"Root turret controlling {rootTurretController.Controlled.Weapons.Count} weapons.");
            foreach (var w in subTurretController.Controlled.Weapons)
            {
                Drawing.Line(w.WorldPosition, w.WorldPosition + w.AimDirection * 10f, Color.blue);
            }

            foreach (var w in rootTurretController.Controlled.Weapons)
            {
                Drawing.Line(w.WorldPosition, w.WorldPosition + w.AimDirection * 10f, Color.green);
            }
        }
        var target = mainframe.PrimaryTarget;
        if (target == null)
        {
            Log("No target found");
            return;
        }

        Log($"Target: {target.Name} at {target.Position}");

        // Draw target position
        Drawing.Sphere(target.Position, radius: 5f, Color.red);

        var rootTrack = rootTurretController?.Track(target);

        var subTrack = subTurretController?.Track(target);

        if (subTrack?.CanFire ?? false)
        {
            subTurretController!.Fire();
        }
        if (rootTrack?.CanFire ?? false)
        {
            rootTurretController!.Fire();
        }
    }
}
