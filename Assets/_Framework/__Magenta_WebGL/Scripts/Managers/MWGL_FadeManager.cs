using UnityEngine;

namespace _Magenta_WebGL
{
    public class MWGL_FadeManager : _Magenta_Framework.FadeManagerEx
    {
        private static string LOG_FORMAT = "<color=#8DA278><b>[MWGL_FadeManager]</b></color> {0}";

        public static new MWGL_FadeManager Instance
        {
            get
            {
                return _instance as MWGL_FadeManager;
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