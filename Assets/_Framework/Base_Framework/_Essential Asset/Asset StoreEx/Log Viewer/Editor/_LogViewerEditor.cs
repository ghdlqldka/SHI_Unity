using UnityEngine;
using UnityEditor;

namespace _Base_Framework
{
    // [CanEditMultipleObjects]
    [CustomEditor(typeof(_LogViewer))]
    public class _LogViewerEditor : Editor
    {
        protected SerializedProperty m_Script;

        // protected SerializedProperty UserData;

        // protected SerializedProperty fps;
        // protected SerializedProperty fpsText;

        // protected SerializedProperty images;

        // protected SerializedProperty size;
        // protected SerializedProperty maxSize;

        protected SerializedProperty gesture;
        // protected SerializedProperty numOfCircleToShow;
        // protected SerializedProperty Initialized;

        protected SerializedProperty _sampleList;
        protected SerializedProperty _cachedStringDic;
        // 

        protected SerializedProperty _viewMode;
        // protected SerializedProperty collapse;
        // protected SerializedProperty clearOnNewSceneLoaded;
        // protected SerializedProperty _showTime;
        // protected SerializedProperty _showScene;
        protected SerializedProperty _showMemory;
        // protected SerializedProperty _showFps;
        // protected SerializedProperty _showGraph;
        protected SerializedProperty _showLog;
        protected SerializedProperty _showWarning;
        protected SerializedProperty _showError;

        // protected SerializedProperty showClearOnNewSceneLoadedButton;
        // protected SerializedProperty _showTimeButton;
        // protected SerializedProperty _showSceneButton;
        // protected SerializedProperty _showMemButton;
        // protected SerializedProperty _showFpsButton;
        // protected SerializedProperty _showSearchText;
        // protected SerializedProperty _showCopyButton;
        // protected SerializedProperty _showSaveButton;

        protected SerializedProperty _buildDate;
        protected SerializedProperty _logDate;

        // protected SerializedProperty _filterText;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            // UserData = serializedObject.FindProperty("UserData");

            // fps = serializedObject.FindProperty("fps");
            // fpsText = serializedObject.FindProperty("fpsText");

            // images = serializedObject.FindProperty("images");

            // size = serializedObject.FindProperty("size");
            // maxSize = serializedObject.FindProperty("maxSize");

            gesture = serializedObject.FindProperty("gesture");
            // numOfCircleToShow = serializedObject.FindProperty("numOfCircleToShow");
            // Initialized = serializedObject.FindProperty("Initialized");

            _sampleList = serializedObject.FindProperty("_sampleList");
            _cachedStringDic = serializedObject.FindProperty("_cachedStringDic");
            // 

            _viewMode = serializedObject.FindProperty("_viewMode");
            // collapse = serializedObject.FindProperty("collapse");
            // clearOnNewSceneLoaded = serializedObject.FindProperty("clearOnNewSceneLoaded");
            // _showTime = serializedObject.FindProperty("_showTime");
            // _showScene = serializedObject.FindProperty("_showScene");
            _showMemory = serializedObject.FindProperty("_showMemory");
            // _showFps = serializedObject.FindProperty("_showFps");
            // _showGraph = serializedObject.FindProperty("_showGraph");
            _showLog = serializedObject.FindProperty("_showLog");
            _showWarning = serializedObject.FindProperty("_showWarning");
            _showError = serializedObject.FindProperty("_showError");

            // showClearOnNewSceneLoadedButton = serializedObject.FindProperty("showClearOnNewSceneLoadedButton");
            // _showTimeButton = serializedObject.FindProperty("_showTimeButton");
            // _showSceneButton = serializedObject.FindProperty("_showSceneButton");
            // _showMemButton = serializedObject.FindProperty("_showMemButton");
            // _showFpsButton = serializedObject.FindProperty("_showFpsButton");
            // _showSearchText = serializedObject.FindProperty("_showSearchText");
            // _showCopyButton = serializedObject.FindProperty("_showCopyButton");
            // _showSaveButton = serializedObject.FindProperty("_showSaveButton");

            _buildDate = serializedObject.FindProperty("_buildDate");
            _logDate = serializedObject.FindProperty("_logDate");

            // _filterText = serializedObject.FindProperty("_filterText");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space(10);

            // EditorGUILayout.PropertyField(UserData);

            /*
            EditorGUILayout.Space(10);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(fps);
            EditorGUILayout.PropertyField(fpsText);
            EditorGUI.EndDisabledGroup();
            */

            // EditorGUILayout.PropertyField(images);

            // EditorGUILayout.Space(10);
            // EditorGUILayout.PropertyField(size);
            // EditorGUILayout.PropertyField(maxSize);

            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(gesture);
            // EditorGUILayout.PropertyField(numOfCircleToShow);
            EditorGUI.BeginDisabledGroup(true);
            // EditorGUILayout.PropertyField(Initialized);
            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(_sampleList);
            EditorGUILayout.PropertyField(_cachedStringDic);
            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(_viewMode);
            // EditorGUILayout.PropertyField(collapse);
            // EditorGUILayout.PropertyField(clearOnNewSceneLoaded);
            // EditorGUILayout.PropertyField(_showTime);
            // EditorGUILayout.PropertyField(_showScene);
            EditorGUILayout.PropertyField(_showMemory);
            // EditorGUILayout.PropertyField(_showFps);
            // EditorGUILayout.PropertyField(_showGraph);
            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(_showLog);
            EditorGUILayout.PropertyField(_showWarning);
            EditorGUILayout.PropertyField(_showError);
            EditorGUILayout.Space(10);

            // EditorGUILayout.PropertyField(showClearOnNewSceneLoadedButton);
            // EditorGUILayout.PropertyField(_showTimeButton);
            // EditorGUILayout.PropertyField(_showSceneButton);
            // EditorGUILayout.PropertyField(_showMemButton);
            // EditorGUILayout.PropertyField(_showFpsButton);
            // EditorGUILayout.PropertyField(_showSearchText);
            // EditorGUILayout.PropertyField(_showCopyButton);
            // EditorGUILayout.PropertyField(_showSaveButton);
            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(_buildDate);
            EditorGUILayout.PropertyField(_logDate);
            // EditorGUILayout.Space(10);

            // EditorGUILayout.PropertyField(_filterText);
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }

    }
}
