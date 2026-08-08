/// <summary>
/// Demonstrates the Propulsion API with a simple altitude hold and target tracking behavior.
/// The construct will:
/// - Maintain a target altitude using pitch to climb/descend
/// - Keep level (zero roll)
/// - Turn towards any detected target using yaw
/// - Apply constant forward thrust
/// </summary>
public class BasicThrustControl
{
    private const float _targetAltitude = 150f;
    private const float _altitudeTolerance = 50f;
    private const float _maxPitchAngle = 10f; // Maximum pitch angle in degrees for altitude control
    private const float _yawSensitivity = 0.2f; // Lower = wider turning circle

    [OnStart]
    public void Initialize()
    {
        Log("BasicThrustControl initialized.");
    }

    [OnPhysicsTick]
    public void Update()
    {
        ClearLogs();
        Log($"Target altitude: {_targetAltitude}m");
        IMainConstruct construct = Game.MainConstruct;
        IPropulsion propulsion = construct.Propulsion;

        var currentAltitude = construct.Position.y;
        var altitudeError = _targetAltitude - currentAltitude;

        var desiredPitchDeg = Mathf.Clamp(altitudeError / _altitudeTolerance * _maxPitchAngle, -_maxPitchAngle, _maxPitchAngle);

        var pitchError = construct.Pitch - desiredPitchDeg;

        propulsion.Pitch = Mathf.Clamp(pitchError * 0.1f, -1f, 1f);

        propulsion.Roll = Mathf.Clamp(construct.Roll * 0.1f, -1f, 1f);

        Log($"Alt: {currentAltitude:F1}m | AltErr: {altitudeError:F1} | DesiredPitch: {desiredPitchDeg:F1}° | CurrentPitch: {construct.Pitch:F1}° | PitchErr: {pitchError:F1}°");

        ITarget? target = AI.HighestPriorityMainframe.PrimaryTarget;

        Vector3 toTarget = ((target?.Position ?? Vector3.zero) - construct.Position).normalized;
        Vector3 localDirection = Quaternion.Inverse(construct.Rotation) * toTarget;

        var yawError = Mathf.Atan2(localDirection.x, localDirection.z);
        propulsion.Yaw = Mathf.Clamp(yawError * _yawSensitivity, -1f, 1f);

        propulsion.Forwards = 1f;

        Log($"Current orientation: Pitch {construct.Pitch:F1}°, Roll {construct.Roll:F1}°, Yaw {construct.Yaw:F1}°");
        Log($"Pitch Request: {propulsion.Pitch:F2} \nRoll Request: {propulsion.Roll:F2}\nYaw Request: {propulsion.Yaw:F2}\nFwd Request: {propulsion.Forwards:F2}");

    }
}
