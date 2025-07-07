using UnityEngine;
using System;

namespace RTG
{
    [Serializable]
    public class _MoveGizmo : MoveGizmo
    {
        private static string LOG_FORMAT = "<color=#773B00><b>[_MoveGizmo]</b></color> {0}";

        public new _Gizmo Gizmo 
        { 
            get 
            { 
                return _gizmo as _Gizmo;
            }
        }

        public _MoveGizmo()
        {
            Debug.LogFormat(LOG_FORMAT, "constructor!!!!!!");
        }
    }
}
