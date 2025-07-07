#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace _Magenta_WebGL
{
    [CustomEditor(typeof(MWGL_DynamicJointLimitHinge))]
    [CanEditMultipleObjects]
    public class MWGL_DynamicJointLimitHingeEditor : AshqarApps.DynamicJoint._DynamicJointLimitHingeEditor
    {
        //
    }
}
#endif

