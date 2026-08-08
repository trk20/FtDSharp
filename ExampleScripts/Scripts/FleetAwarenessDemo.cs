/// <summary>
/// Demonstrates fleet and friendly awareness capabilities.
/// Uses the Drawing API to visualize fleet members and relationships.
/// </summary>
public class FleetAwarenessDemo
{

    [OnPhysicsTick]
    public void Update()
    {
        Drawing.Cross(Game.MainConstruct.Position, Color.white, width: 2f, scale: 5f);

        foreach (IFleet fleet in Friendly.Fleets)
        {
            Vector3 fleetPos = fleet.Position;
            Vector3 flagshipPos = fleet.Flagship.Position;

            foreach (IFriendlyConstruct member in fleet.Members)
            {
                Vector3 memberPos = member.Position;

                Drawing.Gimbal(memberPos, radius: 8f, member.Rotation);

                if (member.Velocity.sqrMagnitude > 1f)
                {
                    Drawing.Arrow(memberPos, memberPos + (member.Velocity.normalized * 15f), Color.green, width: 1.5f);
                }

                if (member.UniqueId != fleet.Flagship.UniqueId)
                {
                    Drawing.Line(memberPos, flagshipPos, Color.yellow, width: 1f);
                }
            }
        }
    }
}
