using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Magenta_Framework
{
    public class ScreenCaptureManagerEx : _Base_Framework._ScreenCaptureManager
    {
        private static string LOG_FORMAT = "<color=#997AC1><b>[ScreenCaptureManagerEx]</b></color> {0}";

        public static new ScreenCaptureManagerEx Instance
        {
            get
            {
                return _instance as ScreenCaptureManagerEx;
            }
            protected set
            {
                _instance = value;
            }
        }

        protected override void Awake()
        {
            if (Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "Awake()");

                Instance = this;
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