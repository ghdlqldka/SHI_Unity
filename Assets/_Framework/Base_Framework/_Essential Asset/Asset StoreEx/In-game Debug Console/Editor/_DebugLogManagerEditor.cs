using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IngameDebugConsole
{
	[CustomEditor( typeof( _DebugLogManager ) )]
	public class _DebugLogManagerEditor : DebugLogManagerEditor
	{
        protected SerializedProperty m_Script;

        protected SerializedProperty maxCollapsedLogLength;
        protected SerializedProperty maxExpandedLogLength;

        protected SerializedProperty logItemPrefab;
        protected SerializedProperty logItemFontOverride;
        protected SerializedProperty commandSuggestionPrefab;
        protected SerializedProperty infoLog;
        protected SerializedProperty warningLog;
        protected SerializedProperty errorLog;
        protected SerializedProperty resizeIconAllDirections;
        protected SerializedProperty resizeIconVerticalOnly;
        protected SerializedProperty collapseButtonNormalColor;
        protected SerializedProperty collapseButtonSelectedColor;
        protected SerializedProperty filterButtonsNormalColor;
        protected SerializedProperty filterButtonsSelectedColor;
        protected SerializedProperty commandSuggestionHighlightStart;
        protected SerializedProperty commandSuggestionHighlightEnd;

        protected SerializedProperty logWindowTR;
        protected SerializedProperty canvasTR;
        protected SerializedProperty logItemsContainer;
        protected SerializedProperty commandSuggestionsContainer;
        protected SerializedProperty commandInputField;
        protected SerializedProperty hideButton;
        protected SerializedProperty clearButton;
        protected SerializedProperty collapseButton;
        protected SerializedProperty filterInfoButton;
        protected SerializedProperty filterWarningButton;
        protected SerializedProperty filterErrorButton;
        protected SerializedProperty infoEntryCountText;
        protected SerializedProperty warningEntryCountText;
        protected SerializedProperty errorEntryCountText;
        protected SerializedProperty searchbar;
        protected SerializedProperty searchbarSlotTop;
        protected SerializedProperty searchbarSlotBottom;
        protected SerializedProperty resizeButton;
        protected SerializedProperty snapToBottomButton;
        protected SerializedProperty logWindowCanvasGroup;
        protected SerializedProperty popupManager;
        protected SerializedProperty logItemsScrollRect;
        protected SerializedProperty recycledListView;

        protected SerializedProperty _ui_window;
        // protected _UI_DebugLogWindow ;

        protected override void OnEnable()
		{
            m_Script = serializedObject.FindProperty("m_Script");

            base.OnEnable();

            maxCollapsedLogLength = serializedObject.FindProperty("maxCollapsedLogLength");
            maxExpandedLogLength = serializedObject.FindProperty("maxExpandedLogLength");

            logItemPrefab = serializedObject.FindProperty("logItemPrefab");
            logItemFontOverride = serializedObject.FindProperty("logItemFontOverride");
            commandSuggestionPrefab = serializedObject.FindProperty("commandSuggestionPrefab");
            infoLog = serializedObject.FindProperty("infoLog");
            warningLog = serializedObject.FindProperty("warningLog");
            errorLog = serializedObject.FindProperty("errorLog");
            resizeIconAllDirections = serializedObject.FindProperty("resizeIconAllDirections");
            resizeIconVerticalOnly = serializedObject.FindProperty("resizeIconVerticalOnly");
            collapseButtonNormalColor = serializedObject.FindProperty("collapseButtonNormalColor");
            collapseButtonSelectedColor = serializedObject.FindProperty("collapseButtonSelectedColor");
            filterButtonsNormalColor = serializedObject.FindProperty("filterButtonsNormalColor");
            filterButtonsSelectedColor = serializedObject.FindProperty("filterButtonsSelectedColor");
            commandSuggestionHighlightStart = serializedObject.FindProperty("commandSuggestionHighlightStart");
            commandSuggestionHighlightEnd = serializedObject.FindProperty("commandSuggestionHighlightEnd");

            logWindowTR = serializedObject.FindProperty("logWindowTR");
            canvasTR = serializedObject.FindProperty("canvasTR");
            logItemsContainer = serializedObject.FindProperty("logItemsContainer");
            commandSuggestionsContainer = serializedObject.FindProperty("commandSuggestionsContainer");
            commandInputField = serializedObject.FindProperty("commandInputField");
            hideButton = serializedObject.FindProperty("hideButton");
            clearButton = serializedObject.FindProperty("clearButton");
            collapseButton = serializedObject.FindProperty("collapseButton");
            filterInfoButton = serializedObject.FindProperty("filterInfoButton");
            filterWarningButton = serializedObject.FindProperty("filterWarningButton");
            filterErrorButton = serializedObject.FindProperty("filterErrorButton");
            infoEntryCountText = serializedObject.FindProperty("infoEntryCountText");
            warningEntryCountText = serializedObject.FindProperty("warningEntryCountText");
            errorEntryCountText = serializedObject.FindProperty("errorEntryCountText");
            searchbar = serializedObject.FindProperty("searchbar");
            searchbarSlotTop = serializedObject.FindProperty("searchbarSlotTop");
            searchbarSlotBottom = serializedObject.FindProperty("searchbarSlotBottom");
            resizeButton = serializedObject.FindProperty("resizeButton");
            snapToBottomButton = serializedObject.FindProperty("snapToBottomButton");
            logWindowCanvasGroup = serializedObject.FindProperty("logWindowCanvasGroup");
            popupManager = serializedObject.FindProperty("popupManager");
            logItemsScrollRect = serializedObject.FindProperty("logItemsScrollRect");
            recycledListView = serializedObject.FindProperty("recycledListView");

            _ui_window = serializedObject.FindProperty("_ui_window");
        }

        public override void OnInspectorGUI()
		{
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            // base.OnInspectorGUI();
            // serializedObject.Update();

            // EditorGUILayout.PropertyField(singleton);

            // EditorGUILayout.Space();

            EditorGUILayout.PropertyField(minimumHeight);

            EditorGUILayout.PropertyField(enableHorizontalResizing);
            if (enableHorizontalResizing.boolValue)
            {
                DrawSubProperty(resizeFromRight);
                DrawSubProperty(minimumWidth);
            }

            EditorGUILayout.PropertyField(avoidScreenCutout);
            DrawSubProperty(popupAvoidsScreenCutout);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(startMinimized);
            EditorGUILayout.PropertyField(logWindowOpacity);
            EditorGUILayout.PropertyField(popupOpacity);

            EditorGUILayout.PropertyField(popupVisibility);
            if (popupVisibility.intValue == (int)PopupVisibility.WhenLogReceived)
            {
                EditorGUI.indentLevel++;
                Rect rect = EditorGUILayout.GetControlRect();
                EditorGUI.BeginProperty(rect, GUIContent.none, popupVisibilityLogFilter);
                popupVisibilityLogFilter.intValue = (int)(DebugLogFilter)EditorGUI.EnumFlagsField(rect, popupVisibilityLogFilterLabel, (DebugLogFilter)popupVisibilityLogFilter.intValue);
                EditorGUI.EndProperty();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(toggleWithKey);
            if (toggleWithKey.boolValue)
                DrawSubProperty(toggleKey);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(enableSearchbar);
            if (enableSearchbar.boolValue)
                DrawSubProperty(topSearchbarMinWidth);

            EditorGUILayout.PropertyField(copyAllLogsOnResizeButtonClick);

            EditorGUILayout.Space();

            // EditorGUILayout.PropertyField(receiveLogsWhileInactive);

            EditorGUILayout.PrefixLabel(receivedLogTypesLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(receiveInfoLogs, receiveInfoLogsLabel);
            EditorGUILayout.PropertyField(receiveWarningLogs, receiveWarningLogsLabel);
            EditorGUILayout.PropertyField(receiveErrorLogs, receiveErrorLogsLabel);
            EditorGUILayout.PropertyField(receiveExceptionLogs, receiveExceptionLogsLabel);
            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(receiveLogcatLogsInAndroid);
            if (receiveLogcatLogsInAndroid.boolValue)
                DrawSubProperty(logcatArguments);

            EditorGUILayout.PropertyField(captureLogTimestamps);
            if (captureLogTimestamps.boolValue)
                DrawSubProperty(alwaysDisplayTimestamps);

            EditorGUILayout.PropertyField(maxLogCount);
            DrawSubProperty(logsToRemoveAfterMaxLogCount);

            EditorGUILayout.PropertyField(queuedLogLimit);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(clearCommandAfterExecution);
            EditorGUILayout.PropertyField(commandHistorySize);
            EditorGUILayout.PropertyField(showCommandSuggestions);
            EditorGUILayout.PropertyField(autoFocusOnCommandInputField);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(maxCollapsedLogLength);
            EditorGUILayout.PropertyField(maxExpandedLogLength);

            EditorGUILayout.PropertyField(logItemPrefab);
            EditorGUILayout.PropertyField(logItemFontOverride);
            EditorGUILayout.PropertyField(commandSuggestionPrefab);
            EditorGUILayout.PropertyField(infoLog);
            EditorGUILayout.PropertyField(warningLog);
            EditorGUILayout.PropertyField(errorLog);
            EditorGUILayout.PropertyField(resizeIconAllDirections);
            EditorGUILayout.PropertyField(resizeIconVerticalOnly);
            EditorGUILayout.PropertyField(collapseButtonNormalColor);
            EditorGUILayout.PropertyField(collapseButtonSelectedColor);
            EditorGUILayout.PropertyField(filterButtonsNormalColor);
            EditorGUILayout.PropertyField(filterButtonsSelectedColor);
            EditorGUILayout.PropertyField(commandSuggestionHighlightStart);
            EditorGUILayout.PropertyField(commandSuggestionHighlightEnd);


            // EditorGUILayout.PropertyField(logWindowTR);
            EditorGUILayout.PropertyField(_ui_window);
            EditorGUILayout.PropertyField(canvasTR);
            EditorGUILayout.PropertyField(logItemsContainer);
            EditorGUILayout.PropertyField(commandSuggestionsContainer);
            EditorGUILayout.PropertyField(commandInputField);
            EditorGUILayout.PropertyField(hideButton);
            EditorGUILayout.PropertyField(clearButton);
            EditorGUILayout.PropertyField(collapseButton);
            EditorGUILayout.PropertyField(filterInfoButton);
            EditorGUILayout.PropertyField(filterWarningButton);
            EditorGUILayout.PropertyField(filterErrorButton);
            EditorGUILayout.PropertyField(infoEntryCountText);
            EditorGUILayout.PropertyField(warningEntryCountText);
            EditorGUILayout.PropertyField(errorEntryCountText);
            EditorGUILayout.PropertyField(searchbar);
            EditorGUILayout.PropertyField(searchbarSlotTop);
            EditorGUILayout.PropertyField(searchbarSlotBottom);
            EditorGUILayout.PropertyField(resizeButton);
            EditorGUILayout.PropertyField(snapToBottomButton);

            EditorGUILayout.PropertyField(logWindowCanvasGroup);
            EditorGUILayout.PropertyField(popupManager);
            EditorGUILayout.PropertyField(logItemsScrollRect);
            EditorGUILayout.PropertyField(recycledListView);

            // _DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }

        protected internal static void _DrawPropertiesExcluding(SerializedObject obj, params string[] propertyToExclude)
        {
            SerializedProperty iterator = obj.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (propertyToExclude.Contains(iterator.name) == false)
                {
                    Debug.Log("iterator.name : " + iterator.name);
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }
        }
    }
}