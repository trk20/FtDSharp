using FtDSharp;
using static FtDSharp.Logging;
using static FtDSharp.Game;
using UnityEngine;

/// <summary>
/// Demonstrates the Propulsion API with a simple altitude hold and target tracking behavior.
/// The construct will:
/// - Maintain a target altitude using pitch to climb/descend
/// - Keep level (zero roll)
/// - Turn towards any detected target using yaw
/// - Apply constant forward thrust
/// </summary>
public class BasicThrustControl : IFtDSharp
{
    private const float TargetAltitude = 150f;
    private const float AltitudeTolerance = 50f;
    private const float MaxPitchAngle = 10f; // Maximum pitch angle in degrees for altitude control
    private const float YawSensitivity = 0.2f; // Lower = wider turning circle

    public BasicThrustControl()
    {
        Log("BasicThrustControl initialized.");
    }

    public void Update(float deltaTime)
    {
        ClearLogs();
        Log($"Target altitude: {TargetAltitude}m");
        var construct = Game.MainConstruct;
        var propulsion = construct.Propulsion;

        float currentAltitude = construct.Position.y;
        float altitudeError = TargetAltitude - currentAltitude;

        float desiredPitchDeg = Mathf.Clamp(altitudeError / AltitudeTolerance * MaxPitchAngle, -MaxPitchAngle, MaxPitchAngle);

        float pitchError = construct.Pitch - desiredPitchDeg;

        propulsion.Pitch = Mathf.Clamp(pitchError * 0.1f, -1f, 1f);

        propulsion.Roll = Mathf.Clamp(construct.Roll * 0.1f, -1f, 1f);

        Log($"Alt: {currentAltitude:F1}m | AltErr: {altitudeError:F1} | DesiredPitch: {desiredPitchDeg:F1}° | CurrentPitch: {construct.Pitch:F1}° | PitchErr: {pitchError:F1}°");

        var target = AI.HighestPriorityMainframe.PrimaryTarget;

        Vector3 toTarget = ((target?.Position ?? Vector3.zero) - construct.Position).normalized;
        Vector3 localDirection = Quaternion.Inverse(construct.Rotation) * toTarget;

        float yawError = Mathf.Atan2(localDirection.x, localDirection.z);
        propulsion.Yaw = Mathf.Clamp(yawError * YawSensitivity, -1f, 1f);

        propulsion.Forwards = 1f;

        Log($"Current orientation: Pitch {construct.Pitch:F1}°, Roll {construct.Roll:F1}°, Yaw {construct.Yaw:F1}°");
        Log($"Pitch Request: {propulsion.Pitch:F2} \nRoll Request: {propulsion.Roll:F2}\nYaw Request: {propulsion.Yaw:F2}\nFwd Request: {propulsion.Forwards:F2}");

    }
}