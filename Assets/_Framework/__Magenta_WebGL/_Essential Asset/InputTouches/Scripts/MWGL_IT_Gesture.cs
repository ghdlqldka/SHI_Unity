using UnityEngine;
using System.Collections;

namespace _Magenta_WebGL
{
	[RequireComponent(typeof(MWGL_BasicDetector))]
	[RequireComponent(typeof(MWGL_DragDetector))]
	[RequireComponent(typeof(MWGL_TapDetector))]
	[RequireComponent(typeof(SwipeDetector))]
	[RequireComponent(typeof(DualFingerDetector))]

	public class MWGL_IT_Gesture : _Magenta_Framework.IT_GestureEx
    {
        private static string LOG_FORMAT = "<color=#94B530><b>[MWGL_IT_Gesture]</b></color> {0}";

        public static new MWGL_IT_Gesture Instance
        {
            get
            {
                return instance as MWGL_IT_Gesture;
            }
            protected set
            {
                instance = value;
            }
        }

        protected override void Awake()
        {
            if (Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "Awake()");
                Instance = this;

                Debug.LogWarningFormat(LOG_FORMAT, "<b><color=yellow>DPI : " + GetCurrentDPI() + "</color></b>");
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }
        }

        protected override void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }
    }
}