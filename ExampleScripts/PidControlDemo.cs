using FtDSharp;
using static FtDSharp.Logging;
using static FtDSharp.Game;

/// <summary>
/// Demonstrates the PID helper class for smooth altitude and attitude control.
/// Uses separate PID controllers for:
/// - Altitude hold (via hover thrust)
/// - Pitch stabilization
/// - Roll stabilization
/// 
/// The PID controllers are bound to input/output using PID.Bind(), 
/// so Update() handles everything automatically.
/// </summary>
public class PIDControlDemo : IFtDSharp
{
    private const float TargetAltitude = 200f;

    private readonly PID _altitudePid;
    private readonly PID _pitchPid;
    private readonly PID _rollPid;

    public PIDControlDemo()
    {
        _altitudePid = PID.Bind(
            input: () => Game.MainConstruct.Position.y,
            output: v => Game.MainConstruct.Propulsion.Hover = v,
            setpoint: () => TargetAltitude,
            kP: 0.1f,
            kI: 0.02f,
            kD: 0.5f,
            integralLimit: 2f
        );

        _pitchPid = PID.Bind(
            () => Game.MainConstruct.Pitch,
            v => Game.MainConstruct.Propulsion.Pitch = v
        );

        _rollPid = PID.Bind(
            () => Game.MainConstruct.Roll,
            v => Game.MainConstruct.Propulsion.Roll = v
        );

        Log("PID Control Demo initialized.");
    }

    public void Update(float deltaTime)
    {
        ClearLogs();

        // Just call Update() on each PID 
        _altitudePid.Update(deltaTime);
        _pitchPid.Update(deltaTime);
        _rollPid.Update(deltaTime);

        var construct = Game.MainConstruct;
        Log($"Altitude: {construct.Position.y:F1}m (target: {TargetAltitude}m, error: {_altitudePid.LastError:F1})");
        Log($"Hover output: {_altitudePid.LastOutput:F3} | Integral: {_altitudePid.Integral:F3}");
        Log($"Pitch: {construct.Pitch:F1}° | Roll: {construct.Roll:F1}°");
    }
}
