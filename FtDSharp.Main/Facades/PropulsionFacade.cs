using BrilliantSkies.Common.Controls.ConstructModules;
using UnityEngine;

namespace FtDSharp.Facades
{
    internal class PropulsionFacade : IPropulsion
    {

        // todo: extra axis controls
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
    }
}
