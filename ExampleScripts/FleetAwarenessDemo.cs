using FtDSharp;
using UnityEngine;
using static FtDSharp.Drawing;

/// <summary>
/// Demonstrates fleet and friendly awareness capabilities.
/// Uses the Drawing API to visualize fleet members and relationships.
/// </summary>
public class FleetAwarenessDemo : IFtDSharp
{

    public void Update(float deltaTime)
    {
        Cross(Game.MainConstruct.Position, Color.white, width: 2f, scale: 5f);

        foreach (var fleet in Friendly.Fleets)
        {
            var fleetPos = fleet.Position;
            var flagshipPos = fleet.Flagship.Position;

            foreach (var member in fleet.Members)
            {
                var memberPos = member.Position;

                Gimbal(memberPos, radius: 8f, member.Rotation);

                if (member.Velocity.sqrMagnitude > 1f)
                {
                    Arrow(memberPos, memberPos + member.Velocity.normalized * 15f, Color.green, width: 1.5f);
                }

                if (member.UniqueId != fleet.Flagship.UniqueId)
                {
                    Line(memberPos, flagshipPos, Color.yellow, width: 1f);
                }
            }
        }
    }
}
