using FtDSharp;
using static FtDSharp.Logging;
using static UnityEngine.Mathf;

/// <summary>
/// Example script demonstrating the auto-generated block API with read/write properties.
/// </summary>
public class GenericBlockGetterSetterDemo : IFtDSharp
{
    private const float YawAmplitude = 15f;
    private const float YawSpeed = 2f;

    public GenericBlockGetterSetterDemo()
    {
        Log($"SteamJetMonitor initialized on {Game.MainConstruct.Name}");
        Log($"Found {Blocks.SteamJets.Count} steam jet(s) on construct");
    }

    public void Update(float deltaTime)
    {
        ClearLogs();

        if (Blocks.SteamJets.Count == 0)
        {
            Log("No steam jets found on this construct.");
            return;
        }

        float totalPressure = 0f;
        foreach (var jet in Blocks.SteamJets)
        {
            Log($"SteamJet [{jet.UniqueId}] at {jet.LocalPosition}: " +
                $"Pressure={jet.PressureReader:F2}, MaxSteam={jet.MaxSteam:F0}");
            totalPressure += jet.PressureReader;
        }

        float avgPressure = totalPressure / Blocks.SteamJets.Count;
        Log($"Average pressure across {Blocks.SteamJets.Count} jets: {avgPressure:F2}");


        float yawAngle = Sin(Game.Time * YawSpeed) * YawAmplitude;
        foreach (var jet in Blocks.SteamJets)
        {
            jet.YawAngle = yawAngle;
        }
    }
}
