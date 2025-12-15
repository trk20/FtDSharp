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
        if (target == null) return;
        foreach (var missile in Weapons.Missiles) missile.AimAt(target.Position);
    }
}
