namespace FtDSharp.Tests.Mocks;

public class MockWeaponsProvider : IWeaponsProvider
{
    public IReadOnlyList<IWeapon> Weapons { get; set; } = Array.Empty<IWeapon>();
    public IReadOnlyList<ITurret> Turrets { get; set; } = Array.Empty<ITurret>();
    public List<object> CreateControllerCalls { get; } = new();

    public IWeaponController CreateController(ITurret turret)
    {
        CreateControllerCalls.Add(turret);
        return new MockWeaponController();
    }

    public IWeaponController CreateController(IEnumerable<IWeapon> weapons)
    {
        var list = weapons.ToList();
        CreateControllerCalls.Add(list);
        return new MockWeaponController();
    }
}
