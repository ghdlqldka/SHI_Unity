using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Magenta_Framework
{
    public class FxManagerEx : _Base_Framework._FxManager
    {
        private static string LOG_FORMAT = "<color=#A4D298><b>[FxManagerEx]</b></color> {0}";

        public static new FxManagerEx Instance
        {
            get
            {
                return _instance as FxManagerEx;
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