using System.Collections;
using System.Collections.Generic;
using AshqarApps.DynamicJoint;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace _Magenta_WebGL
{
    public class MWGL_DynamicJointLimitHinge : _DynamicJointLimitHinge
    {
        private static string LOG_FORMAT = "<color=#F6C130><b>[_DynamicJointLimitHinge]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");
            // base.Awake();

            // Store the local rotation to map the zero rotation point to the current rotation
            if (zeroRotationSet == false)
                SetZeroRotation();

            if (mainAxis == Vector3.zero)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Axis is Vector3.zero.");
                mainAxis = Vector3.forward;
            }

            initialized = true;
        }
    }
}