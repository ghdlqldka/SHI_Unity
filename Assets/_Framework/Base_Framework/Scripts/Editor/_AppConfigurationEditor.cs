using UnityEngine;
using UnityEditor;

namespace _Base_Framework
{
    // [CanEditMultipleObjects]
    [CustomEditor(typeof(_AppConfiguration))]
    public class _AppConfigurationEditor : Editor
    {
        protected SerializedProperty m_Script;

        protected _AppConfiguration _appConfiguration;
        protected _AppConfiguration AppConfig
        {
            get
            {
                return _appConfiguration;
            }
        }

        protected GUIStyle normalStyle;
        protected bool appConfigDataSection;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            _appConfiguration = (_AppConfiguration)target;
            if (normalStyle == null)
            {
                normalStyle = new GUIStyle(EditorStyles.foldout);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(true); // <++++++++++++++++++
            EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
            appConfigDataSection = EditorGUILayout.Foldout(appConfigDataSection, "App Config Data", normalStyle);
            EditorGUILayout.EndHorizontal();

            if (appConfigDataSection)
            {
                AppConfig.Data.UseLogViewer = EditorGUILayout.Toggle("Use Log Viewer", AppConfig.Data.UseLogViewer);
                AppConfig.Data.OpenPlayerLogFolder = EditorGUILayout.Toggle("Open PlayerLog Folder", AppConfig.Data.OpenPlayerLogFolder);
            }
            EditorGUI.EndDisabledGroup(); // <--------------------

            serializedObject.ApplyModifiedProperties();
        }
    }
}
