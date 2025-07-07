using UnityEngine;
using System.Collections;

namespace _Magenta_Framework
{
	[RequireComponent(typeof(BasicDetectorEx))]
	[RequireComponent(typeof(DragDetectorEx))]
	[RequireComponent(typeof(TapDetectorEx))]
	[RequireComponent(typeof(SwipeDetector))]
	[RequireComponent(typeof(DualFingerDetector))]

	public class IT_GestureEx : _InputTouches._IT_Gesture
	{
        private static string LOG_FORMAT = "<color=#94B530><b>[IT_GestureEx]</b></color> {0}";

        public static new IT_GestureEx Instance
        {
            get
            {
                return instance as IT_GestureEx;
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