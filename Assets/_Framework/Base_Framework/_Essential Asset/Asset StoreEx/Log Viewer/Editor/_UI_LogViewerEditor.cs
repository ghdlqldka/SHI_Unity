using UnityEngine;
using UnityEditor;

namespace _Base_Framework
{
    // [CanEditMultipleObjects]
    [CustomEditor(typeof(_UI_LogViewer))]
    public class _UI_LogViewerEditor : Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty _canvas;
        protected SerializedProperty logsContainerT;
        protected SerializedProperty logItemPrefab;

        protected SerializedProperty logsViewPanel;
        protected SerializedProperty infoViewPanel;

        protected SerializedProperty memoryToggle;
        protected SerializedProperty memoryButtonText;

        protected SerializedProperty logToggle;
        protected SerializedProperty logCountText;
        protected SerializedProperty warningLogToggle;
        protected SerializedProperty warningLogCountText;
        protected SerializedProperty errorLogToggle;
        protected SerializedProperty errorLogCountText;

        protected SerializedProperty stacktraceText;
        protected SerializedProperty stacktraceTimeText;
        protected SerializedProperty stacktraceMemoryText;

        protected SerializedProperty buildDateText;
        protected SerializedProperty deviceInfoText;
        protected SerializedProperty gpuInfoText;
        protected SerializedProperty screenInfoText;
        protected SerializedProperty sysMemorySizeText;
        protected SerializedProperty memoryUsageInfoText;
        protected SerializedProperty osInfoText;
        protected SerializedProperty appStartTimeInfoText;
        protected SerializedProperty realtimeSinceStartupText;
        protected SerializedProperty unityVersionText;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            _canvas = serializedObject.FindProperty("_canvas");
            logsContainerT = serializedObject.FindProperty("logsContainerT");
            logItemPrefab = serializedObject.FindProperty("logItemPrefab");

            logsViewPanel = serializedObject.FindProperty("logsViewPanel");
            infoViewPanel = serializedObject.FindProperty("infoViewPanel");

            memoryToggle = serializedObject.FindProperty("memoryToggle");
            memoryButtonText = serializedObject.FindProperty("memoryButtonText");

            logToggle = serializedObject.FindProperty("logToggle");
            logCountText = serializedObject.FindProperty("logCountText");
            warningLogToggle = serializedObject.FindProperty("warningLogToggle");
            warningLogCountText = serializedObject.FindProperty("warningLogCountText");
            errorLogToggle = serializedObject.FindProperty("errorLogToggle");
            errorLogCountText = serializedObject.FindProperty("errorLogCountText");

            stacktraceText = serializedObject.FindProperty("stacktraceText");
            stacktraceTimeText = serializedObject.FindProperty("stacktraceTimeText");
            stacktraceMemoryText = serializedObject.FindProperty("stacktraceMemoryText");

            buildDateText = serializedObject.FindProperty("buildDateText");
            deviceInfoText = serializedObject.FindProperty("deviceInfoText");
            gpuInfoText = serializedObject.FindProperty("gpuInfoText");
            screenInfoText = serializedObject.FindProperty("screenInfoText");
            sysMemorySizeText = serializedObject.FindProperty("sysMemorySizeText");
            memoryUsageInfoText = serializedObject.FindProperty("memoryUsageInfoText");
            osInfoText = serializedObject.FindProperty("osInfoText");
            appStartTimeInfoText = serializedObject.FindProperty("appStartTimeInfoText");
            realtimeSinceStartupText = serializedObject.FindProperty("realtimeSinceStartupText");
            unityVersionText = serializedObject.FindProperty("unityVersionText");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(_canvas);
            EditorGUILayout.PropertyField(logsContainerT);
            EditorGUILayout.PropertyField(logItemPrefab);

            EditorGUILayout.PropertyField(logsViewPanel);
            EditorGUILayout.PropertyField(infoViewPanel);

            EditorGUILayout.PropertyField(memoryToggle);
            EditorGUILayout.PropertyField(memoryButtonText);

            EditorGUILayout.PropertyField(logToggle);
            EditorGUILayout.PropertyField(logCountText);
            EditorGUILayout.PropertyField(warningLogToggle);
            EditorGUILayout.PropertyField(warningLogCountText);
            EditorGUILayout.PropertyField(errorLogToggle);
            EditorGUILayout.PropertyField(errorLogCountText);

            EditorGUILayout.PropertyField(stacktraceText);
            EditorGUILayout.PropertyField(stacktraceTimeText);
            EditorGUILayout.PropertyField(stacktraceMemoryText);

            EditorGUILayout.PropertyField(buildDateText);
            EditorGUILayout.PropertyField(deviceInfoText);
            EditorGUILayout.PropertyField(gpuInfoText);
            EditorGUILayout.PropertyField(screenInfoText);
            EditorGUILayout.PropertyField(sysMemorySizeText);
            EditorGUILayout.PropertyField(memoryUsageInfoText);
            EditorGUILayout.PropertyField(osInfoText);
            EditorGUILayout.PropertyField(appStartTimeInfoText);
            EditorGUILayout.PropertyField(realtimeSinceStartupText);
            EditorGUILayout.PropertyField(unityVersionText);

            serializedObject.ApplyModifiedProperties();
        }

    }
}
