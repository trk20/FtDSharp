using FtDSharp;

public class NaiveMissileGuidance : IFtDSharp
{
    public NaiveMissileGuidance()
    {
        Logging.Log("NaiveMissileGuidance script initialized.");
    }

    public void Update()
    {
        var target = AI.HighestPriorityMainframe.PrimaryTarget;


        if (target == null) foreach (var missile in Guidance.Missiles) missile.Detonate();
        else
        {
            foreach (var missileController in Weapons.MissileControllers) missileController.Fire();

            foreach (var missile in Guidance.Missiles) missile.AimAt(target.Position);
        }

    }
}
