using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Magenta_WebGL
{
    public class MWGL_BGMManager : _Magenta_Framework.BGMManagerEx
    {
        private static string LOG_FORMAT = "<color=#CF9F19><b>[MWGL_BGMManager]</b></color> {0}";

        public static new MWGL_BGMManager Instance
        {
            get
            {
                return _instance as MWGL_BGMManager;
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

                Debug.Assert(audioSource != null);
                Debug.Assert(audioSource.playOnAwake == false);
                Debug.Assert(audioSource.loop == true);
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