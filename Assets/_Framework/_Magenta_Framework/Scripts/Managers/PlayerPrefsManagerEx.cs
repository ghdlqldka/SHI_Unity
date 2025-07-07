using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Magenta_Framework
{
    public class PlayerPrefsManagerEx : _Base_Framework._PlayerPrefsManager
    {
        private static string LOG_FORMAT = "<color=#5A934B><b>[PlayerPrefsManagerEx]</b></color> {0}";

        public static new PlayerPrefsManagerEx Instance
        {
            get
            {
                return _instance as PlayerPrefsManagerEx;
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

                Debug.Assert(_Magenta_Framework_Config.Product == _Base_Framework._Base_Framework_Config._Product.Magenta_Framework); // Must be override this function!!!!!

                Instance = this;

                Init();
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

        public override void _Reset()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "<b>_Reset()</b>");

            PlayerPrefs.DeleteAll();

            Init();

            Invoke_OnReset();
        }
    }
}