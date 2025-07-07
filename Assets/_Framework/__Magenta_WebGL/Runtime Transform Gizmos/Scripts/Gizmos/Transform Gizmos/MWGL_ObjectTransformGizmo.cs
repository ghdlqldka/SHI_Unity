using UnityEngine;
using System;
using System.Collections.Generic;

namespace _Magenta_WebGL
{
    [Serializable]
    public class MWGL_ObjectTransformGizmo : _Magenta_Framework.ObjectTransformGizmoEx
    {
        private static string LOG_FORMAT = "<color=#773B00><b>[MWGL_ObjectTransformGizmo]</b></color> {0}";

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
