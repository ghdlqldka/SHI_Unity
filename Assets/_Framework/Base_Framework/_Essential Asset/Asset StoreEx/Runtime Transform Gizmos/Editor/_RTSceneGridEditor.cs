using UnityEngine;
using UnityEditor;

namespace RTG
{
    [CustomEditor(typeof(_RTSceneGrid))]
    public class _RTSceneGridEditor : RTSceneGridInspector
    {
        protected SerializedProperty m_Script;

        // protected RTSceneGrid _sceneGrid;
        protected _RTSceneGrid _rtSceneGrid
        {
            get
            { 
                return _sceneGrid as _RTSceneGrid;
            }
        }

        protected override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
            _sceneGrid = target as RTSceneGrid;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            _rtSceneGrid.Settings.UsesFoldout = true;
            _rtSceneGrid.Settings.FoldoutLabel = "Settings";
            _rtSceneGrid.Settings.RenderEditorGUI(_rtSceneGrid);

            _rtSceneGrid.LookAndFeel.UsesFoldout = true;
            _rtSceneGrid.LookAndFeel.FoldoutLabel = "Look & feel";
            _rtSceneGrid.LookAndFeel.RenderEditorGUI(_rtSceneGrid);

            _rtSceneGrid.Hotkeys.UsesFoldout = true;
            _rtSceneGrid.Hotkeys.FoldoutLabel = "Hotkeys";
            _rtSceneGrid.Hotkeys.RenderEditorGUI(_rtSceneGrid);
        }
    }
}