using UnityEngine;

namespace Unity.Cinemachine
{
    /// <summary>
    /// This is a CinemachineComponent in the the Body section of the component pipeline.
    /// Its job is to position the camera somewhere on a spheroid centered at the Follow target.
    ///
    /// The position on the sphere and the radius of the sphere can be controlled by user input.
    /// </summary>
    // [AddComponentMenu("Cinemachine/Procedural/Position Control/Cinemachine Orbital Follow")]
    [SaveDuringPlay]
    [DisallowMultipleComponent]
    [CameraPipeline(CinemachineCore.Stage.Body)]
    [RequiredTarget(RequiredTargetAttribute.RequiredTargets.Tracking)]
    // [HelpURL(Documentation.BaseURL + "manual/CinemachineOrbitalFollow.html")]
    public class _CinemachineOrbitalFollow : CinemachineOrbitalFollow
    {
        private static string LOG_FORMAT = "<color=#997AC1><b>[_CinemachineOrbitalFollow]</b></color> {0}";

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
        }
    }
}
