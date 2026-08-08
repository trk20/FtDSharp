public class MissileGuidance
{
    [OnPhysicsTick]
    public void Update()
    {
        ITarget? target = AI.HighestPriorityMainframe.PrimaryTarget;
        if (target == null)
        {
            foreach (IMissile m in Guidance.Missiles) m.Detonate();
            return;
        }

        foreach (IMissileController controller in Weapons.MissileControllers)
            controller.Fire();

        foreach (IMissile missile in Guidance.Missiles)
        {
            var timeToImpact = Vector3.Distance(missile.Position, target.Position)
                               / missile.Velocity.magnitude;

            Vector3 predicted = target.Position + (target.Velocity * timeToImpact);

            missile.AimAt(predicted);
        }
    }
}