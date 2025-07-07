using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace _Base_Framework
{
    public class _GlobalObjectUtilities : MonoBehaviour
    {
        private static string LOG_FORMAT = "<b><color=magenta>[_GlobalObjectUtilities]</color></b> {0}";

        protected static _GlobalObjectUtilities _instance;
        public static _GlobalObjectUtilities Instance
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
        public static string prefabPath = "Base_Framework/_GlobalObjectUtilities";

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected bool _isReady = false;
        public bool IsReady
        {
            get
            {
                return _isReady;
            }
        }

        protected static float _timeScale = 1.0f;
        public static float timeScale 
        { 
            get
            {
                return _timeScale;
            }
            set
            {
                if (_timeScale != value)
                {
                    _timeScale = value;
                    Time.timeScale = value;
                    Debug.LogWarningFormat(LOG_FORMAT, "Time.timeScale CHANGED!!!!!! => <b><color=red>" + Time.timeScale + "</color></b>");
                }
            }
        }

        

        [Header("Manager")]
        [ReadOnly]
        [SerializeField]
        protected _NetworkManager _networkManager;
        public _NetworkManager NetworkManager
        {
            get
            {
                return _networkManager;
            }
        }
        [ReadOnly]
        [SerializeField]
        protected _LocalizedStringTable _localizedStringTable;
        [SerializeField]
        protected bool _enableScreenCapture = true;
        public bool EnableScreenCapture
        {
            get
            {
                return _enableScreenCapture;
            }
        }
#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected _ScreenCaptureManager screenCaptureManager;

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected _LogViewer _logViewer;

        [Space(20)]
#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected EventSystem eventSystem;

        protected bool _isReadyRemoteConfig = true;
#if ENABLE_UNITY_GAME_SERVICES_REMOTE_CONFIG_SUPPORT
        protected _RemoteConfigManager _remoteConfig;
        public _RemoteConfigManager RemoteConfig
        {
            get
            {
                return _remoteConfig;
            }
        }
#endif

        [ReadOnly]
        [SerializeField]
        protected _AppConfiguration _appConfig;
        public _AppConfiguration AppConfig
        {
            get
            {
                return _appConfig;
            }
        }

#if DEBUG
        [Header("==========> For DEBUGGING <==========")]
        [ReadOnly]
        [SerializeField]
        protected float DEBUG_timeScale;
#endif

        protected virtual void Awake()
        {
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
                _RemoteConfigManager.OnConfigRequestStatusChanged += OnRemoteConfigRequestStatusChanged;
                _remoteConfig = this.transform.Find("Unity Services").gameObject.AddMissingComponent<_RemoteConfigManager>();
#else
                _isReadyRemoteConfig = true;
#endif

#if DEBUG
                if (_Base_Framework_Config.Product != _Base_Framework_Config._Product.Base_Framework)
                {
                    Debug.Assert(false);
                }
#endif

                Instance = this;

                // Debug.LogFormat(LOG_FORMAT, "Application.dataPath : " + Application.dataPath);

                _isReady = false;

                screenCaptureManager.enabled = EnableScreenCapture;

                SceneManager.sceneUnloaded += OnSceneUnloaded;
                SceneManager.sceneLoaded += OnSceneLoaded;

                CheckEventSystem();
                DontDestroyOnLoad(this.gameObject);

                this.transform.position = Vector3.zero;

                _AppConfiguration.AppConfigFile = "Base_Framework_AppConfig.ini";
                if (this.GetComponent<_AppConfiguration>() == null)
                {
                    _appConfig = this.gameObject.AddComponent<_AppConfiguration>();
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
                Debug.LogWarningFormat(LOG_FORMAT, "<b>Already <color=red>EXIST \"GlobalObjectUtilities\"</color></b>");
                this.gameObject.SetActive(false);
                Destroy(this.gameObject); // Destory gameObject not script itself!!!!!
                // DestroyImmediate(this.gameObject); // UnityException: Instantiate failed because the clone was destroyed during creation. This can happen if DestroyImmediate is called in MonoBehaviour.Awake.
                return;
            }
        }

        protected virtual IEnumerator PostAwake()
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

#if ENABLE_UNITY_GAME_SERVICES_REMOTE_CONFIG_SUPPORT
        protected virtual void OnRemoteConfigRequestStatusChanged(Unity.Services.RemoteConfig.ConfigRequestStatus requestStatus)
        {
            // throw new System.NotImplementedException();
            Debug.LogWarningFormat(LOG_FORMAT, "On<color=red><b>RemoteConfig</b></color>RequestStatusChanged(), requestStatus : <color=red><b>" + requestStatus + "</b></color>");
            Debug.Assert(Unity.Services.RemoteConfig.RemoteConfigService.Instance.requestStatus == requestStatus);

            if (requestStatus == Unity.Services.RemoteConfig.ConfigRequestStatus.Success)
            {
                //
            }

            _isReadyRemoteConfig = true;
        }
#endif

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            Instance = null;
        }

        protected virtual void OnEnable()
        {
            //
        }

        protected virtual void Update()
        {
            timeScale = Time.timeScale;
#if DEBUG
            DEBUG_timeScale = timeScale;
#endif
            if (AppConfig.Data.OpenPlayerLogFolder == true)
            {
                if (Input.GetKeyDown(KeyCode.F12))
                {
                    OpenPlayerLogFolder();
                }
            }
        }

        // When Run In Background (Edit > Project Settings > Player > Resolution and Presentation) is disabled, a game running in the Editor's
        // Play mode or in a standalone Player will pause any time the Editor or Player application loses focus. In these cases Unity sends
        // OnApplicationPause(true) to all MonoBehaviours.
        // The pauseStatus parameter is either true (paused) or false (running)
        protected virtual void OnApplicationPause(bool pauseStatus)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "OnApplicationPause(), <color=yellow>pauseStatus</color> : <b><color=red>" + pauseStatus + "</color></b>");
        }

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // throw new System.NotImplementedException();

            Debug.LogWarningFormat(LOG_FORMAT, "OnScene<color=yellow>Loaded</color>(), scene : <b><color=yellow>" + scene.name + "</color></b>, mode : <b><color=yellow>" + mode + "</color></b>");

            CheckEventSystem();
        }

        protected virtual void OnSceneUnloaded(Scene scene)
        {
            Debug.LogFormat(LOG_FORMAT, "OnScene<color=red>Unloaded</color>(), scene : <b><color=yellow>" + scene.name + "</color></b>");
        }

        // There can be only one active Event System
        protected virtual void CheckEventSystem()
        {
            EventSystem[] _eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            Debug.LogFormat(LOG_FORMAT, "_eventSystems.Length : <b><color=yellow>" + _eventSystems.Length + "</color></b>");

            if (_eventSystems.Length == 0)
            {
                eventSystem.gameObject.SetActive(true);
            }
            else if (_eventSystems.Length == 2)
            {
                eventSystem.gameObject.SetActive(false);
            }
            else if (_eventSystems.Length > 2)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "_eventSystems count : " + _eventSystems.Length);
            }
            else
            {
                // Debug.Assert(false);
            }
        }

        public virtual void OpenPlayerLogFolder()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "OpenPlayerLogFolder()");

            // Windows : %USERPROFILE%\AppData\LocalLow\CompanyName\ProductName\Player.log
            string path = System.Environment.GetEnvironmentVariable("USERPROFILE"); // path : C:\Users\fov_T
            string url = "file://" + path + ("\\AppData\\LocalLow\\" + _Base_Framework_Config.companyName + "\\" + _Base_Framework_Config.productName);
            Application.OpenURL(url);
        }
    }
}