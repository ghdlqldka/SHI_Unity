using UnityEngine;
using System;
using System.Collections.Generic;

namespace _Magenta_WebGL
{
    [Serializable]
    public class MWGL_MoveGizmo : _Magenta_Framework.MoveGizmoEx
    {
        private static string LOG_FORMAT = "<color=#773B00><b>[MWGL_MoveGizmo]</b></color> {0}";

        public new MWGL_Gizmo Gizmo 
        { 
            get 
            { 
                return _gizmo as MWGL_Gizmo;
            }
        }

        public MWGL_MoveGizmo()
        {
            Debug.LogFormat(LOG_FORMAT, "constructor!!!!!!");
        }
    }
}
