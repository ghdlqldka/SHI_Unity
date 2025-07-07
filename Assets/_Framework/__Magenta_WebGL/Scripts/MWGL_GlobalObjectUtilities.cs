using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
//using _Base_Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace _Magenta_WebGL
{
    public class MWGL_GlobalObjectUtilities : _Magenta_Framework.GlobalObjectUtilitiesEx
    {
        private static string LOG_FORMAT = "<b><color=magenta>[MWGL_GlobalObjectUtilities]</color></b> {0}";

        public static new MWGL_GlobalObjectUtilities Instance
        {
            get
            {
                return _instance as MWGL_GlobalObjectUtilities;
            }
            protected set
            {
                _instance = value;
            }
        }

#if ENABLE_UNITY_GAME_SERVICES_REMOTE_CONFIG_SUPPORT
        public new RemoteConfigManagerEx RemoteConfig
        {
            get
            {
                return _remoteConfig as RemoteConfigManagerEx;
            }
        }
#endif

        public static new string prefabPath = "Magenta_WebGL/MWGL_GlobalObjectUtilities";

        protected override void Awake()
        {
            // base.Awake();

            if (Instance == null)
            {
#if DEBUG
                Debug.LogFormat(LOG_FORMAT, "Awake(), Display.displays.Length : <b><color=yellow>" + Display.displays.Length +
                    "</color></b>, RenderPileline : <b><color=yellow>" + _Utilities.GetRenderPipelineType() + "</color></b>");
#endif

                Debug.Assert(_networkManager != null);

#if UNITY_ANDROID || UNITY_IOS
                // Disable screen dimming
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif

#if ENABLE_UNITY_GAME_SERVICES_REMOTE_CONFIG_SUPPORT
                _isReadyRemoteConfig = false;
                RemoteConfigManagerEx.OnConfigRequestStatusChanged += OnRemoteConfigRequestStatusChanged;
                _remoteConfig = this.transform.Find("Unity Services").gameObject.AddMissingComponent<RemoteConfigManagerEx>();
#else
                _isReadyRemoteConfig = true;
#endif

#if DEBUG
                if (_Magenta_WebGL_Config.Product != _Base_Framework._Base_Framework_Config._Product.Magenta_WebGL)
                {
                    Debug.Assert(false);
                }
#endif

                Instance = this;
                prefabPath = "Magenta_WebGL/MWGL_GlobalObjectUtilities";

                // Debug.LogFormat(LOG_FORMAT, "Application.dataPath : " + Application.dataPath);

                _isReady = false;

                screenCaptureManager.enabled = EnableScreenCapture;

                SceneManager.sceneUnloaded += OnSceneUnloaded;
                SceneManager.sceneLoaded += OnSceneLoaded;

                CheckEventSystem();
                DontDestroyOnLoad(this.gameObject);

                this.transform.position = Vector3.zero;

                MWGL_AppConfiguration.AppConfigFile = "Magenta_WebGL_AppConfig.ini";
                if (this.GetComponent<MWGL_AppConfiguration>() == null)
                {
                    _appConfig = this.gameObject.AddComponent<MWGL_AppConfiguration>();
                }

                if (AppConfig.Data.UseLogViewer == true)
                {
                    _logViewer.transform.parent.gameObject.SetActive(AppConfig.Data.UseLogViewer);
                }
                else
                {
                    _logViewer.gameObject.SetActive(false);
                }

                StartCoroutine(PostAwake());
            }
            else
            {
                Debug.LogWarningFormat(LOG_FORMAT, "<b>Already <color=red>EXIST \"MWGL_GlobalObjectUtilities\"</color></b>");
                this.gameObject.SetActive(false);
                Destroy(this.gameObject); // Destory gameObject not script itself!!!!!
                // DestroyImmediate(this.gameObject); // UnityException: Instantiate failed because the clone was destroyed during creation. This can happen if DestroyImmediate is called in MonoBehaviour.Awake.
                return;
            }
        }

        protected override IEnumerator PostAwake()
        {
            while (_localizedStringTable.IsReady == false)
            {
                yield return null;
            }
#if ENABLE_UNITY_GAME_SERVICES_REMOTE_CONFIG_SUPPORT
            while (_isReadyRemoteConfig == false)
            {
                yield return null;
            }
#endif

            Debug.LogWarningFormat(LOG_FORMAT, "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            _isReady = true;
        }
    }
}