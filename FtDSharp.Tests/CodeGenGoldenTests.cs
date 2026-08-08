using FtDSharp.CodeGen;
using FtDSharp.CodeGen.Models;
using FtDSharp.CodeGen.Passes;
using Xunit;

namespace FtDSharp.Tests;

public sealed class CodeGenGoldenTestContext : IDisposable
{
    public string ApiOutputPath { get; }
    public string FacadeOutputPath { get; }

    public CodeGenGoldenTestContext()
    {
        var root = Path.Combine(Path.GetTempPath(), "FtDSharp.CodeGen.Tests", Guid.NewGuid().ToString("N"));
        ApiOutputPath = Path.Combine(root, "API");
        FacadeOutputPath = Path.Combine(root, "Facades");
        Directory.CreateDirectory(ApiOutputPath);
        Directory.CreateDirectory(FacadeOutputPath);

        new GeneratorPipeline().Run(ApiOutputPath, FacadeOutputPath);
    }

    public void Dispose()
    {
        var root = Path.GetDirectoryName(ApiOutputPath);
        if (root == null)
            return;

        try
        {
            Directory.Delete(root, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}

public class CodeGenGoldenTests : IClassFixture<CodeGenGoldenTestContext>
{
    private readonly CodeGenGoldenTestContext _context;

    public CodeGenGoldenTests(CodeGenGoldenTestContext context) => _context = context;

    [Fact]
    public void GeneratedBlockCount_MatchesBaseline()
    {
        var interfaceFiles = Directory.GetFiles(_context.ApiOutputPath, "I*.g.cs")
            .Where(f => !Path.GetFileName(f).StartsWith("IBlocksProvider", StringComparison.Ordinal))
            .ToList();

        Assert.Equal(275, interfaceFiles.Count);
        Assert.Equal(275, Directory.GetFiles(_context.FacadeOutputPath, "*Facade.g.cs")
            .Count(f => !Path.GetFileName(f).Equals("BlockFactory.g.cs", StringComparison.Ordinal)));
    }

    [Fact]
    public void GeneratedMissilePartCount_MatchesBaseline()
    {
        var missileApiPath = Path.Combine(_context.ApiOutputPath, "MissileParts");
        var missileFacadePath = Path.Combine(_context.FacadeOutputPath, "MissileParts");

        Assert.Equal(31, Directory.GetFiles(missileApiPath, "I*.g.cs").Length);
        Assert.Equal(31, Directory.GetFiles(missileFacadePath, "*Facade.g.cs")
            .Count(f => !Path.GetFileName(f).Equals("MissilePartFactory.g.cs", StringComparison.Ordinal)));
    }

    [Fact]
    public void StandardBlock_FuelEngine_UsesBlockFacadeBaseAndDirectAccessor()
    {
        var facade = ReadFacade("FuelEngineFacade.g.cs");
        var iface = ReadInterface("IFuelEngine.g.cs");

        Assert.Contains("public interface IFuelEngine : IBlock", Normalize(iface));
        Assert.Contains("internal partial class FuelEngineFacade : BlockFacadeBase, IFuelEngine", Normalize(facade));
        Assert.Contains("get => _block.Data.BatteryChargeDrive.Us;", Normalize(facade));
    }

    [Fact]
    public void TurretBlock_UsesTurretFacadeAndIturretInheritance()
    {
        var facade = ReadFacade("TurretBlockFacade.g.cs");
        var iface = ReadInterface("ITurretBlock.g.cs");

        Assert.Contains("public interface ITurretBlock : ITurret, IConstructableWeaponBlock, IDamageLogger", Normalize(iface));
        Assert.Contains("internal partial class TurretBlockFacade : TurretFacade, ITurretBlock", Normalize(facade));
        Assert.Contains("public int ShotsFiredSinceLastCheck => ((Turrets)Weapon).HasFiredReader;", Normalize(facade));
    }

    [Fact]
    public void WeaponBlock_Laser_UsesWeaponFacadeAndIWeaponInheritance()
    {
        var facade = ReadFacade("LaserFacade.g.cs");
        var iface = ReadInterface("ILaser.g.cs");

        Assert.Contains("public interface ILaser : IWeapon, IConstructableWeaponBlock, IDamageLogger", Normalize(iface));
        Assert.Contains("internal partial class LaserFacade : WeaponFacade, ILaser", Normalize(facade));
        Assert.Contains("public int ShotsFiredSinceLastCheck => ((Laser)Weapon).HasFiredReader;", Normalize(facade));
    }

    [Fact]
    public void LaunchpadBlock_ImplementsMissileLaunchpadLogicalInterface()
    {
        var iface = ReadInterface("ISmallLauncher.g.cs");

        Assert.Contains("public interface ISmallLauncher : IBlock, IMissileLaunchpad", Normalize(iface));
    }

    [Fact]
    public void BlocksApi_ExposesPluralizedBlockCollections()
    {
        var blocksApi = ReadApiFile("Blocks.g.cs");

        Assert.Contains("public static IReadOnlyList<IFuelEngine> FuelEngines =>", Normalize(blocksApi));
        Assert.Contains("public static IReadOnlyList<ISmallLauncher> SmallLaunchers =>", Normalize(blocksApi));
    }

    [Fact]
    public void MissilePartEnums_ContainsBeamRiderModeValues()
    {
        var enums = ReadApiFile(Path.Combine("MissileParts", "MissilePartEnums.g.cs"));

        Assert.Contains("public enum BeamRiderMode", Normalize(enums));
        Assert.Contains("OurLasers = 0,", Normalize(enums));
        Assert.Contains("OurVehiclesLasers = 1,", Normalize(enums));
    }

    [Fact]
    public void BeamRider_UsesEnumBackedAimAtParameter()
    {
        var iface = ReadApiFile(Path.Combine("MissileParts", "IBeamRider.g.cs"));
        var facade = ReadFacade(Path.Combine("MissileParts", "BeamRiderFacade.g.cs"));

        Assert.Contains("BeamRiderMode AimAt { get; set; }", Normalize(iface));
        Assert.Contains("get => (BeamRiderMode)(int)_component.parameters[0].Value;", Normalize(facade));
        Assert.Contains("set => _component.SetParameterValue(0, (float)(int)value);", Normalize(facade));
        Assert.DoesNotContain("bool OursOnly", Normalize(iface));
    }

    [Fact]
    public void BlockFactory_WeaponBlocksRequireAllConstruct()
    {
        var factory = ReadFacade("BlockFactory.g.cs");

        Assert.Contains("new LaserFacade((Laser)b, ac)", Normalize(factory));
        Assert.Contains("new FuelEngineFacade((EngineModelBlock)b))", Normalize(factory));
    }

    [Fact]
    public void MissilePartConfig_Validate_RejectsUnknownEnumReference()
    {
        var definition = new MissilePartDefinition
        {
            InterfaceName = "ITestPart",
            GameType = typeof(object),
            Parameters =
            [
                new(0, "Mode", enumTypeName: "NonexistentEnum")
            ]
        };

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            MissilePartConfig.ValidateDefinitions([definition], MissilePartConfig.Enums));

        Assert.Contains("NonexistentEnum", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissilePartConfig_Validate_RejectsUnreferencedEnum()
    {
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            MissilePartConfig.ValidateDefinitions([], [new GeneratedEnum("UnusedEnum", [])]));

        Assert.Contains("UnusedEnum", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void LaserBlock_RenderSurface_ClassifiesAsWeapon()
    {
        BlockDefinition laser = BlockPipeline.Run().Single(b => b.ClassName == "Laser");

        Assert.Equal(BlockKind.Weapon, laser.Surface.Kind);
        Assert.True(laser.Surface.IsWeapon);
        Assert.False(laser.Surface.IsTurret);
        Assert.NotEmpty(laser.Surface.InterfaceProperties);
        Assert.NotEmpty(laser.Surface.FacadeProperties);
    }

    private string ReadInterface(string fileName) =>
        File.ReadAllText(Path.Combine(_context.ApiOutputPath, fileName));

    private string ReadFacade(string fileName) =>
        File.ReadAllText(Path.Combine(_context.FacadeOutputPath, fileName));

    private string ReadApiFile(string relativePath) =>
        File.ReadAllText(Path.Combine(_context.ApiOutputPath, relativePath));

    private static string Normalize(string content) =>
        content.Replace("\r\n", "\n", StringComparison.Ordinal);
}
