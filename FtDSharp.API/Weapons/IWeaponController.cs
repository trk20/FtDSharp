namespace FtDSharp
{
    /// <summary>
    /// Interface for a weapon controller that coordinates aiming and firing of multiple weapons/turrets.
    /// </summary>
    public interface IWeaponController : IWeaponControl
    {
        /// <summary>
        /// Provides access to the weapons and turrets controlled by this controller.
        /// </summary>
        ControlledItems Controlled { get; }

        /// <summary>
        /// Whether all weapons are known types that can fire.
        /// </summary>
        bool AllKnownTypes { get; }

        /// <summary>
        /// Rebuilds the hierarchy. Call if weapons are added/removed from turrets.
        /// </summary>
        void RebuildHierarchy();
    }
}
