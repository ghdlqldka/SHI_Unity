using UnityEngine;
using System;
using System.Collections.Generic;

namespace _Magenta_Framework
{
    [Serializable]
    public class MoveGizmoEx : RTG._MoveGizmo
    {
        private static string LOG_FORMAT = "<color=#773B00><b>[MoveGizmoEx]</b></color> {0}";

        public new GizmoEx Gizmo 
        { 
            get 
            { 
                return _gizmo as GizmoEx;
            }
        }

        public MoveGizmoEx()
        {
            Debug.LogFormat(LOG_FORMAT, "constructor!!!!!!");
        }
    }
}
