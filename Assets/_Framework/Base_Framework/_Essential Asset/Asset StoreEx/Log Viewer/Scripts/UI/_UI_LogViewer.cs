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

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Reporter;

namespace _Base_Framework
{
	public class _UI_LogViewer : MonoBehaviour
	{
		private static string LOG_FORMAT = "<color=white><b>[_UI_LogViewer]</b></color> {0}";

		[SerializeField]
		protected _LogViewer _logViewer;
		protected _LogViewer logViewer
		{
			get
			{
				return _logViewer;
			}
		}

		[Space(10)]
		[SerializeField]
		protected Canvas _canvas;

		[SerializeField]
		protected Transform logsContainerT;
		[SerializeField]
		protected GameObject logItemPrefab;
		protected List<_UI_LogItem> _uiInfoLogItemList = new List<_UI_LogItem>();
		protected List<_UI_LogItem> _uiWarningLogItemList = new List<_UI_LogItem>();
		protected List<_UI_LogItem> _uiErrorLogItemList = new List<_UI_LogItem>();

		[Space(10)]
		[SerializeField]
		protected GameObject logsViewPanel;
		[SerializeField]
		protected GameObject infoViewPanel;

		[Space(10)]
		[SerializeField]
		protected Toggle memoryToggle;
		[SerializeField]
		protected TMP_Text memoryButtonText;

		[Space(10)]
		[SerializeField]
		protected Toggle logToggle;
		[SerializeField]
		protected TMP_Text logCountText;
		[SerializeField]
		protected Toggle warningLogToggle;
		[SerializeField]
		protected TMP_Text warningLogCountText;
		[SerializeField]
		protected Toggle errorLogToggle;
		[SerializeField]
		protected TMP_Text errorLogCountText;

		[Header("Stacktrace")]
		[SerializeField]
		protected TMP_Text stacktraceText;
		[SerializeField]
		protected TMP_Text stacktraceTimeText;
		[SerializeField]
		protected TMP_Text stacktraceMemoryText;

		[Header("InfoView")]
		[SerializeField]
		protected TMP_Text buildDateText;
		[SerializeField]
		protected TMP_Text deviceInfoText; // DeviceModel, DeviceName
		[SerializeField]
		protected TMP_Text gpuInfoText; // graphicsDeviceName, GraphicsMemorySize, MaxTextureSize
		[SerializeField]
		protected TMP_Text screenInfoText;
		[SerializeField]
		protected TMP_Text sysMemorySizeText; // SystemMemorySize
		[SerializeField]
		protected TMP_Text memoryUsageInfoText;
		[SerializeField]
		protected TMP_Text osInfoText;
		[SerializeField]
		protected TMP_Text appStartTimeInfoText;
		[SerializeField]
		protected TMP_Text realtimeSinceStartupText;
		[SerializeField]
		protected TMP_Text unityVersionText;

		protected static string Key_int_Reporter_show = "Reporter_show";

		protected virtual void Awake()
		{
			string __PREFIX__ = _Base_Framework_Config.Product + "_";

			Key_int_Reporter_show = __PREFIX__ + Key_int_Reporter_show;

			_CicleGesture.OnDetect += OnCicleGestureDetect;
			_LogViewer.OnLogAdded += OnLogAdded;
			_LogViewer.OnLogCleared += OnLogCleared;
			_LogViewer.OnSelectedLogChanged += OnSelectedLogChanged;
            _LogViewer.OnViewModeChanged += OnViewModeChanged;

			_canvas.enabled = (PlayerPrefs.GetInt(Key_int_Reporter_show) == 1) ? true : false;
		}

        protected virtual void OnDestroy()
		{
			_LogViewer.OnLogAdded -= OnLogAdded;
			_LogViewer.OnLogCleared -= OnLogCleared;
			_LogViewer.OnSelectedLogChanged -= OnSelectedLogChanged;
			_LogViewer.OnViewModeChanged -= OnViewModeChanged;
			_CicleGesture.OnDetect -= OnCicleGestureDetect;

			PlayerPrefs.SetInt(Key_int_Reporter_show, (_canvas.enabled == true) ? 1 : 0);
			PlayerPrefs.Save();
		}

		protected virtual void Start()
		{
			memoryToggle.isOn = logViewer.ShowMemory;

			logToggle.isOn = logViewer.ShowLog;
			warningLogToggle.isOn = logViewer.ShowWarning;
			errorLogToggle.isOn = logViewer.ShowError;

			OnViewModeChanged(logViewer.ViewMode); // forcelly call
		}

		protected virtual void Update()
		{
			memoryButtonText.text = logViewer.GcTotalMemory.ToString("0.0");
		}

		protected virtual void OnCicleGestureDetect()
		{
			if (_canvas.enabled == false)
			{
				// doShow();
				_canvas.enabled = true;
				logViewer.ViewMode = ReportView.Logs;
			}
		}

		protected virtual void OnLogAdded(Log log)
		{
			logCountText.text = " " + logViewer.NumerOfLogs;
			warningLogCountText.text = " " + logViewer.NumberOfWarningLogs;
			errorLogCountText.text = " " + logViewer.NumberOfErrorLogs;

			GameObject itemObj = GameObject.Instantiate(logItemPrefab);
			itemObj.transform.SetParent(logsContainerT, false);

			_UI_LogItem uiLogItem = itemObj.GetComponent<_UI_LogItem>();

            Reporter.Sample sample = logViewer.SampleList[log.sampleId];
			uiLogItem.Set(log, sample.time.ToString("0.000"), sample.memory.ToString("0.000"));

			if (log.logType == _LogType.Log)
			{
				uiLogItem.gameObject.SetActive(logViewer.ShowLog);
				_uiInfoLogItemList.Add(uiLogItem);
			}
			else if (log.logType == _LogType.Warning)
			{
				uiLogItem.gameObject.SetActive(logViewer.ShowWarning);
				_uiWarningLogItemList.Add(uiLogItem);
			}
			else
			{
				uiLogItem.gameObject.SetActive(logViewer.ShowError);
				_uiErrorLogItemList.Add(uiLogItem);
			}
		}

		protected virtual void OnLogCleared()
		{
			logCountText.text = " " + logViewer.NumerOfLogs;
			warningLogCountText.text = " " + logViewer.NumberOfWarningLogs;
			errorLogCountText.text = " " + logViewer.NumberOfErrorLogs;

			// Info
			for (int i = 0; i < _uiInfoLogItemList.Count; i++)
			{
				Destroy(_uiInfoLogItemList[i].gameObject);
			}
			_uiInfoLogItemList.Clear();

			// Warning
			for (int i = 0; i < _uiWarningLogItemList.Count; i++)
			{
				Destroy(_uiWarningLogItemList[i].gameObject);
			}
			_uiWarningLogItemList.Clear();

			// Error
			for (int i = 0; i < _uiErrorLogItemList.Count; i++)
			{
				Destroy(_uiErrorLogItemList[i].gameObject);
			}
			_uiErrorLogItemList.Clear();
		}

		protected virtual void OnSelectedLogChanged(Log log)
		{
			if (log != null)
			{
				stacktraceText.text = log.condition + "\n" + log.stacktrace;

                Reporter.Sample selectedSample = null;
				try
				{
					// selectedSample = samples[selectedLog.sampleId];
					selectedSample = logViewer.SampleList[logViewer.SelectedLog.sampleId];
				}
				catch (System.Exception e)
				{
					Debug.LogErrorFormat(LOG_FORMAT, "_sampleList.Count : " + logViewer.SampleList.Count + ", _SelectedLog.sampleId : " + logViewer.SelectedLog.sampleId);
					Debug.LogException(e);
				}

				stacktraceTimeText.text = selectedSample.time.ToString("0.000");
				stacktraceMemoryText.text = selectedSample.memory.ToString("0.000");
			}
			else
			{
				stacktraceText.text = "";

				stacktraceTimeText.text = "";
				stacktraceMemoryText.text = "";
			}
		}

		protected virtual void OnViewModeChanged(ReportView mode)
		{
			if (mode == ReportView.Logs)
			{
				logsViewPanel.SetActive(true);
				infoViewPanel.SetActive(false);
			}
			else if (mode == ReportView.Info)
			{
				infoViewPanel.SetActive(true);
				logsViewPanel.SetActive(false);

				_OnUpdateInfoView();
			}
			else
			{
				Debug.Assert(false);
			}
		}

		protected virtual void _OnUpdateInfoView()
		{
			buildDateText.text = logViewer.BuildDate;
			deviceInfoText.text = SystemInfo.deviceModel.ToString() + "\t" + logViewer.DeviceName; // DeviceModel, DeviceName
			gpuInfoText.text = SystemInfo.graphicsDeviceName + SystemInfo.graphicsMemorySize.ToString(); // graphicsDeviceName, GraphicsMemorySize, MaxTextureSize
#if !UNITY_CHANGE1
			gpuInfoText.text += logViewer.MaxTextureSize;
#endif

			screenInfoText.text = "Screen Width " + Screen.width + " x Screen Height " + Screen.height;
			sysMemorySizeText.text = logViewer.SystemMemorySize + " mb";
			memoryUsageInfoText.text = "Mem Usage Of Logs " + logViewer.LogsMemUsage.ToString("0.000") + " mb" + ", GC Memory " + logViewer.GcTotalMemory.ToString("0.000") + " mb";
			osInfoText.text = SystemInfo.operatingSystem;
			appStartTimeInfoText.text = System.DateTime.Now.ToString() + " - Application Started At " + logViewer.LogDate;
			realtimeSinceStartupText.text = Time.realtimeSinceStartup.ToString("000");
			unityVersionText.text = "Unity Version = " + Application.unityVersion;
		}

		public virtual void OnClickClearButton()
		{
			logViewer.Clear();
		}

		public virtual void OnClickCloseButton()
		{
			Debug.LogFormat(LOG_FORMAT, "OnClickCloseButton()");

			_canvas.enabled = false;
		}

		public virtual void OnValueChangedShowMemory(bool value)
		{
			Debug.LogFormat(LOG_FORMAT, "OnValueChangedShowMemory(), value : <b>" + value + "</b>");

			logViewer.ShowMemory = value;
		}

		public virtual void OnClickToInfoView()
		{
			Debug.LogFormat(LOG_FORMAT, "OnClickToInfoView()");

			logViewer.ViewMode = ReportView.Info;
		}

		public virtual void OnValueChangedShowLog(bool value)
		{
			logViewer.ShowLog = value;

			for (int i = 0; i < _uiInfoLogItemList.Count; i++)
			{
				_uiInfoLogItemList[i].gameObject.SetActive(logViewer.ShowLog);
			}
		}

		public virtual void OnValueChangedShowWarningLog(bool value)
		{
			logViewer.ShowWarning = value;

			for (int i = 0; i < _uiWarningLogItemList.Count; i++)
			{
				_uiWarningLogItemList[i].gameObject.SetActive(logViewer.ShowWarning);
			}
		}

		public virtual void OnValueChangedShowErrorLog(bool value)
		{
			logViewer.ShowError = value;

			for (int i = 0; i < _uiErrorLogItemList.Count; i++)
			{
				_uiErrorLogItemList[i].gameObject.SetActive(logViewer.ShowError);
			}
		}

		public virtual void OnClickToLogsView()
		{
			Debug.LogFormat(LOG_FORMAT, "OnClickToLogsView()");

			logViewer.ViewMode = ReportView.Logs;
		}
	}


}