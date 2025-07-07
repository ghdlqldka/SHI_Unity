// using _Base_Framework;
// using _Magenta_Framework;
using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Magenta_WebGL
{
    public class MWGL_UI_GlobalHandler : _Magenta_Framework.UI_GlobalHandlerEx
    {
        private static string LOG_FORMAT = "<color=white><b>[MWGL_UI_GlobalHandler]</b></color> {0}";

        public static new MWGL_UI_GlobalHandler Instance
        {
            get
            {
                return _instance as MWGL_UI_GlobalHandler;
            }
            protected set
            {
                _instance = value;
            }
        }

        protected override void Awake()
        {
            // base.Awake();

            if (Instance == null)
            {
                Instance = this;

                if (_Magenta_WebGL_Config.Product == _Base_Framework._Base_Framework_Config._Product.Magenta_WebGL)
                {
                    if (MWGL_GlobalObjectUtilities.Instance == null)
                    {
                        GameObject prefab = Resources.Load<GameObject>(MWGL_GlobalObjectUtilities.prefabPath);
                        Debug.Assert(prefab != null);
                        GameObject obj = Instantiate(prefab);
                        Debug.LogFormat(LOG_FORMAT, "<color=magenta>Instantiate \"<b>MWGL_GlobalObjectUtilities</b>\"</color>");
                        obj.name = prefab.name;
                    }
                }
                else
                {
                    Debug.Assert(false);
                }

                ShowScreenCaptureButton(false); // Enable/Disable "Panel_ScreenCapture"

                StartCoroutine(PostAwake());
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Instance.gameObject.name : " + Instance.gameObject.name + ", this.gameObject.name : " + this.gameObject.name);
                Destroy(this);
                return;
            }
        }

        protected override IEnumerator PostAwake()
        {
            while (MWGL_PlayerPrefsManager.Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "yield return null");
                yield return null;
            }

            MWGL_PlayerPrefsManager.Instance.OnReset += OnPlayerPrefsManagerReset;
        }

        public override void ShowScreenCaptureButton(bool show)
        {
            Debug.LogFormat(LOG_FORMAT, "ShowScreenCaptureButton(), show : " + show);

            if (MWGL_GlobalObjectUtilities.Instance.EnableScreenCapture == true)
            {
                screenCaptureHandler.ShowCaptureButton(show);
            }
            else // Not support ScreenCapture
            {
                Debug.Assert(show == false);
                screenCaptureHandler.gameObject.SetActive(false);
                screenCaptureHandler = null;
            }
        }
    }
}