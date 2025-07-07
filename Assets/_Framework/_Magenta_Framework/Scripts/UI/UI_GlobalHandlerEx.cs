using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Magenta_Framework
{
    public class UI_GlobalHandlerEx : _Base_Framework._UI_GlobalHandler
    {
        private static string LOG_FORMAT = "<color=white><b>[UI_GlobalHandlerEx]</b></color> {0}";

        public static new UI_GlobalHandlerEx Instance
        {
            get
            {
                return _instance as UI_GlobalHandlerEx;
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

                if (_Magenta_Framework_Config.Product == _Base_Framework._Base_Framework_Config._Product.Magenta_Framework)
                {
                    if (GlobalObjectUtilitiesEx.Instance == null)
                    {
                        GameObject prefab = Resources.Load<GameObject>(GlobalObjectUtilitiesEx.prefabPath);
                        Debug.Assert(prefab != null);
                        GameObject obj = Instantiate(prefab);
                        Debug.LogFormat(LOG_FORMAT, "<color=magenta>Instantiate \"<b>GlobalObjectUtilitiesEx</b>\"</color>");
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
            while (PlayerPrefsManagerEx.Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "yield return null");
                yield return null;
            }

            PlayerPrefsManagerEx.Instance.OnReset += OnPlayerPrefsManagerReset;
        }

        public override void ShowScreenCaptureButton(bool show)
        {
            Debug.LogFormat(LOG_FORMAT, "ShowScreenCaptureButton(), show : " + show);

            if (GlobalObjectUtilitiesEx.Instance.EnableScreenCapture == true)
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

        public override void LoadSceneAsync(string sceneName)
        {
            _loadingHandler.gameObject.SetActive(true);
            throw new System.NotImplementedException("Check below");
            // StartCoroutine(_loadingHandler.LoadSceneAsync(sceneName));
        }
    }
}