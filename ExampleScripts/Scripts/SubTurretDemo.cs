/// <summary>
/// Demonstrates fine-grained turret and weapon control.
/// Shows independent target tracking for sub-turrets.
/// Requires a turret with CustomName set to "sub".
/// </summary>
public class SubTurretDemo
{
    private IWeaponController? _subTurretController;
    private IWeaponController? _rootTurretController;

    [OnStart]
    public void Initialize()
    {
        ITurret subTurret = Weapons.Turrets.FirstOrDefault(t => t.CustomName == "sub");
        if (subTurret == null)
        {
            LogError("No turret named 'sub' found. Set CustomName to 'sub' on a turret.");
            return;
        }
        _subTurretController = Weapons.CreateController(subTurret);

        var rootWeapons = Weapons.Turrets.Concat(Weapons.All).Except(_subTurretController.Controlled.All).ToList();
        if (rootWeapons.Count > 0)
            _rootTurretController = Weapons.CreateController(rootWeapons);
    }

    [OnPhysicsTick]
    public void Update()
    {
        ClearLogs();

        IMainframe mainframe = AI.HighestPriorityMainframe;

        if (_subTurretController == null && _rootTurretController == null)
        {
            Log("No turrets available for control.");
            return;
        }

        if (_subTurretController != null && _rootTurretController != null)
        {
            Log($"Subturret controlling {_subTurretController.Controlled.Weapons.Count} weapons.");
            Log($"Root turret controlling {_rootTurretController.Controlled.Weapons.Count} weapons.");
            foreach (IWeapon w in _subTurretController.Controlled.Weapons)
            {
                Drawing.Line(w.WorldPosition, w.WorldPosition + (w.AimDirection * 10f), Color.blue);
            }

            foreach (IWeapon w in _rootTurretController.Controlled.Weapons)
            {
                Drawing.Line(w.WorldPosition, w.WorldPosition + (w.AimDirection * 10f), Color.green);
            }
        }
        ITarget? target = mainframe.PrimaryTarget;
        if (target == null)
        {
            Log("No target found");
            return;
        }

        Log($"Target: {target.Name} at {target.Position}");

        // Draw target position
        Drawing.Sphere(target.Position, radius: 5f, Color.red);

        TrackResult? rootTrack = _rootTurretController?.Track(target);

        TrackResult? subTrack = _subTurretController?.Track(target);

        if (subTrack?.CanFire ?? false)
        {
            _subTurretController!.Fire();
        }
        if (rootTrack?.CanFire ?? false)
        {
            _rootTurretController!.Fire();
        }
    }
}
