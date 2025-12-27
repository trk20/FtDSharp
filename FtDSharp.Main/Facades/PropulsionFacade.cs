using BrilliantSkies.Common.Controls.ConstructModules;
using UnityEngine;

namespace FtDSharp.Facades
{
    internal class PropulsionFacade : IPropulsion
    {

        // todo: custom axis controls
        private readonly MainConstruct _construct;

        public PropulsionFacade(MainConstruct construct)
        {
            _construct = construct;
        }

        private bool CanMakeRequest => !_construct.DockingRestricted.AmIBeingTractored();

        private void SetAxis(ControlType positive, ControlType negative, float value)
        {
            if (!CanMakeRequest) return;
            value = float.IsNaN(value) ? 0 : Mathf.Clamp(value, -1f, 1f);

            if (value >= 0)
            {
                _construct.ControlsRestricted.SetRequest(positive, value);
                _construct.ControlsRestricted.SetRequest(negative, 0);
            }
            else
            {
                _construct.ControlsRestricted.SetRequest(positive, 0);
                _construct.ControlsRestricted.SetRequest(negative, -value);
            }
        }

        public float GetAxis(ControlType controlType)
        {
            return _construct.ControlsRestricted.Last.GetInput(controlType);
        }


        public float Forwards
        {
            get => GetAxis(ControlType.ThrustForward);
            set => SetAxis(ControlType.ThrustForward, ControlType.ThrustBackward, value);
        }

        public float Strafe
        {
            get => GetAxis(ControlType.StrafeRight);
            set => SetAxis(ControlType.StrafeRight, ControlType.StrafeLeft, value);
        }

        public float Hover
        {
            get => GetAxis(ControlType.HoverUp);
            set => SetAxis(ControlType.HoverUp, ControlType.HoverDown, value);
        }

        public float Yaw
        {
            get => GetAxis(ControlType.YawRight);
            set => SetAxis(ControlType.YawRight, ControlType.YawLeft, value);
        }

        public float Pitch
        {
            get => GetAxis(ControlType.PitchUp);
            set => SetAxis(ControlType.PitchUp, ControlType.PitchDown, value);
        }

        public float Roll
        {
            get => GetAxis(ControlType.RollRight);
            set => SetAxis(ControlType.RollRight, ControlType.RollLeft, value);
        }

        public float A
        {
            get => _construct.ControlsRestricted.Last.GetInput(ControlType.A);
            set => _construct.ControlsRestricted.SetRequest(ControlType.A, float.IsNaN(value) ? 0 : Mathf.Clamp(value, -1f, 1f));
        }

        public float B
        {
            get => _construct.ControlsRestricted.Last.GetInput(ControlType.B);
            set => _construct.ControlsRestricted.SetRequest(ControlType.B, float.IsNaN(value) ? 0 : Mathf.Clamp(value, -1f, 1f));
        }

        public float C
        {
            get => _construct.ControlsRestricted.Last.GetInput(ControlType.C);
            set => _construct.ControlsRestricted.SetRequest(ControlType.C, float.IsNaN(value) ? 0 : Mathf.Clamp(value, -1f, 1f));
        }

        public float D
        {
            get => _construct.ControlsRestricted.Last.GetInput(ControlType.D);
            set => _construct.ControlsRestricted.SetRequest(ControlType.D, float.IsNaN(value) ? 0 : Mathf.Clamp(value, -1f, 1f));
        }

        public float E
        {
            get => _construct.ControlsRestricted.Last.GetInput(ControlType.E);
            set => _construct.ControlsRestricted.SetRequest(ControlType.E, float.IsNaN(value) ? 0 : Mathf.Clamp(value, -1f, 1f));
        }

        public float MainDrive
        {
            get => _construct.ControlsRestricted.Last.GetDrive(Drive.Main);
            set => _construct.ControlsRestricted.SetDrive(Drive.Main, float.IsNaN(value) ? 0 : Mathf.Clamp(value, -1f, 1f));

        }

        public float SecondaryDrive
        {
            get => _construct.ControlsRestricted.Last.GetDrive(Drive.Secondary);
            set => _construct.ControlsRestricted.SetDrive(Drive.Secondary, float.IsNaN(value) ? 0 : Mathf.Clamp(value, -1f, 1f));
        }

        public float TertiaryDrive
        {
            get => _construct.ControlsRestricted.Last.GetDrive(Drive.Tertiary);
            set => _construct.ControlsRestricted.SetDrive(Drive.Tertiary, float.IsNaN(value) ? 0 : Mathf.Clamp(value, -1f, 1f));
        }

    }
}
