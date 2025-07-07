using System.Collections;
using System.Collections.Generic;
// using _Magenta_Framework;
using UnityEngine;

namespace _Magenta_WebGL
{
    public class MWGL_PlayerPrefsManager : _Magenta_Framework.PlayerPrefsManagerEx
    {
        private static string LOG_FORMAT = "<color=#5A934B><b>[MWGL_PlayerPrefsManager]</b></color> {0}";

        public static new MWGL_PlayerPrefsManager Instance
        {
            get
            {
                return _instance as MWGL_PlayerPrefsManager;
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

                Debug.Assert(_Magenta_WebGL_Config.Product == _Base_Framework._Base_Framework_Config._Product.Magenta_WebGL); // Must be override this function!!!!!

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

    }
}