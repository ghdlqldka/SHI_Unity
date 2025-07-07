using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace _Magenta_Framework
{
    public class LocalizationManagerEx : _Base_Framework._LocalizationManager
    {
        private static string LOG_FORMAT = "<color=#00FBFF><b>[LocalizationManagerEx]</b></color> {0}";

        public static new LocalizationManagerEx Instance
        {
            get
            {
                return _instance as LocalizationManagerEx;
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