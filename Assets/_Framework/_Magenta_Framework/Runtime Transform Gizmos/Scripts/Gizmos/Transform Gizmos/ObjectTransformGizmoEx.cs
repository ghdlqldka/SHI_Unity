using UnityEngine;
using System;
using System.Collections.Generic;

namespace _Magenta_Framework
{
    [Serializable]
    public class ObjectTransformGizmoEx : RTG._ObjectTransformGizmo
    {
        private static string LOG_FORMAT = "<color=#773B00><b>[ObjectTransformGizmoEx]</b></color> {0}";

        public override void SetEnabled(bool enabled)
        {
            Debug.LogFormat(LOG_FORMAT, "SetEnabled(), enabled : " + enabled + ", _isEnabled : " + _isEnabled);

            if (enabled == _isEnabled)
                return;

            if (enabled)
            {
                _isEnabled = enabled;
                OnEnabled();
            }
            else
            {
                _isEnabled = false;
                OnDisabled();
            }
        }
    }
}
