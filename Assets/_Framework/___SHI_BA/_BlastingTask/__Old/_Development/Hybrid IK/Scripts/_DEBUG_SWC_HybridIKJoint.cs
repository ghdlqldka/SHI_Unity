using System.Collections.Generic;
using UnityEngine;

namespace _SHI_BA
{
    public class _DEBUG_SWC_HybridIKJoint : _Magenta_WebGL._DEBUG_MWGL_HybridIKJoint
    {
        private static string LOG_FORMAT = "<color=#00FF0E><b>[_DEBUG_SWC_HybridIKJoint]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");
        }
    }
}