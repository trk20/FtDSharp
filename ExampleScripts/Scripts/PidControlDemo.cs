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
public class PIDControlDemo
{
    private const float _targetAltitude = 200f;

    private readonly PID _altitudePid;
    private readonly PID _pitchPid;
    private readonly PID _rollPid;

    // use constructors to initialize readonly fields - will run before [OnStart] 
    public PIDControlDemo()
    {
        _altitudePid = PID.Bind(
            input: () => Game.MainConstruct.Position.y,
            output: v => Game.MainConstruct.Propulsion.Hover = v,
            setpoint: () => _targetAltitude,
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

    [OnPhysicsTick]
    public void Update()
    {
        ClearLogs();

        // Just call Update() on each PID 
        _altitudePid.Update(Game.GameDeltaTime);
        _pitchPid.Update(Game.GameDeltaTime);
        _rollPid.Update(Game.GameDeltaTime);

        IMainConstruct construct = Game.MainConstruct;
        Log($"Altitude: {construct.Position.y:F1}m (target: {_targetAltitude}m, error: {_altitudePid.LastError:F1})");
        Log($"Hover output: {_altitudePid.LastOutput:F3} | Integral: {_altitudePid.Integral:F3}");
        Log($"Pitch: {construct.Pitch:F1}° | Roll: {construct.Roll:F1}°");
    }
}
