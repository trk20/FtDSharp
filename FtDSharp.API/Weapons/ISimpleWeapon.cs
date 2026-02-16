namespace FtDSharp
{
    /// <summary>
    /// Interface for simple weapons.
    /// </summary>
    public interface ISimpleWeapon : IWeapon
    {
        /// <summary>Number of shells currently loaded.</summary>
        int LoadedShellCount { get; }

        /// <summary>Maximum shell capacity.</summary>
        int ShellCapacity { get; }
    }
}
