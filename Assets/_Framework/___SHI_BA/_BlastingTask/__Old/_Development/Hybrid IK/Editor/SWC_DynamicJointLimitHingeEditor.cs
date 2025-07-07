#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace _SHI_BA
{
    [CustomEditor(typeof(SWC_DynamicJointLimitHinge))]
    [CanEditMultipleObjects]
    public class SWC_DynamicJointLimitHingeEditor : _Magenta_WebGL.MWGL_DynamicJointLimitHingeEditor
    {
        //
    }
}
#endif

