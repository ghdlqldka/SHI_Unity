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

namespace _Magenta_WebGL
{
	public class MWGL_LogViewer : _Magenta_Framework.LogViewerEx
    {
		private static string LOG_FORMAT = "<color=#FF00DA><b>[MWGL_LogViewer]</b></color> {0}";

		public static new MWGL_LogViewer Instance
		{
			get
			{
				return _instance as MWGL_LogViewer;
			}
			protected set
			{
				_instance = value;
			}
		}

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
				string __PREFIX__ = _Magenta_WebGL_Config.Product + "_";

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

	}


}