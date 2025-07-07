using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace _Magenta_WebGL
{
    public class MWGL_LocalizationManager : _Magenta_Framework.LocalizationManagerEx
    {
        private static string LOG_FORMAT = "<color=#00FBFF><b>[MWGL_LocalizationManager]</b></color> {0}";

        public static new MWGL_LocalizationManager Instance
        {
            get
            {
                return _instance as MWGL_LocalizationManager;
            }
            protected set
            {
                _instance = value;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            if (Instance == null)
            {
                Instance = this;

#if DEBUG
                DEBUG_availableLocaleList = LocalizationSettings.AvailableLocales.Locales;
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