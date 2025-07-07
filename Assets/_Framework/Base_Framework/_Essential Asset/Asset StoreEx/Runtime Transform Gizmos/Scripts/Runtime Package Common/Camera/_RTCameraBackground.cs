using UnityEngine;
using System.Collections.Generic;

namespace RTG
{
    public class _RTCameraBackground : RTCameraBackground
    {
        private static string LOG_FORMAT = "<color=#AA3B00><b>[_RTCameraBackground]</b></color> {0}";

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
        }
    }
}
