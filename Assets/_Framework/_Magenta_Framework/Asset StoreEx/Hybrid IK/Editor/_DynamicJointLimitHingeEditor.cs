#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AshqarApps.DynamicJoint
{
    [CustomEditor(typeof(_DynamicJointLimitHinge))]
    [CanEditMultipleObjects]
    public class _DynamicJointLimitHingeEditor : DynamicJointLimitHingeInspector
    {
        //
    }
}
#endif

