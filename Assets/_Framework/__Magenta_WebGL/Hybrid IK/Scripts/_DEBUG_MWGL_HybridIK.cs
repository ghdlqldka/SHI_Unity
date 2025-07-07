using System.Collections.Generic;
using UnityEngine;

namespace _Magenta_WebGL
{
    public class _DEBUG_MWGL_HybridIK : _DEBUG_HybridIK
    {
        private static string LOG_FORMAT = "<color=#00FF0E><b>[_DEBUG_MWGL_HybridIK]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");
        }
    }
}