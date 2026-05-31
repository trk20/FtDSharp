public class MissileGuidance
{
    [OnPhysicsTick]
    public void Update()
    {
        var target = AI.HighestPriorityMainframe.PrimaryTarget;
        if (target == null)
        {
            foreach (var m in Guidance.Missiles) m.Detonate();
            return;
        }

        foreach (var controller in Weapons.MissileControllers)
            controller.Fire();

        foreach (var missile in Guidance.Missiles)
        {
            float timeToImpact = Vector3.Distance(missile.Position, target.Position)
                               / missile.Velocity.magnitude;

            Vector3 predicted = target.Position + target.Velocity * timeToImpact;

            missile.AimAt(predicted);
        }
    }
}