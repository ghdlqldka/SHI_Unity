using UnityEngine;

namespace Unity.Cinemachine.Samples
{
    /// <summary>
    /// Orients the GameObject that this script is attached in such a way that it always faces the Target.
    /// </summary>
    [ExecuteAlways]
    public class _LookAtTarget : LookAtTarget
    {
        private static string LOG_FORMAT = "<color=#CF9F19><b>[_LookAtTarget]</b></color> {0}";

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");
        }
    }
}
