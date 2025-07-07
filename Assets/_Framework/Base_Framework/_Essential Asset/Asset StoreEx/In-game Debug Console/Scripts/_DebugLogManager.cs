using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif
#if UNITY_EDITOR && UNITY_2021_1_OR_NEWER
using Screen = UnityEngine.Device.Screen; // To support Device Simulator on Unity 2021.1+
#endif

// Receives debug entries and custom events (e.g. Clear, Collapse, Filter by Type)
// and notifies the recycled list view of changes to the list of debug entries
// 
// - Vocabulary -
// Debug/Log entry: a Debug.Log/LogError/LogWarning/LogException/LogAssertion request made by
//                   the client and intercepted by this manager object
// Debug/Log item: a visual (uGUI) representation of a debug entry
// 
// There can be a lot of debug entries in the system but there will only be a handful of log items 
// to show their properties on screen (these log items are recycled as the list is scrolled)

// An enum to represent filtered log types
namespace IngameDebugConsole
{
	public class _DebugLogManager : DebugLogManager
	{
		private static string LOG_FORMAT = "<color=#FF00DA><b>[_DebugLogManager]</b></color> {0}";

        protected static _DebugLogManager _instance;
        public static new _DebugLogManager Instance 
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

        // protected DebugLogPopup popupManager;
        protected _UI_DebugLogPopup _ui_popup
        {
            get
            {
                return popupManager as _UI_DebugLogPopup;
            }
        }

        [Header("Internal References")]
        [SerializeField]
        protected _UI_DebugLogWindow _ui_window;

        protected override void Awake()
		{
            Debug.LogFormat(LOG_FORMAT, "Awake()");
            // base.Awake();
            // Only one instance of debug console is allowed
            if (Instance == null)
            {
                Instance = this;

                singleton = false; // forcelly set!!!!!!
                receiveLogsWhileInactive = true;
                /*
                // If it is a singleton object, don't destroy it between scene changes
                if (singleton)
                    DontDestroyOnLoad(this.gameObject);
                */

                Debug.Assert(_ui_window != null);

                logWindowTR = _ui_window.GetComponent<RectTransform>();
            }
            else if (Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            pooledLogEntries = new Stack<DebugLogEntry>(64);
            pooledLogItems = new Stack<DebugLogItem>(16);
            commandSuggestionInstances = new List<TextMeshProUGUI>(8);
            matchingCommandSuggestions = new List<ConsoleMethodInfo>(8);
            commandCaretIndexIncrements = new List<int>(8);
            queuedLogEntries = new DynamicCircularBuffer<QueuedDebugLogEntry>(Mathf.Clamp(queuedLogLimit, 16, 4096));
            commandHistory = new CircularBuffer<string>(commandHistorySize);

            logEntriesLock = new object();
            sharedStringBuilder = new StringBuilder(1024);

            canvasTR = (RectTransform)transform;
            logItemsScrollRectTR = (RectTransform)logItemsScrollRect.transform;
            logItemsScrollRectOriginalSize = logItemsScrollRectTR.sizeDelta;

            // Associate sprites with log types
            logSpriteRepresentations = new Sprite[5];
            logSpriteRepresentations[(int)LogType.Log] = infoLog;
            logSpriteRepresentations[(int)LogType.Warning] = warningLog;
            logSpriteRepresentations[(int)LogType.Error] = errorLog;
            logSpriteRepresentations[(int)LogType.Exception] = errorLog;
            logSpriteRepresentations[(int)LogType.Assert] = errorLog;

            // Initially, all log types are visible
            filterInfoButton.color = filterButtonsSelectedColor;
            filterWarningButton.color = filterButtonsSelectedColor;
            filterErrorButton.color = filterButtonsSelectedColor;

            resizeButton.sprite = enableHorizontalResizing ? resizeIconAllDirections : resizeIconVerticalOnly;

            collapsedLogEntries = new DynamicCircularBuffer<DebugLogEntry>(128);
            collapsedLogEntriesMap = new Dictionary<DebugLogEntry, DebugLogEntry>(128, new DebugLogEntryContentEqualityComparer());
            uncollapsedLogEntries = new DynamicCircularBuffer<DebugLogEntry>(256);
            logEntriesToShow = new DynamicCircularBuffer<DebugLogEntry>(256);

            if (captureLogTimestamps)
            {
                collapsedLogEntriesTimestamps = new DynamicCircularBuffer<DebugLogEntryTimestamp>(128);
                uncollapsedLogEntriesTimestamps = new DynamicCircularBuffer<DebugLogEntryTimestamp>(256);
                timestampsOfLogEntriesToShow = new DynamicCircularBuffer<DebugLogEntryTimestamp>(256);
                queuedLogEntriesTimestamps = new DynamicCircularBuffer<DebugLogEntryTimestamp>(queuedLogEntries.Capacity);
            }

            recycledListView.Initialize(this, logEntriesToShow, timestampsOfLogEntriesToShow, logItemPrefab.Transform.sizeDelta.y);

            if (minimumWidth < 100f)
                minimumWidth = 100f;
            if (minimumHeight < 200f)
                minimumHeight = 200f;

            if (!resizeFromRight)
            {
                RectTransform resizeButtonTR = (RectTransform)resizeButton.GetComponentInParent<DebugLogResizeListener>().transform;
                resizeButtonTR.anchorMin = new Vector2(0f, resizeButtonTR.anchorMin.y);
                resizeButtonTR.anchorMax = new Vector2(0f, resizeButtonTR.anchorMax.y);
                resizeButtonTR.pivot = new Vector2(0f, resizeButtonTR.pivot.y);

                ((RectTransform)commandInputField.transform).anchoredPosition += new Vector2(resizeButtonTR.sizeDelta.x, 0f);
            }

            if (enableSearchbar == true)
            {
                searchbar.GetComponent<TMP_InputField>().onValueChanged.AddListener(SearchTermChanged);
            }
            else
            {
                searchbar = null;
                searchbarSlotTop.gameObject.SetActive(false);
                searchbarSlotBottom.gameObject.SetActive(false);
            }

            filterInfoButton.gameObject.SetActive(receiveInfoLogs);
            filterWarningButton.gameObject.SetActive(receiveWarningLogs);
            filterErrorButton.gameObject.SetActive(receiveErrorLogs || receiveExceptionLogs);

            if (commandSuggestionsContainer.gameObject.activeSelf)
                commandSuggestionsContainer.gameObject.SetActive(false);

            // Register to UI events
            commandInputField.onValidateInput += OnValidateCommand;
            commandInputField.onValueChanged.AddListener(OnEditCommand);
            commandInputField.onEndEdit.AddListener(OnEndEditCommand);
            hideButton.onClick.AddListener(HideLogWindow);
            clearButton.onClick.AddListener(ClearLogs);
            collapseButton.GetComponent<Button>().onClick.AddListener(CollapseButtonPressed);
            filterInfoButton.GetComponent<Button>().onClick.AddListener(FilterLogButtonPressed);
            filterWarningButton.GetComponent<Button>().onClick.AddListener(FilterWarningButtonPressed);
            filterErrorButton.GetComponent<Button>().onClick.AddListener(FilterErrorButtonPressed);
            snapToBottomButton.GetComponent<Button>().onClick.AddListener(() => SnapToBottom = true);

            localTimeUtcOffset = System.DateTime.Now - System.DateTime.UtcNow;
            dummyLogEntryTimestamp = new DebugLogEntryTimestamp();
            nullPointerEventData = new PointerEventData(null);

            poolLogEntryAction = PoolLogEntry;
            removeUncollapsedLogEntryAction = RemoveUncollapsedLogEntry;
            shouldRemoveCollapsedLogEntryPredicate = ShouldRemoveCollapsedLogEntry;
            shouldRemoveLogEntryToShowPredicate = ShouldRemoveLogEntryToShow;
            updateLogEntryCollapsedIndexAction = UpdateLogEntryCollapsedIndex;

            // if (receiveLogsWhileInactive == true)
            {
                // Application.logMessageReceivedThreaded -= ReceivedLog;
                Application.logMessageReceivedThreaded += ReceivedLog;
            }

            // OnApplicationQuit isn't reliable on some Unity versions when Application.wantsToQuit is used; Application.quitting is the only reliable solution on those versions
            // https://issuetracker.unity3d.com/issues/onapplicationquit-method-is-called-before-application-dot-wantstoquit-event-is-raised
            Application.quitting += OnApplicationQuitting;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			toggleBinding.performed += ( context ) =>
			{
				if( toggleWithKey )
				{
					if( isLogWindowVisible )
						HideLogWindow();
					else
						ShowLogWindow();
				}
			};

			// On new Input System, scroll sensitivity is much higher than legacy Input system
			logItemsScrollRect.scrollSensitivity *= 0.25f;
#endif
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            // if (receiveLogsWhileInactive)
            {
                Application.logMessageReceivedThreaded -= ReceivedLog;
            }

            Application.quitting -= OnApplicationQuitting;
        }

        protected override void OnEnable()
        {
            /*
            if (Instance != this)
                return;
            */

            Debug.Assert(receiveLogsWhileInactive == true);
            /*
            if (!receiveLogsWhileInactive)
            {
                Application.logMessageReceivedThreaded -= ReceivedLog;
                Application.logMessageReceivedThreaded += ReceivedLog;
            }
            */

            if (receiveLogcatLogsInAndroid)
            {
#if UNITY_ANDROID
#if UNITY_ANDROID_JNI
#if !UNITY_EDITOR
				if( logcatListener == null )
					logcatListener = new DebugLogLogcatListener();

				logcatListener.Start( logcatArguments );
#endif
#else
				Debug.LogWarning( "Android JNI module must be enabled in Package Manager for \"Receive Logcat Logs In Android\" to work." );
#endif
#endif
            }

#if IDG_ENABLE_HELPER_COMMANDS || IDG_ENABLE_LOGS_SAVE_COMMAND
			DebugLogConsole.AddCommand( "logs.save", "Saves logs to persistentDataPath", SaveLogsToFile );
			DebugLogConsole.AddCommand<string>( "logs.save", "Saves logs to the specified file", SaveLogsToFile );
#endif

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			if( toggleWithKey )
				toggleBinding.Enable();
#endif

            //Debug.LogAssertion( "assert" );
            //Debug.LogError( "error" );
            //Debug.LogException( new System.IO.EndOfStreamException() );
            //Debug.LogWarning( "warning" );
            //Debug.Log( "log" );
        }

        protected override void OnDisable()
        {
            /*
            if (Instance != this)
                return;
            */

            Debug.Assert(receiveLogsWhileInactive == true);
            /*
            if (!receiveLogsWhileInactive)
                Application.logMessageReceivedThreaded -= ReceivedLog;
            */

#if !UNITY_EDITOR && UNITY_ANDROID && UNITY_ANDROID_JNI
			if( logcatListener != null )
				logcatListener.Stop();
#endif

            DebugLogConsole.RemoveCommand("logs.save");

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			if( toggleBinding.enabled )
				toggleBinding.Disable();
#endif
        }

        protected override void Update()
        {
#if !IDG_OMIT_ELAPSED_TIME
            lastElapsedSeconds = Time.realtimeSinceStartup;
#endif
#if !IDG_OMIT_FRAMECOUNT
            lastFrameCount = Time.frameCount;
#endif

#if !UNITY_EDITOR && UNITY_ANDROID && UNITY_ANDROID_JNI
			if( logcatListener != null )
			{
				string log;
				while( ( log = logcatListener.GetLog() ) != null )
					ReceivedLog( "LOGCAT: " + log, string.Empty, LogType.Log );
			}
#endif

#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
            // Toggling the console with toggleKey is handled in Update instead of LateUpdate because
            // when we hide the console, we don't want the commandInputField to capture the toggleKey.
            // InputField captures input in LateUpdate so deactivating it in Update ensures that
            // no further input is captured
            if (toggleWithKey)
            {
                if (Input.GetKeyDown(toggleKey))
                {
                    if (isLogWindowVisible)
                        HideLogWindow();
                    else
                        ShowLogWindow();
                }
            }
#endif

#if DEBUG
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Debug.Log("@1");
                Debug.LogWarning("@2");
                Debug.LogError("@3");
            }
#endif
        }

        protected override void LateUpdate()
        {
            if (isQuittingApplication)
                return;

            int numberOfLogsToProcess = isLogWindowVisible ? queuedLogEntries.Count : (queuedLogEntries.Count - queuedLogLimit);
            ProcessQueuedLogs(numberOfLogsToProcess);

            if (uncollapsedLogEntries.Count >= maxLogCount)
            {
                /// If log window isn't visible, remove the logs over time (i.e. don't remove more than <see cref="logsToRemoveAfterMaxLogCount"/>) to avoid performance issues.
                int numberOfLogsToRemove = Mathf.Min(!isLogWindowVisible ? logsToRemoveAfterMaxLogCount : (uncollapsedLogEntries.Count - maxLogCount + logsToRemoveAfterMaxLogCount), uncollapsedLogEntries.Count);
                RemoveOldestLogs(numberOfLogsToRemove);
            }

            // Don't perform CPU heavy tasks if neither the log window nor the popup is visible
            if (!isLogWindowVisible && !PopupEnabled)
                return;

            int newInfoEntryCount, newWarningEntryCount, newErrorEntryCount;
            lock (logEntriesLock)
            {
                newInfoEntryCount = this.newInfoEntryCount;
                newWarningEntryCount = this.newWarningEntryCount;
                newErrorEntryCount = this.newErrorEntryCount;

                this.newInfoEntryCount = 0;
                this.newWarningEntryCount = 0;
                this.newErrorEntryCount = 0;
            }

            // Update entry count texts in a single batch
            if (newInfoEntryCount > 0 || newWarningEntryCount > 0 || newErrorEntryCount > 0)
            {
                if (newInfoEntryCount > 0)
                {
                    infoEntryCount += newInfoEntryCount;
                    if (isLogWindowVisible)
                        infoEntryCountText.text = infoEntryCount.ToString();
                }

                if (newWarningEntryCount > 0)
                {
                    warningEntryCount += newWarningEntryCount;
                    if (isLogWindowVisible)
                        warningEntryCountText.text = warningEntryCount.ToString();
                }

                if (newErrorEntryCount > 0)
                {
                    errorEntryCount += newErrorEntryCount;
                    if (isLogWindowVisible)
                        errorEntryCountText.text = errorEntryCount.ToString();
                }

                // If debug popup is visible, notify it of the new debug entries
                if (!isLogWindowVisible)
                {
                    entryCountTextsDirty = true;

                    if (popupVisibility == PopupVisibility.WhenLogReceived && !popupManager.IsVisible)
                    {
                        if ((newInfoEntryCount > 0 && (popupVisibilityLogFilter & DebugLogFilter.Info) == DebugLogFilter.Info) ||
                            (newWarningEntryCount > 0 && (popupVisibilityLogFilter & DebugLogFilter.Warning) == DebugLogFilter.Warning) ||
                            (newErrorEntryCount > 0 && (popupVisibilityLogFilter & DebugLogFilter.Error) == DebugLogFilter.Error))
                        {
                            popupManager.Show();
                        }
                    }

                    if (popupManager.IsVisible)
                        popupManager.NewLogsArrived(newInfoEntryCount, newWarningEntryCount, newErrorEntryCount);
                }
            }

            if (isLogWindowVisible)
            {
                // Update visible logs if necessary
                if (shouldUpdateRecycledListView)
                    OnLogEntriesUpdated(false, false);

                // Automatically expand the target log (if any)
                if (indexOfLogEntryToSelectAndFocus >= 0)
                {
                    if (indexOfLogEntryToSelectAndFocus < logEntriesToShow.Count)
                        recycledListView.SelectAndFocusOnLogItemAtIndex(indexOfLogEntryToSelectAndFocus);

                    indexOfLogEntryToSelectAndFocus = -1;
                }

                if (entryCountTextsDirty)
                {
                    infoEntryCountText.text = infoEntryCount.ToString();
                    warningEntryCountText.text = warningEntryCount.ToString();
                    errorEntryCountText.text = errorEntryCount.ToString();

                    entryCountTextsDirty = false;
                }

                float logWindowWidth = logWindowTR.rect.width;
                if (!Mathf.Approximately(logWindowWidth, logWindowPreviousWidth))
                {
                    logWindowPreviousWidth = logWindowWidth;

                    if (searchbar)
                    {
                        if (logWindowWidth >= topSearchbarMinWidth)
                        {
                            if (searchbar.parent == searchbarSlotBottom)
                            {
                                searchbarSlotTop.gameObject.SetActive(true);
                                searchbar.SetParent(searchbarSlotTop, false);
                                searchbarSlotBottom.gameObject.SetActive(false);

                                logItemsScrollRectTR.anchoredPosition = Vector2.zero;
                                logItemsScrollRectTR.sizeDelta = logItemsScrollRectOriginalSize;
                            }
                        }
                        else
                        {
                            if (searchbar.parent == searchbarSlotTop)
                            {
                                searchbarSlotBottom.gameObject.SetActive(true);
                                searchbar.SetParent(searchbarSlotBottom, false);
                                searchbarSlotTop.gameObject.SetActive(false);

                                float searchbarHeight = searchbarSlotBottom.sizeDelta.y;
                                logItemsScrollRectTR.anchoredPosition = new Vector2(0f, searchbarHeight * -0.5f);
                                logItemsScrollRectTR.sizeDelta = logItemsScrollRectOriginalSize - new Vector2(0f, searchbarHeight);
                            }
                        }
                    }

                    recycledListView.OnViewportWidthChanged();
                }

                // If SnapToBottom is enabled, force the scrollbar to the bottom
                if (SnapToBottom)
                {
                    logItemsScrollRect.verticalNormalizedPosition = 0f;

                    if (snapToBottomButton.activeSelf)
                        snapToBottomButton.SetActive(false);
                }
                else
                {
                    float scrollPos = logItemsScrollRect.verticalNormalizedPosition;
                    if (snapToBottomButton.activeSelf != (scrollPos > 1E-6f && scrollPos < 0.9999f))
                        snapToBottomButton.SetActive(!snapToBottomButton.activeSelf);
                }

                if (showCommandSuggestions && commandInputField.isFocused && commandInputField.caretPosition != commandInputFieldPrevCaretPos)
                    RefreshCommandSuggestions(commandInputField.text);

                if (commandInputField.isFocused && commandHistory.Count > 0)
                {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
					if( Keyboard.current != null )
#endif
                    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
						if( Keyboard.current[Key.UpArrow].wasPressedThisFrame )
#else
                        if (Input.GetKeyDown(KeyCode.UpArrow))
#endif
                        {
                            if (commandHistoryIndex == -1)
                            {
                                commandHistoryIndex = commandHistory.Count - 1;
                                unfinishedCommand = commandInputField.text;
                            }
                            else if (--commandHistoryIndex < 0)
                                commandHistoryIndex = 0;

                            commandInputField.text = commandHistory[commandHistoryIndex];
                            commandInputField.caretPosition = commandInputField.text.Length;
                        }
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
						else if( Keyboard.current[Key.DownArrow].wasPressedThisFrame && commandHistoryIndex != -1 )
#else
                        else if (Input.GetKeyDown(KeyCode.DownArrow) && commandHistoryIndex != -1)
#endif
                        {
                            if (++commandHistoryIndex < commandHistory.Count)
                                commandInputField.text = commandHistory[commandHistoryIndex];
                            else
                            {
                                commandHistoryIndex = -1;
                                commandInputField.text = unfinishedCommand ?? string.Empty;
                            }
                        }
                    }
                }
            }

            if (screenDimensionsChanged)
            {
                // Update the recycled list view
                if (isLogWindowVisible)
                    recycledListView.OnViewportHeightChanged();
                else
                    popupManager.UpdatePosition(true);

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
                CheckScreenCutout();
#endif

                screenDimensionsChanged = false;
            }
        }

        public override void ReceivedLog(string logString, string stackTrace, LogType logType)
        {
            Debug.LogFormat(LOG_FORMAT, "ReceivedLog(), isQuittingApplication : " + isQuittingApplication);
            if (isQuittingApplication == true)
                return;

            switch (logType)
            {
                case LogType.Log:
                    if (!receiveInfoLogs)
                        return;
                    break;
                case LogType.Warning:
                    if (!receiveWarningLogs)
                        return; 
                    break;
                case LogType.Error:
                    if (!receiveErrorLogs)
                        return;
                    break;
                case LogType.Assert:

                case LogType.Exception:
                    if (!receiveExceptionLogs)
                        return;
                    break;
            }

            QueuedDebugLogEntry queuedLogEntry = new QueuedDebugLogEntry(logString, stackTrace, logType);
            DebugLogEntryTimestamp queuedLogEntryTimestamp;
            if (queuedLogEntriesTimestamps != null)
            {
                // It is 10 times faster to cache local time's offset from UtcNow and add it to UtcNow to get local time at any time
                System.DateTime dateTime = System.DateTime.UtcNow + localTimeUtcOffset;
#if !IDG_OMIT_ELAPSED_TIME && !IDG_OMIT_FRAMECOUNT
                queuedLogEntryTimestamp = new DebugLogEntryTimestamp(dateTime, lastElapsedSeconds, lastFrameCount);
#elif !IDG_OMIT_ELAPSED_TIME
				queuedLogEntryTimestamp = new DebugLogEntryTimestamp( dateTime, lastElapsedSeconds );
#elif !IDG_OMIT_FRAMECOUNT
				queuedLogEntryTimestamp = new DebugLogEntryTimestamp( dateTime, lastFrameCount );
#else
				queuedLogEntryTimestamp = new DebugLogEntryTimestamp( dateTime );
#endif
            }
            else
                queuedLogEntryTimestamp = dummyLogEntryTimestamp;

            lock (logEntriesLock)
            {
                /// Enforce <see cref="maxLogCount"/> in queued logs, as well. That's because when it's exceeded, the oldest queued logs will
                /// be removed by <see cref="RemoveOldestLogs"/> immediately after they're processed anyways (i.e. waste of CPU and RAM).
                if (queuedLogEntries.Count + 1 >= maxLogCount)
                {
                    LogType removedLogType = queuedLogEntries.RemoveFirst().logType;
                    if (removedLogType == LogType.Log)
                        newInfoEntryCount--;
                    else if (removedLogType == LogType.Warning)
                        newWarningEntryCount--;
                    else
                        newErrorEntryCount--;

                    if (queuedLogEntriesTimestamps != null)
                        queuedLogEntriesTimestamps.RemoveFirst();
                }

                queuedLogEntries.Add(queuedLogEntry);

                if (queuedLogEntriesTimestamps != null)
                    queuedLogEntriesTimestamps.Add(queuedLogEntryTimestamp);

                if (logType == LogType.Log)
                    newInfoEntryCount++;
                else if (logType == LogType.Warning)
                    newWarningEntryCount++;
                else
                    newErrorEntryCount++;
            }
        }

        protected override void OnApplicationQuitting()
        {
            isQuittingApplication = true;
        }

        public override void ShowLogWindow()
        {
            Debug.LogFormat(LOG_FORMAT, "ShowLogWindow()");

            // Show the log window
            logWindowCanvasGroup.blocksRaycasts = true;
            logWindowCanvasGroup.alpha = logWindowOpacity;

            _ui_popup.Hide();

            // Update the recycled list view 
            // (in case new entries were intercepted while log window was hidden)
            OnLogEntriesUpdated(true, true);

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            // Focus on the command input field on standalone platforms when the console is opened
            if (autoFocusOnCommandInputField)
                StartCoroutine(ActivateCommandInputFieldCoroutine());
#endif

            isLogWindowVisible = true;

            if (OnLogWindowShown != null)
                OnLogWindowShown();
        }
    }
}