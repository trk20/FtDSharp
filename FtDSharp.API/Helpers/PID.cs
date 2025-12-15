using System;

namespace FtDSharp
{
    /// <summary>
    /// A PID (Proportional-Integral-Derivative) controller for smooth control systems.
    /// Useful for stabilization, targeting, altitude hold, and other feedback control loops.
    /// </summary>
    public class PID
    {
        // Default values
        private const float DefaultKp = 0.05f;
        private const float DefaultKi = 250f;
        private const float DefaultKd = 0.3f;
        private const float DefaultIntegralLimit = 0f;
        private const float DefaultSetpoint = 0f;
        private const float DefaultOutputMin = -1f;
        private const float DefaultOutputMax = 1f;

        /// <summary>Proportional gain - how strongly the controller reacts to current error.</summary>
        public float Kp { get; set; }

        /// <summary>Integral gain - how strongly the controller reacts to accumulated error over time.</summary>
        public float Ki { get; set; }

        /// <summary>Derivative gain - how strongly the controller reacts to rate of error change.</summary>
        public float Kd { get; set; }

        /// <summary>Maximum value for the integral term to prevent windup. Set to 0 or negative to disable.</summary>
        public float IntegralLimit { get; set; }

        /// <summary>Output will be clamped to this range. Set both to 0 to disable clamping.</summary>
        public float OutputMin { get; set; }

        /// <summary>Output will be clamped to this range. Set both to 0 to disable clamping.</summary>
        public float OutputMax { get; set; }

        /// <summary>The target value the PID is trying to reach.</summary>
        public float Setpoint { get; set; }

        /// <summary>Current accumulated integral value.</summary>
        public float Integral { get; private set; }

        /// <summary>The error value from the previous update.</summary>
        public float PreviousError { get; private set; }

        /// <summary>Whether this is the first update (no previous error available).</summary>
        public bool IsFirstUpdate { get; private set; } = true;

        /// <summary>The last computed output value.</summary>
        public float LastOutput { get; private set; }

        /// <summary>The last computed error value.</summary>
        public float LastError { get; private set; }

        // Bound input/output/setpoint delegates
        private readonly Func<float>? _input;
        private readonly Action<float>? _output;
        private readonly Func<float>? _setpointFunc;

        #region Static Factory Methods

        /// <summary>
        /// Creates a PID controller with bound input and output.
        /// Just call Update(deltaTime) each frame - it reads, calculates, and writes automatically.
        /// </summary>
        /// <param name="input">Function that returns the current measured value.</param>
        /// <param name="output">Action that receives the control output.</param>
        /// <param name="setpoint">The target value to reach. Default 0.</param>
        /// <param name="kP">Proportional gain. Default 0.05.</param>
        /// <param name="kI">Integral gain. Default 250.</param>
        /// <param name="kD">Derivative gain. Default 0.3.</param>
        /// <param name="outputMin">Minimum output value. Default -1.</param>
        /// <param name="outputMax">Maximum output value. Default 1.</param>
        /// <param name="integralLimit">Maximum absolute value for integral term. Default 0 (disabled).</param>
        public static PID Bind(
            Func<float> input,
            Action<float> output,
            float setpoint = DefaultSetpoint,
            float kP = DefaultKp,
            float kI = DefaultKi,
            float kD = DefaultKd,
            float outputMin = DefaultOutputMin,
            float outputMax = DefaultOutputMax,
            float integralLimit = DefaultIntegralLimit)
        {
            return new PID(input, output, setpoint, kP, kI, kD, outputMin, outputMax, integralLimit);
        }

        /// <summary>
        /// Creates a PID controller with bound input, output, and dynamic setpoint.
        /// Just call Update(deltaTime) each frame - it reads all values and writes output automatically.
        /// </summary>
        /// <param name="input">Function that returns the current measured value.</param>
        /// <param name="output">Action that receives the control output.</param>
        /// <param name="setpoint">Function that returns the target value (evaluated each update).</param>
        /// <param name="kP">Proportional gain. Default 0.05.</param>
        /// <param name="kI">Integral gain. Default 250.</param>
        /// <param name="kD">Derivative gain. Default 0.3.</param>
        /// <param name="outputMin">Minimum output value. Default -1.</param>
        /// <param name="outputMax">Maximum output value. Default 1.</param>
        /// <param name="integralLimit">Maximum absolute value for integral term. Default 0 (disabled).</param>
        public static PID Bind(
            Func<float> input,
            Action<float> output,
            Func<float> setpoint,
            float kP = DefaultKp,
            float kI = DefaultKi,
            float kD = DefaultKd,
            float outputMin = DefaultOutputMin,
            float outputMax = DefaultOutputMax,
            float integralLimit = DefaultIntegralLimit)
        {
            return new PID(input, output, setpoint, kP, kI, kD, outputMin, outputMax, integralLimit);
        }

        /// <summary>
        /// Creates a PID controller with bound input only. 
        /// Call Update(deltaTime) and handle the returned output manually.
        /// </summary>
        /// <param name="input">Function that returns the current measured value.</param>
        /// <param name="setpoint">The target value to reach. Default 0.</param>
        /// <param name="kP">Proportional gain. Default 0.05.</param>
        /// <param name="kI">Integral gain. Default 250.</param>
        /// <param name="kD">Derivative gain. Default 0.3.</param>
        /// <param name="outputMin">Minimum output value. Default -1.</param>
        /// <param name="outputMax">Maximum output value. Default 1.</param>
        /// <param name="integralLimit">Maximum absolute value for integral term. Default 0 (disabled).</param>
        public static PID Bind(
            Func<float> input,
            float setpoint = DefaultSetpoint,
            float kP = DefaultKp,
            float kI = DefaultKi,
            float kD = DefaultKd,
            float outputMin = DefaultOutputMin,
            float outputMax = DefaultOutputMax,
            float integralLimit = DefaultIntegralLimit)
        {
            return new PID(input, null, setpoint, kP, kI, kD, outputMin, outputMax, integralLimit);
        }

        /// <summary>
        /// Creates a PID controller with bound input and dynamic setpoint.
        /// Call Update(deltaTime) and handle the returned output manually.
        /// </summary>
        /// <param name="input">Function that returns the current measured value.</param>
        /// <param name="setpoint">Function that returns the target value (evaluated each update).</param>
        /// <param name="kP">Proportional gain. Default 0.05.</param>
        /// <param name="kI">Integral gain. Default 250.</param>
        /// <param name="kD">Derivative gain. Default 0.3.</param>
        /// <param name="outputMin">Minimum output value. Default -1.</param>
        /// <param name="outputMax">Maximum output value. Default 1.</param>
        /// <param name="integralLimit">Maximum absolute value for integral term. Default 0 (disabled).</param>
        public static PID Bind(
            Func<float> input,
            Func<float> setpoint,
            float kP = DefaultKp,
            float kI = DefaultKi,
            float kD = DefaultKd,
            float outputMin = DefaultOutputMin,
            float outputMax = DefaultOutputMax,
            float integralLimit = DefaultIntegralLimit)
        {
            return new PID(input, null, setpoint, kP, kI, kD, outputMin, outputMax, integralLimit);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a standalone PID controller with default gains.
        /// Use Update(current, setpoint, deltaTime) to compute output.
        /// </summary>
        public PID()
            : this(DefaultKp, DefaultKi, DefaultKd, DefaultOutputMin, DefaultOutputMax, DefaultIntegralLimit)
        {
        }

        /// <summary>
        /// Creates a standalone PID controller. Use Update(current, setpoint, deltaTime) to compute output.
        /// </summary>
        /// <param name="kP">Proportional gain. Default 0.05.</param>
        /// <param name="kI">Integral gain. Default 250.</param>
        /// <param name="kD">Derivative gain. Default 0.3.</param>
        /// <param name="outputMin">Minimum output value. Default -1.</param>
        /// <param name="outputMax">Maximum output value. Default 1.</param>
        /// <param name="integralLimit">Maximum absolute value for integral term. Default 0 (disabled).</param>
        public PID(
            float kP = DefaultKp,
            float kI = DefaultKi,
            float kD = DefaultKd,
            float outputMin = DefaultOutputMin,
            float outputMax = DefaultOutputMax,
            float integralLimit = DefaultIntegralLimit)
        {
            _input = null;
            _output = null;
            _setpointFunc = null;
            Kp = kP;
            Ki = kI;
            Kd = kD;
            OutputMin = outputMin;
            OutputMax = outputMax;
            IntegralLimit = integralLimit;
            Setpoint = DefaultSetpoint;
        }

        /// <summary>
        /// Creates a PID controller with bound input and optional output (static setpoint).
        /// </summary>
        private PID(
            Func<float> input,
            Action<float>? output,
            float setpoint,
            float kP,
            float kI,
            float kD,
            float outputMin,
            float outputMax,
            float integralLimit)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _output = output;
            _setpointFunc = null;
            Setpoint = setpoint;
            Kp = kP;
            Ki = kI;
            Kd = kD;
            OutputMin = outputMin;
            OutputMax = outputMax;
            IntegralLimit = integralLimit;
        }

        /// <summary>
        /// Creates a PID controller with bound input, optional output, and dynamic setpoint.
        /// </summary>
        private PID(
            Func<float> input,
            Action<float>? output,
            Func<float> setpointFunc,
            float kP,
            float kI,
            float kD,
            float outputMin,
            float outputMax,
            float integralLimit)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _output = output;
            _setpointFunc = setpointFunc ?? throw new ArgumentNullException(nameof(setpointFunc));
            Setpoint = DefaultSetpoint; // Will be overwritten each update
            Kp = kP;
            Ki = kI;
            Kd = kD;
            OutputMin = outputMin;
            OutputMax = outputMax;
            IntegralLimit = integralLimit;
        }

        #endregion

        /// <summary>
        /// Updates the PID using bound input/output. Reads current value, calculates output, and writes to bound output.
        /// If a setpoint function is bound, it will be evaluated each update.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update in seconds.</param>
        /// <returns>The control output value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no input function was bound.</exception>
        public float Update(float deltaTime)
        {
            if (_input == null)
                throw new InvalidOperationException("No input function bound. Use Update(current, setpoint, deltaTime) or create PID with an input function.");

            // Evaluate dynamic setpoint if bound
            if (_setpointFunc != null)
            {
                Setpoint = _setpointFunc();
            }

            float current = _input();
            float output = Update(current, Setpoint, deltaTime);

            _output?.Invoke(output);
            return output;
        }

        /// <summary>
        /// Updates the PID with explicit current and setpoint values.
        /// </summary>
        /// <param name="current">The current measured value.</param>
        /// <param name="setpoint">The target value.</param>
        /// <param name="deltaTime">Time elapsed since last update in seconds.</param>
        /// <returns>The control output value.</returns>
        public float Update(float current, float setpoint, float deltaTime)
        {
            float error = setpoint - current;
            return UpdateWithError(error, deltaTime);
        }

        /// <summary>
        /// Updates the PID with a pre-calculated error value.
        /// </summary>
        /// <param name="error">The current error (setpoint - measured value).</param>
        /// <param name="deltaTime">Time elapsed since last update in seconds.</param>
        /// <returns>The control output value.</returns>
        public float UpdateWithError(float error, float deltaTime)
        {
            LastError = error;

            if (deltaTime <= 0f)
            {
                LastOutput = 0f;
                return 0f;
            }

            // Proportional term
            float proportional = Kp * error;

            // Integral term
            Integral += error * deltaTime;
            if (IntegralLimit > 0f)
            {
                Integral = Clamp(Integral, -IntegralLimit, IntegralLimit);
            }
            float integral = Ki * Integral;

            // Derivative term
            float derivative = 0f;
            if (!IsFirstUpdate)
            {
                derivative = Kd * (error - PreviousError) / deltaTime;
            }

            PreviousError = error;
            IsFirstUpdate = false;

            float output = proportional + integral + derivative;

            // Apply output clamping if configured
            if (OutputMin != 0f || OutputMax != 0f)
            {
                output = Clamp(output, OutputMin, OutputMax);
            }

            LastOutput = output;
            return output;
        }

        /// <summary>
        /// Resets the controller state, clearing the integral and previous error.
        /// Call this when the setpoint changes significantly or control is interrupted.
        /// </summary>
        public void Reset()
        {
            Integral = 0f;
            PreviousError = 0f;
            IsFirstUpdate = true;
            LastOutput = 0f;
            LastError = 0f;
        }

        /// <summary>
        /// Resets only the integral term, keeping the derivative calculation intact.
        /// Useful when you want to clear windup without losing derivative smoothing.
        /// </summary>
        public void ResetIntegral()
        {
            Integral = 0f;
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
