using System;
using System.Collections.Generic;
using System.Linq;
using FtDSharp.Facades;
using FtDSharp.Helpers;
using UnityEngine;

namespace FtDSharp
{
    internal sealed class ProviderScope : IProviderScope
    {
        private readonly object _drawingOwner = new();

        public ProviderScope(BasicScriptContext context)
        {
            Game = new GameProvider(context);
            Log = new LogProvider(context);
            Drawing = new DrawingProvider(_drawingOwner);
            AI = new AIProvider(context);
            Weapons = new WeaponsProvider(context);
            Guidance = new GuidanceProvider(context);
            Warnings = new WarningsProvider(context);
            Fleet = new FleetProvider(context);
            Blocks = new BlocksProvider(context);
            Propulsion = new PropulsionProvider(context);
        }

        public IGameProvider Game { get; }
        public ILogProvider Log { get; }
        public IDrawingProvider Drawing { get; }
        public IAIProvider AI { get; }
        public IWeaponsProvider Weapons { get; }
        public IGuidanceProvider Guidance { get; }
        public IWarningsProvider Warnings { get; }
        public IFleetProvider Fleet { get; }
        public IBlocksProvider Blocks { get; }
        public IPropulsionProvider Propulsion { get; }

        public void Dispose() => DrawingService.Instance.RemoveOwner(_drawingOwner);
    }

    internal sealed class GameProvider : IGameProvider
    {
        private readonly BasicScriptContext _context;

        public GameProvider(BasicScriptContext context) => _context = context;

        public IMainConstruct MainConstruct => _context.Self;
        public float GameTime => _context.GameTimeSinceStart;
        public float RealTime => _context.RealTimeSinceStart;
        public float GameDeltaTime => _context.GameDeltaTime;
        public float RealDeltaTime => _context.RealDeltaTime;
        public long TicksSinceStart => _context.TicksSinceStart;
    }

    internal sealed class LogProvider : ILogProvider
    {
        private readonly BasicScriptContext _context;

        public LogProvider(BasicScriptContext context) => _context = context;

        public void Info(string message) => _context.Log.Info(message);
        public void Warn(string message) => _context.Log.Warn(message);
        public void Error(string message) => _context.Log.Error(message);
        public void ClearLogs() => _context.Log.ClearLogs();
    }

    internal sealed class DrawingProvider : IDrawingProvider
    {
        private readonly object _owner;

        public DrawingProvider(object owner) => _owner = owner;

        public void AddFigure(IDrawFigure figure) => DrawingService.Instance.AddFigure(_owner, figure);

        public void Clear() => DrawingService.Instance.Clear(_owner);
    }

    internal sealed class AIProvider : IAIProvider
    {
        private readonly BasicScriptContext _context;

        public AIProvider(BasicScriptContext context) => _context = context;

        public IReadOnlyList<IMainframe> Mainframes =>
            _context.MainConstructFacade?.Mainframes ?? Array.Empty<IMainframe>();
    }

    internal sealed class WeaponsProvider : IWeaponsProvider
    {
        private readonly BasicScriptContext _context;

        public WeaponsProvider(BasicScriptContext context) => _context = context;

        public IReadOnlyList<IWeapon> Weapons =>
            _context.MainConstructFacade?.Weapons ?? Array.Empty<IWeapon>();

        public IReadOnlyList<ITurret> Turrets =>
            _context.MainConstructFacade?.Turrets ?? Array.Empty<ITurret>();

        public IWeaponController CreateController(ITurret turret) => new WeaponController(turret);

        public IWeaponController CreateController(IEnumerable<IWeapon> weapons) => new WeaponController(weapons);
    }

    internal sealed class GuidanceProvider : IGuidanceProvider
    {
        private readonly BasicScriptContext _context;

        public GuidanceProvider(BasicScriptContext context) => _context = context;

        public IReadOnlyList<IMissile> Missiles =>
            (IReadOnlyList<IMissile>?)_context.MainConstructFacade?.Missiles ?? Array.Empty<IMissile>();
    }

    internal sealed class WarningsProvider : IWarningsProvider
    {
        private readonly BasicScriptContext _context;
        private readonly FrameCache<IReadOnlyList<IProjectileWarning>> _allCache;
        private readonly FrameCache<IReadOnlyList<IProjectileWarning>> _missilesCache;
        private readonly FrameCache<IReadOnlyList<IProjectileWarning>> _shellsCache;

        public WarningsProvider(BasicScriptContext context)
        {
            _context = context;
            _allCache = new FrameCache<IReadOnlyList<IProjectileWarning>>(GetWarnings, () => _context.RawAllConstruct);
            _missilesCache = new FrameCache<IReadOnlyList<IProjectileWarning>>(
                () => _allCache.Value.Where(w => w.Type == ProjectileType.Missile).ToList(),
                () => _context.RawAllConstruct);
            _shellsCache = new FrameCache<IReadOnlyList<IProjectileWarning>>(
                () => _allCache.Value.Where(w => w.Type == ProjectileType.Shell || w.Type == ProjectileType.Cram).ToList(),
                () => _context.RawAllConstruct);
        }

        public IReadOnlyList<IProjectileWarning> IncomingProjectiles => _allCache.Value;

        public IReadOnlyList<IProjectileWarning> IncomingMissiles => _missilesCache.Value;

        public IReadOnlyList<IProjectileWarning> IncomingShells => _shellsCache.Value;

        private IReadOnlyList<IProjectileWarning> GetWarnings()
        {
            var construct = _context.RawAllConstruct as MainConstruct;
            if (construct?.MWM?.Warnings == null)
            {
                return Array.Empty<IProjectileWarning>();
            }

            return construct.MWM.Warnings
                .Where(warning => warning?.IsValid ?? false)
                .Select(warning => new ProjectileWarningFacade(warning))
                .Cast<IProjectileWarning>()
                .ToList();
        }
    }

    internal sealed class FleetProvider : IFleetProvider
    {
        private readonly BasicScriptContext _context;
        private readonly FrameCache<IReadOnlyList<IFriendlyConstruct>> _allCache;
        private readonly FrameCache<IReadOnlyList<IFriendlyConstruct>> _excludingSelfCache;
        private readonly FrameCache<IReadOnlyList<IFleet>> _fleetsCache;

        public FleetProvider(BasicScriptContext context)
        {
            _context = context;
            _allCache = new FrameCache<IReadOnlyList<IFriendlyConstruct>>(GetAll, () => _context.RawAllConstruct);
            _excludingSelfCache = new FrameCache<IReadOnlyList<IFriendlyConstruct>>(GetAllExcludingSelf, () => _context.RawAllConstruct);
            _fleetsCache = new FrameCache<IReadOnlyList<IFleet>>(GetFleets, () => _context.RawAllConstruct);
        }

        public IReadOnlyList<IFriendlyConstruct> All => _allCache.Value;

        public IReadOnlyList<IFriendlyConstruct> AllExcludingSelf => _excludingSelfCache.Value;

        public IReadOnlyList<IFleet> Fleets => _fleetsCache.Value;

        public IFleet MyFleet => _context.Self.Fleet;

        private IReadOnlyList<IFriendlyConstruct> GetAll()
        {
            AllConstruct? construct = _context.RawAllConstruct;
            if (construct == null)
            {
                return Array.Empty<IFriendlyConstruct>();
            }

            BrilliantSkies.Core.Id.ObjectId myTeam = construct.GetTeam();
            return StaticConstructablesManager.Constructables
                .Where(c => c != null && !c.Destroyed && c.GetTeam() == myTeam)
                .Select(c => new FriendlyConstructFacade(c))
                .Cast<IFriendlyConstruct>()
                .ToList();
        }

        private IReadOnlyList<IFriendlyConstruct> GetAllExcludingSelf()
        {
            AllConstruct? construct = _context.RawAllConstruct;
            if (construct == null)
            {
                return Array.Empty<IFriendlyConstruct>();
            }

            BrilliantSkies.Core.Id.ObjectId myTeam = construct.GetTeam();
            return StaticConstructablesManager.Constructables
                .Where(c => c != null && !c.Destroyed && c != construct && c.GetTeam() == myTeam)
                .Select(c => new FriendlyConstructFacade(c))
                .Cast<IFriendlyConstruct>()
                .ToList();
        }

        private IReadOnlyList<IFleet> GetFleets()
        {
            return All
                .Select(construct => construct.Fleet)
                .Where(fleet => fleet != null)
                .GroupBy(fleet => fleet.Id)
                .Select(group => group.First())
                .ToList();
        }
    }

    internal sealed class PropulsionProvider : IPropulsionProvider
    {
        private readonly BasicScriptContext _context;

        public PropulsionProvider(BasicScriptContext context) => _context = context;

        public IPropulsion Propulsion => _context.Self.Propulsion;
    }
}
