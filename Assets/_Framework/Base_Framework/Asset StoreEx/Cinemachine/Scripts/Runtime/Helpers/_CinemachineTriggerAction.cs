// #if CINEMACHINE_PHYSICS || CINEMACHINE_PHYSICS_2D
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

#if CINEMACHINE_TIMELINE
using UnityEngine.Playables;
#endif

namespace Unity.Cinemachine
{
    /// <summary>
    /// A multi-purpose script which causes an action to occur when
    /// a trigger collider is entered and exited.
    /// </summary>
    [SaveDuringPlay]
    // [AddComponentMenu("Cinemachine/Helpers/Cinemachine Trigger Action")]
    // [HelpURL(Documentation.BaseURL + "manual/CinemachineTriggerAction.html")]
    public class _CinemachineTriggerAction : CinemachineTriggerAction
    {
        private static string LOG_FORMAT = "<color=#C09F19><b>[_CinemachineTriggerAction]</b></color> {0}";

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");
        }

    }
}
// #endif
