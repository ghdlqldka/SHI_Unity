using UnityEngine;

namespace Unity.Cinemachine
{
    /// <summary>
    /// This is a CinemachineComponent in the Body section of the component pipeline.
    /// Its job is to position the camera in a fixed relationship to the vcam's Tracking
    /// Target object, with offsets and damping.
    ///
    /// This component will only change the camera's position in space.  It will not
    /// re-orient or otherwise aim the camera.  To to that, you need to instruct
    /// the camera in the Aim section of its pipeline.
    /// </summary>
    // [AddComponentMenu("Cinemachine/Procedural/Position Control/Cinemachine Follow")]
    [SaveDuringPlay]
    [DisallowMultipleComponent]
    [CameraPipeline(CinemachineCore.Stage.Body)]
    [RequiredTarget(RequiredTargetAttribute.RequiredTargets.Tracking)]
    // [HelpURL(Documentation.BaseURL + "manual/CinemachineFollow.html")]
    public class _CinemachineFollow : CinemachineFollow
    {
        private static string LOG_FORMAT = "<color=#997AC1><b>[_CinemachineFollow]</b></color> {0}";

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
        }
    }
}
