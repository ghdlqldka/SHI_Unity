using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework
{
    public class _UI_GlobalHandler : _UI_BaseHandler
    {
        private static string LOG_FORMAT = "<color=white><b>[_UI_GlobalHandler]</b></color> {0}";

        protected static _UI_GlobalHandler _instance;
        public static _UI_GlobalHandler Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        [Header("Topmost")]
        // [Space(10)]
        [SerializeField]
        protected _UI_PopupDialog_Landscape _popupDialog; // Landscape
        public _UI_PopupDialog_Landscape PopupDialog
        {
            get
            {
                return _popupDialog;
            }
        }
        [SerializeField]
        protected _UI_PopupDialog_Portrait _popupDialog_Portrait; // Portrait
        public _UI_PopupDialog_Portrait PopupDialog_Portrait
        {
            get
            {
                return _popupDialog_Portrait;
            }
        }

        [SerializeField]
        protected _UI_ToastMessageHandler _toastMessageHandler;
        public _UI_ToastMessageHandler Toast
        {
            get
            {
                return _toastMessageHandler;
            }
        }

        [SerializeField]
        protected _UI_SubtitleWindow _subtitleWindow;
        public _UI_SubtitleWindow SubtitleWindow
        {
            get
            {
                return _subtitleWindow;
            }
        }

        [Space(10)]
        [SerializeField]
        protected _UI_ScreenCaptureHandler screenCaptureHandler;

        [Header("Loading")]
        [SerializeField]
        protected _UI_LoadingHandler _loadingHandler;

        protected _PlayerPrefsManager PlayerPrefsManager
        {
            get
            {
                return _PlayerPrefsManager.Instance;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");
            // base.Awake();

            if (Instance == null)
            {
                Instance = this;

                if (_Base_Framework_Config.Product == _Base_Framework_Config._Product.Base_Framework)
                {
                    if (_GlobalObjectUtilities.Instance == null)
                    {
                        GameObject prefab = Resources.Load<GameObject>(_GlobalObjectUtilities.prefabPath);
                        Debug.Assert(prefab != null);
                        GameObject obj = Instantiate(prefab);
                        Debug.LogFormat(LOG_FORMAT, "<color=magenta>Instantiate \"<b>GlobalObjectUtilities</b>\"</color>");
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

        protected virtual IEnumerator PostAwake()
        {
            while (PlayerPrefsManager == null)
            {
                Debug.LogFormat(LOG_FORMAT, "yield return null");
                yield return null;
            }

            PlayerPrefsManager.OnReset += OnPlayerPrefsManagerReset;
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            if (PlayerPrefsManager != null)
            {
                PlayerPrefsManager.OnReset -= OnPlayerPrefsManagerReset;
            }

            Instance = null;
        }

        protected virtual void Start()
        {
#if UNITY_ANDROID || UNITY_IOS
            //
#endif
        }

        /*
        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                QuitApplication();
            }
        }
        */

        protected override void HandleKeyCode()
        {
            // throw new System.NotImplementedException();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                QuitApplication();
            }
        }

#if DEBUG
        protected virtual void OnDialogResult(Button button)
        {
            string buttonText = button.GetComponent<_MUI_Button>().buttonText;
            Debug.LogWarningFormat(LOG_FORMAT, "<color=red><b>OnDialogResult()</b></color>, button : " + button.gameObject.name + ", buttonText : " + buttonText);
        }
#endif

        protected virtual void OnPlayerPrefsManagerReset()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "OnPlayerPrefsManagerReset()");
            // throw new System.NotImplementedException();

#if UNITY_ANDROID || UNITY_IOS
            //
#endif
        }

        protected string oneButtonText = "";
        public virtual void QuitApplication()
        {
            oneButtonText = "Yes";
            PopupDialog.Show("", "Application Quit?", oneButtonText, "No", QuitApplicationDialogResult);
        }

        protected virtual void QuitApplicationDialogResult(Button button)
        {
            string buttonText = button.GetComponent<_MUI_Button>().buttonText;
            Debug.LogWarningFormat(LOG_FORMAT, "<color=red><b>QuitApplicationDialogResult()</b></color>, button : " + button.gameObject.name + ", buttonText : " + buttonText);

            // if (button.gameObject.name.Contains("Button_One")) // Yes
            if (string.Compare(buttonText, oneButtonText) == 0) // Yes
            {
                StartCoroutine(DoApplicationQuit());
            }
            else
            {
                //
            }
        }

        protected virtual IEnumerator DoApplicationQuit()
        {
            yield return new WaitForSeconds(0.313f); // for button click sound

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public virtual void ResetApplication()
        {
            oneButtonText = "Yes";
            PopupDialog.Show("", "Reset all Application data?", oneButtonText, "No", ResetApplicationDialogResult);
        }

        protected virtual void ResetApplicationDialogResult(Button button)
        {
            string buttonText = button.GetComponent<_MUI_Button>().buttonText;
            Debug.LogWarningFormat(LOG_FORMAT, "<color=red><b>ResetApplicationDialogResult()</b></color>, button : <b>" + button.gameObject.name + "</b>, buttonText : <b>" + buttonText + "</b>");

            if (string.Compare(buttonText, oneButtonText) == 0) // Yes
            {
                PlayerPrefsManager._Reset();
            }
            else
            {
                //
            }
        }

        public virtual void ShowScreenCaptureButton(bool show)
        {
            Debug.LogFormat(LOG_FORMAT, "ShowScreenCaptureButton(), show : <b>" + show + "</b>");

            if (_GlobalObjectUtilities.Instance.EnableScreenCapture == true)
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

        public virtual void LoadSceneAsync(string sceneName)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "LoadSceneAsync(), sceneName : <b><color=yellow>" + sceneName + "</color></b>");

            _loadingHandler.gameObject.SetActive(true);
            // throw new System.NotImplementedException("Check below");
            StartCoroutine(_loadingHandler.LoadSceneAsyncRoutine(sceneName));
        }
    }
}