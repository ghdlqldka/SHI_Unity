using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Magenta_Framework
{
    public class FadeManagerEx : _Base_Framework._FadeManager
    {
        private static string LOG_FORMAT = "<color=#8DA278><b>[FadeManagerEx]</b></color> {0}";

        public static new FadeManagerEx Instance
        {
            get
            {
                return _instance as FadeManagerEx;
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

                // fadeImage = GetComponent<Image>();
                Debug.Assert(fadeImage != null);
                Debug.LogFormat(LOG_FORMAT, "Initial fadeImage alpha : <color=red><b>" + fadeImage.color.a + "</b></color>");
                fadeImage.enabled = false; // <= defaultly disable
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