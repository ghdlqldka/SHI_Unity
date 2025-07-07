using UnityEngine;
using UnityEditor;

namespace Michsky.MUIP
{
    [CustomEditor(typeof(_MUI_TabControl))]
    public class _MUI_TabControlEditor : WindowManagerEditor
    {
        protected SerializedProperty m_Script;

        protected override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space();
            GUI.enabled = true;

            base.OnInspectorGUI();
        }
    }
}