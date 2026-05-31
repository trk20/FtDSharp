using System.Collections.Generic;
using UnityEngine;

namespace FtDSharp
{
    public interface IGameProvider
    {
        IMainConstruct MainConstruct { get; }
        float GameTime { get; }
        float RealTime { get; }
        float GameDeltaTime { get; }
        float RealDeltaTime { get; }
        long TicksSinceStart { get; }
    }

    public interface ILogProvider
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void ClearLogs();
    }

    public interface IDrawingProvider
    {
        void AddFigure(IDrawFigure figure);
        void Clear();
    }

    public interface IAIProvider
    {
        IReadOnlyList<IMainframe> Mainframes { get; }
    }

    public interface IWeaponsProvider
    {
        IReadOnlyList<IWeapon> Weapons { get; }
        IReadOnlyList<ITurret> Turrets { get; }
        IWeaponController CreateController(ITurret turret);
        IWeaponController CreateController(IEnumerable<IWeapon> weapons);
    }

    public interface IGuidanceProvider
    {
        IReadOnlyList<IMissile> Missiles { get; }
    }

    public interface IWarningsProvider
    {
        IReadOnlyList<IProjectileWarning> IncomingProjectiles { get; }
        IReadOnlyList<IProjectileWarning> IncomingMissiles { get; }
        IReadOnlyList<IProjectileWarning> IncomingShells { get; }
    }

    public interface IFleetProvider
    {
        IReadOnlyList<IFriendlyConstruct> All { get; }
        IReadOnlyList<IFriendlyConstruct> AllExcludingSelf { get; }
        IReadOnlyList<IFleet> Fleets { get; }
        IFleet MyFleet { get; }
    }

    public interface IPropulsionProvider
    {
        IPropulsion Propulsion { get; }
    }
}
