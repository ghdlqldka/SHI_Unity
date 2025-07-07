#if UNITY_CHANGE1 || UNITY_CHANGE2 || UNITY_CHANGE3 || UNITY_CHANGE4
#warning UNITY_CHANGE has been set manually
#elif UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
#define UNITY_CHANGE1
#elif UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define UNITY_CHANGE2
#else
#define UNITY_CHANGE3
#endif

#if UNITY_2018_3_OR_NEWER
#define UNITY_CHANGE4
#endif
//use UNITY_CHANGE1 for unity older than "unity 5"
//use UNITY_CHANGE2 for unity 5.0 -> 5.3 
//use UNITY_CHANGE3 for unity 5.3 (fix for new SceneManger system)
//use UNITY_CHANGE4 for unity 2018.3 (Networking system)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using AYellowpaper.SerializedCollections;

namespace _Base_Framework
{
	public class _LogViewer : Reporter
	{
		private static string LOG_FORMAT = "<color=#F500DA><b>[_LogViewer]</b></color> {0}";

		// public bool Initialized = false;
		protected static _LogViewer _instance;
		public static _LogViewer Instance
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

		protected static string Key_int_Reporter_currentView = "Reporter_currentView";
		// protected static string Key_int_Reporter_collapse = "Reporter_collapse";
		// protected static string Key_int_Reporter_clearOnNewSceneLoaded = "Reporter_clearOnNewSceneLoaded";
		// protected static string Key_int_Reporter_showTime = "Reporter_showTime";
		// protected static string Key_int_Reporter_showScene = "Reporter_showScene";
		protected static string Key_int_Reporter_showMemory = "Reporter_showMemory";
		// protected static string Key_int_Reporter_showFps = "Reporter_showFps";
		// protected static string Key_int_Reporter_showGraph = "Reporter_showGraph";
		protected static string Key_int_Reporter_showLog = "Reporter_showLog";
		protected static string Key_int_Reporter_showWarning = "Reporter_showWarning";
		protected static string Key_int_Reporter_showError = "Reporter_showError";
		// protected static string Key_int_Reporter_filterText = "Reporter_filterText";

		[Space(10)]
		[ReadOnly]
		[SerializeField]
		// protected List<Sample> samples = new List<Sample>();
		protected List<Sample> _sampleList = new List<Sample>();
		public List<Sample> SampleList
		{
			get
			{
				return _sampleList;
			}
		}

		// protected List<Log> threadedLogs = new List<Log>();
		protected List<Log> ThreadedLogList
		{
			get
			{
				return threadedLogs;
			}
		}

		// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
		public delegate void LogAdded(Log log);
		public static event LogAdded OnLogAdded; // "CurrentLogList" changed!!!!!
		protected void Invoke_OnLogAdded(Log log)
		{
			if (OnLogAdded != null)
			{
				OnLogAdded(log);
			}
		}

		public delegate void LogCleared();
		public static event LogCleared OnLogCleared; // "CurrentLogList" changed!!!!!
		protected void Invoke_OnLogCleared()
		{
			if (OnLogCleared != null)
			{
				OnLogCleared();
			}
		}

		// protected List<Log> logs = new List<Log>();
		public List<Log> LogList
		{
			get
			{
				return logs;
			}
		}

		// protected int numOfLogs = 0;
		public int NumerOfLogs
		{
			get
			{
				return numOfLogs;
			}
			protected set
			{
				numOfLogs = value;
			}
		}
		// protected int numOfLogsWarning = 0;
		public int NumberOfWarningLogs
		{
			get
			{
				return numOfLogsWarning;
			}
			protected set
			{
				numOfLogsWarning = value;
			}
		}
		// protected int numOfLogsError = 0;
		public int NumberOfErrorLogs
		{
			get
			{
				return numOfLogsError;
			}
			protected set
			{
				numOfLogsError = value;
			}
		}

		public delegate void SelectedLogChanged(Log log);
		public static event SelectedLogChanged OnSelectedLogChanged;
		protected void Invoke_OnSelectedLogChanged(Log log)
		{
			if (OnSelectedLogChanged != null)
			{
				OnSelectedLogChanged(log);
			}
		}

		// protected Log selectedLog;
		protected Log _selectedLog;
		public Log SelectedLog
		{
			get
			{
				return _selectedLog;
			}
			set
			{
				_selectedLog = value;
				Invoke_OnSelectedLogChanged(value);
			}
		}
		// ----------------------------------------------------------------

		// protected Dictionary<string, string> cachedString = new Dictionary<string, string>();
		[ReadOnly]
		[SerializeField]
		[SerializedDictionary("string", "string")]
		protected SerializedDictionary<string, string> _cachedStringDic = new SerializedDictionary<string, string>();

		public delegate void ViewModeChanged(ReportView mode);
		public static event ViewModeChanged OnViewModeChanged;
		[ReadOnly]
		[SerializeField]
		// protected ReportView currentView = ReportView.Logs;
		protected ReportView _viewMode = ReportView.Logs;
		public ReportView ViewMode
		{
			get
			{
				return _viewMode;
			}
			set
			{
				if (_viewMode != value)
				{
					_viewMode = value;
					if (OnViewModeChanged != null)
					{
						OnViewModeChanged(value);
					}
				}
			}
		}

		[Space(10)]
		// protected bool collapse;
		// protected bool clearOnNewSceneLoaded;
		// protected bool showTime;
		// protected bool showScene;
		[ReadOnly]
		[SerializeField]
		// protected bool showMemory;
		protected bool _showMemory;
		public bool ShowMemory
		{
			get
			{
				return _showMemory;
			}
			set
			{
				_showMemory = value;
			}
		}
		// protected bool showFps;
		// protected bool showGraph;

		[ReadOnly]
		[SerializeField]
		// protected bool showLog = true;
		protected bool _showLog = true;
		public bool ShowLog
		{
			get
			{
				return _showLog;
            }
			set
			{
				_showLog = value;
			}
        }
        [ReadOnly]
        [SerializeField]
        // protected bool showWarning = true;
        protected bool _showWarning = true;
		public bool ShowWarning
		{
			get
			{
				return _showWarning;
			}
			set
			{
				_showWarning = value;
			}
		}
		[ReadOnly]
		[SerializeField]
		// protected bool showError = true;
		protected bool _showError = true;
		public bool ShowError
		{
			get
			{
				return _showError;
			}
			set
			{
				_showError = value;
			}
		}

		// protected bool showClearOnNewSceneLoadedButton = true;
		// protected bool showTimeButton = true;
		// protected bool showSceneButton = true;
		
		// protected bool showFpsButton = true;
		
		

		// protected string deviceModel;
		public string DeviceModel
		{
			get
			{
				return deviceModel;
			}
		}
		// protected string deviceType;
		public string DeviceType
		{
			get
			{
				return deviceType;
			}
		}
		// protected string deviceName;
		public string DeviceName
		{
			get
			{
				return deviceName;
			}
		}
		// protected string graphicsMemorySize;
		public string GraphicsMemorySize
		{
			get
			{
				return graphicsMemorySize;
			}
		}
#if !UNITY_CHANGE1
		// protected string maxTextureSize;
		public string MaxTextureSize
		{
			get
			{
				return maxTextureSize;
			}
		}
#endif
		// protected string systemMemorySize;
		public string SystemMemorySize
		{
			get
			{
				return systemMemorySize;
			}
		}

		[Space(10)]
		[ReadOnly]
		[SerializeField]
		// protected string buildDate;
		protected string _buildDate;
		public string BuildDate
		{
			get
			{
				return _buildDate;
			}
		}
		[ReadOnly]
		[SerializeField]
		// protected string logDate;
		protected string _logDate;
		public string LogDate
		{
			get
			{
				return _logDate;
			}
		}

		// protected float logsMemUsage;
		public float LogsMemUsage
		{
			get
			{
				return logsMemUsage;
			}
		}
		// protected float gcTotalMemory;
		public float GcTotalMemory
		{
			get
			{
				return gcTotalMemory;
			}
		}

		// protected string currentScene;

		public _CicleGesture gesture;

		protected override void Awake()
		{
			Debug.LogFormat(LOG_FORMAT, "Awake()");
			// base.Awake();

			if (Instance == null)
			{
				Instance = this;
			}
			else
			{
				Debug.LogErrorFormat(LOG_FORMAT, "");
				Destroy(this);
				return;
			}

			Debug.Assert(Initialized == false); // Do NOT use this variable!!!!!!

			samples = null; // Do NOT use this variable!!!!!
			currentLog = null; // Not used!!!!!
			logsDic = null; // Not used!!!!!
			cachedString = null; // Do NOT use this variable!!!!!

			// +collapse does NOT support!!!!!
			Debug.Assert(collapse == false);
			collapseContent = null;

			collapsedLogs = null;
			numOfCollapsedLogs = 0; // Do NOT use this variable!!!!!
			numOfCollapsedLogsWarning = 0; // Do NOT use this variable!!!!!
			numOfCollapsedLogsError = 0; // Do NOT use this variable!!!!!
			// -collapse does NOT support!!!!!

			// +clearOnNewSceneLoaded does NOT support!!!!!
			clearOnNewSceneLoaded = false;
			showClearOnNewSceneLoadedButton = false;
			clearOnNewSceneContent = null;
			// -clearOnNewSceneLoaded does NOT support!!!!!

			// +showFps does NOT support!!!!!
			showFps = false;
			showFpsButton = false;
			showFpsContent = null;

			fps = 0.0f;
			fpsText = "";
			// -showFps does NOT support!!!!!

			// +showTime Always TRUE!!!!!
			showTime = true;
			showTimeButton = false;
			// -showTime Always TRUE!!!!!

			// +showScene does NOT support!!!!!
			showScene = false;
			showSceneButton = false;
			showSceneContent = null;

			currentScene = null;
			scenes = null;
			// -showScene does NOT support!!!!!

			UserData = null; // Do NOT use this variable!!!!!

			showGraph = false; // Do NOT use this variable!!!!!

			images = null; //  Do NOT use this variable!!!!!
			clearContent = null; // Do NOT use this variable!!!!!
			showTimeContent = null; // Do NOT use this variable!!!!!
			userContent = null; // Do NOT use this variable!!!!!
			showMemoryContent = null; // Do NOT use this variable!!!!!
			softwareContent = null; // Do NOT use this variable!!!!!
			dateContent = null; // Do NOT use this variable!!!!!
			infoContent = null; // Do NOT use this variable!!!!!
			saveLogsContent = null; // Do NOT use this variable!!!!!
			searchContent = null; // Do NOT use this variable!!!!!
			copyContent = null; // Do NOT use this variable!!!!!
			closeContent = null; // Do NOT use this variable!!!!!

			buildFromContent = null; // Do NOT use this variable!!!!!
			systemInfoContent = null; // Do NOT use this variable!!!!!
			graphicsInfoContent = null; // Do NOT use this variable!!!!!
			backContent = null; // Do NOT use this variable!!!!!

			logContent = null; // Do NOT use this variable!!!!!
			warningContent = null; // Do NOT use this variable!!!!!
			errorContent = null; // Do NOT use this variable!!!!!
			barStyle = null; // Do NOT use this variable!!!!!
			buttonActiveStyle = null; // Do NOT use this variable!!!!!

			nonStyle = null; // Do NOT use this variable!!!!!
			lowerLeftFontStyle = null; // Do NOT use this variable!!!!!
			backStyle = null; // Do NOT use this variable!!!!!
			evenLogStyle = null; // Do NOT use this variable!!!!!
			oddLogStyle = null; // Do NOT use this variable!!!!!
			logButtonStyle = null; // Do NOT use this variable!!!!!
			selectedLogStyle = null; // Do NOT use this variable!!!!!
			selectedLogFontStyle = null; // Do NOT use this variable!!!!!
			stackLabelStyle = null; // Do NOT use this variable!!!!!
			scrollerStyle = null; // Do NOT use this variable!!!!!
			searchStyle = null; // Do NOT use this variable!!!!!
			sliderBackStyle = null; // Do NOT use this variable!!!!!
			sliderThumbStyle = null; // Do NOT use this variable!!!!!
			toolbarScrollerSkin = null; // Do NOT use this variable!!!!!
			logScrollerSkin = null; // Do NOT use this variable!!!!!
			graphScrollerSkin = null; // Do NOT use this variable!!!!!

			size = Vector2.negativeInfinity; // Do NOT use this variable!!!!!
			maxSize = float.NegativeInfinity; // Do NOT use this variable!!!!!

			filterText = string.Empty; // Do NOT use this variable!!!!!

			screenRect = Rect.zero; // Do NOT use this variable!!!!!
			toolBarRect = Rect.zero; // Do NOT use this variable!!!!!
			logsRect = Rect.zero; // Do NOT use this variable!!!!!
			stackRect = Rect.zero; // Do NOT use this variable!!!!!
			graphRect = Rect.zero; // Do NOT use this variable!!!!!
			graphMinRect = Rect.zero; // Do NOT use this variable!!!!!
			graphMaxRect = Rect.zero; // Do NOT use this variable!!!!!
			buttomRect = Rect.zero; // Do NOT use this variable!!!!!
			stackRectTopLeft = Vector2.negativeInfinity;
			detailRect = Rect.zero; // Do NOT use this variable!!!!!

			toolbarOldDrag = float.NegativeInfinity; // Do NOT use this variable!!!!!
			downPos = Vector2.zero; // Do NOT use this variable!!!!!

			scrollPosition = Vector2.negativeInfinity; // Do NOT use this variable!!!!!
			scrollPosition2 = Vector2.negativeInfinity; // Do NOT use this variable!!!!!
			toolbarScrollPosition = Vector2.negativeInfinity; // Do NOT use this variable!!!!!
			tempRect = Rect.zero; // Do NOT use this variable!!!!!

			infoScrollPosition = Vector2.zero; // Do NOT use this variable!!!!!
			oldInfoDrag = Vector2.zero; // Do NOT use this variable!!!!!

			oldDrag = float.NegativeInfinity; // Do NOT use this variable!!!!!
			oldDrag2 = float.NegativeInfinity; // Do NOT use this variable!!!!!
			startIndex = int.MinValue;

			countRect = Rect.zero; // Do NOT use this variable!!!!!
			timeRect = Rect.zero; // Do NOT use this variable!!!!!
			timeLabelRect = Rect.zero; // Do NOT use this variable!!!!!
			sceneRect = Rect.zero; // Do NOT use this variable!!!!!
			memoryRect = Rect.zero; // Do NOT use this variable!!!!!
			memoryLabelRect = Rect.zero; // Do NOT use this variable!!!!!
			fpsRect = Rect.zero; // Do NOT use this variable!!!!!
			tempContent = null; // Do NOT use this variable!!!!!

			numOfCircleToShow = int.MinValue; // Do NOT use this variable!!!!!
			// + Gesture
			gestureDetector = null; // Do NOT use this variable!!!!!
			gestureSum = Vector2.zero; // Do NOT use this variable!!!!!
			gestureLength = float.NegativeInfinity; // Do NOT use this variable!!!!!
			gestureCount = int.MinValue; // Do NOT use this variable!!!!!
			// - Gesture

			{
				string __PREFIX__ = _Base_Framework_Config.Product + "_";

				Key_int_Reporter_currentView = __PREFIX__ + Key_int_Reporter_currentView;
				// Key_int_Reporter_collapse = __PREFIX__ + Key_int_Reporter_collapse;
				// Key_int_Reporter_clearOnNewSceneLoaded = __PREFIX__ + Key_int_Reporter_clearOnNewSceneLoaded;
				// Key_int_Reporter_showTime = __PREFIX__ + Key_int_Reporter_showTime;
				// Key_int_Reporter_showScene = __PREFIX__ + Key_int_Reporter_showScene;
				Key_int_Reporter_showMemory = __PREFIX__ + Key_int_Reporter_showMemory;
				// Key_int_Reporter_showFps = __PREFIX__ + Key_int_Reporter_showFps;
				// Key_int_Reporter_showGraph = __PREFIX__ + Key_int_Reporter_showGraph;
				Key_int_Reporter_showLog = __PREFIX__ + Key_int_Reporter_showLog;
				Key_int_Reporter_showWarning = __PREFIX__ + Key_int_Reporter_showWarning;
				Key_int_Reporter_showError = __PREFIX__ + Key_int_Reporter_showError;
				// Key_int_Reporter_filterText = __PREFIX__ + Key_int_Reporter_filterText;


				// Initialize();
				// if (created == false)
				{
					/*
					try
					{
						this.gameObject.SendMessage("OnPreStart");
					}
					catch (System.Exception e)
					{
						Debug.LogException(e);
					}
					*/

					/*
					scenes = new string[SceneManager.sceneCountInBuildSettings];
					currentScene = SceneManager.GetActiveScene().name;
					*/

					// DontDestroyOnLoad(this.gameObject);

					//Application.logMessageReceived += CaptureLog ;
					Application.logMessageReceivedThreaded += OnReceiveLogMessage; // <===============================================================

					// created = true;
					//addSample();
				}
				/*
				else
				{
					Debug.LogErrorFormat(LOG_FORMAT, "tow manager is exists delete the second");
					// DestroyImmediate(this.gameObject, true);
					Destroy(this);
					return;
				}
				*/

				ViewMode = (ReportView)PlayerPrefs.GetInt(Key_int_Reporter_currentView, 1);
				// collapse = (PlayerPrefs.GetInt(Key_int_Reporter_collapse) == 1) ? true : false;
				// clearOnNewSceneLoaded = (PlayerPrefs.GetInt(Key_int_Reporter_clearOnNewSceneLoaded) == 1) ? true : false;
				// showTime = (PlayerPrefs.GetInt(Key_int_Reporter_showTime) == 1) ? true : false;
				// showScene = (PlayerPrefs.GetInt(Key_int_Reporter_showScene) == 1) ? true : false;
				ShowMemory = (PlayerPrefs.GetInt(Key_int_Reporter_showMemory) == 1) ? true : false;
				// showFps = (PlayerPrefs.GetInt(Key_int_Reporter_showFps) == 1) ? true : false;
				// showGraph = (PlayerPrefs.GetInt(Key_int_Reporter_showGraph) == 1) ? true : false;
				ShowLog = (PlayerPrefs.GetInt(Key_int_Reporter_showLog, 1) == 1) ? true : false;
				ShowWarning = (PlayerPrefs.GetInt(Key_int_Reporter_showWarning, 1) == 1) ? true : false;
				ShowError = (PlayerPrefs.GetInt(Key_int_Reporter_showError, 1) == 1) ? true : false;
				// FilterText = PlayerPrefs.GetString(Key_int_Reporter_filterText);


				// initializeStyle();

				deviceModel = SystemInfo.deviceModel.ToString();
				deviceType = SystemInfo.deviceType.ToString();
				deviceName = SystemInfo.deviceName.ToString();
				graphicsMemorySize = SystemInfo.graphicsMemorySize.ToString();
				maxTextureSize = SystemInfo.maxTextureSize.ToString();
				systemMemorySize = SystemInfo.systemMemorySize.ToString();
			}

            // SceneManager.sceneLoaded += _OnLevelWasLoaded;
		}

        protected override void OnDestroy()
		{
			// base.OnDestroy();
			if (Instance != this)
			{
				return;
			}

			// SceneManager.sceneLoaded -= _OnLevelWasLoaded;

			Application.logMessageReceivedThreaded -= OnReceiveLogMessage; // <===============================================================

			PlayerPrefs.SetInt(Key_int_Reporter_currentView, (int)ViewMode);
			// PlayerPrefs.SetInt(Key_int_Reporter_collapse, (collapse == true) ? 1 : 0);
			// PlayerPrefs.SetInt(Key_int_Reporter_clearOnNewSceneLoaded, (clearOnNewSceneLoaded == true) ? 1 : 0);
			// PlayerPrefs.SetInt(Key_int_Reporter_showTime, (showTime == true) ? 1 : 0);
			// PlayerPrefs.SetInt(Key_int_Reporter_showScene, (showScene == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_showMemory, (ShowMemory == true) ? 1 : 0);
			// PlayerPrefs.SetInt(Key_int_Reporter_showFps, (showFps == true) ? 1 : 0);
			// PlayerPrefs.SetInt(Key_int_Reporter_showGraph, (showGraph == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_showLog, (ShowLog == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_showWarning, (ShowWarning == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_showError, (ShowError == true) ? 1 : 0);
			// PlayerPrefs.SetString(Key_int_Reporter_filterText, FilterText);
			
			PlayerPrefs.Save();

			Instance = null;
		}

		protected override void OnApplicationQuit()
		{
#if false
			PlayerPrefs.SetInt(Key_int_Reporter_currentView, (int)currentView);
			PlayerPrefs.SetInt(Key_int_Reporter_show, (show == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_collapse, (collapse == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_clearOnNewSceneLoaded, (clearOnNewSceneLoaded == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_showTime, (showTime == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_showScene, (showScene == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_showMemory, (showMemory == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_showFps, (showFps == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_showGraph, (showGraph == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_showLog, (showLog == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_showWarning, (showWarning == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_showError, (showError == true) ? 1 : 0);
			PlayerPrefs.SetString(Key_int_Reporter_filterText, filterText);
			PlayerPrefs.SetFloat(Key_int_Reporter_size, uiLogViewer._size.x);

			PlayerPrefs.SetInt(Key_int_Reporter_showClearOnNewSceneLoadedButton, (showClearOnNewSceneLoadedButton == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_showTimeButton, (showTimeButton == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_showSceneButton, (showSceneButton == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_showMemButton, (showMemButton == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_showFpsButton, (showFpsButton == true) ? 1 : 0);
			PlayerPrefs.SetInt(Key_int_Reporter_showSearchText, (showSearchText == true) ? 1 : 0);

			PlayerPrefs.Save();
#endif
		}

		protected override void OnEnable()
		{
			Debug.LogWarningFormat(LOG_FORMAT, "OnEnable()");

			if (LogList.Count == 0)//if recompile while in play mode
			{
				// clear();
				Clear();
			}

			StartCoroutine(_Update());
		}

		protected override void OnDisable()
		{
			StopAllCoroutines();
		}

		protected override void Start()
		{
			_logDate = System.DateTime.Now.ToString();
			// StartCoroutine("readInfo");
		}

		protected override void Update()
		{
#if false // use _Update()
			// base.Update();

			// fpsText = fps.ToString("0.000");
			gcTotalMemory = (((float)System.GC.GetTotalMemory(false)) / 1024 / 1024);
			//addSample();

			/*
			int sceneIndex = SceneManager.GetActiveScene().buildIndex;
			if (sceneIndex != -1 && string.IsNullOrEmpty(scenes[sceneIndex]))
			{
				scenes[SceneManager.GetActiveScene().buildIndex] = SceneManager.GetActiveScene().name;
			}
			*/

			// calculateStartIndex();
			

			if (ThreadedLogList.Count > 0)
			{
				lock (ThreadedLogList)
				{
					for (int i = 0; i < ThreadedLogList.Count; i++)
					{
						Log l = ThreadedLogList[i];
						AddLog(l.condition, l.stacktrace, (LogType)l.logType);
					}
					ThreadedLogList.Clear();
				}
			}

#if false // showFps does NOT support!!!!!
			// FPS Counter
			if (firstTime == true)
			{
				firstTime = false;
				lastUpdate = Time.realtimeSinceStartup;
				frames = 0;
				return;
			}
			frames++;
			float dt = Time.realtimeSinceStartup - lastUpdate;
			if (dt > updateInterval && frames > requiredFrames)
			{
				fps = (float)frames / dt;
				lastUpdate = Time.realtimeSinceStartup;
				frames = 0;
			}
#endif
#endif
		}

		protected virtual IEnumerator _Update()
		{
			while (true)
			{
				// base.Update();

				// fpsText = fps.ToString("0.000");
				gcTotalMemory = (((float)System.GC.GetTotalMemory(false)) / 1024 / 1024);
				//addSample();

				/*
				int sceneIndex = SceneManager.GetActiveScene().buildIndex;
				if (sceneIndex != -1 && string.IsNullOrEmpty(scenes[sceneIndex]))
				{
					scenes[SceneManager.GetActiveScene().buildIndex] = SceneManager.GetActiveScene().name;
				}
				*/

				// calculateStartIndex();

				if (ThreadedLogList.Count > 0)
				{
					lock (ThreadedLogList)
					{
						for (int i = 0; i < ThreadedLogList.Count; i++)
						{
							Log l = ThreadedLogList[i];
							AddLog(l.condition, l.stacktrace, (LogType)l.logType);
							yield return null; // <================
						}
						ThreadedLogList.Clear();
					}
				}

#if false // showFps does NOT support!!!!!
				// FPS Counter
				if (firstTime == true)
				{
					firstTime = false;
					lastUpdate = Time.realtimeSinceStartup;
					frames = 0;
					return;
				}
				frames++;
				float dt = Time.realtimeSinceStartup - lastUpdate;
				if (dt > updateInterval && frames > requiredFrames)
				{
					fps = (float)frames / dt;
					lastUpdate = Time.realtimeSinceStartup;
					frames = 0;
				}
#endif

				yield return new WaitForSeconds(0.1f); // <================
			}
		}

		protected override void CaptureLogThread(string condition, string stacktrace, LogType type)
		{
			base.CaptureLogThread(condition, stacktrace, type);
			throw new System.NotSupportedException("");
		}

		protected virtual void OnReceiveLogMessage(string condition, string stacktrace, LogType type)
		{
			Log log = new Log() { condition = condition, stacktrace = stacktrace, logType = (_LogType)type };
			lock (ThreadedLogList)
			{
				ThreadedLogList.Add(log);
			}
		}

		// condition : "<color=#FF00DA><b>[_LogViewer]</b></color> Clear()"
		// stacktrace : "UnityEngine.Debug:LogFormat (string,object[])\n_Base_Framework._LogViewer:Clear () (at Assets/_Framework/Base_Framework/Asset StoreEx/Log Viewer/Scripts/_LogViewer.cs:853)\n_Base_Framework._LogViewer:OnEnable () (at Assets/_Framework/Base_Framework/Asset StoreEx/Log Viewer/Scripts/_LogViewer.cs:602)\n"
		protected override void AddLog(string condition, string stacktrace, LogType type)
		{
			// Debug.LogWarningFormat(LOG_FORMAT, "condition : " + condition + ", stacktrace : " + stacktrace + ", type : " + type);
			// base.AddLog(condition, stacktrace, type);

			if (this.gameObject.activeInHierarchy == false)
			{
				return;
			}

			float memUsage = 0f;
			string _condition;

			if (_cachedStringDic.ContainsKey(condition) == true)
			{
				_condition = _cachedStringDic[condition];
			}
			else
			{
				_condition = condition;
				_cachedStringDic.Add(_condition, _condition);
				memUsage += (string.IsNullOrEmpty(_condition) ? 0 : _condition.Length * sizeof(char));
				memUsage += System.IntPtr.Size;
			}

			string _stacktrace;
			if (_cachedStringDic.ContainsKey(stacktrace) == true)
			{
				_stacktrace = _cachedStringDic[stacktrace];
			}
			else
			{
				_stacktrace = stacktrace;
				_cachedStringDic.Add(_stacktrace, _stacktrace);
				memUsage += (string.IsNullOrEmpty(_stacktrace) ? 0 : _stacktrace.Length * sizeof(char));
				memUsage += System.IntPtr.Size;
			}

			// bool newLogAdded = false;

			// addSample();
			AddSample();
			// Log log = new Log() { logType = (_LogType)type, condition = _condition, stacktrace = _stacktrace, sampleId = samples.Count - 1 };
			Log log = new Log() { logType = (_LogType)type, condition = _condition, stacktrace = _stacktrace, sampleId = SampleList.Count - 1 };
			memUsage += log.GetMemoryUsage();
			//memUsage += SampleList.Count * 13 ;

			logsMemUsage += memUsage / 1024 / 1024;

#if false //
			// bool isNew = false;
			//string key = _condition;// + "_!_" + _stacktrace ;
			if (logsDic.ContainsKey(_condition, _stacktrace) == true)
			{
				// isNew = false;
				logsDic[_condition][_stacktrace].count++;
			}
			else
			{
				// isNew = true;
				// collapsedLogs.Add(log);
				logsDic[_condition][_stacktrace] = log;

				/*
				if (type == LogType.Log)
					numOfCollapsedLogs++;
				else if (type == LogType.Warning)
					numOfCollapsedLogsWarning++;
				else
					numOfCollapsedLogsError++;
				*/
			}
#endif

			if (type == LogType.Log)
			{
				NumerOfLogs++;
			}
			else if (type == LogType.Warning)
			{
				NumberOfWarningLogs++;
			}
			else
			{
				NumberOfErrorLogs++;
			}

			LogList.Add(log); // <==================================

			Invoke_OnLogAdded(log);
		}

		public override void OnGUIDraw()
		{
			throw new System.NotSupportedException("");
		}

		protected virtual void OnGUI()
		{
			//
		}

		protected override void _OnLevelWasLoaded(Scene _null1, LoadSceneMode _null2)
		{
			throw new System.NotSupportedException("");
		}

		protected override void clear()
		{
			throw new System.NotSupportedException("");
		}

		public virtual void Clear()
		{
			Debug.LogFormat(LOG_FORMAT, "============> Clear() <=================");
			// base.clear();

			LogList.Clear();
			// collapsedLogs.Clear();
			// currentLog.Clear();

			SelectedLog = null;
			// logsDic.Clear();
			//selectedIndex = -1;
			// SelectedLog = null;

			NumerOfLogs = 0;
			NumberOfWarningLogs = 0;
			NumberOfErrorLogs = 0;

			Invoke_OnLogCleared();

			// numOfCollapsedLogs = 0;
			// numOfCollapsedLogsWarning = 0;
			// numOfCollapsedLogsError = 0;

			logsMemUsage = 0;
			graphMemUsage = 0;
			// samples.Clear();
			SampleList.Clear();
			System.GC.Collect();
		}

		public override void Initialize()
		{
			throw new System.NotSupportedException("");
		}

		protected override void initializeStyle()
		{
			throw new System.NotSupportedException("");
		}

		protected override void calculateStartIndex()
		{
			throw new System.NotSupportedException("");
		}

		protected override void doShow()
		{
			throw new System.NotSupportedException("");
		}

		protected override void addSample()
		{
			throw new System.NotSupportedException("");
		}

		protected virtual void AddSample()
		{
			Sample sample = new Sample();

			// sample.fps = fps;
			// sample.fpsText = fpsText;
			sample.loadedScene = (byte)SceneManager.GetActiveScene().buildIndex;
			sample.time = Time.realtimeSinceStartup;
			sample.memory = gcTotalMemory;
			// samples.Add(sample);
			SampleList.Add(sample);

			// graphMemUsage = (samples.Count * Sample.MemSize()) / 1024 / 1024;
			graphMemUsage = (SampleList.Count * Sample.MemSize()) / 1024 / 1024;
		}

		protected override IEnumerator readInfo()
		{
			throw new System.NotSupportedException("");
		}

		protected override void SaveLogsToDevice()
		{
			throw new System.NotSupportedException("");
		}

		protected override void calculateCurrentLog()
		{
			throw new System.NotSupportedException("");
		}

		protected override Vector2 getDownPos()
		{
			throw new System.NotSupportedException("");
		}

		protected override Vector2 getDrag()
		{
			throw new System.NotSupportedException("");
		}

		protected override bool isGestureDone()
		{
			throw new System.NotSupportedException("");
		}

		protected virtual bool IsGestureDone()
		{
			base.isGestureDone();
			throw new System.NotSupportedException("");
		}

		protected override void DrawLogs()
		{
			throw new System.NotSupportedException("");
		}

		protected override void drawGraph()
		{
			throw new System.NotSupportedException("");
		}

		protected override void drawStack()
		{
			throw new System.NotSupportedException("");
		}

		protected override void drawInfo_enableDisableToolBarButtons()
		{
			throw new System.NotSupportedException("");
		}

		protected override void drawToolBar()
		{
			throw new System.NotSupportedException("");
		}

		protected override void DrawInfo()
		{
			throw new System.NotSupportedException("");
		}
	}


}