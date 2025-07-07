using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Magenta_Framework
{
    /// <summary>
    /// Cinemachine ClearShot is a "manager camera" that owns and manages a set of
    /// Virtual Camera gameObject children.  When Live, the ClearShot will check the
    /// children, and choose the one with the best quality shot and make it Live.
    ///
    /// This can be a very powerful tool.  If the child cameras have shot evaluator
    /// extensions, they will analyze the scene for target obstructions, optimal target
    /// distance, and other items, and report their assessment of shot quality back to
    /// the ClearShot parent, who will then choose the best one.  You can use this to set
    /// up complex multi-camera coverage of a scene, and be assured that a clear shot of
    /// the target will always be available.
    ///
    /// If multiple child cameras have the same shot quality, the one with the highest
    /// priority will be chosen.
    ///
    /// You can also define custom blends between the ClearShot children.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [ExcludeFromPreset]
    [SaveDuringPlay]
    // [AddComponentMenu("Cinemachine/Cinemachine ClearShot")]
    // [HelpURL(Documentation.BaseURL + "manual/CinemachineClearShot.html")]
    public class CinemachineClearShotEx : Unity.Cinemachine._CinemachineClearShot
    {
        private static string LOG_FORMAT = "<color=#4C64FF><b>[CinemachineClearShotEx]</b></color> {0}";

        protected override void Reset()
        {
            Debug.LogFormat(LOG_FORMAT, "Reset()");
            base.Reset();
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");
        }
    }
}
