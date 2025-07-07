using System.Collections;
using System.Collections.Generic;
using AshqarApps.DynamicJoint;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace _SHI_BA
{
    public class SWC_DynamicJointLimitHinge : _Magenta_WebGL.MWGL_DynamicJointLimitHinge
    {
        private static string LOG_FORMAT = "<color=#F6C130><b>[SWC_DynamicJointLimitHinge]</b></color> {0}";

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