using UnityEngine;

namespace FtDSharp
{
    /// <summary>
    /// Interface for weapon control operations (aiming, tracking, firing).
    /// Implemented by both individual weapons and weapon controllers.
    /// </summary>
    public interface IWeaponControl
    {
        /// <summary>
        /// Aims at a world-space position (direct aim, no lead calculation).
        /// </summary>
        /// <param name="worldPosition">The target position in world space.</param>
        /// <returns>Information about the aim status.</returns>
        AimResult AimAt(Vector3 worldPosition);

        /// <summary>
        /// Tracks a moving target with lead calculation.
        /// </summary>
        /// <param name="targetPosition">Current target position in world space.</param>
        /// <param name="targetVelocity">Target velocity vector.</param>
        /// <returns>Tracking result with aim status and calculated values.</returns>
        TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity);

        /// <summary>
        /// Tracks a moving target with lead calculation including acceleration.
        /// </summary>
        /// <param name="targetPosition">Current target position in world space.</param>
        /// <param name="targetVelocity">Target velocity vector.</param>
        /// <param name="targetAcceleration">Target acceleration vector.</param>
        /// <returns>Tracking result with aim status and calculated values.</returns>
        TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetAcceleration);

        /// <summary>
        /// Tracks any targetable object (ITarget, IProjectileWarning, IConstruct) with lead calculation.
        /// </summary>
        /// <param name="targetable">The object to track.</param>
        /// <returns>Tracking result with aim status and calculated values.</returns>
        TrackResult Track(ITargetable targetable);

        /// <summary>
        /// Tracks a targetable object with custom tracking options.
        /// </summary>
        /// <param name="targetable">The object to track.</param>
        /// <param name="options">Tracking options (gravity, arc preference, projectile speed override).</param>
        /// <returns>Tracking result with aim status and calculated values.</returns>
        TrackResult Track(ITargetable targetable, TrackOptions options);

        /// <summary>
        /// Tracks a position/velocity/acceleration with custom tracking options.
        /// </summary>
        /// <param name="targetPosition">Current target position in world space.</param>
        /// <param name="targetVelocity">Target velocity vector.</param>
        /// <param name="targetAcceleration">Target acceleration vector.</param>
        /// <param name="options">Tracking options (gravity, arc preference, projectile speed override).</param>
        /// <returns>Tracking result with aim status and calculated values.</returns>
        TrackResult Track(Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetAcceleration, TrackOptions options);

        /// <summary>
        /// Attempts to fire the weapon(s).
        /// </summary>
        /// <returns>True if any weapon fired successfully.</returns>
        bool Fire();

        /// <summary>
        /// Aims at a position and fires if on target.
        /// </summary>
        /// <param name="worldPosition">The target position to aim at.</param>
        /// <returns>True if any weapon fired successfully.</returns>
        bool TryFireAt(Vector3 worldPosition);
    }
}
