using UnityEngine;

namespace FtDSharp.Tests.Mocks;

public class MockWeaponController : IWeaponController
{
    public ControlledItems Controlled { get; } = new(Array.Empty<IWeapon>(), Array.Empty<ITurret>());
    public bool AllKnownTypes => true;

    public void RebuildHierarchy() { }

    public AimResult AimAt(Vector3 worldPosition) => default;
    public TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity) => default;
    public TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetAcceleration) => default;
    public TrackResult Track(ITargetable targetable) => default;
    public TrackResult Track(ITargetable targetable, TrackOptions options) => default;
    public TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetAcceleration, TrackOptions options) => default;
    public bool Fire() => false;
    public bool TryFireAt(Vector3 worldPosition) => false;
}
