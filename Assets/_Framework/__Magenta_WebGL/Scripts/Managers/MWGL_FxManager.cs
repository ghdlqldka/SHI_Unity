using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Magenta_WebGL
{
    public class MWGL_FxManager : _Magenta_Framework.FxManagerEx
    {
        private static string LOG_FORMAT = "<color=#A4D298><b>[MWGL_FxManager]</b></color> {0}";

        public static new MWGL_FxManager Instance
        {
            get
            {
                return _instance as MWGL_FxManager;
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
                Debug.Assert(audioSource.loop == false);
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