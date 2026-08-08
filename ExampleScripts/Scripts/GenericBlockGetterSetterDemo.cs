using static UnityEngine.Mathf;

/// <summary>
/// Example script demonstrating the auto-generated block API with read/write properties.
/// </summary>
public class GenericBlockGetterSetterDemo
{
    private const float _yawAmplitude = 15f;
    private const float _yawSpeed = 2f;

    [OnStart]
    public void Initialize()
    {
        Log($"SteamJetMonitor initialized on {Game.MainConstruct.Name}");
        Log($"Found {Blocks.SteamJets.Count} steam jet(s) on construct");
    }

    [OnPhysicsTick]
    public void Update()
    {
        ClearLogs();

        if (Blocks.SteamJets.Count == 0)
        {
            Log("No steam jets found on this construct.");
            return;
        }

        var totalPressure = 0f;
        foreach (ISteamJet jet in Blocks.SteamJets)
        {
            Log($"SteamJet [{jet.UniqueId}] at {jet.LocalPosition}: " +
                $"Pressure={jet.PressureReader:F2}, MaxSteam={jet.MaxSteam:F0}");
            totalPressure += jet.PressureReader;
        }

        var avgPressure = totalPressure / Blocks.SteamJets.Count;
        Log($"Average pressure across {Blocks.SteamJets.Count} jets: {avgPressure:F2}");


        var yawAngle = Sin(Game.Time * _yawSpeed) * _yawAmplitude;
        foreach (ISteamJet jet in Blocks.SteamJets)
        {
            jet.YawAngle = yawAngle;
        }
    }
}
