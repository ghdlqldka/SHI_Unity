// #if CINEMACHINE_PHYSICS

using UnityEngine;
using System.Collections.Generic;
using System;

namespace Unity.Cinemachine
{
    /// <summary>
    /// An add-on module for CinemachineCamera that post-processes
    /// the final position of the camera. Based on the supplied settings,
    /// the Deoccluder will attempt to preserve the line of sight
    /// with the LookAt target of the camera by moving
    /// away from objects that will obstruct the view.
    ///
    /// Additionally, the Deoccluder can be used to assess the shot quality and
    /// report this as a field in the camera State.
    /// </summary>
    // [AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine Deoccluder")]
    [SaveDuringPlay]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequiredTarget(RequiredTargetAttribute.RequiredTargets.Tracking)]
    // [HelpURL(Documentation.BaseURL + "manual/CinemachineDeoccluder.html")]
    public class _CinemachineDeoccluder : CinemachineDeoccluder
    {
        private static string LOG_FORMAT = "<color=#8DA278><b>[_CinemachineDeoccluder]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            // base.Awake();
            ConnectToVcam(true);
        }

        public override void OnTargetObjectWarped(CinemachineVirtualCameraBase vcam, Transform target, Vector3 positionDelta)
        {
            Debug.LogFormat(LOG_FORMAT, "OnTargetObjectWarped()");

            base.OnTargetObjectWarped (vcam, target, positionDelta);
        }

        public override void ForceCameraPosition(CinemachineVirtualCameraBase vcam, Vector3 pos, Quaternion rot)
        {
            Debug.LogFormat(LOG_FORMAT, "ForceCameraPosition()");

            base.ForceCameraPosition(vcam, pos, rot);
        }
    }
}
// #endif
