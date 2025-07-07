using UnityEngine;

namespace Unity.Cinemachine
{
    /// <summary>
    /// This component will generate CinemachineBrain-specific events.
    /// </summary>
    // [AddComponentMenu("Cinemachine/Helpers/Cinemachine Brain Events")]
    [SaveDuringPlay]
    // [HelpURL(Documentation.BaseURL + "manual/CinemachineBrainEvents.html")]
    public class _CinemachineBrainEvents : CinemachineBrainEvents
    {
        private static string LOG_FORMAT = "<color=#F0F4D6><b>[_CinemachineBrainEvents]</b></color> {0}";

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
            // base.Awake();

            Debug.Assert(Brain != null);
        }
    }
}
