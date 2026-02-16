using FtDSharp;

public class NaiveMissileGuidance : IFtDSharp
{
    public NaiveMissileGuidance()
    {
        Logging.Log("NaiveMissileGuidance script initialized.");
    }

    public void Update(float deltaTime)
    {
        var target = AI.HighestPriorityMainframe.PrimaryTarget;
        foreach (var missileController in Weapons.MissileControllers)
            missileController.Fire();

        if (target == null) foreach (var missile in Guidance.Missiles) missile.Detonate();

        else foreach (var missile in Guidance.Missiles) missile.AimAt(target.Position);
    }
}
