using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#elif PLATFORM_IOS
using UnityEngine.iOS;
#endif

namespace _Magenta_Framework
{
    public class PermissionManagerEx : _Base_Framework._PermissionManager
    {
        private static string LOG_FORMAT = "<color=#4C64FF><b>[PermissionManagerEx]</b></color> {0}";

        public static new PermissionManagerEx Instance
        {
            get
            {
                return _instance as PermissionManagerEx;
            }
            protected set
            {
                _instance = value;
            }
        }

        protected override void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake()");

            if (Instance == null)
            {
                Instance = this;

#if PLATFORM_ANDROID
                //
#elif PLATFORM_IOS
                //
#else
                Destroy(this.gameObject); // <=========================
#endif
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