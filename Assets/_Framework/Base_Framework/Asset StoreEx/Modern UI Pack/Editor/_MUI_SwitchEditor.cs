using UnityEngine;
using UnityEditor;

namespace Michsky.MUIP
{
    [CustomEditor(typeof(_MUI_Switch))]
    public class _MUI_SwitchEditor : SwitchManagerEditor
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
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}