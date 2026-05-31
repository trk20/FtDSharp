using System.Linq;

namespace FtDSharp.Tests.Mocks;

public class MockWarningsProvider : IWarningsProvider
{
    public IReadOnlyList<IProjectileWarning> IncomingProjectiles { get; set; } = Array.Empty<IProjectileWarning>();

    public IReadOnlyList<IProjectileWarning> IncomingMissiles =>
        IncomingProjectiles.Where(w => w.Type == ProjectileType.Missile).ToList();

    public IReadOnlyList<IProjectileWarning> IncomingShells =>
        IncomingProjectiles.Where(w => w.Type == ProjectileType.Shell || w.Type == ProjectileType.Cram).ToList();
}
